#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2017 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [StorableClass]
  [Item("SemanticTestAnalyzationManipulator", "Base class to test different Semantics.")]
  public abstract class SemanticTestAnalyzationManipulator<T> : AbstractSemanticAnalyzationManipulator<T>
    where T : class, ICFGPythonProblemData {

    [StorableConstructor]
    protected SemanticTestAnalyzationManipulator(bool deserializing) : base(deserializing) { }
    protected SemanticTestAnalyzationManipulator(SemanticTestAnalyzationManipulator<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticTestAnalyzationManipulator() { }

    public override void ReplaceBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, ItemArray<PythonStatementSemantic> semantics, PythonProcess pythonProcess, double timeout, int maxTreeLength, int maxTreeDepth, int maximumSemanticTries) {
      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(symbolicExpressionTree.Root.Grammar);
      var variables = problemData.Variables.GetVariableNames().ToList();
      string variableSettings = problemData.VariableSettings.Count == 0 ? String.Empty : String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));

      var allowedSymbols = new List<ISymbol>();
      // repeat until a fitting parent and child are found (MAX_TRIES times)
      int tries = 0;
      int semanticTries = 0;

      var saveOriginalSemantics = new List<JObject>(semanticTries);
      var saveReplaceSemantics = new List<JObject>(semanticTries);
      var possibleChildren = new List<Tuple<ISymbolicExpressionTreeNode, int>>(semanticTries);
      bool success = false;
      do {
        #region find mutation point
#pragma warning disable 612, 618
        ISymbolicExpressionTreeNode parent = symbolicExpressionTree.Root.IterateNodesPrefix().Skip(1).Where(n => n.SubtreeCount > 0).SelectRandom(random);
#pragma warning restore 612, 618

        int childIndex = random.Next(parent.SubtreeCount);
        var child = parent.GetSubtree(childIndex);
        int maxLength = maxTreeLength - symbolicExpressionTree.Length + child.GetLength();
        int maxDepth = maxTreeDepth - symbolicExpressionTree.Root.GetBranchLevel(child);

        allowedSymbols.Clear();
        foreach (var symbol in parent.Grammar.GetAllowedChildSymbols(parent.Symbol, childIndex)) {
          // check basic properties that the new symbol must have
          if ((symbol.Name != child.Symbol.Name || symbol.MinimumArity > 0) &&
            symbol.InitialFrequency > 0 &&
            parent.Grammar.GetMinimumExpressionDepth(symbol) <= maxDepth &&
            parent.Grammar.GetMinimumExpressionLength(symbol) <= maxLength) {
            allowedSymbols.Add(symbol);
          }
        }
        #endregion
        #region check for semantic difference with a new random tree
        if (allowedSymbols.Count > 0) {
          if (semanticTries <= maximumSemanticTries) {
            // do semantic mutation
            #region calculate original json output
            ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(child, statementProductionNames);
            var statementPos0 = symbolicExpressionTree.IterateNodesPrefix().ToList().IndexOf(statement);
            if (String.IsNullOrEmpty(variableSettings)) {
              variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(semantics.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
            }

            var jsonOriginal = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);

            // compare jsonOriginal to semantic after! Maybe avoid additional evaluation.

            #endregion

            var seedNode = GenerateAndInsertNewSubtree(random, parent, allowedSymbols, childIndex, maxLength, maxDepth);

            #region calculate new json output
            JObject jsonReplaced;
            if (child == statement) {
              // child is executable, so is the new child
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(seedNode, pythonProcess, random, problemData, variables, variableSettings, timeout);
            } else {
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
            }

            if (SemanticMeasure(jsonOriginal, jsonReplaced)) {
              success = true;
              SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(Different);
              MutationTypeParameter.ActualValue = new IntValue(SemanticMutation);
            } else {
              // undo mutation
              parent.RemoveSubtree(childIndex);
              parent.InsertSubtree(childIndex, child);
              allowedSymbols.Clear();
              saveOriginalSemantics.Add(jsonOriginal);
              saveReplaceSemantics.Add(jsonReplaced);
              possibleChildren.Add(new Tuple<ISymbolicExpressionTreeNode, int>(seedNode, childIndex));
            }

            if (problemData.VariableSettings.Count == 0) {
              // reset variableSettings
              variableSettings = String.Empty;
            }
            semanticTries++;
            #endregion

            #region try second semantic comparison
            if (semanticTries >= maximumSemanticTries) {
              for (int index = 0; index < saveOriginalSemantics.Count; index++) {
                if (AdditionalSemanticMeasure(saveOriginalSemantics[index], saveReplaceSemantics[index])) {
                  var mutation = possibleChildren[index];
                  parent.RemoveSubtree(mutation.Item2);
                  parent.InsertSubtree(mutation.Item2, mutation.Item1);
                  success = true;
                  break;
                }
              }
            }
            #endregion

          } else {
            // do random mutation
            #region calculate original json output
            ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(child, statementProductionNames);
            var statementPos0 = symbolicExpressionTree.IterateNodesPrefix().ToList().IndexOf(statement);
            if (String.IsNullOrEmpty(variableSettings)) {
              variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(semantics.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
            }

            var jsonOriginal = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
            #endregion

            var seedNode = GenerateAndInsertNewSubtree(random, parent, allowedSymbols, childIndex, maxLength, maxDepth);
            JObject jsonReplaced;
            if (child == statement) {
              // child is executable, so is the new child
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(seedNode, pythonProcess, random, problemData, variables, variableSettings, timeout);
            } else {
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
            }
            if (JToken.EqualityComparer.Equals(jsonOriginal, jsonReplaced)) {
              SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(Equvivalent);
            } else {
              SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(Different);
            }
            MutationTypeParameter.ActualValue = new IntValue(RandomMutation);
            success = true;
          }
        }
        #endregion
        tries++;
      } while (tries < MAX_TRIES && !success);

      NumberOfTriesParameter.ActualValue = new IntValue(semanticTries);
      if (SemanticallyEquivalentMutationParameter.ActualValue == null) {
        SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(NoMutation);
        MutationTypeParameter.ActualValue = new IntValue(NoMutation);
      }
    }

    /// <summary>
    /// return true if the mutation should take place
    /// </summary>
    protected abstract bool SemanticMeasure(JObject original, JObject replaced);

    /// <summary>
    /// return the index where the mutation should take place. If no mutation should take place, return a value out of range. 
    /// </summary>
    protected virtual bool AdditionalSemanticMeasure(JObject original, JObject replaced) {
      return false;
    }
  }
}

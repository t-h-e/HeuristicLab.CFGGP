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
      if (semantics == null || semantics.Length == 0) {
        ReplaceBranchManipulation.ReplaceRandomBranch(random, symbolicExpressionTree, maxTreeLength, maxTreeDepth);
        SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(NoSemantics);
        MutationTypeParameter.ActualValue = new IntValue(RandomMutation);
        return;
      }

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(symbolicExpressionTree.Root.Grammar);
      var variables = problemData.Variables.GetVariableNames().ToList();
      string variableSettings = problemData.VariableSettings.Count == 0 ? String.Empty : String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));

      var allowedSymbols = new List<ISymbol>();
      // repeat until a fitting parent and child are found (MAX_TRIES times)
      int tries = 0;
      int semanticTries = 0;

      List<JObject> saveOriginalSemantics = null;
      List<JObject> saveReplaceSemantics = null;
      List<Tuple<ISymbolicExpressionTreeNode, ISymbolicExpressionTreeNode, int>> possibleChildren = null; // Item1 = parent, Item2 = seedNode, Item3 = childIndex
      if (UsesAdditionalSemanticMeasure()) {
        saveOriginalSemantics = new List<JObject>(semanticTries);
        saveReplaceSemantics = new List<JObject>(semanticTries);
        possibleChildren = new List<Tuple<ISymbolicExpressionTreeNode, ISymbolicExpressionTreeNode, int>>(semanticTries);
      }
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
            PythonStatementSemantic curSemantics = null;
            if (String.IsNullOrEmpty(variableSettings)) {
              curSemantics = semantics.First(x => x.TreeNodePrefixPos == statementPos0);
              variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(curSemantics.Before, problemData.Variables.GetVariableTypes());
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

            var exception = jsonOriginal["exception"] != null || jsonReplaced["exception"] != null;
            if (curSemantics != null && !exception) {
              jsonOriginal = PythonSemanticComparer.ReplaceNotExecutedCases(jsonOriginal, curSemantics.Before, curSemantics.ExecutedCases);
              jsonReplaced = PythonSemanticComparer.ReplaceNotExecutedCases(jsonReplaced, curSemantics.Before, curSemantics.ExecutedCases);

              jsonOriginal = PythonSemanticComparer.ProduceDifference(jsonOriginal, curSemantics.Before);
              jsonReplaced = PythonSemanticComparer.ProduceDifference(jsonReplaced, curSemantics.Before);
            }

            if (!exception && SemanticMeasure(jsonOriginal, jsonReplaced)) {
              success = true;
              SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(Different);
              MutationTypeParameter.ActualValue = new IntValue(SemanticMutation);
            } else {
              // undo mutation
              parent.RemoveSubtree(childIndex);
              parent.InsertSubtree(childIndex, child);
              allowedSymbols.Clear();

              if (!exception && UsesAdditionalSemanticMeasure()) {
                saveOriginalSemantics.Add(jsonOriginal);
                saveReplaceSemantics.Add(jsonReplaced);
                possibleChildren.Add(new Tuple<ISymbolicExpressionTreeNode, ISymbolicExpressionTreeNode, int>(parent, seedNode, childIndex));
              }
            }

            if (problemData.VariableSettings.Count == 0) {
              // reset variableSettings
              variableSettings = String.Empty;
            }
            semanticTries++;
            #endregion

            #region try second semantic comparison
            if (!success && semanticTries >= maximumSemanticTries && UsesAdditionalSemanticMeasure()) {
              for (int index = 0; index < saveOriginalSemantics.Count; index++) {
                if (AdditionalSemanticMeasure(saveOriginalSemantics[index], saveReplaceSemantics[index])) {
                  var mutation = possibleChildren[index];
                  mutation.Item1.RemoveSubtree(mutation.Item3);
                  mutation.Item1.InsertSubtree(mutation.Item3, mutation.Item2);
                  success = true;
                  SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(Different);
                  MutationTypeParameter.ActualValue = new IntValue(SemanticMutation);
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
    /// if no additional second measure is used, the semantic of a previous try does not have to be saved
    /// </summary>
    protected virtual bool UsesAdditionalSemanticMeasure() {
      return false;
    }

    /// <summary>
    /// return the index where the mutation should take place. If no mutation should take place, return a value out of range. 
    /// </summary>
    protected virtual bool AdditionalSemanticMeasure(JObject original, JObject replaced) {
      return false;
    }
  }
}

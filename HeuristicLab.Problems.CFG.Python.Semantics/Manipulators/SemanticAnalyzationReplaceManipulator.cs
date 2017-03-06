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
  [Item("SemanticAnalyzationReplaceManipulator",
    "Removes a random sub-tree of the input tree and fixes the tree by generating random subtrees if necessary..")]
  public class SemanticAnalyzationReplaceManipulator<T> : AbstractSemanticAnalyzationManipulator<T>
    where T : class, ICFGPythonProblemData {

    [StorableConstructor]
    protected SemanticAnalyzationReplaceManipulator(bool deserializing) : base(deserializing) { }
    protected SemanticAnalyzationReplaceManipulator(SemanticAnalyzationReplaceManipulator<T> original, Cloner cloner)
      : base(original, cloner) { }
    public SemanticAnalyzationReplaceManipulator() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticAnalyzationReplaceManipulator<T>(this, cloner);
    }

    public override void ReplaceBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, ItemArray<PythonStatementSemantic> semantics, PythonProcess pythonProcess, double timeout, int maxTreeLength, int maxTreeDepth, int maximumSemanticTries) {
      var allowedSymbols = new List<ISymbol>();
      ISymbolicExpressionTreeNode parent;
      int childIndex;
      int maxLength;
      int maxDepth;
      // repeat until a fitting parent and child are found (MAX_TRIES times)
      int tries = 0;
      ISymbolicExpressionTreeNode child;
      do {
#pragma warning disable 612, 618
        parent = symbolicExpressionTree.Root.IterateNodesPrefix().Skip(1).Where(n => n.SubtreeCount > 0).SelectRandom(random);
#pragma warning restore 612, 618

        childIndex = random.Next(parent.SubtreeCount);
        child = parent.GetSubtree(childIndex);
        maxLength = maxTreeLength - symbolicExpressionTree.Length + child.GetLength();
        maxDepth = maxTreeDepth - symbolicExpressionTree.Root.GetBranchLevel(child);

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
        tries++;
      } while (tries < MAX_TRIES && allowedSymbols.Count == 0);

      NumberOfTriesParameter.ActualValue = new IntValue(tries); //normal tries. not semantic tries

      if (tries < MAX_TRIES) {
        var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(symbolicExpressionTree.Root.Grammar);
        var variables = problemData.Variables.GetVariableNames().ToList();

        #region calculate original json output
        ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(child, statementProductionNames);
        string variableSettings = problemData.VariableSettings.Count == 0 ? String.Empty : String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));

        var statementPos0 = symbolicExpressionTree.IterateNodesPrefix().ToList().IndexOf(statement);
        if (String.IsNullOrEmpty(variableSettings)) {
          variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(semantics.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
        }

        var jsonOriginal = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
        #endregion

        var weights = allowedSymbols.Select(s => s.InitialFrequency).ToList();
#pragma warning disable 612, 618
        var seedSymbol = allowedSymbols.SelectRandom(weights, random);
#pragma warning restore 612, 618

        // replace the old node with the new node
        var seedNode = seedSymbol.CreateTreeNode();
        if (seedNode.HasLocalParameters)
          seedNode.ResetLocalParameters(random);

        parent.RemoveSubtree(childIndex);
        parent.InsertSubtree(childIndex, seedNode);
        ProbabilisticTreeCreator.PTC2(random, seedNode, maxLength, maxDepth);

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
      } else {
        SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(NoMutation);
        MutationTypeParameter.ActualValue = new IntValue(NoMutation);
      }
    }
  }
}

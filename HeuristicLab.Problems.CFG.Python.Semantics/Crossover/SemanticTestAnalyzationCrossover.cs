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
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [StorableClass]
  [Item("SemanticTestAnalyzationCrossover", "Base class to test different Semantics.")]
  public abstract class SemanticTestAnalyzationCrossover<T> : AbstractSemanticAnalyzationCrossover<T>
    where T : class, ICFGPythonProblemData {

    [StorableConstructor]
    protected SemanticTestAnalyzationCrossover(bool deserializing) : base(deserializing) { }

    protected SemanticTestAnalyzationCrossover(SemanticTestAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticTestAnalyzationCrossover() { }

    protected override ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, T problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        parent0 = SubtreeCrossover.Cross(random, parent0, parent1, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth);
        newSemantics = null;
        AddStatisticsNoCrossover(NoXoNoSemantics);
        return parent0;
      }
      newSemantics = semantic0;

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(parent0.Root.Grammar);
      var variables = problemData.Variables.GetVariableNames().ToList();
      string variableSettings = problemData.VariableSettings.Count == 0 ? String.Empty : String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));

      int maximumSemanticTries = MaxComparesParameter.Value.Value;
      int semanticTries = 0;

      List<JObject> saveOriginalSemantics = null;
      List<JObject> saveReplaceSemantics = null;
      List<Tuple<CutPoint, ISymbolicExpressionTreeNode>> possibleChildren = null;
      if (UsesAdditionalSemanticMeasure()) {
        saveOriginalSemantics = new List<JObject>(semanticTries);
        saveReplaceSemantics = new List<JObject>(semanticTries);
        possibleChildren = new List<Tuple<CutPoint, ISymbolicExpressionTreeNode>>(semanticTries);
      }
      bool success = false;
      do {
        // select a random crossover point in the first parent 
        CutPoint crossoverPoint0;
        SelectCrossoverPoint(random, parent0, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth, out crossoverPoint0);

        int childLength = crossoverPoint0.Child != null ? crossoverPoint0.Child.GetLength() : 0;
        // calculate the max length and depth that the inserted branch can have 
        int maxInsertedBranchLength = Math.Max(0, maxTreeLength - (parent0.Length - childLength));
        int maxInsertedBranchDepth = Math.Max(0, maxTreeDepth - parent0.Root.GetBranchLevel(crossoverPoint0.Parent));

        List<ISymbolicExpressionTreeNode> allowedBranches = new List<ISymbolicExpressionTreeNode>();
        parent1.Root.ForEachNodePostfix((n) => {
          if (n.GetLength() <= maxInsertedBranchLength &&
              n.GetDepth() <= maxInsertedBranchDepth && crossoverPoint0.IsMatchingPointType(n))
            allowedBranches.Add(n);
        });
        // empty branch
        if (crossoverPoint0.IsMatchingPointType(null)) allowedBranches.Add(null);

        if (allowedBranches.Count != 0) {
          var selectedBranch = SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability);

          ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(crossoverPoint0.Child, statementProductionNames);
          var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
          PythonStatementSemantic curSemantics = null;
          if (String.IsNullOrEmpty(variableSettings)) {
            curSemantics = semantic0.First(x => x.TreeNodePrefixPos == statementPos0);
            variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(curSemantics.Before, problemData.Variables.GetVariableTypes());
          }

          var jsonOriginal = SemanticOperatorHelper.EvaluateStatementNode(statement, PyProcess, random, problemData, variables, variableSettings, Timeout);

          JObject jsonReplaced;
          if (statement == crossoverPoint0.Child) {
            // selectedBranch is also executable
            jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(selectedBranch, PyProcess, random, problemData, variables, variableSettings, Timeout);
          } else {
            crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);
            var parent = selectedBranch.Parent; // save parent
            crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, selectedBranch); // this will affect node.Parent
            jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(statement, PyProcess, random, problemData, variables, variableSettings, Timeout);
            crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex); // removes intermediate parent from node
            selectedBranch.Parent = parent; // restore parent
            crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, crossoverPoint0.Child); // restore cutPoint
          }

          if (curSemantics != null) {
            jsonOriginal = PythonSemanticComparer.ReplaceNotExecutedCases(jsonOriginal, curSemantics.Before, curSemantics.ExecutedCases);
            jsonReplaced = PythonSemanticComparer.ReplaceNotExecutedCases(jsonReplaced, curSemantics.Before, curSemantics.ExecutedCases);

            jsonOriginal = PythonSemanticComparer.ProduceDifference(jsonOriginal, curSemantics.Before);
            jsonReplaced = PythonSemanticComparer.ProduceDifference(jsonReplaced, curSemantics.Before);
          }

          if (SemanticMeasure(jsonOriginal, jsonReplaced)) {
            newSemantics = SemanticSwap(crossoverPoint0, selectedBranch, parent0, parent1, semantic0, semantic1);
            AddStatistics(semantic0, parent0, statement == crossoverPoint0.Child ? selectedBranch : statement, crossoverPoint0, jsonOriginal, selectedBranch, random, problemData, variables, variableSettings); // parent zero has been changed is now considered the child
            success = true;
          } else if (UsesAdditionalSemanticMeasure()) {
            saveOriginalSemantics.Add(jsonOriginal);
            saveReplaceSemantics.Add(jsonReplaced);
            possibleChildren.Add(new Tuple<CutPoint, ISymbolicExpressionTreeNode>(crossoverPoint0, selectedBranch));
          }
        }
        semanticTries++;

        #region try second semantic comparison

        if (!success && semanticTries >= maximumSemanticTries && UsesAdditionalSemanticMeasure()) {
          for (int index = 0; index < saveOriginalSemantics.Count; index++) {
            if (AdditionalSemanticMeasure(saveOriginalSemantics[index], saveReplaceSemantics[index])) {
              var crossover = possibleChildren[index];
              crossoverPoint0 = crossover.Item1;
              newSemantics = SemanticSwap(crossoverPoint0, crossover.Item2, parent0, parent1, semantic0, semantic1);
              var statement = SemanticOperatorHelper.GetStatementNode(crossoverPoint0.Child, statementProductionNames);
              AddStatistics(semantic0, parent0, statement, crossoverPoint0, saveOriginalSemantics[index], crossover.Item2, random, problemData, variables, variableSettings); // parent zero has been changed is now considered the child
              success = true;
              break;
            }
          }
        }

        #endregion
      } while (!success && semanticTries < maximumSemanticTries);

      if (!success) {
        // Last change. If any possible crossover was found, do a crossover with the first one
        if (saveOriginalSemantics.Any()) {
          var crossover = possibleChildren.First();
          var crossoverPoint0 = crossover.Item1;
          newSemantics = SemanticSwap(crossoverPoint0, crossover.Item2, parent0, parent1, semantic0, semantic1);
          var statement = SemanticOperatorHelper.GetStatementNode(crossoverPoint0.Child, statementProductionNames);
          AddStatistics(semantic0, parent0, statement, crossoverPoint0, saveOriginalSemantics.First(), crossover.Item2, random, problemData, variables, variableSettings); // parent zero has been changed is now considered the child
        }
        AddStatisticsNoCrossover(NoXoNoAllowedBranch);
      }

      return parent0;
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

#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2015 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Random;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("SemanticAnalyzationCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class SemanticCrossoverAnalyzationCrossover<T> : AbstractSemanticAnalyzationCrossover<T>
  where T : class, ICFGPythonProblemData {
    [StorableConstructor]
    protected SemanticCrossoverAnalyzationCrossover(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzationCrossover(SemanticCrossoverAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticCrossoverAnalyzationCrossover() : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzationCrossover<T>(this, cloner);
    }

    protected override ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, T problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        parent0 = SubtreeCrossover.Cross(random, parent0, parent1, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth);
        newSemantics = null;
        AddStatisticsNoCrossover(NoXoNoSemantics);
        return parent0;
      }
      newSemantics = semantic0;

      // select a random crossover point in the first parent 
      CutPoint crossoverPoint0;
      SelectCrossoverPoint(random, parent0, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth, out crossoverPoint0);

      int childLength = crossoverPoint0.Child != null ? crossoverPoint0.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = maxTreeLength - (parent0.Length - childLength);
      int maxInsertedBranchDepth = maxTreeDepth - parent0.Root.GetBranchLevel(crossoverPoint0.Child);

      List<ISymbolicExpressionTreeNode> allowedBranches = new List<ISymbolicExpressionTreeNode>();
      parent1.Root.ForEachNodePostfix((n) => {
        if (n.GetLength() <= maxInsertedBranchLength &&
            n.GetDepth() <= maxInsertedBranchDepth && crossoverPoint0.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });
      // empty branch
      if (crossoverPoint0.IsMatchingPointType(null)) allowedBranches.Add(null);

      // set NumberOfAllowedBranches
      NumberOfAllowedBranches = allowedBranches.Count;

      if (allowedBranches.Count == 0) {
        AddStatisticsNoCrossover(NoXoNoAllowedBranch);
        return parent0;
      }

      // select MaxCompares random crossover points
      // Use set to avoid having the same node multiple times
      HashSet<ISymbolicExpressionTreeNode> compBranches;
      if (allowedBranches.Count < MaxComparesParameter.Value.Value) {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>(allowedBranches);
      } else {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>();
        for (int i = 0; i < MaxComparesParameter.Value.Value; i++) {
          var possibleBranch = SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability);
          allowedBranches.Remove(possibleBranch);
          compBranches.Add(possibleBranch);
        }
      }

      // set NumberOfPossibleBranchesSelected
      NumberOfPossibleBranchesSelected = compBranches.Count;

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(crossoverPoint0.Parent.Grammar);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(crossoverPoint0.Child, statementProductionNames);

      if (statement == null) {
        newSemantics = SemanticSwap(crossoverPoint0, compBranches.SampleRandom(random), parent0, parent1, semantic0, semantic1);
        AddStatisticsNoCrossover(NoXoNoStatement);
        return parent0;
      }

      var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
      string variableSettings;
      if (problemData.VariableSettings.Count == 0) {
        variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(semantic0.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
      } else {
        variableSettings = String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));
      }
      var variables = problemData.Variables.GetVariableNames().ToList();

      var json0 = SemanticOperatorHelper.EvaluateStatementNode(statement, PyProcess, random, problemData, variables, variableSettings, Timeout);

      ISymbolicExpressionTreeNode selectedBranch;
      if (!String.IsNullOrWhiteSpace((string)json0["exception"])) {
        CrossoverExceptionsParameter.ActualValue.Add(new StringValue(json0["exception"].ToString()));
        selectedBranch = compBranches.SampleRandom(random);
      } else {
        selectedBranch = SelectBranch(statement, crossoverPoint0, compBranches, random, variables, variableSettings, json0, problemData);
      }

      // perform the actual swap
      if (selectedBranch != null) {
        newSemantics = SemanticSwap(crossoverPoint0, selectedBranch, parent0, parent1, semantic0, semantic1);
        AddStatistics(semantic0, parent0, statement == crossoverPoint0.Child ? selectedBranch : statement, crossoverPoint0, json0, selectedBranch, random, problemData, variables, variableSettings); // parent zero has been changed is now considered the child
      } else {
        AddStatisticsNoCrossover(NoXoNoSelectedBranch);
      }

      return parent0;
    }

    private ISymbolicExpressionTreeNode SelectBranch(ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, IEnumerable<ISymbolicExpressionTreeNode> compBranches, IRandom random, List<string> variables, string variableSettings, JObject jsonParent0, ICFGPythonProblemData problemData) {
      List<JObject> evaluationPerNode = new List<JObject>();
      List<double> similarity = new List<double>();

      crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex); // removes parent from child
      foreach (var node in compBranches) {
        JObject json;
        if (statementNode == crossoverPoint0.Child) {
          json = SemanticOperatorHelper.EvaluateStatementNode(node, PyProcess, random, problemData, variables, variableSettings, Timeout);
        } else {
          var parent = node.Parent; // save parent
          crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, node); // this will affect node.Parent
          json = SemanticOperatorHelper.EvaluateStatementNode(statementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
          crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex); // removes intermediate parent from node
          node.Parent = parent; // restore parent
        }
        evaluationPerNode.Add(json);
        similarity.Add(0);
      }
      crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, crossoverPoint0.Child); // restore crossoverPoint0

      #region remove branches that threw an exception
      List<int> branchesCausedExceptions = new List<int>();
      for (int i = evaluationPerNode.Count - 1; i >= 0; i--) {
        if (evaluationPerNode[i]["exception"] != null) {
          branchesCausedExceptions.Add(i);
          CrossoverExceptionsParameter.ActualValue.Add(new StringValue(evaluationPerNode[i]["exception"].ToString()));
        }
      }
      var branchesWithoutException = compBranches.ToList();
      foreach (int index in branchesCausedExceptions) {
        branchesWithoutException.RemoveAt(index);
        evaluationPerNode.RemoveAt(index);
        similarity.RemoveAt(index);
      }
      #endregion

      Dictionary<VariableType, List<string>> differencesPerType = new Dictionary<VariableType, List<string>>();
      foreach (var entry in problemData.Variables.GetTypesOfVariables()) {
        List<string> differences = new List<string>();
        foreach (var variableName in entry.Value) {
          if (evaluationPerNode.Any(x => !JToken.EqualityComparer.Equals(jsonParent0[variableName], x[variableName]))) {
            differences.Add(variableName);
          }
        }

        if (differences.Count > 0) {
          differencesPerType.Add(entry.Key, differences);
        }
      }

      if (differencesPerType.Count == 0) return compBranches.SampleRandom(random); // no difference found, crossover with any branch

      var typeDifference = differencesPerType.SampleRandom(random);

      //set TypeSelectedForSimilarity
      TypeSelectedForSimilarityParameter.ActualValue = new StringValue(typeDifference.Key.ToString());

      foreach (var variableName in typeDifference.Value) {
        var variableSimilarity = CalculateDifference(jsonParent0[variableName], evaluationPerNode.Select(x => x[variableName]), typeDifference.Key, true);
        similarity = similarity.Zip(variableSimilarity, (x, y) => x + y).ToList();
      }
      similarity = similarity.Select(x => x / typeDifference.Value.Count).ToList(); // normalize between 0 and 1 again (actually not necessary)

      // set NumberOfNoChangeDetected
      NumberOfNoChangeDetected = similarity.Count(x => x.IsAlmost(0.0));

      double best = Double.MaxValue;
      int pos = -1;
      for (int i = 0; i < similarity.Count; i++) {
        if (similarity[i] > 0 && similarity[i] < best) {
          best = similarity[i];
          pos = i;
        }
      }

      // set SemanticallyEquivalentCrossover
      SemanticallyEquivalentCrossoverParameter.ActualValue = new IntValue(pos >= 0 ? 2 : 1);

      return pos >= 0 ? branchesWithoutException.ElementAt(pos) : compBranches.SampleRandom(random);
    }
  }
}

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
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("SimpleSemanticAnalyzationCrossover", "Simple semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class SimpleSemanticAnalyzationCrossover<T> : AbstractSemanticAnalyzationCrossover<T>
  where T : class, ICFGPythonProblemData {

    private const string NumberOfCutPointsFirstParentParameterName = "NumberOfCutPointsFirstParent";
    private const string NumberOfCutPointsSecondParentParameterName = "NumberOfCutPointsSecondParent";

    #region Parameter Properties
    public IValueLookupParameter<IntValue> NumberOfCutPointsFirstParentParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[NumberOfCutPointsFirstParentParameterName]; }
    }
    public IValueLookupParameter<IntValue> NumberOfCutPointsSecondParentParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[NumberOfCutPointsSecondParentParameterName]; }
    }
    #endregion

    #region Properties
    public int NumberOfCutPointsFirst {
      get { return NumberOfCutPointsFirstParentParameter.Value.Value; }
    }
    public int NumberOfCutPointsSecond {
      get { return NumberOfCutPointsFirstParentParameter.Value.Value; }
    }
    #endregion

    [StorableConstructor]
    protected SimpleSemanticAnalyzationCrossover(bool deserializing) : base(deserializing) { }
    protected SimpleSemanticAnalyzationCrossover(SimpleSemanticAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }

    public SimpleSemanticAnalyzationCrossover() : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(NumberOfCutPointsFirstParentParameterName, "", new IntValue(3)));
      Parameters.Add(new ValueLookupParameter<IntValue>(NumberOfCutPointsSecondParentParameterName, "", new IntValue(3)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SimpleSemanticAnalyzationCrossover<T>(this, cloner);
    }

    protected override ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, T problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics) {
      newSemantics = semantic0;
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        AddStatisticsNoCrossover(NoXoNoSemantics);
        return parent0;
      }

      // select andom crossover points in the first parent 
      var crossoverPoints0 = SelectCrossoverPointsOfMatchingType(random, parent0, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth, NumberOfCutPointsFirst);
      var primaryCrossoverPoint = crossoverPoints0.First();

      int childLength = primaryCrossoverPoint.Child != null ? primaryCrossoverPoint.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = maxTreeLength - (parent0.Length - childLength);
      int maxInsertedBranchDepth = maxTreeDepth - parent0.Root.GetBranchLevel(primaryCrossoverPoint.Child);

      List<ISymbolicExpressionTreeNode> allowedBranches = new List<ISymbolicExpressionTreeNode>();
      parent1.Root.ForEachNodePostfix((n) => {
        if (n.GetLength() <= maxInsertedBranchLength &&
            n.GetDepth() <= maxInsertedBranchDepth && primaryCrossoverPoint.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });
      // empty branch
      if (primaryCrossoverPoint.IsMatchingPointType(null)) allowedBranches.Add(null);

      // set NumberOfAllowedBranches
      NumberOfAllowedBranches = allowedBranches.Count;

      if (allowedBranches.Count == 0) {
        AddStatisticsNoCrossover(NoXoNoAllowedBranch);
        return parent0;
      }

      // select MaxCompares random crossover points
      // Use set to avoid having the same node multiple times
      HashSet<ISymbolicExpressionTreeNode> compBranches;
      if (allowedBranches.Count < NumberOfCutPointsSecond) {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>(allowedBranches);
      } else {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>();
        for (int i = 0; i < NumberOfCutPointsSecond; i++) {
          var possibleBranch = SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability);
          allowedBranches.Remove(possibleBranch);
          compBranches.Add(possibleBranch);
        }
      }

      var allowedBranchesPerCutpoint = new List<IEnumerable<ISymbolicExpressionTreeNode>>() { allowedBranches };
      allowedBranchesPerCutpoint.AddRange(crossoverPoints0.Skip(1).Select(x => FindFittingNodes(x, parent0, allowedBranches, maxTreeLength, maxTreeDepth)));

      // set NumberOfPossibleBranchesSelected
      NumberOfPossibleBranchesSelected = compBranches.Count;

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(primaryCrossoverPoint.Parent.Grammar);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(primaryCrossoverPoint.Child, statementProductionNames);

      if (statement == null) {
        newSemantics = SemanticSwap(primaryCrossoverPoint, compBranches.SampleRandom(random), parent0, parent1, semantic0, semantic1);
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

      // only used for analyzation, otherwise could be removed
      var json0 = SemanticOperatorHelper.EvaluateStatementNode(statement, PyProcess, random, problemData, variables, variableSettings, Timeout);

      ISymbolicExpressionTreeNode selectedBranch;
      CutPoint selectedCutPoint;
      if (statement == primaryCrossoverPoint.Child) {
        SelectBranchFromExecutableNodes(crossoverPoints0.ToList(), allowedBranchesPerCutpoint, random, variables, variableSettings, problemData, out selectedBranch, out selectedCutPoint);
      } else {
        //SelectBranchFromNodes1(statement, crossoverPoints0.ToList(), allowedBranchesPerCutpoint, random, variables, variableSettings, problemData, out selectedBranch, out selectedCutPoint);
        SelectBranchFromNodes2(statementProductionNames, crossoverPoints0.ToList(), allowedBranchesPerCutpoint, random, variables, variableSettings, problemData, out selectedBranch, out selectedCutPoint);
      }

      // perform the actual swap
      if (selectedBranch != null) {
        newSemantics = SemanticSwap(selectedCutPoint, selectedBranch, parent0, parent1, semantic0, semantic1);
        AddStatistics(semantic0, parent0, statement, selectedCutPoint, json0, selectedBranch, random, problemData, variables, variableSettings); // parent zero has been changed is now considered the child
      } else {
        AddStatisticsNoCrossover(NoXoNoSelectedBranch);
      }

      return parent0;
    }

    private void SelectBranchFromExecutableNodes(IList<CutPoint> crossoverPoints0, IList<IEnumerable<ISymbolicExpressionTreeNode>> allowedBranchesPerCutpoint, IRandom random, List<string> variables, string variableSettings, ICFGPythonProblemData problemData, out ISymbolicExpressionTreeNode selectedBranch, out CutPoint selectedCutPoint) {
      var jsonOutput = new Dictionary<ISymbolicExpressionTreeNode, JObject>();
      for (int i = 0; i < crossoverPoints0.Count; i++) {
        var cutPoint = crossoverPoints0[i];
        var jsonCur = SemanticOperatorHelper.EvaluateStatementNode(cutPoint.Child, PyProcess, random, problemData, variables, variableSettings, Timeout);
        if (!String.IsNullOrWhiteSpace((string)jsonCur["exception"])) { continue; }
        foreach (var possibleBranch in allowedBranchesPerCutpoint[i]) {
          JObject jsonPossibleBranch;
          if (!jsonOutput.ContainsKey(possibleBranch)) {
            jsonPossibleBranch = SemanticOperatorHelper.EvaluateStatementNode(cutPoint.Child, PyProcess, random, problemData, variables, variableSettings, Timeout);
            jsonOutput.Add(possibleBranch, jsonPossibleBranch);
          } else {
            jsonPossibleBranch = jsonOutput[possibleBranch];
          }

          if (!String.IsNullOrWhiteSpace((string)jsonPossibleBranch["exception"])) { continue; }

          if (!JToken.EqualityComparer.Equals(jsonCur, jsonPossibleBranch)) {
            selectedBranch = possibleBranch;
            selectedCutPoint = cutPoint;
            return;
          }
        }
      }

      // no difference was found with any comparison, select random
      selectedBranch = allowedBranchesPerCutpoint[0].SampleRandom(random);
      selectedCutPoint = crossoverPoints0[0];
    }

    // version 1
    // only use primary cut point
    private void SelectBranchFromNodes1(ISymbolicExpressionTreeNode statementNode, IList<CutPoint> crossoverPoints0, IList<IEnumerable<ISymbolicExpressionTreeNode>> allowedBranchesPerCutpoint, IRandom random, List<string> variables, string variableSettings, ICFGPythonProblemData problemData, out ISymbolicExpressionTreeNode selectedBranch, out CutPoint selectedCutPoint) {
      var jsonOutput = new Dictionary<ISymbolicExpressionTreeNode, JObject>();

      var primaryCutPoint = crossoverPoints0[0];
      primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes parent from child
      for (int i = 0; i < crossoverPoints0.Count; i++) {
        var cutPoint = crossoverPoints0[i];
        primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, cutPoint.Child); // this will affect cutPoint.Parent
        var jsonCur = SemanticOperatorHelper.EvaluateStatementNode(statementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
        primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes intermediate parent from node
        cutPoint.Child.Parent = cutPoint.Parent; // restore parent

        if (!String.IsNullOrWhiteSpace((string)jsonCur["exception"])) { continue; }
        foreach (var possibleBranch in allowedBranchesPerCutpoint[i]) {
          JObject jsonPossibleBranch;
          if (!jsonOutput.ContainsKey(possibleBranch)) {
            var parent = possibleBranch.Parent; // save parent
            primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, possibleBranch); // this will affect node.Parent
            jsonPossibleBranch = SemanticOperatorHelper.EvaluateStatementNode(statementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
            primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes intermediate parent from node
            possibleBranch.Parent = parent; // restore parent
            jsonOutput.Add(possibleBranch, jsonPossibleBranch);
          } else {
            jsonPossibleBranch = jsonOutput[possibleBranch];
          }

          if (!String.IsNullOrWhiteSpace((string)jsonPossibleBranch["exception"])) { continue; }

          if (!JToken.EqualityComparer.Equals(jsonCur, jsonPossibleBranch)) {
            primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, primaryCutPoint.Child); // restore primaryCutPoint
            selectedBranch = possibleBranch;
            selectedCutPoint = cutPoint;
            return;
          }
        }
      }

      primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, primaryCutPoint.Child); // restore primaryCutPoint
      // no difference was found with any comparison, select random
      selectedBranch = allowedBranchesPerCutpoint[0].SampleRandom(random);
      selectedCutPoint = crossoverPoints0[0];
    }

    // version 2
    // check the statement node of every cut point in the parent
    // takes longer, but really checks for differences
    private void SelectBranchFromNodes2(IEnumerable<string> statementProductionNames, IList<CutPoint> crossoverPoints0, IList<IEnumerable<ISymbolicExpressionTreeNode>> allowedBranchesPerCutpoint, IRandom random, List<string> variables, string variableSettings, ICFGPythonProblemData problemData, out ISymbolicExpressionTreeNode selectedBranch, out CutPoint selectedCutPoint) {
      var primaryCutPoint = crossoverPoints0[0];
      primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes parent from child
      for (int i = 0; i < crossoverPoints0.Count; i++) {
        var cutPoint = crossoverPoints0[i];
        var curStatementNode = SemanticOperatorHelper.GetStatementNode(cutPoint.Child, statementProductionNames); ;
        primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, cutPoint.Child); // this will affect cutPoint.Parent
        var jsonCur = SemanticOperatorHelper.EvaluateStatementNode(curStatementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
        primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes intermediate parent from node
        cutPoint.Child.Parent = cutPoint.Parent; // restore parent

        if (!String.IsNullOrWhiteSpace((string)jsonCur["exception"])) { continue; }
        foreach (var possibleBranch in allowedBranchesPerCutpoint[i]) {
          var parent = possibleBranch.Parent; // save parent
          primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, possibleBranch); // this will affect node.Parent
          JObject jsonPossibleBranch = SemanticOperatorHelper.EvaluateStatementNode(curStatementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
          primaryCutPoint.Parent.RemoveSubtree(primaryCutPoint.ChildIndex); // removes intermediate parent from node
          possibleBranch.Parent = parent; // restore parent

          if (!String.IsNullOrWhiteSpace((string)jsonPossibleBranch["exception"])) { continue; }

          if (!JToken.EqualityComparer.Equals(jsonCur, jsonPossibleBranch)) {
            primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, primaryCutPoint.Child); // restore primaryCutPoint
            selectedBranch = possibleBranch;
            selectedCutPoint = cutPoint;
            return;
          }
        }
      }

      primaryCutPoint.Parent.InsertSubtree(primaryCutPoint.ChildIndex, primaryCutPoint.Child); // restore primaryCutPoint
      // no difference was found with any comparison, select random
      selectedBranch = allowedBranchesPerCutpoint[0].SampleRandom(random);
      selectedCutPoint = crossoverPoints0[0];
    }

    private IEnumerable<ISymbolicExpressionTreeNode> FindFittingNodes(CutPoint cutPoint, ISymbolicExpressionTree parent0, List<ISymbolicExpressionTreeNode> allowedBranches, int maxTreeLength, int maxTreeDepth) {
      int childLength = cutPoint.Child != null ? cutPoint.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = maxTreeLength - (parent0.Length - childLength);
      int maxInsertedBranchDepth = maxTreeDepth - parent0.Root.GetBranchLevel(cutPoint.Child);

      return allowedBranches.Where(n => n.GetLength() <= maxInsertedBranchLength && n.GetDepth() <= maxInsertedBranchDepth);
    }

    protected static IEnumerable<CutPoint> SelectCrossoverPointsOfMatchingType(IRandom random, ISymbolicExpressionTree parent0, double internalNodeProbability, int maxBranchLength, int maxBranchDepth, int count) {
      if (internalNodeProbability < 0.0 || internalNodeProbability > 1.0) throw new ArgumentException("internalNodeProbability");
      List<CutPoint> internalCrossoverPoints = new List<CutPoint>();
      List<CutPoint> leafCrossoverPoints = new List<CutPoint>();
      parent0.Root.ForEachNodePostfix((n) => {
        if (n.SubtreeCount > 0 && n != parent0.Root) {
          //avoid linq to reduce memory pressure
          for (int i = 0; i < n.SubtreeCount; i++) {
            var child = n.GetSubtree(i);
            if (child.GetLength() <= maxBranchLength &&
                child.GetDepth() <= maxBranchDepth) {
              if (child.SubtreeCount > 0)
                internalCrossoverPoints.Add(new CutPoint(n, child));
              else
                leafCrossoverPoints.Add(new CutPoint(n, child));
            }
          }

          // add one additional extension point if the number of sub trees for the symbol is not full
          if (n.SubtreeCount < n.Grammar.GetMaximumSubtreeCount(n.Symbol)) {
            // empty extension point
            internalCrossoverPoints.Add(new CutPoint(n, n.SubtreeCount));
          }
        }
      });

      if (random.NextDouble() < internalNodeProbability) {
        // select from internal node if possible
        if (internalCrossoverPoints.Count > 0) {
          // select internal crossover point or leaf
          return GetCutPointsOfMatchingType(random, internalCrossoverPoints, count);
        } else {
          // otherwise select external node
          return GetCutPointsOfMatchingType(random, leafCrossoverPoints, count);
        }
      } else if (leafCrossoverPoints.Count > 0) {
        // select from leaf crossover point if possible                                
        return GetCutPointsOfMatchingType(random, leafCrossoverPoints, count);
      } else {
        // otherwise select internal crossover point                                            
        return GetCutPointsOfMatchingType(random, internalCrossoverPoints, count);
      }
    }

    private static IEnumerable<CutPoint> GetCutPointsOfMatchingType(IRandom random, IEnumerable<CutPoint> crossoverPoints, int count) {
      CutPoint cutPoint = crossoverPoints.SampleRandom(random);
      var cutPoints = new List<CutPoint>() { cutPoint };
      cutPoints.AddRange(crossoverPoints.Where(x => x != cutPoint && cutPoint.IsMatchingPointType(x.Child)).SampleRandomWithoutRepetition(random, count));
      return cutPoints;
    }
  }
}

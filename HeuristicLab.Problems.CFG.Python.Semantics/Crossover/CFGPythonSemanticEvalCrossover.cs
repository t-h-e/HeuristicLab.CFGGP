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
  [Item("PythonSemanticEvaluationCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class CFGPythonSemanticEvalCrossover<T> : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator,
                                                ICFGPythonSemanticsCrossover<T>
  where T : class, ICFGPythonProblemData {
    private const string InternalCrossoverPointProbabilityParameterName = "InternalCrossoverPointProbability";
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";
    private const string ProblemDataParameterName = "ProblemData";
    private const string SemanticsParameterName = "Semantic";
    private const string MaxComparesParameterName = "MaxCompares";
    private const string TimeoutParameterName = "Timeout";
    private const string PythonProcessParameterName = "PythonProcess";
    private const string NewSemanticParameterName = "NewSemantic";

    #region Parameter Properties
    public IValueLookupParameter<PercentValue> InternalCrossoverPointProbabilityParameter {
      get { return (IValueLookupParameter<PercentValue>)Parameters[InternalCrossoverPointProbabilityParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public IValueLookupParameter<PercentValue> CrossoverProbabilityParameter {
      get { return (IValueLookupParameter<PercentValue>)Parameters[CrossoverProbabilityParameterName]; }
    }
    public ILookupParameter<ItemArray<ItemArray<PythonStatementSemantic>>> SemanticsParameter {
      get { return (ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>)Parameters[SemanticsParameterName]; }
    }
    public ILookupParameter<T> ProblemDataParameter {
      get { return (ILookupParameter<T>)Parameters[ProblemDataParameterName]; }
    }
    public IValueParameter<IntValue> MaxComparesParameter {
      get { return (IValueParameter<IntValue>)Parameters[MaxComparesParameterName]; }
    }
    public ILookupParameter<IntValue> TimeoutParameter {
      get { return (ILookupParameter<IntValue>)Parameters[TimeoutParameterName]; }
    }
    public ILookupParameter<PythonProcess> PythonProcessParameter {
      get { return (ILookupParameter<PythonProcess>)Parameters[PythonProcessParameterName]; }
    }
    public ILookupParameter<ItemArray<PythonStatementSemantic>> NewSemanticParameter {
      get { return (ILookupParameter<ItemArray<PythonStatementSemantic>>)Parameters[NewSemanticParameterName]; }
    }
    #endregion

    #region Properties
    public PercentValue InternalCrossoverPointProbability {
      get { return InternalCrossoverPointProbabilityParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeLength {
      get { return MaximumSymbolicExpressionTreeLengthParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeDepth {
      get { return MaximumSymbolicExpressionTreeDepthParameter.ActualValue; }
    }
    public PercentValue CrossoverProbability {
      get { return CrossoverProbabilityParameter.ActualValue; }
    }
    public T ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    public ItemArray<ItemArray<PythonStatementSemantic>> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    public double Timeout { get { return TimeoutParameter.ActualValue.Value / 1000.0; } }
    public PythonProcess PyProcess { get { return PythonProcessParameter.ActualValue; } }
    #endregion
    [StorableConstructor]
    protected CFGPythonSemanticEvalCrossover(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticEvalCrossover(CFGPythonSemanticEvalCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonSemanticEvalCrossover()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));
      Parameters.Add(new ValueLookupParameter<PercentValue>(InternalCrossoverPointProbabilityParameterName, "The probability to select an internal crossover point (instead of a leaf node).", new PercentValue(0.9)));
      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>(SemanticsParameterName, ""));
      Parameters.Add(new LookupParameter<T>(ProblemDataParameterName, "Problem data"));
      Parameters.Add(new ValueParameter<IntValue>(MaxComparesParameterName, "Maximum number of branches that ae going to be compared for crossover.", new IntValue(10)));
      Parameters.Add(new LookupParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped. (In milliseconds)"));
      Parameters.Add(new LookupParameter<PythonProcess>(PythonProcessParameterName, "Python process"));
      Parameters.Add(new LookupParameter<ItemArray<PythonStatementSemantic>>(NewSemanticParameterName, ""));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticEvalCrossover<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value) {
        ItemArray<PythonStatementSemantic> newSemantics;
        var child = Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value, out newSemantics);
        NewSemanticParameter.ActualValue = newSemantics;
        return child;
      }

      NewSemanticParameter.ActualValue = Semantics[0];
      return parent0;
    }

    protected virtual ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics) {
      newSemantics = semantic0;
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        return parent0;
      }

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

      if (allowedBranches.Count == 0) {
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

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(crossoverPoint0.Parent.Grammar);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(crossoverPoint0.Child, statementProductionNames);

      if (statement == null) {
        newSemantics = SemanticSwap(crossoverPoint0, compBranches.SampleRandom(random), parent0, parent1, semantic0, semantic1);
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
        selectedBranch = compBranches.SampleRandom(random);
      } else {
        selectedBranch = SelectBranch(statement, crossoverPoint0, compBranches, random, variables, variableSettings, json0, problemData);
      }

      // perform the actual swap
      newSemantics = SemanticSwap(crossoverPoint0, selectedBranch, parent0, parent1, semantic0, semantic1);

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
      foreach (var variableName in typeDifference.Value) {
        var variableSimilarity = CalculateDifference(jsonParent0[variableName], evaluationPerNode.Select(x => x[variableName]), typeDifference.Key, true);
        similarity = similarity.Zip(variableSimilarity, (x, y) => x + y).ToList();
      }
      similarity = similarity.Select(x => x / typeDifference.Value.Count).ToList(); // normalize between 0 and 1 again (actually not necessary)

      double best = Double.MaxValue;
      int pos = -1;
      for (int i = 0; i < similarity.Count; i++) {
        if (similarity[i] > 0 && similarity[i] < best) {
          best = similarity[i];
          pos = i;
        }
      }
      return pos >= 0 ? branchesWithoutException.ElementAt(pos) : compBranches.SampleRandom(random);
    }

    protected IEnumerable<double> CalculateDifference(JToken curDiff0, IEnumerable<JToken> curDiffOthers, VariableType variableType, bool normalize) {
      switch (variableType) {
        case VariableType.Bool:
          return PythonSemanticComparer.Compare(curDiff0.Values<bool>(), curDiffOthers.Select(x => x.Values<bool>()), normalize);
        case VariableType.Int:
        case VariableType.Float:
          return PythonSemanticComparer.Compare(ConvertIntJsonToDouble(curDiff0), curDiffOthers.Select(x => ConvertIntJsonToDouble(x)), normalize);
        case VariableType.String:
          return PythonSemanticComparer.Compare(curDiff0.Values<string>(), curDiffOthers.Select(x => x.Values<string>()), normalize);
        case VariableType.List_Bool:
          return PythonSemanticComparer.Compare(curDiff0.Select(x => x.Values<bool>().ToList()), curDiffOthers.Select(x => x.Select(y => y.Values<bool>().ToList())), normalize);
        case VariableType.List_Int:
          return PythonSemanticComparer.Compare(curDiff0.Select(x => x.Values<int>().ToList()), curDiffOthers.Select(x => x.Select(y => y.Values<int>().ToList())), normalize);
        case VariableType.List_Float:
          return PythonSemanticComparer.Compare(curDiff0.Select(x => x.Values<double>().ToList()), curDiffOthers.Select(x => x.Select(y => y.Values<double>().ToList())), normalize);
        case VariableType.List_String:
          return PythonSemanticComparer.Compare(curDiff0.Select(x => x.Values<string>().ToList()), curDiffOthers.Select(x => x.Select(y => y.Values<string>().ToList())), normalize);
      }
      throw new ArgumentException("Variable Type cannot be compared.");
    }

    /// <summary>
    /// This method is required to convert Integer values from JSON to double, because of problems with BigInteger conversion to double
    /// </summary>
    /// <param name="curDiff0"></param>
    /// <returns></returns>
    private IEnumerable<double> ConvertIntJsonToDouble(JToken curDiff0) {
      var converted = new List<double>();
      foreach (var child in curDiff0.Children()) {
        converted.Add((double)child);
      }
      return converted;
    }

    // copied from SubtreeCrossover
    protected static void SelectCrossoverPoint(IRandom random, ISymbolicExpressionTree parent0, double internalNodeProbability, int maxBranchLength, int maxBranchDepth, out CutPoint crossoverPoint) {
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
          crossoverPoint = internalCrossoverPoints[random.Next(internalCrossoverPoints.Count)];
        } else {
          // otherwise select external node
          crossoverPoint = leafCrossoverPoints[random.Next(leafCrossoverPoints.Count)];
        }
      } else if (leafCrossoverPoints.Count > 0) {
        // select from leaf crossover point if possible
        crossoverPoint = leafCrossoverPoints[random.Next(leafCrossoverPoints.Count)];
      } else {
        // otherwise select internal crossover point
        crossoverPoint = internalCrossoverPoints[random.Next(internalCrossoverPoints.Count)];
      }
    }

    //copied from SubtreeCrossover
    protected static ISymbolicExpressionTreeNode SelectRandomBranch(IRandom random, IEnumerable<ISymbolicExpressionTreeNode> branches, double internalNodeProbability) {
      if (internalNodeProbability < 0.0 || internalNodeProbability > 1.0) throw new ArgumentException("internalNodeProbability");
      List<ISymbolicExpressionTreeNode> allowedInternalBranches;
      List<ISymbolicExpressionTreeNode> allowedLeafBranches;
      if (random.NextDouble() < internalNodeProbability) {
        // select internal node if possible
        allowedInternalBranches = (from branch in branches
                                   where branch != null && branch.SubtreeCount > 0
                                   select branch).ToList();
        if (allowedInternalBranches.Count > 0) {
          return allowedInternalBranches.SampleRandom(random);

        } else {
          // no internal nodes allowed => select leaf nodes
          allowedLeafBranches = (from branch in branches
                                 where branch == null || branch.SubtreeCount == 0
                                 select branch).ToList();
          return allowedLeafBranches.SampleRandom(random);
        }
      } else {
        // select leaf node if possible
        allowedLeafBranches = (from branch in branches
                               where branch == null || branch.SubtreeCount == 0
                               select branch).ToList();
        if (allowedLeafBranches.Count > 0) {
          return allowedLeafBranches.SampleRandom(random);
        } else {
          allowedInternalBranches = (from branch in branches
                                     where branch != null && branch.SubtreeCount > 0
                                     select branch).ToList();
          return allowedInternalBranches.SampleRandom(random);

        }
      }
    }

    /// <summary>
    /// Swaps the child node of the cutpoint with the selected branch. Semantics of the child branch will be removed from semantics0. Semantics from semantics1 which belonged to the selected branch will be added to semantics0.
    /// </summary>
    /// <param name="crossoverPoint">Defines parent and child node from the parent0</param>
    /// <param name="selectedBranch">Branch to crossover from parent1</param>
    /// <param name="parent0">Parent0</param>
    /// <param name="parent1">Parent1</param>
    /// <param name="semantics0">Semantics of parent0</param>
    /// <param name="semantics1">Semantics of parent1</param>
    protected static ItemArray<PythonStatementSemantic> SemanticSwap(CutPoint crossoverPoint, ISymbolicExpressionTreeNode selectedBranch, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantics0, ItemArray<PythonStatementSemantic> semantics1) {
      var allNodes0Prefix = parent0.IterateNodesPrefix().ToList();
      Dictionary<ISymbolicExpressionTreeNode, PythonStatementSemantic> sem0ByNode = new Dictionary<ISymbolicExpressionTreeNode, PythonStatementSemantic>();
      var sem0ByNodeIndex = semantics0.ToDictionary(x => x.TreeNodePrefixPos, y => y);
      for (int i = 0; i < allNodes0Prefix.Count; i++) {
        if (sem0ByNodeIndex.ContainsKey(i)) {
          sem0ByNode.Add(allNodes0Prefix[i], sem0ByNodeIndex[i]);
        }
      }

      var allNodes1Prefix = parent1.IterateNodesPrefix().ToList();
      Dictionary<ISymbolicExpressionTreeNode, PythonStatementSemantic> sem1ByNode = new Dictionary<ISymbolicExpressionTreeNode, PythonStatementSemantic>();
      var sem1ByNodeIndex = semantics1.ToDictionary(x => x.TreeNodePrefixPos, y => y);
      for (int i = 0; i < allNodes1Prefix.Count; i++) {
        if (sem1ByNodeIndex.ContainsKey(i)) {
          sem1ByNode.Add(allNodes1Prefix[i], sem1ByNodeIndex[i]);
        }
      }

      if (crossoverPoint.Child != null) {
        // manipulate the tree of parent0 in place
        // replace the branch in tree0 with the selected branch from tree1
        crossoverPoint.Parent.RemoveSubtree(crossoverPoint.ChildIndex);
        if (selectedBranch != null) {
          crossoverPoint.Parent.InsertSubtree(crossoverPoint.ChildIndex, selectedBranch);
        }
      } else {
        // child is null (additional child should be added under the parent)
        if (selectedBranch != null) {
          crossoverPoint.Parent.AddSubtree(selectedBranch);
        }
      }

      List<PythonStatementSemantic> newSemantics = new List<PythonStatementSemantic>();
      var newNodes0Prefix = parent0.IterateNodesPrefix().ToList();
      for (int i = 0; i < newNodes0Prefix.Count; i++) {
        PythonStatementSemantic sem = null;
        if (sem0ByNode.ContainsKey(newNodes0Prefix[i])) {
          sem = sem0ByNode[newNodes0Prefix[i]];
          sem.TreeNodePrefixPos = i;
        } else if (sem1ByNode.ContainsKey(newNodes0Prefix[i])) {
          sem = sem1ByNode[newNodes0Prefix[i]];
          sem.TreeNodePrefixPos = i;
        }
        if (sem != null) {
          newSemantics.Add(sem);
        }
      }
      return new ItemArray<PythonStatementSemantic>(newSemantics);
    }
  }
}

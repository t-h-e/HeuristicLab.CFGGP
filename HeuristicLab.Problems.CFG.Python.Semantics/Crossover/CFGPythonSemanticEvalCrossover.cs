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
using System.Text.RegularExpressions;
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
  public class CFGPythonSemanticEvalCrossover<T> : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator,
                                                ICFGPythonSemanticsCrossover<T>
  where T : class, ICFGPythonProblemData {
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";
    private const string ProblemDataParameterName = "ProblemData";

    private const string SymbolicExpressionTreeGrammarParameterName = "SymbolicExpressionTreeGrammar";
    private const string SemanticsParameterName = "Semantic";

    private const string MaxComparesParameterName = "MaxCompares";

    /// <summary>
    /// 0 = variable names
    /// 1 = variable settings
    /// 2 = variable settings names
    /// 3 = code
    /// </summary>
    private const string EVAL_TRACE_SCRIPT = @"variables = [{0}]
{1}

trace = {{}}
for v in variables:
    trace[v] = []

for {2} in zip({3}):
{4}
    for v in variables:
        trace[v].append(locals()[v])

for v in variables:
    locals()[v] = trace[v]";

    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public IValueLookupParameter<PercentValue> CrossoverProbabilityParameter {
      get { return (IValueLookupParameter<PercentValue>)Parameters[CrossoverProbabilityParameterName]; }
    }
    public IValueLookupParameter<ISymbolicExpressionGrammar> SymbolicExpressionTreeGrammarParameter {
      get { return (IValueLookupParameter<ISymbolicExpressionGrammar>)Parameters[SymbolicExpressionTreeGrammarParameterName]; }
    }
    public ILookupParameter<ItemArray<ItemArray<PythonStatementSemantic>>> SemanticsParameter {
      get { return (ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>)Parameters[SemanticsParameterName]; }
    }
    public ILookupParameter<T> ProblemDataParameter {
      get { return (ILookupParameter<T>)Parameters[ProblemDataParameterName]; }
    }
    #endregion
    #region Properties
    public IntValue MaximumSymbolicExpressionTreeLength {
      get { return MaximumSymbolicExpressionTreeLengthParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeDepth {
      get { return MaximumSymbolicExpressionTreeDepthParameter.ActualValue; }
    }
    public PercentValue CrossoverProbability {
      get { return CrossoverProbabilityParameter.ActualValue; }
    }

    public IValueParameter<IntValue> MaxComparesParameter {
      get { return (IValueParameter<IntValue>)Parameters["MaxCompares"]; }
    }

    public ICFGPythonProblemData ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    private ItemArray<ItemArray<PythonStatementSemantic>> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    #endregion
    [StorableConstructor]
    protected CFGPythonSemanticEvalCrossover(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticEvalCrossover(CFGPythonSemanticEvalCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonSemanticEvalCrossover()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));

      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));

      Parameters.Add(new ValueLookupParameter<ISymbolicExpressionGrammar>(SymbolicExpressionTreeGrammarParameterName, "Tree grammar"));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>(SemanticsParameterName, ""));
      Parameters.Add(new LookupParameter<T>(ProblemDataParameterName, "Problem data"));

      Parameters.Add(new ValueParameter<IntValue>(MaxComparesParameterName, "Maximum number of branches that ae going to be compared for crossover.", new IntValue(10)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticEvalCrossover<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value);

      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) return parent0;

      var p0NodeIndices = semantic0.Select(x => x.TreeNodePrefixPos).ToList();
      var crossoverPoints0 = parent0.IterateNodesPrefix().ToList().Where((value, index) => p0NodeIndices.Contains(index)).Select(x => new CutPoint(x.Parent, x));
      var p1NodeIndices = semantic1.Select(x => x.TreeNodePrefixPos).ToList();
      var p1StatementNodes = parent1.IterateNodesPrefix().ToList().Where((value, index) => p1NodeIndices.Contains(index)).ToList();

      var crossoverPoint0 = crossoverPoints0.SampleRandom(random);
      int level = parent0.Root.GetBranchLevel(crossoverPoint0.Child);
      int length = parent0.Root.GetLength() - crossoverPoint0.Child.GetLength();

      var allowedBranches = new List<ISymbolicExpressionTreeNode>();
      p1StatementNodes.ForEach((n) => {
        if (n.GetDepth() + level <= maxTreeDepth && n.GetLength() + length <= maxTreeLength && crossoverPoint0.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });

      if (allowedBranches.Count == 0)
        return parent0;

      // add variable setting to problem data
      var variableSettings = String.Join("\n", problemData.VariableSettings.Select(x => x.Value));
      var variables = problemData.Variables.GetVariableNames().ToList();

      // create symbols in order to improvize an ad-hoc tree so that the child can be evaluated
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      //-------------------------------------TEMP START
      JObject jsonOriginal = PythonProcess.GetInstance().SendAndEvaluateProgram(new EvaluationScript() {
        Script = FormatScript(new SymbolicExpressionTree(new SymbolicExpressionTreeTopLevelNode(rootSymbol)), variables, variableSettings),
        Variables = variables
      });
      //-------------------------------------TEMP END


      EvaluationScript crossoverPointScript0 = new EvaluationScript() {
        Script = FormatScript(CreateTreeFromNode(random, crossoverPoint0.Child, rootSymbol, startSymbol), variables, variableSettings),
        Variables = variables
      };
      JObject json0 = PythonProcess.GetInstance().SendAndEvaluateProgram(crossoverPointScript0);
      crossoverPoint0.Child.Parent = crossoverPoint0.Parent; // restore parent
      //ISymbolicExpressionTreeNode selectedBranch = null;

      var compBranches = allowedBranches.SampleRandomWithoutRepetition(random, MaxComparesParameter.Value.Value); // TODO: use parameter
      ISymbolicExpressionTreeNode selectedBranch = SelectBranch(compBranches, random, variables, variableSettings, json0, jsonOriginal, problemData.Variables.GetTypesOfVariables());


      //// pick the first node that fulfills the semantic similarity conditions
      //foreach (var node in allowedBranches) {
      //  var parent = node.Parent;
      //  var tree1 = CreateTreeFromNode(random, node, rootSymbol, startSymbol); // this will affect node.Parent 
      //  EvaluationScript evaluationScript1 = new EvaluationScript() {
      //    Script = FormatScript(tree1, variables, variableSettings),
      //    Variables = variables
      //  };
      //  JObject json1 = PythonProcess.GetInstance().SendAndEvaluateProgram(evaluationScript1);
      //  node.Parent = parent; // restore parent

      //  if (DoSimilarityCalculations(json0, json1, variables, problemData.Variables.GetTypesOfVariables(), problemData.Variables.GetVariableTypes(), jsonOriginal) >= 0) {
      //    selectedBranch = node;
      //    break;
      //  }
      //}

      // perform the actual swap
      if (selectedBranch != null)
        Swap(crossoverPoint0, selectedBranch);
      return parent0;
    }

    private ISymbolicExpressionTreeNode SelectBranch(IEnumerable<ISymbolicExpressionTreeNode> compBranches, IRandom random, List<string> variables, string variableSettings, JObject jsonParent0, JObject jsonOriginal, IDictionary<VariableType, List<string>> variablesPerType) {
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      Dictionary<ISymbolicExpressionTreeNode, JObject> evaluationPerNode = new Dictionary<ISymbolicExpressionTreeNode, JObject>();
      foreach (var node in compBranches) {
        var parent = node.Parent;
        var tree1 = CreateTreeFromNode(random, node, rootSymbol, startSymbol); // this will affect node.Parent 
        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = FormatScript(tree1, variables, variableSettings),
          Variables = variables
        };
        JObject json = PythonProcess.GetInstance().SendAndEvaluateProgram(evaluationScript1);
        node.Parent = parent; // restore parent
        evaluationPerNode.Add(node, json);
      }

      Dictionary<VariableType, List<string>> differencesPerType = new Dictionary<VariableType, List<string>>();
      List<string> differences;
      foreach (var entry in variablesPerType) {
        differences = new List<string>();
        foreach (var variableName in entry.Value) {
          if (evaluationPerNode.Any(pair => !JToken.EqualityComparer.Equals(jsonOriginal[variableName], pair.Value[variableName]))
          || !JToken.EqualityComparer.Equals(jsonOriginal[variableName], jsonParent0[variableName])) {
            differences.Add(variableName);
          }
        }

        if (differences.Count > 0) {
          differencesPerType.Add(entry.Key, differences);
        }
      }

      if (differencesPerType.Count == 0) return compBranches.SampleRandom(random); // no difference found, crossover with any branch

      var typeDifference = differencesPerType.SampleRandom(random);
      double best = Double.MaxValue;
      ISymbolicExpressionTreeNode selectedNode = null;
      foreach (var item in evaluationPerNode) {
        double cur = 0;
        foreach (var variableName in typeDifference.Value) {
          cur += CalculateDifference(jsonParent0[variableName], item.Value[variableName], typeDifference.Key);
        }
        if (cur > 0 && cur < best) {
          best = cur;
          selectedNode = item.Key;
        }
      }

      return selectedNode;
    }

    private string FormatScript(ISymbolicExpressionTree symbolicExpressionTree, List<string> variables, string variableSettings) {
      Regex r = new Regex(@"^(.*?)\s*=", RegexOptions.Multiline);
      string variableSettingsSubstitute = r.Replace(variableSettings, "${1}_setting =");
      return String.Format(EVAL_TRACE_SCRIPT, String.Format("'{0}'", String.Join("','", variables)),
                                              variableSettingsSubstitute,
                                              String.Join(",", variables),
                                              String.Join(",", variables.Select(x => x + "_setting")),
                                              PythonHelper.FormatToProgram(symbolicExpressionTree, "    "));
    }

    //private double DoSimilarityCalculations(JObject json0, JObject json1, IEnumerable<string> variableNames, IDictionary<VariableType, List<string>> variablesPerType, IDictionary<string, VariableType> typeOfVariable, JObject jsonOriginal) {
    //  List<string> differences = new List<string>();

    //  double distance = 0;
    //  foreach (var entry in variablesPerType) {
    //    foreach (var variableName in entry.Value) {
    //      if (!JToken.EqualityComparer.Equals(jsonOriginal[variableName], json0[variableName])
    //       || !JToken.EqualityComparer.Equals(jsonOriginal[variableName], json1[variableName])) {
    //        differences.Add(variableName);
    //      }
    //    }

    //    if (differences.Count == 0) continue;

    //    double[,] cost = new double[differences.Count, differences.Count];
    //    for (int i = 0; i < differences.Count; i++) {
    //      var curDiff0 = json0[differences[i]];
    //      for (int j = 0; j < differences.Count; j++) {
    //        var curDiff1 = json1[differences[j]];
    //        cost[i, j] = CalculateDifference(curDiff0, curDiff1, entry.Key);
    //        if (Double.IsNaN(cost[i, j]) || Double.IsInfinity(cost[i, j])) {
    //          cost[i, j] = Double.MaxValue;
    //        }
    //      }
    //    }
    //    HungarianAlgorithm ha = new HungarianAlgorithm(cost);
    //    var assignment = ha.Run();
    //    for (int i = 0; i < assignment.Length; i++) {
    //      distance += cost[i, assignment[i]];
    //    }
    //    differences.Clear();
    //  }
    //  return distance;
    //}

    private double CalculateDifference(JToken curDiff0, JToken curDiff1, VariableType variableType) {
      switch (variableType) {
        case VariableType.Bool:
          return PythonSemanticComparer.Compare(curDiff0.Values<bool>(), curDiff1.Values<bool>());
        case VariableType.Int:
          return PythonSemanticComparer.Compare(curDiff0.Values<long>(), curDiff1.Values<long>());
        case VariableType.Float:
          return PythonSemanticComparer.Compare(curDiff0.Values<double>(), curDiff1.Values<double>());
        case VariableType.String:
          return PythonSemanticComparer.Compare(curDiff0.Values<string>(), curDiff1.Values<string>());
        case VariableType.List_Bool:
          return PythonSemanticComparer.Compare(curDiff0.Values<List<bool>>(), curDiff1.Values<List<bool>>());
        case VariableType.List_Int:
          return PythonSemanticComparer.Compare(curDiff0.Values<List<int>>(), curDiff1.Values<List<int>>());
        case VariableType.List_Float:
          return PythonSemanticComparer.Compare(curDiff0.Values<List<double>>(), curDiff1.Values<List<double>>());
        case VariableType.List_String:
          return PythonSemanticComparer.Compare(curDiff0.Values<List<string>>(), curDiff1.Values<List<string>>());
      }
      throw new ArgumentException("Variable Type cannot be compared.");
    }

    //copied from SymbolicDataAnalysisExpressionCrossover<T>
    protected static void Swap(CutPoint crossoverPoint, ISymbolicExpressionTreeNode selectedBranch) {
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
    }
    //copied from SymbolicDataAnalysisExpressionCrossover<T>
    protected static ISymbolicExpressionTree CreateTreeFromNode(IRandom random, ISymbolicExpressionTreeNode node, ISymbol rootSymbol, ISymbol startSymbol) {
      var rootNode = new SymbolicExpressionTreeTopLevelNode(rootSymbol);
      if (rootNode.HasLocalParameters) rootNode.ResetLocalParameters(random);

      var startNode = new SymbolicExpressionTreeTopLevelNode(startSymbol);
      if (startNode.HasLocalParameters) startNode.ResetLocalParameters(random);

      startNode.AddSubtree(node);
      rootNode.AddSubtree(startNode);

      return new SymbolicExpressionTree(rootNode);
    }
  }
}

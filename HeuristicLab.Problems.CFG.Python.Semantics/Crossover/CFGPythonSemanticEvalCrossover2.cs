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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("PythonSemanticEvaluationCrossover2", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class CFGPythonSemanticEvalCrossover2<T> : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator,
                                                ICFGPythonSemanticsCrossover<T>
  where T : class, ICFGPythonProblemData {
    private const string InternalCrossoverPointProbabilityParameterName = "InternalCrossoverPointProbability";
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";
    private const string ProblemDataParameterName = "ProblemData";

    private const string SymbolicExpressionTreeGrammarParameterName = "SymbolicExpressionTreeGrammar";
    private const string SemanticsParameterName = "Semantic";

    private const string MaxComparesParameterName = "MaxCompares";
    private const string TimeoutParameterName = "Timeout";

    /// <summary>
    /// 0 = variable names
    /// 1 = variable settings
    /// 2 = variable settings names
    /// 3 = code
    /// </summary>
    private const string EVAL_TRACE_SCRIPT = @"{0}
variables = [{1}]
{2}

trace = {{}}
for v in variables:
    trace[v] = []

for {3} in zip({4}):
{5}
    for v in variables:
        trace[v].append(locals()[v])

for v in variables:
    locals()[v] = trace[v]";

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
    public IValueLookupParameter<ISymbolicExpressionGrammar> SymbolicExpressionTreeGrammarParameter {
      get { return (IValueLookupParameter<ISymbolicExpressionGrammar>)Parameters[SymbolicExpressionTreeGrammarParameterName]; }
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
      get { return (ILookupParameter<PythonProcess>)Parameters["PythonProcess"]; }
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
    public ICFGPythonProblemData ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    public ItemArray<ItemArray<PythonStatementSemantic>> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    public double Timeout { get { return TimeoutParameter.ActualValue.Value / 1000.0; } }
    public PythonProcess PyProcess { get { return PythonProcessParameter.ActualValue; } }
    #endregion
    [StorableConstructor]
    protected CFGPythonSemanticEvalCrossover2(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticEvalCrossover2(CFGPythonSemanticEvalCrossover2<T> original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonSemanticEvalCrossover2()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));
      Parameters.Add(new ValueLookupParameter<PercentValue>(InternalCrossoverPointProbabilityParameterName, "The probability to select an internal crossover point (instead of a leaf node).", new PercentValue(0.9)));

      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));

      Parameters.Add(new ValueLookupParameter<ISymbolicExpressionGrammar>(SymbolicExpressionTreeGrammarParameterName, "Tree grammar"));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>(SemanticsParameterName, ""));
      Parameters.Add(new LookupParameter<T>(ProblemDataParameterName, "Problem data"));

      Parameters.Add(new ValueParameter<IntValue>(MaxComparesParameterName, "Maximum number of branches that ae going to be compared for crossover.", new IntValue(10)));
      Parameters.Add(new LookupParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped. (In milliseconds)"));

      Parameters.Add(new LookupParameter<PythonProcess>("PythonProcess", "Python process"));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticEvalCrossover2<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value);

      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability) {
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
      } else {
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

        // get possible semantic positions
        var statementProductions = ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <code>")).Symbols;
        var statementProductionNames = statementProductions.Select(x => x.Name);

        // find first node that can be used for evaluation in parent0
        ISymbolicExpressionTreeNode statement = statementProductionNames.Contains(crossoverPoint0.Child.Symbol.Name) ? crossoverPoint0.Child : crossoverPoint0.Parent;
        while (statement != null && !statementProductionNames.Contains(statement.Symbol.Name)) {
          statement = statement.Parent;
        }

        if (statement == null) {
          Swap(crossoverPoint0, compBranches.SampleRandom(random));
          return parent0;
        }

        var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
        string variableSettings;
        if (problemData.VariableSettings.Count == 0) {
          variableSettings = SemanticToPythonVariableSettings(semantic0.First(x => x.TreeNodePrefixPos == statementPos0).Before);
        } else {
          variableSettings = String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));
        }
        var variables = problemData.Variables.GetVariableNames().ToList();

        // create symbols in order to improvize an ad-hoc tree so that the child can be evaluated
        var rootSymbol = new ProgramRootSymbol();
        var startSymbol = new StartSymbol();
        var statementParent = statement.Parent;
        EvaluationScript crossoverPointScript0 = new EvaluationScript() {
          Script = FormatScript(CreateTreeFromNode(random, statement, rootSymbol, startSymbol), problemData.LoopBreakConst, variables, variableSettings),
          Variables = variables,
          Timeout = Timeout
        };
        JObject json0 = PyProcess.SendAndEvaluateProgram(crossoverPointScript0);
        statement.Parent = statementParent; // restore parent

        ISymbolicExpressionTreeNode selectedBranch = SelectBranch(statement, crossoverPoint0, compBranches, random, variables, variableSettings, json0, problemData.LoopBreakConst, problemData.Variables.GetTypesOfVariables());

        // perform the actual swap
        if (selectedBranch != null)
          Swap(crossoverPoint0, selectedBranch);
      }

      return parent0;
    }

    private string SemanticToPythonVariableSettings(IDictionary<string, IList> semantic) {
      StringBuilder strBuilder = new StringBuilder();
      foreach (var setting in semantic) {
        strBuilder.AppendLine(String.Format("{0} = {1}", setting.Key,
                                                         JsonConvert.SerializeObject(setting.Value)));
      }
      return strBuilder.ToString();
    }

    private ISymbolicExpressionTreeNode SelectBranch(ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, IEnumerable<ISymbolicExpressionTreeNode> compBranches, IRandom random, List<string> variables, string variableSettings, JObject jsonParent0, int loopBreakConst, IDictionary<VariableType, List<string>> variablesPerType) {
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      var statementNodetParent = statementNode.Parent; // save statement parent
      List<JObject> evaluationPerNode = new List<JObject>();
      List<double> similarity = new List<double>();

      var evaluationTree = CreateTreeFromNode(random, statementNode, rootSymbol, startSymbol); // this will affect statementNode.Parent
      foreach (var node in compBranches) {
        var parent = node.Parent; // save parent

        crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);
        crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, node); // this will affect node.Parent

        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = FormatScript(evaluationTree, loopBreakConst, variables, variableSettings),
          Variables = variables,
          Timeout = Timeout
        };
        JObject json = PyProcess.SendAndEvaluateProgram(evaluationScript1);
        node.Parent = parent; // restore parent

        evaluationPerNode.Add(json);
        similarity.Add(0);
      }
      statementNode.Parent = statementNodetParent; // restore statement parent
      crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);
      crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, crossoverPoint0.Child); // restore crossoverPoint0

      Dictionary<VariableType, List<string>> differencesPerType = new Dictionary<VariableType, List<string>>();
      List<string> differences;
      foreach (var entry in variablesPerType) {
        differences = new List<string>();
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
      return pos >= 0 ? compBranches.ElementAt(pos) : compBranches.SampleRandom(random);
    }

    protected string FormatScript(ISymbolicExpressionTree symbolicExpressionTree, int loopBreakConst, List<string> variables, string variableSettings) {
      Regex r = new Regex(@"^(.*?)\s*=", RegexOptions.Multiline);
      string variableSettingsSubstitute = r.Replace(variableSettings, "${1}_setting =");
      return String.Format(EVAL_TRACE_SCRIPT, ProblemData.HelperCode.Value,
                                              String.Format("'{0}'", String.Join("','", variables)),
                                              variableSettingsSubstitute,
                                              String.Join(",", variables),
                                              String.Join(",", variables.Select(x => x + "_setting")),
                                              PythonHelper.FormatToProgram(symbolicExpressionTree, loopBreakConst, "    "));
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

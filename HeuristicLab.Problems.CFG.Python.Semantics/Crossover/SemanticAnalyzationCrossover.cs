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
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("SemanticAnalyzationCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class SemanticCrossoverAnalyzationCrossover<T> : CFGPythonSemanticEvalCrossover<T>, IIterationBasedOperator
  where T : class, ICFGPythonProblemData {
    private const string NumberOfAllowedBranchesParameterName = "NumberOfAllowedBranches";
    private const string NumberOfPossibleBranchesSelectedParameterName = "NumberOfPossibleBranchesSelected";
    private const string NumberOfNoChangeDetectedParameterName = "NumberOfNoChangeDetected";
    private const string TypeSelectedForSimilarityParameterName = "TypeSelectedForSimilarity";

    private const string SemanticallyEquivalentCrossoverParameterName = "SemanticallyEquivalentCrossover";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParent";
    private const string SemanticLocalityParameterName = "SemanticLocality";
    private const string ConstructiveEffectParameterName = "ConstructiveEffect";

    private const string QualityParameterName = "Quality";

    public const int NoXoProbability = 3;
    public const int NoXoNoSemantics = 4;
    public const int NoXoNoAllowedBranch = 5;
    public const int NoXoNoStatement = 6;
    public const int NoXoNoSelectedBranch = 7;

    #region Parameter Properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public ILookupParameter<IntValue> NumberOfAllowedBranchesParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfAllowedBranchesParameterName]; }
    }
    public ILookupParameter<IntValue> NumberOfPossibleBranchesSelectedParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfPossibleBranchesSelectedParameterName]; }
    }
    public ILookupParameter<IntValue> NumberOfNoChangeDetectedParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfNoChangeDetectedParameterName]; }
    }
    public ILookupParameter<StringValue> TypeSelectedForSimilarityParameter {
      get { return (ILookupParameter<StringValue>)Parameters[TypeSelectedForSimilarityParameterName]; }
    }
    public ILookupParameter<IntValue> SemanticallyEquivalentCrossoverParameter {
      get { return (ILookupParameter<IntValue>)Parameters[SemanticallyEquivalentCrossoverParameterName]; }
    }
    public ILookupParameter<BoolValue> SemanticallyDifferentFromRootedParentParameter {
      get { return (ILookupParameter<BoolValue>)Parameters[SemanticallyDifferentFromRootedParentParameterName]; }
    }
    public ILookupParameter<DoubleValue> SemanticLocalityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters[SemanticLocalityParameterName]; }
    }
    public ILookupParameter<IntValue> ConstructiveEffectParameter {
      get { return (ILookupParameter<IntValue>)Parameters[ConstructiveEffectParameterName]; }
    }
    // Quality values of the parents
    public IScopeTreeLookupParameter<DoubleValue> QualityParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[QualityParameterName]; }
    }
    #endregion

    #region Properties
    public int Iterations {
      get { return IterationsParameter.ActualValue.Value; }
    }
    public int NumberOfAllowedBranches {
      set { NumberOfAllowedBranchesParameter.ActualValue = new IntValue(value); }
    }
    public int NumberOfPossibleBranchesSelected {
      set { NumberOfPossibleBranchesSelectedParameter.ActualValue = new IntValue(value); }
    }
    public int NumberOfNoChangeDetected {
      set { NumberOfNoChangeDetectedParameter.ActualValue = new IntValue(value); }
    }

    #endregion
    [StorableConstructor]
    protected SemanticCrossoverAnalyzationCrossover(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzationCrossover(SemanticCrossoverAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticCrossoverAnalyzationCrossover()
      : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new LookupParameter<IntValue>(NumberOfAllowedBranchesParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(NumberOfPossibleBranchesSelectedParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(NumberOfNoChangeDetectedParameterName, ""));
      Parameters.Add(new LookupParameter<StringValue>(TypeSelectedForSimilarityParameterName, ""));

      Parameters.Add(new LookupParameter<IntValue>(SemanticallyEquivalentCrossoverParameterName, ""));
      Parameters.Add(new LookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new LookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(ConstructiveEffectParameterName, ""));

      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, "The qualities of the trees that should be analyzed."));

      MaximumIterationsParameter.Hidden = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzationCrossover<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value);

      AddStatisticsNoCrossover(NoXoProbability);
      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        AddStatisticsNoCrossover(NoXoNoSemantics);
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

      // get possible semantic positions
      var statementProductions = ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <code>")).Symbols.Union(
                                 ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <statement>")).Symbols).Union(
                                 ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <predefined>")).Symbols);
      var statementProductionNames = statementProductions.Select(x => x.Name);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = statementProductionNames.Contains(crossoverPoint0.Child.Symbol.Name) ? crossoverPoint0.Child : crossoverPoint0.Parent;
      while (statement != null && !statementProductionNames.Contains(statement.Symbol.Name)) {
        statement = statement.Parent;
      }

      if (statement == null) {
        Swap(crossoverPoint0, compBranches.SampleRandom(random));
        AddStatisticsNoCrossover(NoXoNoStatement);
        return parent0;
      }

      var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
      string variableSettings;
      if (problemData.VariableSettings.Count == 0) {
        variableSettings = SemanticToPythonVariableSettings(semantic0.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
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
      if (selectedBranch != null) {
        Swap(crossoverPoint0, selectedBranch);

        AddStatistics(semantic0, parent0, statement, crossoverPoint0, json0, selectedBranch, random, variables, variableSettings, problemData.LoopBreakConst, problemData.Variables.GetTypesOfVariables()); // parent zero has been changed is now considered the child
      } else {
        AddStatisticsNoCrossover(NoXoNoSelectedBranch);
      }

      return parent0;
    }

    protected void AddStatisticsNoCrossover(int reason) {
      if (NumberOfPossibleBranchesSelectedParameter.ActualValue == null) {
        NumberOfPossibleBranchesSelected = 0;
      }
      if (NumberOfAllowedBranchesParameter.ActualValue == null) {
        NumberOfAllowedBranches = 0;
      }
      if (NumberOfNoChangeDetectedParameter.ActualValue == null) {
        NumberOfNoChangeDetected = 0;
      }
      if (TypeSelectedForSimilarityParameter.ActualValue == null) {
        TypeSelectedForSimilarityParameter.ActualValue = new StringValue("No Crossover");
      }

      var parentQualities = QualityParameter.ActualValue;
      double parent0Quality = parentQualities[0].Value;
      double parent1Quality = parentQualities[1].Value;

      SemanticallyEquivalentCrossoverParameter.ActualValue = new IntValue(reason);
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);
      SemanticLocalityParameter.ActualValue = new DoubleValue(0.0);
      ConstructiveEffectParameter.ActualValue = new IntValue(parent0Quality < parent1Quality ? 1 : 0);
    }

    protected void AddStatistics(ItemArray<PythonStatementSemantic> semantic0, ISymbolicExpressionTree child, ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, JObject jsonOriginal, ISymbolicExpressionTreeNode swapedBranch, IRandom random, List<string> variables, string variableSettings, int loopBreakConst, IDictionary<VariableType, List<string>> variablesPerType) {
      if (SemanticallyEquivalentCrossoverParameter.ActualValue == null) {
        var rootSymbol = new ProgramRootSymbol();
        var startSymbol = new StartSymbol();
        var statementNodetParent = statementNode.Parent; // save statement parent

        var evaluationTree = CreateTreeFromNode(random, statementNode, rootSymbol, startSymbol);
        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = FormatScript(evaluationTree, loopBreakConst, variables, variableSettings),
          Variables = variables,
          Timeout = Timeout
        };
        JObject jsonNow = PyProcess.SendAndEvaluateProgram(evaluationScript1);
        statementNode.Parent = statementNodetParent; // restore statement parent

        SemanticallyEquivalentCrossoverParameter.ActualValue = new IntValue(JToken.EqualityComparer.Equals(jsonOriginal, jsonNow) ? 1 : 2);
      }
      AddStatistics(semantic0, child);
    }

    protected void AddStatistics(ItemArray<PythonStatementSemantic> semantic0, ISymbolicExpressionTree child) {
      if (NumberOfPossibleBranchesSelectedParameter.ActualValue == null) {
        NumberOfPossibleBranchesSelected = 0;
      }
      if (NumberOfAllowedBranchesParameter.ActualValue == null) {
        NumberOfAllowedBranches = 0;
      }
      if (NumberOfNoChangeDetectedParameter.ActualValue == null) {
        NumberOfNoChangeDetected = 0;
      }
      if (TypeSelectedForSimilarityParameter.ActualValue == null) {
        TypeSelectedForSimilarityParameter.ActualValue = new StringValue("Random crossover");
      }

      var parentQualities = QualityParameter.ActualValue;
      double parent0Quality = parentQualities[0].Value;
      double parent1Quality = parentQualities[1].Value;

      var pythonSemanticHelper = new PythonProcessSemanticHelper(ProblemData.Variables.GetVariableNames(), 1000); // hardcoded value!!! // TODO: object created for every crossover

      var childResults = pythonSemanticHelper.EvaluateAndTraceProgram(PythonProcessParameter.ActualValue,
                                             PythonHelper.FormatToProgram(child, ProblemData.LoopBreakConst, ProblemData.FullHeader, ProblemData.FullFooter),
                                             PythonHelper.ConvertToPythonValues(ProblemData.Input, ProblemData.TrainingIndices),
                                             PythonHelper.ConvertToPythonValues(ProblemData.Output, ProblemData.TrainingIndices),
                                             ProblemData.TrainingIndices,
                                             ProblemData.FullHeader,
                                             ProblemData.FullFooter,
                                             child,
                                             Timeout);

      var childQuality = childResults.Item3;
      SemanticLocalityParameter.ActualValue = new DoubleValue(Math.Abs(parent0Quality - childQuality));
      ConstructiveEffectParameter.ActualValue = new IntValue(childQuality < parent0Quality
                                                                ? childQuality < parent1Quality ? 2 : 1
                                                                : 0);

      if (!String.IsNullOrEmpty(childResults.Item4)) {
        SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(true);
        return; // no semantics is available, but the child is different because it failed, which is different from its parent
      }
      var parent0Semantic = semantic0.Last();
      var childSemantic = childResults.Item5.Last();

      var parent0Res = parent0Semantic.After.Keys.Contains("res") ? parent0Semantic.After["res"] : parent0Semantic.Before["res"];
      var child0Res = childSemantic.After.Keys.Contains("res") ? childSemantic.After["res"] : childSemantic.Before["res"];

      var enumParent = parent0Res.GetEnumerator();
      var enumChild = child0Res.GetEnumerator();

      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);

      var type = ProblemData.Variables.GetTypesOfVariables().First(x => x.Value.Contains("res")).Key;
      if (type.IsListType()) {
        // always move forward both enumerators (do not use short-circuit evaluation!)
        while (enumParent.MoveNext() & enumChild.MoveNext()) {
          if (!JToken.EqualityComparer.Equals((JArray)enumParent.Current, (JArray)enumChild.Current)) {
            SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
            break;
          }
        }
        if (enumParent.MoveNext() || enumChild.MoveNext()) {
          SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
        }
      } else {
        // always move forward both enumerators (do not use short-circuit evaluation!)
        while (enumParent.MoveNext() & enumChild.MoveNext()) {
          if (!enumParent.Current.Equals(enumChild.Current)) {
            SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
            break;
          }
        }
        if (enumParent.MoveNext() || enumChild.MoveNext()) {
          SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
        }
      }
    }

    protected ISymbolicExpressionTreeNode SelectBranch(ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, IEnumerable<ISymbolicExpressionTreeNode> compBranches, IRandom random, List<string> variables, string variableSettings, JObject jsonParent0, int loopBreakConst, IDictionary<VariableType, List<string>> variablesPerType) {
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      var statementNodetParent = statementNode.Parent; // save statement parent
      List<JObject> evaluationPerNode = new List<JObject>();
      List<double> similarity = new List<double>();

      var evaluationTree = CreateTreeFromNode(random, statementNode, rootSymbol, startSymbol); // this will affect statementNode.Parent
      crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex); // removes parent from child
      foreach (var node in compBranches) {
        var parent = node.Parent; // save parent

        crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, node); // this will affect node.Parent

        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = FormatScript(evaluationTree, loopBreakConst, variables, variableSettings),
          Variables = variables,
          Timeout = Timeout
        };
        JObject json = PyProcess.SendAndEvaluateProgram(evaluationScript1);
        crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex); // removes intermediate parent from node
        node.Parent = parent; // restore parent

        evaluationPerNode.Add(json);
        similarity.Add(0);
      }
      statementNode.Parent = statementNodetParent; // restore statement parent  
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

      return pos >= 0 ? compBranches.ElementAt(pos) : compBranches.SampleRandom(random);
    }
  }
}

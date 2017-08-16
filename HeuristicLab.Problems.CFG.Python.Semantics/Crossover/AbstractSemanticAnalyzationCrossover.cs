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
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("AbstractSemanticAnalyzationCrossover", "Base class for semantic analyzation crossover.")]
  [StorableClass]
  public abstract class AbstractSemanticAnalyzationCrossover<T> : CFGPythonSemanticEvalCrossover<T>, IIterationBasedOperator
    where T : class, ICFGPythonProblemData {
    private const string NumberOfAllowedBranchesParameterName = "NumberOfAllowedBranches";
    private const string NumberOfPossibleBranchesSelectedParameterName = "NumberOfPossibleBranchesSelected";
    private const string NumberOfNoChangeDetectedParameterName = "NumberOfNoChangeDetected";
    private const string TypeSelectedForSimilarityParameterName = "TypeSelectedForSimilarity";

    private const string SemanticallyEquivalentCrossoverParameterName = "SemanticallyEquivalentCrossover";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParent";
    private const string SemanticLocalityParameterName = "SemanticLocality";
    private const string ConstructiveEffectParameterName = "ConstructiveEffect";

    private const string NumberOfCrossoverTriesParameterName = "NumberOfCrossoverTries";

    private const string CrossoverExceptionsParameterName = "CrossoverExceptions";

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
    public ILookupParameter<IntValue> NumberOfCrossoverTriesParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfCrossoverTriesParameterName]; }
    }
    public ILookupParameter<ItemCollection<StringValue>> CrossoverExceptionsParameter {
      get { return (ILookupParameter<ItemCollection<StringValue>>)Parameters[CrossoverExceptionsParameterName]; }
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
    public int NumberOfCrossoverTries {
      set { NumberOfCrossoverTriesParameter.ActualValue = new IntValue(value); }
    }
    #endregion

    // ToDo: Remove! This is just a quickfix
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      if (!Parameters.ContainsKey(CrossoverExceptionsParameterName)) {
        Parameters.Add(new LookupParameter<ItemCollection<StringValue>>(CrossoverExceptionsParameterName, ""));
      }
    }

    [StorableConstructor]
    protected AbstractSemanticAnalyzationCrossover(bool deserializing) : base(deserializing) { }
    protected AbstractSemanticAnalyzationCrossover(AbstractSemanticAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public AbstractSemanticAnalyzationCrossover()
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
      Parameters.Add(new LookupParameter<IntValue>(NumberOfCrossoverTriesParameterName, ""));
      Parameters.Add(new LookupParameter<ItemCollection<StringValue>>(CrossoverExceptionsParameterName, ""));

      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, "The qualities of the trees that should be analyzed."));

      MaximumIterationsParameter.Hidden = true;
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      CrossoverExceptionsParameter.ActualValue = new ItemCollection<StringValue>();
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value) {
        ItemArray<PythonStatementSemantic> newSemantics;
        var child = Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value, out newSemantics);
        NewSemanticParameter.ActualValue = newSemantics;
        return child;
      }

      NewSemanticParameter.ActualValue = Semantics[0];
      AddStatisticsNoCrossover(NoXoProbability);
      return parent0;
    }

    protected abstract ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, T problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics);

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
      if (NumberOfCrossoverTriesParameter.ActualValue == null) {
        NumberOfCrossoverTries = 0;
      }
      if (TypeSelectedForSimilarityParameter.ActualValue == null) {
        TypeSelectedForSimilarityParameter.ActualValue = new StringValue(reason == NoXoNoStatement ? "Random crossover (No Statement)" : "No Crossover");
      }

      var parentQualities = QualityParameter.ActualValue;
      double parent0Quality = parentQualities[0].Value;
      double parent1Quality = parentQualities[1].Value;

      SemanticallyEquivalentCrossoverParameter.ActualValue = new IntValue(reason);
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);
      SemanticLocalityParameter.ActualValue = new DoubleValue(0.0);
      ConstructiveEffectParameter.ActualValue = new IntValue(parent0Quality < parent1Quality ? 1 : 0);
    }

    protected void AddStatistics(ItemArray<PythonStatementSemantic> semantic0, ISymbolicExpressionTree child, ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, JObject jsonOriginal, ISymbolicExpressionTreeNode swapedBranch, IRandom random, T problemData, List<string> variables, string variableSettings) {
      if (SemanticallyEquivalentCrossoverParameter.ActualValue == null) {
        JObject jsonNow = SemanticOperatorHelper.EvaluateStatementNode(statementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
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
      if (NumberOfCrossoverTriesParameter.ActualValue == null) {
        NumberOfCrossoverTries = 0;
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
      // first semantic statement is <predefined> which contains all code and therefore all changes to res*
      var parent0Semantic = semantic0.First();
      var childSemantic = childResults.Item5.First();

      // check all results
      var resKeys = parent0Semantic.After.Keys.Where(x => x.StartsWith("res"));
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);
      foreach (var resKey in resKeys) {
        var parent0Res = parent0Semantic.After.Keys.Contains(resKey) ? parent0Semantic.After[resKey] : parent0Semantic.Before[resKey];
        var child0Res = childSemantic.After.Keys.Contains(resKey) ? childSemantic.After[resKey] : childSemantic.Before[resKey];

        var enumParent = parent0Res.GetEnumerator();
        var enumChild = child0Res.GetEnumerator();

        var type = ProblemData.Variables.GetTypesOfVariables().First(x => x.Value.Contains(resKey)).Key;
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
        // break if a change has already been found
        if (SemanticallyDifferentFromRootedParentParameter.ActualValue.Value) { break; }
      }
    }
  }
}

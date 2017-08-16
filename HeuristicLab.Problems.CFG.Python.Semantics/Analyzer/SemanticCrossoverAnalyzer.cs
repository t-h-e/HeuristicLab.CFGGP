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
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python.Semantics.Analyzer {
  [Item("SemanticCrossoverAnalyzer", "")]
  [StorableClass]
  public class SemanticCrossoverAnalyzer : SingleSuccessorOperator, IAnalyzer, IIterationBasedOperator {
    private const string NumberOfAllowedBranchesParameterName = "NumberOfAllowedBranches";
    private const string NumberOfPossibleBranchesSelectedParameterName = "NumberOfPossibleBranchesSelected";
    private const string NumberOfNoChangeDetectedParameterName = "NumberOfNoChangeDetected";
    private const string TypeSelectedForSimilarityParameterName = "TypeSelectedForSimilarity";

    private const string SemanticallyEquivalentCrossoverParameterName = "SemanticallyEquivalentCrossover";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParent";
    private const string SemanticLocalityParameterName = "SemanticLocality";
    private const string NumberOfCrossoverTriesParameterName = "NumberOfCrossoverTries";
    private const string ConstructiveEffectParameterName = "ConstructiveEffect";

    private const string CrossoverExceptionsParameterName = "CrossoverExceptions";

    #region parameter properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public IScopeTreeLookupParameter<IntValue> NumberOfAllowedBranchesParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[NumberOfAllowedBranchesParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> NumberOfPossibleBranchesSelectedParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[NumberOfPossibleBranchesSelectedParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> NumberOfNoChangeDetectedParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[NumberOfNoChangeDetectedParameterName]; }
    }
    public IScopeTreeLookupParameter<StringValue> TypeSelectedForSimilarityParameter {
      get { return (IScopeTreeLookupParameter<StringValue>)Parameters[TypeSelectedForSimilarityParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> SemanticallyEquivalentCrossoverParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[SemanticallyEquivalentCrossoverParameterName]; }
    }
    public IScopeTreeLookupParameter<BoolValue> SemanticallyDifferentFromRootedParentParameter {
      get { return (IScopeTreeLookupParameter<BoolValue>)Parameters[SemanticallyDifferentFromRootedParentParameterName]; }
    }
    public IScopeTreeLookupParameter<DoubleValue> SemanticLocalityParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[SemanticLocalityParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> ConstructiveEffectParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[ConstructiveEffectParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> NumberOfCrossoverTriesParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[NumberOfCrossoverTriesParameterName]; }
    }
    public IScopeTreeLookupParameter<ItemCollection<StringValue>> CrossoverExceptionsParameter {
      get { return (IScopeTreeLookupParameter<ItemCollection<StringValue>>)Parameters[CrossoverExceptionsParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters["Results"]; }
    }
    #endregion

    #region properties
    public bool EnabledByDefault { get { return false; } }
    public ResultCollection ResultCollection {
      get { return ResultsParameter.ActualValue; }
    }
    public DataTable SemanticallyEquivalentCrossoverDataTable {
      get { return ResultCollection.ContainsKey(SemanticallyEquivalentCrossoverParameterName) ? ((DataTable)ResultCollection[SemanticallyEquivalentCrossoverParameterName].Value) : null; }
      set { ResultCollection.Add(new Result(SemanticallyEquivalentCrossoverParameterName, value)); }
    }
    public DataTable SemanticallyDifferentFromRootedParentDataTable {
      get { return ResultCollection.ContainsKey(SemanticallyDifferentFromRootedParentParameterName) ? ((DataTable)ResultCollection[SemanticallyDifferentFromRootedParentParameterName].Value) : null; }
      set { ResultCollection.Add(new Result(SemanticallyDifferentFromRootedParentParameterName, value)); }
    }
    public DataTable SemanticLocalityDataTable {
      get { return ResultCollection.ContainsKey(SemanticLocalityParameterName) ? ((DataTable)ResultCollection[SemanticLocalityParameterName].Value) : null; }
      set { ResultCollection.Add(new Result(SemanticLocalityParameterName, value)); }
    }
    public DataTable ConstructiveEffectDataTable {
      get { return ResultCollection.ContainsKey(ConstructiveEffectParameterName) ? ((DataTable)ResultCollection[ConstructiveEffectParameterName].Value) : null; }
      set { ResultCollection.Add(new Result(ConstructiveEffectParameterName, value)); }
    }
    #endregion

    // ToDo: Remove! This is just a quickfix
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      if (!Parameters.ContainsKey(CrossoverExceptionsParameterName)) {
        Parameters.Add(new ScopeTreeLookupParameter<ItemCollection<StringValue>>(CrossoverExceptionsParameterName, ""));
      }
    }

    [StorableConstructor]
    protected SemanticCrossoverAnalyzer(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzer(SemanticCrossoverAnalyzer original, Cloner cloner) : base(original, cloner) {
    }
    public SemanticCrossoverAnalyzer() : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(NumberOfAllowedBranchesParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(NumberOfPossibleBranchesSelectedParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(NumberOfNoChangeDetectedParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<StringValue>(TypeSelectedForSimilarityParameterName, ""));

      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(SemanticallyEquivalentCrossoverParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(ConstructiveEffectParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(NumberOfCrossoverTriesParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<ItemCollection<StringValue>>(CrossoverExceptionsParameterName, ""));

      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the exception frequencies should be stored."));

      IterationsParameter.ActualName = "Generations";
      MaximumIterationsParameter.Hidden = true;
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      if (IterationsParameter.ActualValue.Value <= 0) { return base.Apply(); }

      var numberOfAllowedBranches = NumberOfAllowedBranchesParameter.ActualValue.Where(x => x != null).ToArray();
      var numberOfPossibleBranchesSelected = NumberOfPossibleBranchesSelectedParameter.ActualValue.Where(x => x != null).ToArray();
      var numberOfNoChangeDetected = NumberOfNoChangeDetectedParameter.ActualValue.Where(x => x != null).ToArray();
      var typeSelectedForSimilarity = TypeSelectedForSimilarityParameter.ActualValue.Where(x => x != null).ToArray();
      var numberOfCrossoverTries = NumberOfCrossoverTriesParameter.ActualValue.Where(x => x != null).ToArray();

      AddAverageTableEntry(numberOfAllowedBranches, NumberOfAllowedBranchesParameterName);
      AddAverageTableEntry(numberOfPossibleBranchesSelected, NumberOfPossibleBranchesSelectedParameterName);
      AddAverageTableEntry(numberOfNoChangeDetected, NumberOfNoChangeDetectedParameterName);
      AddTypeSelectedForSimilarityTableEntry(typeSelectedForSimilarity);
      AddAverageTableEntry(numberOfCrossoverTries, NumberOfCrossoverTriesParameterName);

      var semanticallyEquivalentCrossover = SemanticallyEquivalentCrossoverParameter.ActualValue.Where(x => x != null).ToArray();
      var semanticallyDifferentFromRootedParent = SemanticallyDifferentFromRootedParentParameter.ActualValue.Where(x => x != null).ToArray();
      var semanticLocality = SemanticLocalityParameter.ActualValue.Where(x => x != null).ToArray();
      var constructiveEffect = ConstructiveEffectParameter.ActualValue.Where(x => x != null).ToArray();

      AddSemanticallyEquivalentCrossoverTableEntry(semanticallyEquivalentCrossover);
      AddSemanticallyDifferentFromRootedParentTableEntry(semanticallyDifferentFromRootedParent);
      AddSemanticLocalityTableEntry(semanticLocality);
      AddConstructiveEffectTableEntry(constructiveEffect);

      var crossoverExceptions = CrossoverExceptionsParameter.ActualValue.Where(x => x != null).SelectMany(x => x).ToArray();
      AddCrossoverExceptionsTableEntry(crossoverExceptions);

      // Remove values, otherwise they might be saved in the elitists
      var subScopeCount = ExecutionContext.Scope.SubScopes.Count;
      var nullObjects = Enumerable.Repeat<object>(null, subScopeCount).ToList();
      var nullIntValueList = nullObjects.Cast<IntValue>().ToList();
      NumberOfAllowedBranchesParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      NumberOfPossibleBranchesSelectedParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      NumberOfNoChangeDetectedParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      TypeSelectedForSimilarityParameter.ActualValue = new ItemArray<StringValue>(nullObjects.Cast<StringValue>());
      NumberOfCrossoverTriesParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      SemanticallyEquivalentCrossoverParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new ItemArray<BoolValue>(nullObjects.Cast<BoolValue>());
      SemanticLocalityParameter.ActualValue = new ItemArray<DoubleValue>(nullObjects.Cast<DoubleValue>());
      ConstructiveEffectParameter.ActualValue = new ItemArray<IntValue>(nullIntValueList);
      CrossoverExceptionsParameter.ActualValue = new ItemArray<ItemCollection<StringValue>>(nullObjects.Cast<ItemCollection<StringValue>>());

      return base.Apply();
    }

    private void AddCrossoverExceptionsTableEntry(StringValue[] crossoverExceptions) {
      if (!ResultCollection.ContainsKey(CrossoverExceptionsParameterName)) {
        var newTable = new DataTable(CrossoverExceptionsParameterName, "");
        newTable.VisualProperties.YAxisTitle = "Absolute Exception Frequency";
        newTable.VisualProperties.YAxisMaximumAuto = false;

        ResultCollection.Add(new Result(CrossoverExceptionsParameterName, newTable));
      }
      var exceptionFrequencies = ((DataTable)ResultCollection[CrossoverExceptionsParameterName].Value);

      // all rows must have the same number of values
      int numberOfValues = IterationsParameter.ActualValue.Value - 1;

      foreach (var pair in crossoverExceptions.GroupBy(x => x.Value).ToDictionary(g => g.Key, g => g.Count())) {
        string key = String.IsNullOrEmpty(pair.Key) ? "No Exception" : pair.Key;
        if (!exceptionFrequencies.Rows.ContainsKey(key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(key, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          exceptionFrequencies.Rows.Add(row);
        }
        exceptionFrequencies.Rows[key].Values.Add(pair.Value);
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in exceptionFrequencies.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
    }

    private void AddTypeSelectedForSimilarityTableEntry(StringValue[] typeSelectedForSimilarity) {
      if (!ResultCollection.ContainsKey(TypeSelectedForSimilarityParameterName)) {
        var newTable = new DataTable(TypeSelectedForSimilarityParameterName, "");
        newTable.VisualProperties.YAxisTitle = "Percentage";
        newTable.VisualProperties.YAxisMaximumAuto = false;

        ResultCollection.Add(new Result(TypeSelectedForSimilarityParameterName, newTable));
      }
      var table = ((DataTable)ResultCollection[TypeSelectedForSimilarityParameterName].Value);

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = table.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      double count = typeSelectedForSimilarity.Count();
      var groupedValues = typeSelectedForSimilarity.Select(x => x.Value).GroupBy(x => x);
      foreach (var type in groupedValues) {
        if (!table.Rows.ContainsKey(type.Key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(type.Key, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          table.Rows.Add(row);
        }
        table.Rows[type.Key].Values.Add(type.Count() / count * 100);
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in table.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
    }

    private void AddAverageTableEntry(IntValue[] values, string tableName) {
      if (!ResultCollection.ContainsKey(tableName)) {
        var newTable = new DataTable(tableName, "");
        newTable.VisualProperties.YAxisTitle = "Average";
        newTable.VisualProperties.YAxisMaximumAuto = false;

        List<string> rowNames = new List<string>() { "Average", "Average (excluding Zero)" };
        foreach (var name in rowNames) {
          DataRow row = new DataRow(name);
          row.VisualProperties.StartIndexZero = true;
          newTable.Rows.Add(row);
        }

        ResultCollection.Add(new Result(tableName, newTable));
      }
      var table = ((DataTable)ResultCollection[tableName].Value);
      table.Rows["Average"].Values.Add(values.Select(x => x.Value).DefaultIfEmpty().Average());
      table.Rows["Average (excluding Zero)"].Values.Add(values.Select(x => x.Value).Where(x => x != 0).DefaultIfEmpty().Average());
    }

    private void AddSemanticallyEquivalentCrossoverTableEntry(IntValue[] semanticallyEquivalentCrossover) {
      if (SemanticallyEquivalentCrossoverDataTable == null) {
        var table = new DataTable(SemanticallyEquivalentCrossoverParameterName, "");
        table.VisualProperties.YAxisTitle = "Percentage";
        table.VisualProperties.YAxisMaximumFixedValue = 100.0;
        table.VisualProperties.YAxisMaximumAuto = false;

        List<string> rowNames = new List<string>() { "No Crossover", "Equivalent", "Different", "NoXoProbability", "NoXoNoStatement", "NoXoNoAllowedBranch", "NoXoNoSelectedBranch", "NoXoNoSemantics" };
        foreach (var name in rowNames) {
          DataRow row = new DataRow(name);
          row.VisualProperties.StartIndexZero = true;
          table.Rows.Add(row);
        }
        SemanticallyEquivalentCrossoverDataTable = table;
      }
      List<int> semanticallyEquivalentCrossoverCount = Enumerable.Repeat(0, 3 + 5).ToList();
      for (int i = 0; i < semanticallyEquivalentCrossover.Length; i++) {
        semanticallyEquivalentCrossoverCount[semanticallyEquivalentCrossover[i].Value]++;
      }
      double total = semanticallyEquivalentCrossover.Length;
      SemanticallyEquivalentCrossoverDataTable.Rows["No Crossover"].Values.Add(semanticallyEquivalentCrossoverCount[0] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["Equivalent"].Values.Add(semanticallyEquivalentCrossoverCount[1] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["Different"].Values.Add(semanticallyEquivalentCrossoverCount[2] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["NoXoProbability"].Values.Add(semanticallyEquivalentCrossoverCount[AbstractSemanticAnalyzationCrossover<ICFGPythonProblemData>.NoXoProbability] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["NoXoNoStatement"].Values.Add(semanticallyEquivalentCrossoverCount[AbstractSemanticAnalyzationCrossover<ICFGPythonProblemData>.NoXoNoStatement] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["NoXoNoAllowedBranch"].Values.Add(semanticallyEquivalentCrossoverCount[AbstractSemanticAnalyzationCrossover<ICFGPythonProblemData>.NoXoNoAllowedBranch] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["NoXoNoSelectedBranch"].Values.Add(semanticallyEquivalentCrossoverCount[AbstractSemanticAnalyzationCrossover<ICFGPythonProblemData>.NoXoNoSelectedBranch] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["NoXoNoSemantics"].Values.Add(semanticallyEquivalentCrossoverCount[AbstractSemanticAnalyzationCrossover<ICFGPythonProblemData>.NoXoNoSemantics] / total * 100.0);
    }

    private void AddSemanticallyDifferentFromRootedParentTableEntry(BoolValue[] semanticallyDifferentFromRootedParent) {
      if (SemanticallyDifferentFromRootedParentDataTable == null) {
        var table = new DataTable(SemanticallyDifferentFromRootedParentParameterName, "");
        table.VisualProperties.YAxisTitle = "Percentage";
        table.VisualProperties.YAxisMaximumFixedValue = 100.0;
        table.VisualProperties.YAxisMaximumAuto = false;

        DataRow differentRow = new DataRow("Different From Parent");
        differentRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(differentRow);

        DataRow sameRow = new DataRow("Same As Parent");
        sameRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(sameRow);

        SemanticallyDifferentFromRootedParentDataTable = table;
      }
      double different = semanticallyDifferentFromRootedParent.Count(x => x.Value);

      SemanticallyDifferentFromRootedParentDataTable.Rows["Different From Parent"].Values.Add(different / semanticallyDifferentFromRootedParent.Length * 100.0);
      SemanticallyDifferentFromRootedParentDataTable.Rows["Same As Parent"].Values.Add((semanticallyDifferentFromRootedParent.Length - different) / semanticallyDifferentFromRootedParent.Length * 100.0);
    }

    private void AddSemanticLocalityTableEntry(DoubleValue[] semanticLocality) {
      if (SemanticLocalityDataTable == null) {
        var table = new DataTable(SemanticLocalityParameterName, "");
        table.VisualProperties.YAxisTitle = "Average Fitness Change";

        List<string> rowNames = new List<string>() { "Sematic Locality", "Sematic Locality Without NaN And Infinity", "Sematic Locality Without NaN, Infinity and Equivalent (0)" };
        foreach (var name in rowNames) {
          DataRow row = new DataRow(name);
          row.VisualProperties.StartIndexZero = true;
          table.Rows.Add(row);
        }

        SemanticLocalityDataTable = table;
      }
      SemanticLocalityDataTable.Rows["Sematic Locality"].Values.Add(semanticLocality.Average(x => x.Value));
      var semanticLocalityAvgWithout = semanticLocality.Where(x => !Double.IsInfinity(x.Value) && !Double.IsNaN(x.Value) && x.Value < Double.MaxValue);
      if (!semanticLocalityAvgWithout.Any()) {
        SemanticLocalityDataTable.Rows["Sematic Locality Without NaN And Infinity"].Values.Add(Double.NaN);
      } else {
        SemanticLocalityDataTable.Rows["Sematic Locality Without NaN And Infinity"].Values.Add(semanticLocalityAvgWithout.Average(x => x.Value));
      }
      var semanticLocalityAvgWithout0 = semanticLocality.Where(x => !Double.IsInfinity(x.Value) && !Double.IsNaN(x.Value) && x.Value < Double.MaxValue && x.Value > 0);
      if (!semanticLocalityAvgWithout0.Any()) {
        SemanticLocalityDataTable.Rows["Sematic Locality Without NaN, Infinity and Equivalent (0)"].Values.Add(Double.NaN);
      } else {
        SemanticLocalityDataTable.Rows["Sematic Locality Without NaN, Infinity and Equivalent (0)"].Values.Add(semanticLocalityAvgWithout0.Average(x => x.Value));
      }
    }

    private void AddConstructiveEffectTableEntry(IntValue[] constructiveEffect) {
      if (ConstructiveEffectDataTable == null) {
        var table = new DataTable(ConstructiveEffectParameterName, "");
        table.VisualProperties.YAxisTitle = "Percentage";
        table.VisualProperties.YAxisMaximumFixedValue = 100.0;
        table.VisualProperties.YAxisMaximumAuto = false;

        DataRow worseThanRootedRow = new DataRow("Worse than or equal to rooted");
        worseThanRootedRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(worseThanRootedRow);

        DataRow betterThanRootedRow = new DataRow("Better than rooted");
        betterThanRootedRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(betterThanRootedRow);

        DataRow betterThanBothRow = new DataRow("Better than both");
        betterThanBothRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(betterThanBothRow);

        ConstructiveEffectDataTable = table;
      }
      List<double> constructiveEffectCount = new List<double>() { 0.0, 0.0, 0.0 };
      for (int i = 0; i < constructiveEffect.Length; i++) {
        constructiveEffectCount[constructiveEffect[i].Value]++;
      }

      ConstructiveEffectDataTable.Rows["Worse than or equal to rooted"].Values.Add(constructiveEffectCount[0] / constructiveEffect.Length * 100.0);
      ConstructiveEffectDataTable.Rows["Better than rooted"].Values.Add((constructiveEffectCount[1] + constructiveEffectCount[2]) / constructiveEffect.Length * 100.0);
      ConstructiveEffectDataTable.Rows["Better than both"].Values.Add(constructiveEffectCount[2] / constructiveEffect.Length * 100.0);
    }
  }
}

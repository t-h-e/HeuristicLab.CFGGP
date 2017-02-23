﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
  [Item("SemanticManipulatorAnalyzer", "")]
  [StorableClass]
  public class SemanticManipulatorAnalyzer : SingleSuccessorOperator, IAnalyzer, IIterationBasedOperator {
    private const string NumberOfTriesParameterName = "NumberOfTriesMutation";

    // options: semantic mutation, random mutation, no mutation
    private const string SemanticMutationParameterName = "SemanticMutation";

    private const string SemanticallyEquivalentMutationParameterName = "SemanticallyEquivalentMutation";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParentMutation";
    private const string SemanticLocalityParameterName = "SemanticLocalityMutation";
    private const string ConstructiveEffectParameterName = "ConstructiveEffectMutation";

    private const string MutationExceptionsParameterName = "MutationExceptions";

    #region parameter properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public IScopeTreeLookupParameter<IntValue> NumberOfTriesParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[NumberOfTriesParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> SemanticMutationParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[SemanticMutationParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> SemanticallyEquivalentMutationParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[SemanticallyEquivalentMutationParameterName]; }
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
    public IScopeTreeLookupParameter<ItemCollection<StringValue>> MutationExceptionsParameter {
      get { return (IScopeTreeLookupParameter<ItemCollection<StringValue>>)Parameters[MutationExceptionsParameterName]; }
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
    public DataTable SemanticallyEquivalentMutationDataTable {
      get { return ResultCollection.ContainsKey(SemanticallyEquivalentMutationParameterName) ? ((DataTable)ResultCollection[SemanticallyEquivalentMutationParameterName].Value) : null; }
      set { ResultCollection.Add(new Result(SemanticallyEquivalentMutationParameterName, value)); }
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

    [StorableConstructor]
    protected SemanticManipulatorAnalyzer(bool deserializing) : base(deserializing) { }
    protected SemanticManipulatorAnalyzer(SemanticManipulatorAnalyzer original, Cloner cloner) : base(original, cloner) {
    }
    public SemanticManipulatorAnalyzer() : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(NumberOfTriesParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(SemanticMutationParameterName, ""));

      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(SemanticallyEquivalentMutationParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(ConstructiveEffectParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<ItemCollection<StringValue>>(MutationExceptionsParameterName, ""));

      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the exception frequencies should be stored."));

      IterationsParameter.ActualName = "Generations";
      MaximumIterationsParameter.Hidden = true;
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticManipulatorAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      if (IterationsParameter.ActualValue.Value <= 0) { return base.Apply(); }

      var numberOfTries = NumberOfTriesParameter.ActualValue.ToArray();

      AddAverageTableEntry(numberOfTries, NumberOfTriesParameterName);

      var semanticallyEquivalentMutation = SemanticallyEquivalentMutationParameter.ActualValue.ToArray();
      var semanticallyDifferentFromRootedParent = SemanticallyDifferentFromRootedParentParameter.ActualValue.ToArray();
      var semanticLocality = SemanticLocalityParameter.ActualValue.ToArray();
      var constructiveEffect = ConstructiveEffectParameter.ActualValue.ToArray();

      AddSemanticallyEquivalentMutationTableEntry(semanticallyEquivalentMutation);
      AddSemanticallyDifferentFromRootedParentTableEntry(semanticallyDifferentFromRootedParent);
      AddSemanticLocalityTableEntry(semanticLocality);
      AddConstructiveEffectTableEntry(constructiveEffect);

      var mutationExceptions = MutationExceptionsParameter.ActualValue.SelectMany(x => x).ToArray();
      AddMutationExceptionsTableEntry(mutationExceptions);

      return base.Apply();
    }

    private void AddMutationExceptionsTableEntry(StringValue[] mutationExceptions) {
      if (!ResultCollection.ContainsKey(MutationExceptionsParameterName)) {
        var newTable = new DataTable(MutationExceptionsParameterName, "");
        newTable.VisualProperties.YAxisTitle = "Absolute Exception Frequency";
        newTable.VisualProperties.YAxisMaximumAuto = false;

        ResultCollection.Add(new Result(MutationExceptionsParameterName, newTable));
      }
      var exceptionFrequencies = ((DataTable)ResultCollection[MutationExceptionsParameterName].Value);

      // all rows must have the same number of values
      int numberOfValues = IterationsParameter.ActualValue.Value - 1;

      foreach (var pair in mutationExceptions.GroupBy(x => x.Value).ToDictionary(g => g.Key, g => g.Count())) {
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

    private void AddAverageTableEntry(IntValue[] values, string tableName) {
      if (!ResultCollection.ContainsKey(tableName)) {
        var newTable = new DataTable(tableName, "");
        newTable.VisualProperties.YAxisTitle = "Average";
        newTable.VisualProperties.YAxisMaximumAuto = false;

        DataRow row = new DataRow("Average");
        row.VisualProperties.StartIndexZero = true;
        newTable.Rows.Add(row);

        ResultCollection.Add(new Result(tableName, newTable));
      }
      var table = ((DataTable)ResultCollection[tableName].Value);
      table.Rows["Average"].Values.Add(values.Select(x => x.Value).Average());
    }

    private void AddSemanticallyEquivalentMutationTableEntry(IntValue[] semanticallyEquivalentMutation) {
      if (SemanticallyEquivalentMutationDataTable == null) {
        var table = new DataTable(SemanticallyEquivalentMutationParameterName, "");
        table.VisualProperties.YAxisTitle = "Percentage";
        table.VisualProperties.YAxisMaximumFixedValue = 100.0;
        table.VisualProperties.YAxisMaximumAuto = false;

        List<string> rowNames = new List<string>() { "No Mutation", "Equivalent", "Different", "NoXoProbability", "NoXoNoStatement", "NoXoNoAllowedBranch", "NoXoNoSelectedBranch", "NoXoNoSemantics" };
        foreach (var name in rowNames) {
          DataRow row = new DataRow(name);
          row.VisualProperties.StartIndexZero = true;
          table.Rows.Add(row);
        }
        SemanticallyEquivalentMutationDataTable = table;
      }
      List<int> semanticallyEquivalentMutationCount = Enumerable.Repeat(0, 3 + 5).ToList();
      for (int i = 0; i < semanticallyEquivalentMutation.Length; i++) {
        semanticallyEquivalentMutationCount[semanticallyEquivalentMutation[i].Value]++;
      }
      double total = semanticallyEquivalentMutation.Length;
      SemanticallyEquivalentMutationDataTable.Rows["No Mutation"].Values.Add(semanticallyEquivalentMutationCount[0] / total * 100.0);
      SemanticallyEquivalentMutationDataTable.Rows["Equivalent"].Values.Add(semanticallyEquivalentMutationCount[1] / total * 100.0);
      SemanticallyEquivalentMutationDataTable.Rows["Different"].Values.Add(semanticallyEquivalentMutationCount[2] / total * 100.0);
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

        DataRow worseThanRootedRow = new DataRow("Worse than rooted");
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

      ConstructiveEffectDataTable.Rows["Worse than rooted"].Values.Add(constructiveEffectCount[0] / constructiveEffect.Length * 100.0);
      ConstructiveEffectDataTable.Rows["Better than rooted"].Values.Add((constructiveEffectCount[1] + constructiveEffectCount[2]) / constructiveEffect.Length * 100.0);
      ConstructiveEffectDataTable.Rows["Better than both"].Values.Add(constructiveEffectCount[2] / constructiveEffect.Length * 100.0);
    }
  }
}

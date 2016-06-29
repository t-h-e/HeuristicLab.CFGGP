#region License Information
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
    private const string BestTrainingSimilarityAverage = "Best Training Similarity Average";
    private const string BestTrainingCrossoverCount = "Best Training Crossover Count";

    private const string SemanticallyEquivalentCrossoverParameterName = "SemanticallyEquivalentCrossover";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParent";
    private const string SemanticLocalityParameterName = "SemanticLocality";
    private const string ConstructiveEffectParameterName = "ConstructiveEffect";

    #region parameter properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
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
    [StorableConstructor]
    protected SemanticCrossoverAnalyzer(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzer(SemanticCrossoverAnalyzer original, Cloner cloner) : base(original, cloner) {
    }
    public SemanticCrossoverAnalyzer() : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(SemanticallyEquivalentCrossoverParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(ConstructiveEffectParameterName, ""));

      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the exception frequencies should be stored."));

      IterationsParameter.ActualName = "Generations";
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      if (IterationsParameter.ActualValue.Value <= 0) { return base.Apply(); }

      var semanticallyEquivalentCrossover = SemanticallyEquivalentCrossoverParameter.ActualValue.ToArray();
      var semanticallyDifferentFromRootedParent = SemanticallyDifferentFromRootedParentParameter.ActualValue.ToArray();
      var semanticLocality = SemanticLocalityParameter.ActualValue.Average(x => x.Value);
      var constructiveEffect = ConstructiveEffectParameter.ActualValue.ToArray();
      ResultCollection results = ResultsParameter.ActualValue;

      AddSemanticallyEquivalentCrossoverTableEntry(semanticallyEquivalentCrossover, results);
      AddSemanticallyDifferentFromRootedParentTableEntry(semanticallyDifferentFromRootedParent, results);
      AddSemanticLocalityTableEntry(semanticLocality, results);
      AddConstructiveEffectTableEntry(constructiveEffect, results);

      return base.Apply();
    }

    private void AddSemanticallyEquivalentCrossoverTableEntry(IntValue[] semanticallyEquivalentCrossover, ResultCollection results) {
      if (SemanticallyEquivalentCrossoverDataTable == null) {
        var table = new DataTable(SemanticallyEquivalentCrossoverParameterName, "");
        table.VisualProperties.YAxisTitle = "Percentage";
        table.VisualProperties.YAxisMaximumFixedValue = 100.0;
        table.VisualProperties.YAxisMaximumAuto = false;

        DataRow noCrossoverRow = new DataRow("No Crossover");
        noCrossoverRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(noCrossoverRow);

        DataRow equivalentRow = new DataRow("Equivalent");
        equivalentRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(equivalentRow);

        DataRow equivalentOrNoCrossoverRow = new DataRow("Equivalent + No Crossover");
        equivalentOrNoCrossoverRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(equivalentOrNoCrossoverRow);

        DataRow differentRow = new DataRow("Different");
        differentRow.VisualProperties.StartIndexZero = true;
        table.Rows.Add(differentRow);

        SemanticallyEquivalentCrossoverDataTable = table;
      }
      List<int> semanticallyEquivalentCrossoverCount = new List<int>() { 0, 0, 0 };
      for (int i = 0; i < semanticallyEquivalentCrossover.Length; i++) {
        semanticallyEquivalentCrossoverCount[semanticallyEquivalentCrossover[i].Value]++;
      }
      double total = semanticallyEquivalentCrossover.Length;
      SemanticallyEquivalentCrossoverDataTable.Rows["No Crossover"].Values.Add(semanticallyEquivalentCrossoverCount[0] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["Equivalent"].Values.Add(semanticallyEquivalentCrossoverCount[1] / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["Equivalent + No Crossover"].Values.Add((semanticallyEquivalentCrossoverCount[0] + semanticallyEquivalentCrossoverCount[1]) / total * 100.0);
      SemanticallyEquivalentCrossoverDataTable.Rows["Different"].Values.Add(semanticallyEquivalentCrossoverCount[2] / total * 100.0);
    }

    private void AddSemanticallyDifferentFromRootedParentTableEntry(BoolValue[] semanticallyDifferentFromRootedParent, ResultCollection results) {
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

    private void AddSemanticLocalityTableEntry(double average, ResultCollection results) {
      if (SemanticLocalityDataTable == null) {
        var table = new DataTable(SemanticLocalityParameterName, "");
        table.VisualProperties.YAxisTitle = "Average Fitness Change";

        DataRow row = new DataRow("Sematic Locality");
        row.VisualProperties.StartIndexZero = true;
        table.Rows.Add(row);

        SemanticLocalityDataTable = table;
      }
      SemanticLocalityDataTable.Rows["Sematic Locality"].Values.Add(average);
    }

    private void AddConstructiveEffectTableEntry(IntValue[] constructiveEffect, ResultCollection results) {
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
      ConstructiveEffectDataTable.Rows["Better than rooted"].Values.Add(constructiveEffectCount[1] / constructiveEffect.Length * 100.0);
      ConstructiveEffectDataTable.Rows["Better than both"].Values.Add(constructiveEffectCount[2] / constructiveEffect.Length * 100.0);
    }
  }
}

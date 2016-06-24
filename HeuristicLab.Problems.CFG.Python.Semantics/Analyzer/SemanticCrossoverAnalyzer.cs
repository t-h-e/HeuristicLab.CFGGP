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

    private const string SimilarityAverageParameterName = "SimilarityAverage";
    private const string SimilarityAverageTableParameterName = "SimilarityAverageTable";
    private const string SimilarityCrossoverCountParameterName = "SimilarityCrossoverCount";
    private const string SimilarityCrossoverCountTableParameterName = "SimilarityCrossoverCountTable";
    private const string QualityParameterName = "Quality";
    private const string ParentQualityParameterName = "ParentQuality";

    #region parameter properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public IScopeTreeLookupParameter<DoubleValue> SimilarityAverageParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[SimilarityAverageParameterName]; }
    }
    public ILookupParameter<DataTable> SimilarityAverageTableParameter {
      get { return (ILookupParameter<DataTable>)Parameters[SimilarityAverageTableParameterName]; }
    }
    public IScopeTreeLookupParameter<IntValue> SimilarityCrossoverCountParameter {
      get { return (IScopeTreeLookupParameter<IntValue>)Parameters[SimilarityCrossoverCountParameterName]; }
    }
    public ILookupParameter<DataTable> SimilarityCrossoverCountTableParameter {
      get { return (ILookupParameter<DataTable>)Parameters[SimilarityCrossoverCountTableParameterName]; }
    }
    public IScopeTreeLookupParameter<DoubleValue> QualityParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[QualityParameterName]; }
    }
    public IScopeTreeLookupParameter<ItemArray<DoubleValue>> ParentQualityParameter {
      get { return (IScopeTreeLookupParameter<ItemArray<DoubleValue>>)Parameters[ParentQualityParameterName]; }
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
    public ItemArray<DoubleValue> Quality {
      get { return QualityParameter.ActualValue; }
    }
    #endregion
    [StorableConstructor]
    protected SemanticCrossoverAnalyzer(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzer(SemanticCrossoverAnalyzer original, Cloner cloner) : base(original, cloner) {
    }
    public SemanticCrossoverAnalyzer() : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(SimilarityAverageParameterName, ""));
      Parameters.Add(new LookupParameter<DataTable>(SimilarityAverageTableParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(SimilarityCrossoverCountParameterName, ""));
      Parameters.Add(new LookupParameter<DataTable>(SimilarityCrossoverCountTableParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, "The qualities of the trees that should be analyzed."));

      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<DoubleValue>>(ParentQualityParameterName, ""));

      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the exception frequencies should be stored."));

      IterationsParameter.ActualName = "Generations";

      SimilarityAverageParameter.ActualName = "NewSimilarityAverage";
      SimilarityCrossoverCountParameter.ActualName = "NewSimilarityCrossoverCount";
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      if (IterationsParameter.ActualValue.Value <= 0) { return base.Apply(); }

      #region find best tree
      double bestQuality = double.PositiveInfinity;
      int curIndex = -1;
      double[] quality = Quality.Select(x => x.Value).ToArray();
      for (int i = 0; i < quality.Length; i++) {
        if (IsBetter(quality[i], bestQuality, false)) {
          bestQuality = quality[i];
          curIndex = i;
        }
      }
      #endregion

      var similarityAverageArray = SimilarityAverageParameter.ActualValue.ToArray();
      var similarityCrossoverCountArray = SimilarityCrossoverCountParameter.ActualValue.ToArray();

      ResultCollection results = ResultsParameter.ActualValue;

      if (!results.ContainsKey(BestTrainingSimilarityAverage)) {
        results.Add(new Result(BestTrainingSimilarityAverage, similarityAverageArray[curIndex]));
      } else {
        results[BestTrainingSimilarityAverage].Value = similarityAverageArray[curIndex];
      }

      if (!results.ContainsKey(BestTrainingCrossoverCount)) {
        results.Add(new Result(BestTrainingCrossoverCount, similarityCrossoverCountArray[curIndex]));
      } else {
        results[BestTrainingCrossoverCount].Value = similarityCrossoverCountArray[curIndex];
      }

      AddTableEntry(SimilarityAverageTableParameter, "Similarity Average", "Similarity", similarityAverageArray.Sum(x => x.Value), similarityAverageArray.Length, results);
      AddTableEntry(SimilarityCrossoverCountTableParameter, "Similarity Crossover Count", "Count", similarityCrossoverCountArray.Sum(x => x.Value), similarityCrossoverCountArray.Length, results);

      return base.Apply();
    }

    private void AddTableEntry(ILookupParameter<DataTable> tableParameter, string tableName, string yaxisTitle, double sum, int length, ResultCollection results) {
      DataTable table = tableParameter.ActualValue;
      if (table == null) {
        table = new DataTable(tableName, "");
        table.VisualProperties.YAxisTitle = yaxisTitle;

        DataRow row = new DataRow("Average");
        row.VisualProperties.StartIndexZero = true;
        table.Rows.Add(row);

        tableParameter.ActualValue = table;
        results.Add(new Result(tableName, table));
      }
      table.Rows["Average"].Values.Add(sum / length);
    }

    private bool IsBetter(double lhs, double rhs, bool maximization) {
      if (maximization) return lhs > rhs;
      else return lhs < rhs;
    }
  }
}

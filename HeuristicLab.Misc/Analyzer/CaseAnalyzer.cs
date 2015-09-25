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
using System.Linq;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  /// <summary>
  /// An operator that tracks the solved cases.
  /// </summary>
  [Item("CaseAnalyzer", "An operator that tracks the solved cases.")]
  [StorableClass]
  public class CaseAnalyzer : SingleSuccessorOperator, IAnalyzer {
    #region parameter properties
    public IScopeTreeLookupParameter<BoolArray> CasesParameter {
      get { return (IScopeTreeLookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public IScopeTreeLookupParameter<DoubleArray> CaseQualitiesParameter {
      get { return (IScopeTreeLookupParameter<DoubleArray>)Parameters["CaseQualities"]; }
    }
    public ILookupParameter<DataTable> CasesSolvedFrequenciesParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CasesSolved"]; }
    }
    public ILookupParameter<DataTable> SummedCaseQualitiesParameter {
      get { return (ILookupParameter<DataTable>)Parameters["SummedCaseQualities"]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters["Results"]; }
    }
    #endregion

    #region properties
    public virtual bool EnabledByDefault {
      get { return false; }
    }
    #endregion

    [StorableConstructor]
    protected CaseAnalyzer(bool deserializing) : base(deserializing) { }
    protected CaseAnalyzer(CaseAnalyzer original, Cloner cloner)
      : base(original, cloner) {
    }
    public CaseAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<BoolArray>("Cases", "The cases to analyze."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual"));
      Parameters.Add(new LookupParameter<DataTable>("CasesSolved", "The string matrix which represents the solved cases"));
      Parameters.Add(new LookupParameter<DataTable>("SummedCaseQualities", "The string matrix which represents the quality of each cases summed over all individuals"));
      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the cases solved frequencies should be stored."));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CaseAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      ItemArray<BoolArray> cases = CasesParameter.ActualValue;
      ItemArray<DoubleArray> caseQualities = CaseQualitiesParameter.ActualValue;
      int length = cases.First().Length;
      if (cases.Any(x => x.Length != length) || caseQualities.Any(x => x.Length != length)) {
        throw new ArgumentException("Every individual has to have the same number of cases.");
      }

      double count = cases.Count();
      int rows = cases.First().Length;
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable casesSolvedFrequencies = CasesSolvedFrequenciesParameter.ActualValue;
      DataTable summedCaseQualities = SummedCaseQualitiesParameter.ActualValue;
      if (casesSolvedFrequencies == null || summedCaseQualities == null) {
        casesSolvedFrequencies = new DataTable("Cases solved", "Absolute frequency of cases solved aggregated over the whole population.");
        casesSolvedFrequencies.VisualProperties.YAxisTitle = "Absolute Cases Solved Frequency";
        summedCaseQualities = new DataTable("Summed case qualities", "Absolute frequency of cases solved aggregated over the whole population.");
        summedCaseQualities.VisualProperties.YAxisTitle = "Summed Cases Qualities";


        CasesSolvedFrequenciesParameter.ActualValue = casesSolvedFrequencies;
        SummedCaseQualitiesParameter.ActualValue = summedCaseQualities;
        results.Add(new Result("Cases solved", casesSolvedFrequencies));
        results.Add(new Result("Summed cases qualities", summedCaseQualities));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = casesSolvedFrequencies.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var triple in Enumerable.Range(0, cases[0].Length).Select(i => new Tuple<string, int, double>("Case " + i, cases.Count(caseArray => caseArray[i]), caseQualities.Sum(casesQualityArray => casesQualityArray[i])))) {
        if (!casesSolvedFrequencies.Rows.ContainsKey(triple.Item1)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(triple.Item1, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          casesSolvedFrequencies.Rows.Add(row);

          row = new DataRow(triple.Item1, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          summedCaseQualities.Rows.Add(row);
        }
        casesSolvedFrequencies.Rows[triple.Item1].Values.Add(triple.Item2);
        summedCaseQualities.Rows[triple.Item1].Values.Add(triple.Item3);
      }
      return base.Apply();
    }
  }
}

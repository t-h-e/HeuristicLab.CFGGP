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

namespace HeuristicLab.Problems.CFG {
  /// <summary>
  /// An operator that tracks the solved cases.
  /// </summary>
  [Item("CaseAnalyzer", "An operator that tracks the solved cases.")]
  [StorableClass]
  public class CaseAnalyzer : SingleSuccessorOperator, IAnalyzer {
    #region parameter properties
    public IScopeTreeLookupParameter<BoolArray> CasesTreeParameter {
      get { return (IScopeTreeLookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public ILookupParameter<DataTable> CasesSolvedFrequenciesParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CasesSolved"]; }
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
      Parameters.Add(new LookupParameter<DataTable>("CasesSolved", "The string matrix which represents the solved cases"));
      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the cases solved frequencies should be stored."));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CaseAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      ItemArray<BoolArray> cases = CasesTreeParameter.ActualValue;
      double count = cases.Count();
      int rows = cases.First().Length;
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable casesSolvedFrequencies = CasesSolvedFrequenciesParameter.ActualValue;
      if (casesSolvedFrequencies == null) {
        casesSolvedFrequencies = new DataTable("Cases solved", "Absolute frequency of cases solved aggregated over the whole population.");
        casesSolvedFrequencies.VisualProperties.YAxisTitle = "Absolute Cases Solved Frequency";

        CasesSolvedFrequenciesParameter.ActualValue = casesSolvedFrequencies;
        results.Add(new Result("Cases solved", casesSolvedFrequencies));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = casesSolvedFrequencies.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in Enumerable.Range(0, cases[0].Length).Select(i => new Tuple<string, int>("Case " + i, cases.Count(caseArray => caseArray[i])))) {
        if (!casesSolvedFrequencies.Rows.ContainsKey(pair.Item1)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(pair.Item1, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          casesSolvedFrequencies.Rows.Add(row);
        }
        casesSolvedFrequencies.Rows[pair.Item1].Values.Add(pair.Item2);
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in casesSolvedFrequencies.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
      return base.Apply();
    }
  }
}

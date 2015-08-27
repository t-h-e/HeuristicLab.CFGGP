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

namespace HeuristicLab.Problems.CFG.Python {
  /// <summary>
  /// An operator that tracks the frequencies of distinct exceptions during the evaluation.
  /// </summary>
  [Item("CFGPythonExceptionAnalyzer", "An operator that tracks the frequencies of distinct exceptions during the evaluation")]
  [StorableClass]
  public class CFGPythonExceptionAnalyzer : SingleSuccessorOperator, IAnalyzer {
    #region parameter properties
    public IScopeTreeLookupParameter<StringValue> ExceptionTreeParameter {
      get { return (IScopeTreeLookupParameter<StringValue>)Parameters["Exception"]; }
    }
    public ILookupParameter<DataTable> ExceptionFrequenciesParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ExceptionFrequencies"]; }
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
    protected CFGPythonExceptionAnalyzer(bool deserializing) : base(deserializing) { }
    protected CFGPythonExceptionAnalyzer(CFGPythonExceptionAnalyzer original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGPythonExceptionAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<StringValue>("Exception", "The symbolic expression trees to analyze."));
      Parameters.Add(new LookupParameter<DataTable>("ExceptionFrequencies", "The data table to store the exception frequencies."));
      Parameters.Add(new LookupParameter<ResultCollection>("Results", "The result collection where the exception frequencies should be stored."));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonExceptionAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      ItemArray<StringValue> exceptions = ExceptionTreeParameter.ActualValue;
      double count = exceptions.Count();
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable exceptionFrequencies = ExceptionFrequenciesParameter.ActualValue;
      if (exceptionFrequencies == null) {
        exceptionFrequencies = new DataTable("Exception frequencies", "Relative frequency of exception aggregated over the whole population.");
        exceptionFrequencies.VisualProperties.YAxisTitle = "Relative Exception Frequency";

        ExceptionFrequenciesParameter.ActualValue = exceptionFrequencies;
        results.Add(new Result("Exception frequencies", exceptionFrequencies));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = exceptionFrequencies.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in exceptions.GroupBy(x => x.Value).ToDictionary(g => g.Key, g => (double)g.Count() / count)) {
        string key = String.IsNullOrEmpty(pair.Key) ? "No Exception" : pair.Key;
        if (!exceptionFrequencies.Rows.ContainsKey(key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(key, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          exceptionFrequencies.Rows.Add(row);
        }
        exceptionFrequencies.Rows[key].Values.Add(Math.Round(pair.Value, 3));
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in exceptionFrequencies.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
      return base.Apply();
    }
  }
}

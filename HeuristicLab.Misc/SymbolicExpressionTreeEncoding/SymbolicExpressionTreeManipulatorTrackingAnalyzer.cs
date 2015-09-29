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
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  /// <summary>
  /// An operator that tracks the frequencies of distinct symbols in symbolic expression trees.
  /// </summary>
  [Item("SymbolicExpressionTreeManipulatorTrackingAnalyzer", "An operator that tracks frequencies of symbols in symbolic expression trees.")]
  [StorableClass]
  public class SymbolicExpressionTreeManipulatorTrackingAnalyzer : SingleSuccessorOperator, ISymbolicExpressionTreeAnalyzer, IManipulatorTrackingAnalyzer<ISymbolicExpressionTree> {
    private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";
    private const string ResultsParameterName = "Results";
    private const string SymbolFrequenciesParameterName = "SymbolFrequencies";
    private const string AggregateSymbolsWithDifferentSubtreeCountParameterName = "AggregateSymbolsWithDifferentSubtreeCount";

    #region parameter properties
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<DataTable> SymbolFrequenciesParameter {
      get { return (ILookupParameter<DataTable>)Parameters[SymbolFrequenciesParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters[ResultsParameterName]; }
    }
    public IValueParameter<BoolValue> AggregateSymbolsWithDifferentSubtreeCountParameter {
      get { return (IValueParameter<BoolValue>)Parameters[AggregateSymbolsWithDifferentSubtreeCountParameterName]; }
    }

    public IScopeTreeLookupParameter<ISymbolicExpressionTree> ChildParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["Child"]; }
    }
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> ManipulatorParentParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["ManipulatorParent"]; }
    }
    #endregion

    #region properties
    public virtual bool EnabledByDefault {
      get { return false; }
    }
    public BoolValue AggregrateSymbolsWithDifferentSubtreeCount {
      get { return AggregateSymbolsWithDifferentSubtreeCountParameter.Value; }
      set { AggregateSymbolsWithDifferentSubtreeCountParameter.Value = value; }
    }
    #endregion

    [StorableConstructor]
    protected SymbolicExpressionTreeManipulatorTrackingAnalyzer(bool deserializing) : base(deserializing) { }
    protected SymbolicExpressionTreeManipulatorTrackingAnalyzer(SymbolicExpressionTreeManipulatorTrackingAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public SymbolicExpressionTreeManipulatorTrackingAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression trees to analyze."));
      Parameters.Add(new LookupParameter<DataTable>(SymbolFrequenciesParameterName, "The data table to store the symbol frequencies."));
      Parameters.Add(new LookupParameter<ResultCollection>(ResultsParameterName, "The result collection where the symbol frequencies should be stored."));
      Parameters.Add(new ValueParameter<BoolValue>(AggregateSymbolsWithDifferentSubtreeCountParameterName, "Flag that indicates if the frequencies of symbols with the same name but different number of sub-trees should be aggregated.", new BoolValue(true)));

      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("Child", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("ManipulatorParent", ""));
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicExpressionTreeManipulatorTrackingAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      //first generation only
      if (ManipulatorParentParameter.ActualValue.Count() == 0) return base.Apply();

      if (ManipulatorParentParameter.ActualValue.Length != ChildParameter.ActualValue.Length) throw new ArgumentException("Number of children and crossover parents does not match. A reason might be that elitism was used.");

      ItemArray<ISymbolicExpressionTree> expressions = SymbolicExpressionTreeParameter.ActualValue;
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable symbolFrequencies = SymbolFrequenciesParameter.ActualValue;
      if (symbolFrequencies == null) {
        symbolFrequencies = new DataTable("Symbol frequencies", "Relative frequency of symbols aggregated over the whole population.");
        symbolFrequencies.VisualProperties.YAxisTitle = "Relative Symbol Frequency";

        SymbolFrequenciesParameter.ActualValue = symbolFrequencies;
        results.Add(new Result("Symbol frequencies", symbolFrequencies));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = symbolFrequencies.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in SymbolicExpressionSymbolFrequencyAnalyzer.CalculateSymbolFrequencies(expressions, AggregrateSymbolsWithDifferentSubtreeCount.Value)) {
        if (!symbolFrequencies.Rows.ContainsKey(pair.Key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(pair.Key, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          symbolFrequencies.Rows.Add(row);
        }
        symbolFrequencies.Rows[pair.Key].Values.Add(Math.Round(pair.Value, 3));
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in symbolFrequencies.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);

      return base.Apply();
    }

    public static IEnumerable<KeyValuePair<string, double>> CalculateSymbolFrequencies(IEnumerable<ISymbolicExpressionTree> trees, bool aggregateDifferentNumberOfSubtrees = true) {
      Dictionary<string, double> symbolFrequencies = new Dictionary<string, double>();
      int totalNumberOfSymbols = 0;

      foreach (var tree in trees) {
        foreach (var node in tree.IterateNodesPrefix()) {
          string symbolName;
          if (aggregateDifferentNumberOfSubtrees) symbolName = node.Symbol.Name;
          else symbolName = node.Symbol.Name + "-" + node.SubtreeCount;
          if (symbolFrequencies.ContainsKey(symbolName)) symbolFrequencies[symbolName] += 1;
          else symbolFrequencies.Add(symbolName, 1);
          totalNumberOfSymbols++;
        }
      }

      foreach (var pair in symbolFrequencies)
        yield return new KeyValuePair<string, double>(pair.Key, pair.Value / totalNumberOfSymbols);
    }
  }
}

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

    #region parameter properties
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters[ResultsParameterName]; }
    }

    public ILookupParameter<ISymbolicExpressionGrammar> SymbolicExpressionGrammarParameter {
      get { return (ILookupParameter<ISymbolicExpressionGrammar>)Parameters["Grammar"]; }
    }
    public IValueParameter<BoolValue> IncludeTerminalsParameter {
      get { return (IValueParameter<BoolValue>)Parameters["IncludeTerminals"]; }
    }

    public IScopeTreeLookupParameter<ISymbolicExpressionTree> ChildParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["Child"]; }
    }
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> ManipulatorParentParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["ManipulatorParent"]; }
    }

    public ILookupParameter<DataTable> ManipulatorGeneticMaterialDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorGeneticMaterialDepthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> ManipulatorGeneticMaterialLengthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorGeneticMaterialLengthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> ManipulatorDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorDepthPerGeneration"]; }
    }


    public ILookupParameter<DataTable> ManipulatorSymbolGeneticMaterialDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorSymbolGeneticMaterialDepth"]; }
    }
    public ILookupParameter<DataTable> ManipulatorSymbolGeneticMaterialLengthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorSymbolGeneticMaterialLength"]; }
    }
    public ILookupParameter<DataTable> ManipulatorSymbolDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorSymbolDepth"]; }
    }

    public ILookupParameter<DataTable> ManipulatorAbsolutePerSymbolParameter {
      get { return (ILookupParameter<DataTable>)Parameters["ManipulatorAbsolutePerSymbol"]; }
    }
    #endregion

    public virtual bool EnabledByDefault {
      get { return false; }
    }

    [StorableConstructor]
    protected SymbolicExpressionTreeManipulatorTrackingAnalyzer(bool deserializing) : base(deserializing) { }
    protected SymbolicExpressionTreeManipulatorTrackingAnalyzer(SymbolicExpressionTreeManipulatorTrackingAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public SymbolicExpressionTreeManipulatorTrackingAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression trees to analyze."));
      Parameters.Add(new LookupParameter<ResultCollection>(ResultsParameterName, "The result collection where the symbol frequencies should be stored."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionGrammar>("Grammar", ""));
      Parameters.Add(new ValueParameter<BoolValue>("IncludeTerminals", "", new BoolValue(true)));

      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("Child", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("ManipulatorParent", ""));

      Parameters.Add(new LookupParameter<DataTable>("ManipulatorGeneticMaterialDepthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("ManipulatorGeneticMaterialLengthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("ManipulatorDepthPerGeneration", ""));

      Parameters.Add(new LookupParameter<DataTable>("ManipulatorSymbolGeneticMaterialDepth", ""));
      Parameters.Add(new LookupParameter<DataTable>("ManipulatorSymbolGeneticMaterialLength", ""));
      Parameters.Add(new LookupParameter<DataTable>("ManipulatorSymbolDepth", ""));

      Parameters.Add(new LookupParameter<DataTable>("ManipulatorAbsolutePerSymbol", ""));
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicExpressionTreeManipulatorTrackingAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      //first generation only
      if (ManipulatorParentParameter.ActualValue.Count() == 0) return base.Apply();

      if (ManipulatorParentParameter.ActualValue.Length != ChildParameter.ActualValue.Length) throw new ArgumentException("Number of children and manipulator parents does not match. A reason might be that elitism was used.");

      var children = ChildParameter.ActualValue;
      var parents = ManipulatorParentParameter.ActualValue;

      List<Tuple<string, int, int, int>> values = new List<Tuple<string, int, int, int>>();
      for (int i = 0; i < ManipulatorParentParameter.ActualValue.Length; i++) {
        values.Add(CalculateGeneticMaterialChange(children[i], parents[i]));
      }

      CreateTables(values);

      return base.Apply();
    }

    private const string NOCHANGE = "No change";

    private void CreateTables(List<Tuple<string, int, int, int>> values) {
      var valuesWithChange = values.Where(x => !x.Item1.Equals(NOCHANGE));

      if (!IncludeTerminalsParameter.Value.Value) {
        var terminalSymbolNames = SymbolicExpressionGrammarParameter.ActualValue.Symbols.Where(x => x.MinimumArity > 0).Select(x => x.Name);
        values = values.Where(x => terminalSymbolNames.Contains(x.Item1) || NOCHANGE.Equals(x.Item1)).ToList();
        valuesWithChange = valuesWithChange.Where(x => terminalSymbolNames.Contains(x.Item1)).ToList();
      }

      CreatePerGenerationTable(ManipulatorGeneticMaterialDepthPerGenerationParameter, valuesWithChange.Average(x => x.Item2), "Manipulator genetic material depth");
      CreatePerGenerationTable(ManipulatorGeneticMaterialLengthPerGenerationParameter, valuesWithChange.Average(x => x.Item3), "Manipulator genetic material length");
      CreatePerGenerationTable(ManipulatorDepthPerGenerationParameter, valuesWithChange.Average(x => x.Item4), "Manipulator depth");

      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item2)), ManipulatorSymbolGeneticMaterialDepthParameter, "Manipulator per symbol genetic material depth");
      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item3)), ManipulatorSymbolGeneticMaterialLengthParameter, "Manipulator per symbol genetic material length");
      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item4)), ManipulatorSymbolDepthParameter, "Manipulator symbol depth");

      CreateAbsoluteManipulatorTable(values);
    }

    private void CreateAbsoluteManipulatorTable(List<Tuple<string, int, int, int>> values) {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable dataTable = ManipulatorAbsolutePerSymbolParameter.ActualValue;

      if (dataTable == null) {
        dataTable = new DataTable("Manipulator per symbol", description);
        dataTable.VisualProperties.YAxisTitle = "Manipulator per symbol";
        dataTable.VisualProperties.XAxisTitle = "Generation";

        ManipulatorAbsolutePerSymbolParameter.ActualValue = dataTable;
        results.Add(new Result("Manipulator per symbol", dataTable));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = dataTable.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in values.GroupBy(x => x.Item1, x => x)) {
        if (!dataTable.Rows.ContainsKey(pair.Key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(pair.Key, "", Enumerable.Repeat(0.0, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          dataTable.Rows.Add(row);
        }
        dataTable.Rows[pair.Key].Values.Add(pair.Count());
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in dataTable.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
    }

    private void CreatePerSymbolAndGenerationTable(IEnumerable<Tuple<string, double>> values, ILookupParameter<DataTable> dataTableParameter, string title, string description = "") {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable dataTable = dataTableParameter.ActualValue;

      if (dataTable == null) {
        dataTable = new DataTable(title, description);
        dataTable.VisualProperties.YAxisTitle = title;
        dataTable.VisualProperties.XAxisTitle = "Generation";

        dataTableParameter.ActualValue = dataTable;
        results.Add(new Result(title, dataTable));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = dataTable.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in values.GroupBy(x => x.Item1, x => x)) {
        if (!dataTable.Rows.ContainsKey(pair.Key)) {
          // initialize a new row for the symbol and pad with zeros
          DataRow row = new DataRow(pair.Key, "", Enumerable.Repeat(double.NaN, numberOfValues));
          row.VisualProperties.StartIndexZero = true;
          dataTable.Rows.Add(row);
        }
        dataTable.Rows[pair.Key].Values.Add(Math.Round(pair.Average(x => x.Item2), 3));
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in dataTable.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(double.NaN);
    }

    private void CreatePerGenerationTable(ILookupParameter<DataTable> generationalDataTableParameter, double value, string title, string description = "") {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable generationalDataTable = generationalDataTableParameter.ActualValue;
      if (generationalDataTable == null) {
        generationalDataTable = new DataTable(title, description);
        generationalDataTable.VisualProperties.YAxisTitle = title;
        generationalDataTable.VisualProperties.XAxisTitle = "Generation";

        generationalDataTableParameter.ActualValue = generationalDataTable;
        results.Add(new Result(title, generationalDataTable));
      }

      if (!generationalDataTable.Rows.ContainsKey(title)) {
        // initialize a new row for the symbol and pad with zeros
        DataRow row = new DataRow(title, "");
        row.VisualProperties.StartIndexZero = true;
        generationalDataTable.Rows.Add(row);
      }
      generationalDataTable.Rows[title].Values.Add(Math.Round(value, 3));
    }

    public static Tuple<string, int, int, int> CalculateGeneticMaterialChange(ISymbolicExpressionTree child, ISymbolicExpressionTree parent) {
      Tuple<ISymbolicExpressionTreeNode, int> manipulatorPoint = FindManipulatorPoint(child, parent);
      if (manipulatorPoint == null) return new Tuple<string, int, int, int>(NOCHANGE, 0, 0, 0);

      ISymbolicExpressionTreeNode manipulatorPointNode = manipulatorPoint.Item1;

      //may be needed to check difference to parent subtree, otherwise delete
      //int indexOfManipulatorPoint = child.Root.IterateNodesBreadth().ToList().IndexOf(manipulatorPointNode);
      //var parentSubtree = parent.Root.IterateNodesBreadth().ToList()[indexOfManipulatorPoint];

      return new Tuple<string, int, int, int>(manipulatorPointNode.Symbol.Name, manipulatorPointNode.GetDepth(), manipulatorPointNode.GetLength(), manipulatorPoint.Item2);
    }

    private static Tuple<ISymbolicExpressionTreeNode, int> FindManipulatorPoint(ISymbolicExpressionTree child, ISymbolicExpressionTree parent) {
      ISymbolicExpressionTreeNode childNode = child.Root;
      List<ISymbolicExpressionTreeNode> nextChildNodes = new List<ISymbolicExpressionTreeNode>() { childNode };

      ISymbolicExpressionTreeNode parentNode = parent.Root;
      List<ISymbolicExpressionTreeNode> nextParentNodes = new List<ISymbolicExpressionTreeNode>() { parentNode };

      List<Tuple<ISymbolicExpressionTreeNode, int>> possibleManipulatorPoints = new List<Tuple<ISymbolicExpressionTreeNode, int>>();

      // compare trees
      FindManipulatorPoint(childNode, parentNode, 0, possibleManipulatorPoints);

      // if only one possibility, than that is it
      if (possibleManipulatorPoints.Count == 1) return possibleManipulatorPoints.First();
      // if no possibility, than no manipulator toke place
      else if (possibleManipulatorPoints.Count == 0) return null;

      int minDepth = possibleManipulatorPoints.Select(x => x.Item2).Min();

      HashSet<ISymbolicExpressionTreeNode> manipulatorPointsSet = new HashSet<ISymbolicExpressionTreeNode>();

      // go to the same depth
      foreach (var item in possibleManipulatorPoints) {
        ISymbolicExpressionTreeNode curNode = item.Item1;
        int curDepth = item.Item2;
        while (curDepth > minDepth) {
          curNode = curNode.Parent;
          curDepth -= 1;
        }
        manipulatorPointsSet.Add(curNode);
      }

      // if only one possibility after reaching the same depth, than that is it
      if (manipulatorPointsSet.Count == 1) return new Tuple<ISymbolicExpressionTreeNode, int>(manipulatorPointsSet.First(), minDepth);


      while (manipulatorPointsSet.Count > 1) {
        List<ISymbolicExpressionTreeNode> tempList = new List<ISymbolicExpressionTreeNode>(manipulatorPointsSet);
        manipulatorPointsSet.Clear();

        foreach (var node in tempList) {
          ISymbolicExpressionTreeNode curNode = node;
          curNode = curNode.Parent;
          manipulatorPointsSet.Add(curNode);
        }

        --minDepth;
      }

      // if only one possibility after reaching the same depth, than that is it
      if (manipulatorPointsSet.Count == 1) return new Tuple<ISymbolicExpressionTreeNode, int>(manipulatorPointsSet.First(), minDepth);
      else throw new ArgumentException("Something went wrong. There has to be a manipulator point at this line.");
    }

    private static void FindManipulatorPoint(ISymbolicExpressionTreeNode childNode, ISymbolicExpressionTreeNode parentNode, int curDepth, IList<Tuple<ISymbolicExpressionTreeNode, int>> possibleManipulatorPoints) {
      if (childNode.Symbol.Name != parentNode.Symbol.Name) {
        possibleManipulatorPoints.Add(new Tuple<ISymbolicExpressionTreeNode, int>(childNode, curDepth));
        return;
      }

      if (childNode.Subtrees != null) {
        for (int i = 0; i < childNode.SubtreeCount; i++) {
          FindManipulatorPoint(childNode.GetSubtree(i), parentNode.GetSubtree(i), curDepth + 1, possibleManipulatorPoints);
        }
      }
    }
  }
}

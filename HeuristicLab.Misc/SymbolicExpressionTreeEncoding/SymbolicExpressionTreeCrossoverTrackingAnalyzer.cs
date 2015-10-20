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
  [Item("SymbolicExpressionTreeCrossoverTrackingAnalyzer", "An operator that tracks frequencies of symbols in symbolic expression trees.")]
  [StorableClass]
  public class SymbolicExpressionTreeCrossoverTrackingAnalyzer : SingleSuccessorOperator, ISymbolicExpressionTreeAnalyzer, ICrossoverTrackingAnalyzer<ISymbolicExpressionTree> {
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
    public IScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>> CrossoverParentsParameter {
      get { return (IScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>>)Parameters["CrossoverParents"]; }
    }

    public ILookupParameter<DataTable> CrossoverGeneticMaterialDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverGeneticMaterialDepthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> CrossoverGeneticMaterialLengthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverGeneticMaterialLengthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> CrossoverDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverDepthPerGeneration"]; }
    }


    public ILookupParameter<DataTable> CrossoverSymbolGeneticMaterialDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverSymbolGeneticMaterialDepth"]; }
    }
    public ILookupParameter<DataTable> CrossoverSymbolGeneticMaterialLengthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverSymbolGeneticMaterialLength"]; }
    }
    public ILookupParameter<DataTable> CrossoverSymbolDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverSymbolDepth"]; }
    }

    public ILookupParameter<DataTable> CrossoverAbsolutePerSymbolParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverAbsolutePerSymbol"]; }
    }

    public IScopeTreeLookupParameter<ISymbolicExpressionTree> RemovedBranchesParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["RemovedBranch"]; }
    }
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> AddedBranchesParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["AddedBranch"]; }
    }
    public IScopeTreeLookupParameter<ISymbol> CutPointSymbolParameter {
      get { return (IScopeTreeLookupParameter<ISymbol>)Parameters["CutPointSymbol"]; }
    }

    public ILookupParameter<DataTable> CrossoverActualRemovedMaterialDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualRemovedMaterialDepth"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualRemovedMaterialLengthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualRemovedMaterialLength"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualAddedMaterialDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualAddedMaterialDepth"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualAddedMaterialLengthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualAddedMaterialLength"]; }
    }

    public ILookupParameter<DataTable> CrossoverActualCutPointParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualCutPoint"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualCutPointParentParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualCutPointParent"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualCutPointChildParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualCutPointChild"]; }
    }
    public ILookupParameter<DataTable> CrossoverActualCutPointExchangeParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossoverActualCutPointExchange"]; }
    }
    #endregion

    public virtual bool EnabledByDefault {
      get { return false; }
    }

    [StorableConstructor]
    protected SymbolicExpressionTreeCrossoverTrackingAnalyzer(bool deserializing) : base(deserializing) { }
    protected SymbolicExpressionTreeCrossoverTrackingAnalyzer(SymbolicExpressionTreeCrossoverTrackingAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public SymbolicExpressionTreeCrossoverTrackingAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression trees to analyze."));
      Parameters.Add(new LookupParameter<ResultCollection>(ResultsParameterName, "The result collection where the symbol frequencies should be stored."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionGrammar>("Grammar", ""));
      Parameters.Add(new ValueParameter<BoolValue>("IncludeTerminals", "", new BoolValue(true)));

      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("Child", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>>("CrossoverParents", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossoverGeneticMaterialDepthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverGeneticMaterialLengthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverDepthPerGeneration", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossoverSymbolGeneticMaterialDepth", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverSymbolGeneticMaterialLength", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverSymbolDepth", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossoverAbsolutePerSymbol", ""));

      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("RemovedBranch", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("AddedBranch", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbol>("CutPointSymbol", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualRemovedMaterialDepth", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualRemovedMaterialLength", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualAddedMaterialDepth", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualAddedMaterialLength", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualCutPoint", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualCutPointParent", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualCutPointChild", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossoverActualCutPointExchange", ""));
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicExpressionTreeCrossoverTrackingAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      //first generation only
      if (CrossoverParentsParameter.ActualValue.Count() == 0) return base.Apply();

      if (CrossoverParentsParameter.ActualValue.Length != ChildParameter.ActualValue.Length) throw new ArgumentException("Number of children and crossover parents does not match. A reason might be that elitism was used.");

      var children = ChildParameter.ActualValue;
      var parents = CrossoverParentsParameter.ActualValue;

      List<Tuple<string, int, int, int>> values = new List<Tuple<string, int, int, int>>();
      for (int i = 0; i < CrossoverParentsParameter.ActualValue.Length; i++) {
        values.Add(CalculateGeneticMaterialChange(children[i], parents[i]));
      }

      CreateTables(values);

      RealChanges();

      return base.Apply();
    }

    private void RealChanges() {
      var cutPoint = CutPointSymbolParameter.ActualValue;
      var addedBranches = AddedBranchesParameter.ActualValue;
      var removedBranches = RemovedBranchesParameter.ActualValue;
      if (cutPoint == null) { return; }

      if (addedBranches != null) {
        CreatePerSymbolAndGenerationTable(cutPoint.Zip(addedBranches, (x, y) => new Tuple<string, double>(x.Name, y.Depth)), CrossoverActualAddedMaterialDepthParameter, "CrossoverActualAddedMaterialDepth");
        CreatePerSymbolAndGenerationTable(cutPoint.Zip(addedBranches, (x, y) => new Tuple<string, double>(x.Name, y.Length)), CrossoverActualAddedMaterialLengthParameter, "CrossoverActualAddedMaterialLength");
      }
      if (removedBranches != null) {
        CreatePerSymbolAndGenerationTable(cutPoint.Zip(removedBranches, (x, y) => new Tuple<string, double>(x.Name, y.Depth)), CrossoverActualRemovedMaterialDepthParameter, "CrossoverActualRemovedMaterialDepth");
        CreatePerSymbolAndGenerationTable(cutPoint.Zip(removedBranches, (x, y) => new Tuple<string, double>(x.Name, y.Length)), CrossoverActualRemovedMaterialLengthParameter, "CrossoverActualRemovedMaterialLength");
      }

      CreateAbsoluteCrossoverTable(cutPoint.Select(x => x.Name).Zip(addedBranches.Select(x => x.Root.Symbol.Name), (x, y) => x + "-" + y), CrossoverActualCutPointParameter, "Crossover Actual CutPoint");
      CreateAbsoluteCrossoverTable(cutPoint.Select(x => x.Name), CrossoverActualCutPointParentParameter, "Crossover Actual CutPoint Parent");
      CreateAbsoluteCrossoverTable(removedBranches.Select(x => x.Root.Symbol.Name), CrossoverActualCutPointChildParameter, "Crossover Actual CutPoint Child");
      CreateAbsoluteCrossoverTable(removedBranches.Zip(addedBranches, (x, y) => x.Root.Symbol.Name + "-" + y.Root.Symbol.Name), CrossoverActualCutPointExchangeParameter, "Crossover Actual Exchange");
    }

    private const string NOCHANGE = "No change";

    private void CreateTables(List<Tuple<string, int, int, int>> values) {
      var valuesWithChange = values.Where(x => !x.Item1.Equals(NOCHANGE)).DefaultIfEmpty(new Tuple<string, int, int, int>(NOCHANGE, 0, 0, 0));

      if (!IncludeTerminalsParameter.Value.Value) {
        var terminalSymbolNames = SymbolicExpressionGrammarParameter.ActualValue.Symbols.Where(x => x.MinimumArity > 0).Select(x => x.Name);
        values = values.Where(x => terminalSymbolNames.Contains(x.Item1) || NOCHANGE.Equals(x.Item1)).ToList();
        valuesWithChange = valuesWithChange.Where(x => terminalSymbolNames.Contains(x.Item1)).ToList();
      }

      CreatePerGenerationTable(CrossoverGeneticMaterialDepthPerGenerationParameter, valuesWithChange.Average(x => x.Item2), "Crossover genetic material depth");
      CreatePerGenerationTable(CrossoverGeneticMaterialLengthPerGenerationParameter, valuesWithChange.Average(x => x.Item3), "Crossover genetic material length");
      CreatePerGenerationTable(CrossoverDepthPerGenerationParameter, valuesWithChange.Average(x => x.Item4), "Crossover depth");

      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item2)), CrossoverSymbolGeneticMaterialDepthParameter, "Crossover per symbol genetic material depth");
      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item3)), CrossoverSymbolGeneticMaterialLengthParameter, "Crossover per symbol genetic material length");
      CreatePerSymbolAndGenerationTable(valuesWithChange.Select(x => new Tuple<string, double>(x.Item1, x.Item4)), CrossoverSymbolDepthParameter, "Crossover symbol depth");

      CreateAbsoluteCrossoverTable(values.Select(x => x.Item1), CrossoverAbsolutePerSymbolParameter, "Crossover per symbol");
    }

    private void CreateAbsoluteCrossoverTable(IEnumerable<string> values, ILookupParameter<DataTable> dataTableParameter, string title) {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable dataTable = dataTableParameter.ActualValue;

      if (dataTable == null) {
        dataTable = new DataTable(title, description);
        dataTable.VisualProperties.YAxisTitle = "Crossover per symbol";
        dataTable.VisualProperties.XAxisTitle = "Generation";

        dataTableParameter.ActualValue = dataTable;
        results.Add(new Result(title, dataTable));
      }

      // all rows must have the same number of values so we can just take the first
      int numberOfValues = dataTable.Rows.Select(r => r.Values.Count).DefaultIfEmpty().First();

      foreach (var pair in values.GroupBy(x => x, x => x)) {
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

    public static Tuple<string, int, int, int> CalculateGeneticMaterialChange(ISymbolicExpressionTree child, ItemArray<ISymbolicExpressionTree> parents) {
      Tuple<ISymbolicExpressionTreeNode, int> crossoverPoint = FindCrossoverPoint(child, parents[0]);
      if (crossoverPoint == null) return new Tuple<string, int, int, int>(NOCHANGE, 0, 0, 0);

      ISymbolicExpressionTreeNode crossoverPointNode = crossoverPoint.Item1;

      //may be needed to check difference to parent subtree, otherwise delete
      //int indexOfCrossoverPoint = child.Root.IterateNodesBreadth().ToList().IndexOf(crossoverPointNode);
      //var parentSubtree = parents[0].Root.IterateNodesBreadth().ToList()[indexOfCrossoverPoint];

      return new Tuple<string, int, int, int>(crossoverPointNode.Symbol.Name, crossoverPointNode.GetDepth(), crossoverPointNode.GetLength(), crossoverPoint.Item2);
    }

    private static Tuple<ISymbolicExpressionTreeNode, int> FindCrossoverPoint(ISymbolicExpressionTree child, ISymbolicExpressionTree parent) {
      ISymbolicExpressionTreeNode childNode = child.Root;
      List<ISymbolicExpressionTreeNode> nextChildNodes = new List<ISymbolicExpressionTreeNode>() { childNode };

      ISymbolicExpressionTreeNode parentNode = parent.Root;
      List<ISymbolicExpressionTreeNode> nextParentNodes = new List<ISymbolicExpressionTreeNode>() { parentNode };

      List<Tuple<ISymbolicExpressionTreeNode, int>> possibleCrossoverPoints = new List<Tuple<ISymbolicExpressionTreeNode, int>>();

      // compare trees
      FindCrossoverPoint(childNode, parentNode, 0, possibleCrossoverPoints);

      // if only one possibility, than that is it
      if (possibleCrossoverPoints.Count == 1) return possibleCrossoverPoints.First();
      // if no possibility, than no crossover toke place
      else if (possibleCrossoverPoints.Count == 0) return null;

      int minDepth = possibleCrossoverPoints.Select(x => x.Item2).Min();

      HashSet<ISymbolicExpressionTreeNode> crossoverPointsSet = new HashSet<ISymbolicExpressionTreeNode>();

      // go to the same depth
      foreach (var item in possibleCrossoverPoints) {
        ISymbolicExpressionTreeNode curNode = item.Item1;
        int curDepth = item.Item2;
        while (curDepth > minDepth) {
          curNode = curNode.Parent;
          curDepth -= 1;
        }
        crossoverPointsSet.Add(curNode);
      }

      // if only one possibility after reaching the same depth, than that is it
      if (crossoverPointsSet.Count == 1) return new Tuple<ISymbolicExpressionTreeNode, int>(crossoverPointsSet.First(), minDepth);


      while (crossoverPointsSet.Count > 1) {
        List<ISymbolicExpressionTreeNode> tempList = new List<ISymbolicExpressionTreeNode>(crossoverPointsSet);
        crossoverPointsSet.Clear();

        foreach (var node in tempList) {
          ISymbolicExpressionTreeNode curNode = node;
          curNode = curNode.Parent;
          crossoverPointsSet.Add(curNode);
        }

        --minDepth;
      }

      // if only one possibility after reaching the same depth, than that is it
      if (crossoverPointsSet.Count == 1) return new Tuple<ISymbolicExpressionTreeNode, int>(crossoverPointsSet.First(), minDepth);
      else throw new ArgumentException("Something went wrong. There has to be a crossover point at this line.");
    }

    private static void FindCrossoverPoint(ISymbolicExpressionTreeNode childNode, ISymbolicExpressionTreeNode parentNode, int curDepth, IList<Tuple<ISymbolicExpressionTreeNode, int>> possibleCrossoverPoints) {
      if (childNode.Symbol.Name != parentNode.Symbol.Name) {
        possibleCrossoverPoints.Add(new Tuple<ISymbolicExpressionTreeNode, int>(childNode, curDepth));
        return;
      }

      if (childNode.Subtrees != null) {
        for (int i = 0; i < childNode.SubtreeCount; i++) {
          FindCrossoverPoint(childNode.GetSubtree(i), parentNode.GetSubtree(i), curDepth + 1, possibleCrossoverPoints);
        }
      }
    }
  }
}

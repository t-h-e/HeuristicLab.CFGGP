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
    private const string SymbolFrequenciesParameterName = "SymbolFrequencies";
    private const string AggregateSymbolsWithDifferentSubtreeCountParameterName = "AggregateSymbolsWithDifferentSubtreeCount";

    #region parameter properties
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters[ResultsParameterName]; }
    }

    public IScopeTreeLookupParameter<ISymbolicExpressionTree> ChildParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters["Child"]; }
    }
    public IScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>> CrossoverParentsParameter {
      get { return (IScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>>)Parameters["CrossoverParents"]; }
    }

    public ILookupParameter<DataTable> CrossocerGeneticMaterialDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerGeneticMaterialDepthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> CrossocerGeneticMaterialLengthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerGeneticMaterialLengthPerGeneration"]; }
    }
    public ILookupParameter<DataTable> CrossocerDepthPerGenerationParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerDepthPerGeneration"]; }
    }


    public ILookupParameter<DataTable> CrossocerSymbolGeneticMaterialDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerSymbolGeneticMaterialDepth"]; }
    }
    public ILookupParameter<DataTable> CrossocerSymbolGeneticMaterialLengthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerSymbolGeneticMaterialLength"]; }
    }
    public ILookupParameter<DataTable> CrossocerSymbolDepthParameter {
      get { return (ILookupParameter<DataTable>)Parameters["CrossocerSymbolDepth"]; }
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

      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>("Child", ""));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<ISymbolicExpressionTree>>("CrossoverParents", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossocerGeneticMaterialDepthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossocerGeneticMaterialLengthPerGeneration", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossocerDepthPerGeneration", ""));

      Parameters.Add(new LookupParameter<DataTable>("CrossocerSymbolGeneticMaterialDepth", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossocerSymbolGeneticMaterialLength", ""));
      Parameters.Add(new LookupParameter<DataTable>("CrossocerSymbolDepth", ""));
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

      return base.Apply();
    }

    private void CreateTables(List<Tuple<string, int, int, int>> values) {
      CreatePerGenerationTable(CrossocerGeneticMaterialDepthPerGenerationParameter, values.Average(x => x.Item2), "Crossover genetic material depth");
      CreatePerGenerationTable(CrossocerGeneticMaterialLengthPerGenerationParameter, values.Average(x => x.Item3), "Crossover genetic material length");
      CreatePerGenerationTable(CrossocerDepthPerGenerationParameter, values.Average(x => x.Item4), "Crossover depth");

      CreatePerSymbolAndGenerationTable(values.Select(x => new Tuple<string, double>(x.Item1, x.Item2)), CrossocerSymbolGeneticMaterialDepthParameter, "Crossover per symbol genetic material depth");
      CreatePerSymbolAndGenerationTable(values.Select(x => new Tuple<string, double>(x.Item1, x.Item3)), CrossocerSymbolGeneticMaterialLengthParameter, "Crossover per symbol genetic material length");
      CreatePerSymbolAndGenerationTable(values.Select(x => new Tuple<string, double>(x.Item1, x.Item4)), CrossocerSymbolDepthParameter, "Crossover symbol depth");
    }

    private void CreatePerSymbolAndGenerationTable(IEnumerable<Tuple<string, double>> values, ILookupParameter<DataTable> dataTableParameter, string title, string description = "") {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable dataTable = dataTableParameter.ActualValue;

      if (dataTable == null) {
        dataTable = new DataTable(title, description);
        dataTable.VisualProperties.YAxisTitle = title;

        dataTableParameter.ActualValue = dataTable;
        results.Add(new Result(title, dataTable));
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
        dataTable.Rows[pair.Key].Values.Add(Math.Round(pair.Average(x => x.Item2), 3));
      }

      // add a zero for each data row that was not modified in the previous loop 
      foreach (var row in dataTable.Rows.Where(r => r.Values.Count != numberOfValues + 1))
        row.Values.Add(0.0);
    }

    private const string generationalRowKey = "generationalRowKey";
    private void CreatePerGenerationTable(ILookupParameter<DataTable> generationalDataTableParameter, double value, string title, string description = "") {
      ResultCollection results = ResultsParameter.ActualValue;
      DataTable generationalDataTable = generationalDataTableParameter.ActualValue;
      if (generationalDataTable == null) {
        generationalDataTable = new DataTable(title, description);
        generationalDataTable.VisualProperties.YAxisTitle = title;

        generationalDataTableParameter.ActualValue = generationalDataTable;
        results.Add(new Result(title, generationalDataTable));
      }

      if (!generationalDataTable.Rows.ContainsKey(generationalRowKey)) {
        // initialize a new row for the symbol and pad with zeros
        DataRow row = new DataRow(generationalRowKey, "");
        row.VisualProperties.StartIndexZero = true;
        generationalDataTable.Rows.Add(row);
      }
      generationalDataTable.Rows[generationalRowKey].Values.Add(Math.Round(value, 3));
    }

    public static Tuple<string, int, int, int> CalculateGeneticMaterialChange(ISymbolicExpressionTree child, ItemArray<ISymbolicExpressionTree> parents) {
      Tuple<ISymbolicExpressionTreeNode, int> crossoverPoint = FindCrossoverPoint(child, parents[0]);
      if (crossoverPoint == null) return new Tuple<string, int, int, int>("No change", 0, 0, 0);

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

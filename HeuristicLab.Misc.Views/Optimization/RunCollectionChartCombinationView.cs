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
using System.ComponentModel;
using System.Linq;
using HeuristicLab.Analysis;
using HeuristicLab.Collections;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;
using HeuristicLab.MainForm.WindowsForms;
using HeuristicLab.Optimization;

namespace HeuristicLab.Misc.Views {
  [View("Chart Combination")]
  [Content(typeof(RunCollection), false)]
  public partial class RunCollectionChartCombinationView : ItemView {

    public new RunCollection Content {
      get { return (RunCollection)base.Content; }
      set { base.Content = value; }
    }

    private bool suppressUpdates;
    private readonly DataTable combinedDataTable;
    public DataTable CombinedDataTable {
      get { return combinedDataTable; }
    }

    public RunCollectionChartCombinationView() {
      InitializeComponent();
      combinedDataTable = new DataTable("Combined DataTable", "A data table containing data rows from multiple runs.");
      viewHost.Content = combinedDataTable;
      suppressUpdates = false;
    }

    #region Content events
    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.ItemsAdded += Content_ItemsAdded;
      Content.ItemsRemoved += Content_ItemsRemoved;
      Content.CollectionReset += Content_CollectionReset;
      Content.UpdateOfRunsInProgressChanged += Content_UpdateOfRunsInProgressChanged;
      Content.OptimizerNameChanged += Content_AlgorithmNameChanged;
    }
    protected override void DeregisterContentEvents() {
      Content.ItemsAdded -= Content_ItemsAdded;
      Content.ItemsRemoved -= Content_ItemsRemoved;
      Content.CollectionReset -= Content_CollectionReset;
      Content.UpdateOfRunsInProgressChanged -= Content_UpdateOfRunsInProgressChanged;
      Content.OptimizerNameChanged -= Content_AlgorithmNameChanged;
      base.DeregisterContentEvents();
    }

    private void Content_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      if (suppressUpdates) return;
      if (InvokeRequired) {
        Invoke(new CollectionItemsChangedEventHandler<IRun>(Content_ItemsAdded), sender, e);
        return;
      }
      UpdateDataTableComboBox();
      AddRuns(e.Items);
    }
    private void Content_ItemsRemoved(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      if (suppressUpdates) return;
      if (InvokeRequired) {
        Invoke(new CollectionItemsChangedEventHandler<IRun>(Content_ItemsRemoved), sender, e);
        return;
      }
      UpdateDataTableComboBox();
      RemoveRuns(e.Items);
    }
    private void Content_CollectionReset(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      if (suppressUpdates) return;
      if (InvokeRequired) {
        Invoke(new CollectionItemsChangedEventHandler<IRun>(Content_CollectionReset), sender, e);
        return;
      }
      UpdateDataTableComboBox();
      RemoveRuns(e.OldItems);
      AddRuns(e.Items);
    }
    private void Content_AlgorithmNameChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Content_AlgorithmNameChanged), sender, e);
      else UpdateCaption();
    }
    private void Content_UpdateOfRunsInProgressChanged(object sender, EventArgs e) {
      if (InvokeRequired) {
        Invoke(new EventHandler(Content_UpdateOfRunsInProgressChanged), sender, e);
        return;
      }
      suppressUpdates = Content.UpdateOfRunsInProgress;
      if (!suppressUpdates) {
        UpdateDataTableComboBox();
        UpdateDataTable(Content);
      }
    }

    private void RegisterRunEvents(IRun run) {
      run.PropertyChanged += run_PropertyChanged;
    }
    private void DeregisterRunEvents(IRun run) {
      run.PropertyChanged -= run_PropertyChanged;
    }
    private void run_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (suppressUpdates) return;
      if (InvokeRequired) {
        Invoke((Action<object, PropertyChangedEventArgs>)run_PropertyChanged, sender, e);
      } else {
        var run = (IRun)sender;
        if (e.PropertyName == "Color" || e.PropertyName == "Visible")
          UpdateDataTable(Content);
      }
    }
    #endregion

    protected override void OnContentChanged() {
      base.OnContentChanged();
      dataTableComboBox.Items.Clear();
      combinedDataTable.Rows.Clear();

      UpdateCaption();
      if (Content != null) {
        UpdateDataTableComboBox();
      }
    }

    private void RebuildCombinedDataTable() {
      RemoveRuns(Content);
      AddRuns(Content);
      UpdateDataTable(Content);
    }

    private void UpdateDataTable(IEnumerable<IRun> runs) {
      combinedDataTable.Rows.Clear();

      var visibleRuns = runs.Where(r => r.Visible);

      var resultName = (string)dataTableComboBox.SelectedItem;
      if (string.IsNullOrEmpty(resultName)) return;

      var dataTables = visibleRuns.Where(r => r.Results.ContainsKey(resultName)).Select(r => (DataTable)r.Results[resultName]);
      if (dataTables.Count() != visibleRuns.Count()) {
        errorTextBox.Text = String.Format("One or more runs do not contain a data table {0}", resultName);
        viewHost.Visible = false;
        errorTextBox.Visible = true;
        return;
      }

      var dataRows = dataTables.SelectMany(dt => dt.Rows).GroupBy(r => r.Name, r => r);

      int tableCount = dataTables.Count();
      foreach (var row in dataRows) {
        var aggregateRows = row.Select(r => (IEnumerable<double>)r.Values).ToList();
        // check if all rows have the same length
        if (row.Any(r => r.Values.Count != row.First().Values.Count)) {
          errorTextBox.Text = String.Format("One or more runs do not contain the same number of entries per row {0}", resultName);
          viewHost.Visible = false;
          errorTextBox.Visible = true;
          return;
        }

        // add zero rows for missing rows, otherwise the aggragation is off
        if (row.Count() < tableCount) {
          var zeroRows = Enumerable.Repeat(Enumerable.Repeat(0.0, row.First().Values.Count), tableCount - row.Count());
          aggregateRows.AddRange(zeroRows);
        }

        foreach (var item in AggregateRows(row.Key, aggregateRows)) {
          // Windows Forms calculates internally with Decimal instead of Double, which can lead to Overflow exceptions
          // To avoid this exception, values get replaced with +/-7.92E+27 as max and min value
          var helper = item.Item3.Select(x => Math.Max(Math.Min(x, 7.92E+27), -7.92E+27));
          DataRow combinedRows = new DataRow(item.Item1, item.Item2, helper);
          combinedDataTable.Rows.Add(combinedRows);
        }
      }
      viewHost.Visible = true;
      errorTextBox.Visible = false;
      combinedDataTable.Name = String.Format("Combined {0}", resultName);
    }

    protected virtual List<Tuple<string, string, IEnumerable<double>>> AggregateRows(string rowKey, IEnumerable<IEnumerable<double>> aggreateRows) {
      // do nothing here
      return new List<Tuple<string, string, IEnumerable<double>>>();
    }

    private void AddRuns(IEnumerable<IRun> runs) {
      foreach (var run in runs) {
        RegisterRunEvents(run);
      }
    }

    private void RemoveRuns(IEnumerable<IRun> runs) {
      foreach (var run in runs) {
        DeregisterRunEvents(run);
      }
    }

    private void UpdateDataTableComboBox() {
      string selectedItem = (string)dataTableComboBox.SelectedItem;

      dataTableComboBox.Items.Clear();
      var dataTables = (from run in Content
                        from result in run.Results
                        where result.Value is DataTable
                        select result.Key).Distinct().ToArray();

      dataTableComboBox.Items.AddRange(dataTables);
      if (selectedItem != null && dataTableComboBox.Items.Contains(selectedItem)) {
        dataTableComboBox.SelectedItem = selectedItem;
      } else if (dataTableComboBox.Items.Count > 0) {
        dataTableComboBox.SelectedItem = dataTableComboBox.Items[0];
      }
    }

    protected virtual void UpdateCaption() {
      Caption = Content != null ? Content.OptimizerName + " Chart Combination" : ViewAttribute.GetViewName(GetType());
    }

    private void dataTableComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      if (suppressUpdates) return;
      RebuildCombinedDataTable();
    }
  }
}

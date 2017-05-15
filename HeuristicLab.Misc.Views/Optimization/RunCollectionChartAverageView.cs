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
using HeuristicLab.MainForm;
using HeuristicLab.MainForm.WindowsForms;
using HeuristicLab.Optimization;

namespace HeuristicLab.Misc.Views {
  [View("Chart Average")]
  [Content(typeof(RunCollection), false)]
  public partial class RunCollectionChartAverageView : RunCollectionChartCombinationView {
    public RunCollectionChartAverageView() {
      InitializeComponent();
    }

    protected override List<Tuple<string, string, IEnumerable<double>>> AggregateRows(string rowKey, IEnumerable<IEnumerable<double>> aggreateRows) {
      var averageValues = DataRowsAggregate(Enumerable.Average, aggreateRows);
      return new List<Tuple<string, string, IEnumerable<double>>>() { new Tuple<string, string, IEnumerable<double>>(rowKey, "Average of Values", averageValues) };

    }

    private IEnumerable<double> DataRowsAggregate(Func<IEnumerable<double>, double> aggregate, IEnumerable<IEnumerable<double>> arrays) {
      return Enumerable.Range(0, arrays.First().Count())
        .Select(i => aggregate(arrays.Select(a => a.Skip(i).First()).Select(x => Double.IsNaN(x) ? 0 : x)));
    }

    protected override void UpdateCaption() {
      Caption = Content != null ? Content.OptimizerName + " Chart Average" : ViewAttribute.GetViewName(GetType());
    }
  }
}

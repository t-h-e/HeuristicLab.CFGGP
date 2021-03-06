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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using HeuristicLab.Selection;

namespace HeuristicLab.Misc {
  /// <summary>
  /// A lexicase selection operator which considers all successful evaluated training cases for selection.
  /// 
  /// ToDo: LexicaseSelector and ICaseSingleObjectiveSelector are ISingleObjectiveOperator, which contains Maximization and Qualities which is not needed
  /// </summary>
  [Item("LexicaseSelector", "A lexicase selection operator which considers all successful evaluated training cases for selection.")]
  [StorableClass]
  public sealed class LexicaseSelector : StochasticSingleObjectiveSelector, ICaseSingleObjectiveSelector {
    public ILookupParameter<ItemArray<DoubleArray>> CaseQualitiesParameter {
      get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["CaseQualities"]; }
    }

    [StorableConstructor]
    private LexicaseSelector(bool deserializing) : base(deserializing) { }
    private LexicaseSelector(LexicaseSelector original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new LexicaseSelector(this, cloner);
    }

    public LexicaseSelector()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual."));
    }

    protected override IScope[] Select(List<IScope> scopes) {
      int count = NumberOfSelectedSubScopesParameter.ActualValue.Value;
      bool copy = CopySelectedParameter.Value.Value;
      IRandom random = RandomParameter.ActualValue;
      bool maximization = MaximizationParameter.ActualValue.Value;
      List<double> qualities = QualityParameter.ActualValue.Where(x => IsValidQuality(x.Value)).Select(x => x.Value).ToList();
      List<DoubleArray> caseQualities = CaseQualitiesParameter.ActualValue.ToList();

      // remove scopes, qualities and case qualities, if the case qualities are empty
      var removeindices = Enumerable.Range(0, caseQualities.Count)
                                    .Zip(caseQualities, (i, c) => new { Index = i, CaseQuality = c })
                                    .Where(c => c.CaseQuality.Count() == 0)
                                    .Select(c => c.Index)
                                    .Reverse();
      foreach (var i in removeindices) {
        scopes.RemoveAt(i);
        qualities.RemoveAt(i);
        caseQualities.RemoveAt(i);
      }

      if (caseQualities.Any(x => x.Count() != caseQualities[0].Length)) { throw new ArgumentException("Not all case qualities have the same length"); }

      IScope[] selected = new IScope[count];

      var groupedCaseQualities = GroupEqualCaseQualities(caseQualities);

      for (int i = 0; i < count; i++) {
        int index = LexicaseSelect(groupedCaseQualities, RandomParameter.ActualValue, maximization);

        if (copy)
          selected[i] = (IScope)scopes[index].Clone();
        else {
          selected[i] = scopes[index];
          scopes.RemoveAt(index);
          qualities.RemoveAt(index);
          caseQualities.RemoveAt(index);
        }
      }
      return selected;
    }

    private int LexicaseSelect(List<Tuple<DoubleArray, List<int>>> groupedCaseQualities, IRandom random, bool maximization) {
      IList<int> candidates = Enumerable.Range(0, groupedCaseQualities.Count()).ToList();
      IEnumerable<int> order = Enumerable.Range(0, groupedCaseQualities.First().Item1.Count()).Shuffle(random);

      foreach (int curCase in order) {
        List<int> nextCandidates = new List<int>();
        double best = maximization ? double.NegativeInfinity : double.PositiveInfinity;
        foreach (int candidate in candidates) {
          if (groupedCaseQualities[candidate].Item1[curCase].IsAlmost(best)) {
            // if the individuals is as good as the best one, add it
            nextCandidates.Add(candidate);
          } else if (((maximization) && (groupedCaseQualities[candidate].Item1[curCase] > best)) ||
             ((!maximization) && (groupedCaseQualities[candidate].Item1[curCase] < best))) {
            // if the individuals is better than the best one, remove all previous candidates and add the new one
            nextCandidates.Clear();
            nextCandidates.Add(candidate);
            // also set the nes best quality value
            best = groupedCaseQualities[candidate].Item1[curCase];
          }
          // else {do nothing}
        }

        if (nextCandidates.Count == 1) {
          return groupedCaseQualities[nextCandidates.First()].Item2.SampleRandom(random);
        } else if (nextCandidates.Count < 1) {
          return groupedCaseQualities[candidates.SampleRandom(random)].Item2.SampleRandom(random);
        }
        candidates = nextCandidates;
      }

      if (candidates.Count == 1) {
        return groupedCaseQualities[candidates.First()].Item2.SampleRandom(random);
      }
      return groupedCaseQualities[candidates.SampleRandom(random)].Item2.SampleRandom(random);
    }

    private List<Tuple<DoubleArray, List<int>>> GroupEqualCaseQualities(List<DoubleArray> caseQualities) {
      List<Tuple<DoubleArray, List<int>>> groupedCaseQualities = new List<Tuple<DoubleArray, List<int>>>();
      List<int> indices = Enumerable.Range(0, caseQualities.Count()).ToList();
      while (indices.Count > 0) {
        var curI = indices.First();
        indices.Remove(curI);
        var cur = caseQualities[curI];
        List<int> group = new List<int>();

        foreach (var i in indices) {
          if (CompareDoubleArray(cur, caseQualities[i])) {
            group.Add(i);
          }
        }
        foreach (var i in group) {
          indices.Remove(i);
        }

        group.Add(curI);
        groupedCaseQualities.Add(new Tuple<DoubleArray, List<int>>(cur, group));
      }

      return groupedCaseQualities;
    }

    private bool CompareDoubleArray(DoubleArray d1, DoubleArray d2) {
      for (int i = 0; i < d1.Length; i++) {
        if (d1[i] != d2[i]) {
          return false;
        }
      }
      return true;
    }
  }
}

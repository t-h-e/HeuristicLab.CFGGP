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

using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using HeuristicLab.Selection;

namespace HeuristicLab.Problems.CFG {
  /// <summary>
  /// A lexicase selection operator which considers all successful evaluated training cases for selection.
  /// 
  /// ToDo: LexicaseSelector and ICaseSingleObjectiveSelector are ISingleObjectiveOperator, which contains Maximization and Qualities which is not needed
  /// </summary>
  [Item("LexicaseSelector", "A lexicase selection operator which considers all successful evaluated training cases for selection.")]
  [StorableClass]
  public sealed class LexicaseSelector : StochasticSingleObjectiveSelector, ICaseSingleObjectiveSelector {
    public ILookupParameter<ItemArray<BoolArray>> CasesParameter {
      get { return (ILookupParameter<ItemArray<BoolArray>>)Parameters["Cases"]; }
    }

    [StorableConstructor]
    private LexicaseSelector(bool deserializing) : base(deserializing) { }
    private LexicaseSelector(LexicaseSelector original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new LexicaseSelector(this, cloner);
    }

    public LexicaseSelector()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<BoolArray>("Cases", "The successful evaluated cases."));
    }

    protected override IScope[] Select(List<IScope> scopes) {
      int count = NumberOfSelectedSubScopesParameter.ActualValue.Value;
      bool copy = CopySelectedParameter.Value.Value;
      IRandom random = RandomParameter.ActualValue;
      List<double> qualities = QualityParameter.ActualValue.Where(x => IsValidQuality(x.Value)).Select(x => x.Value).ToList();
      ItemArray<BoolArray> cases = CasesParameter.ActualValue;

      IScope[] selected = new IScope[count];

      for (int i = 0; i < count; i++) {
        int index = LexicaseSelect(cases, RandomParameter.ActualValue);

        if (copy)
          selected[i] = (IScope)scopes[index].Clone();
        else {
          selected[i] = scopes[index];
          scopes.RemoveAt(index);
          qualities.RemoveAt(index);
        }
      }
      return selected;
    }

    private int LexicaseSelect(ItemArray<BoolArray> cases, IRandom random) {

      IList<int> candidates = Enumerable.Range(0, cases.Count()).ToList();
      IEnumerable<int> order = Enumerable.Range(0, cases[0].Count()).Shuffle(random);

      foreach (int curCase in order) {
        List<int> nextCandidates = new List<int>();
        foreach (int candidate in candidates) {
          if (cases[candidate][curCase] == true) {
            nextCandidates.Add(candidate);
          }
        }

        if (nextCandidates.Count == 1) {
          return nextCandidates.First();
        } else if (nextCandidates.Count < 1) {
          return candidates.SampleRandom(random);
        }
        candidates = nextCandidates;
      }


      if (candidates.Count == 1) {
        return candidates.First();
      }
      return candidates.SampleRandom(random);
    }
  }
}

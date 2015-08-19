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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class SmallOrLarge : CFGArtificialDataDescriptor {
    public override string Name { get { return "Small Or Large"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "SmallOrLarge"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      //hardcoded train
      var x0 = new List<int>(1100) { -10000, 0, 980, 1020, 1980, 2020, 10000 };
      x0.AddRange(Enumerable.Range(995, 10));
      x0.AddRange(Enumerable.Range(1995, 10));
      //random train
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(73, -10000, 10000).ToList());

      x0 = ValueGenerator.Shuffle(x0).ToList();

      //hardcoded test
      x0.AddRange(Enumerable.Range(980, 40));
      x0.AddRange(Enumerable.Range(1980, 40));
      //random test
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(920, -10000, 10000).ToList());

      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => x < 1000 ? "\"small\"" : x >= 2000 ? "\"large\"" : "\"\"").ToArray();
      return new Tuple<string[], string[]>(input, output);
    }
  }
}

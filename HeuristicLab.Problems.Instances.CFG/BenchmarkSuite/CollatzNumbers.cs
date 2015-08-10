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

namespace HeuristicLab.Problems.Instances.CFG {
  public class CollatzNumbers : CFGArtificialDataDescriptor {
    public override string Name { get { return "Collatz Numbers"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "CollatzNumbers"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      var x0 = new List<int>(TrainingPartitionEnd) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 6171, 6943, 7963, 9257, 9999, 10000 };
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(184, 1, 10000).ToList());
      x0 = ValueGenerator.Shuffle(x0).ToList();

      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(2000, 1, 10000));

      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => CalcCollatz(x).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private int CalcCollatz(int n) {
      if (n == 1) return 1;
      else if (n % 2 == 0) return CalcCollatz(n / 2) + 1;
      else return CalcCollatz(3 * n + 1) + 1;
    }
  }
}

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
  public class EvenSquares : CFGArtificialDataDescriptor {
    public override string Name { get { return "Even Squares"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "EvenSquares"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      var x0 = new List<int>() { 1, 2, 3, 4, 5, 6, 15, 16, 17, 18, 36, 37, 64, 65, 9600, 9700, 9999 };
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(83, 1, 9999).ToList());
      x0 = ValueGenerator.Shuffle(x0).ToList();

      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(1000, 1, 9999).ToList());

      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => CalcEvenSquares(x).ToString()).ToArray();
      return new Tuple<string[], string[]>(new string[0], new string[0]);
    }

    private int CalcEvenSquares(int n) {
      int floor = (int)Math.Floor(Math.Sqrt(n));
      int count = floor / 2 + 1; // + 1 to add the even square 0
      count -= floor * floor >= n ? 1 : 0;
      return count;
    }
  }
}

﻿#region License Information
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
  public class SumOfSquares : CFGArtificialDataDescriptor {
    public override string Name { get { return "Sum of Squares"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "SumOfSquares"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 50; } }
    protected override int TestPartitionStart { get { return 50; } }
    protected override int TestPartitionEnd { get { return 100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      var x0 = new List<int>() { 1, 2, 3, 4, 5, 100 };
      var numbers = Enumerable.Range(6, 94);
      x0.AddRange(ValueGenerator.SampleRandomWithoutRepetition(numbers, 44).ToList());
      x0 = ValueGenerator.Shuffle(x0).ToList();

      x0.AddRange(numbers.Except(x0));
      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => CalcSumOfSquares(x).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private int CalcSumOfSquares(int n) {
      return Enumerable.Range(1, n).Select(x => x * x).Sum();
    }
  }
}

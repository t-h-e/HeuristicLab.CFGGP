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
using System.Text;

namespace HeuristicLab.Problems.Instances.CFG {
  public class EvenSquares : BenchmarkSuiteDataDescritpor<int> {
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

    protected override IEnumerable<int> GenerateTraining() {
      var x0 = new List<int>() { 1, 2, 3, 4, 5, 6, 15, 16, 17, 18, 36, 37, 64, 65, 9600, 9700, 9999 };
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(83, 1, 9999, rand).ToList());
      return x0;
    }

    protected override IEnumerable<int> GenerateTest() {
      return ValueGenerator.GenerateUniformDistributedValues(1000, 1, 9999, rand);
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<int> x0) {
      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => CalcEvenSquares(x).PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcEvenSquares(int n) {
      if (n <= 4) return String.Empty;

      StringBuilder strBuilder = new StringBuilder();
      for (int i = 2; i * i < n; i += 2) {
        strBuilder.Append((i * i).ToString());
        strBuilder.Append("\n");
      }
      strBuilder.Length--;
      return strBuilder.ToString();
    }
  }
}

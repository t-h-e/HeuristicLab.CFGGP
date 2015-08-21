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
using HeuristicLab.Core;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class ForLoopIndex : BenchmarkSuiteDataDescritpor<Tuple<int, int, int>> {
    public override string Name { get { return "For Loop Index"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "ForLoopIndex"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<Tuple<int, int, int>> GenerateTraining() {
      var loop = CreateCasesAroundZero(10).ToList();
      loop.AddRange(CreateCasesEverywhere(90));
      return loop;
    }

    protected override IEnumerable<Tuple<int, int, int>> GenerateTest() {
      var loop = CreateCasesAroundZero(100).ToList();
      loop.AddRange(CreateCasesEverywhere(900));
      return loop;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<Tuple<int, int, int>> loop) {
      var input = loop.Select(x => String.Format("{0}, {1}, {2}", x.Item1, x.Item2, x.Item3)).ToArray();
      List<string> output = new List<string>(input.Length);
      foreach (var item in loop) {
        output.Add(String.Join("\n", Enumerable.Range(item.Item1, (item.Item2 - item.Item1) / item.Item3)).PrepareStringForPython());
      }

      return new Tuple<string[], string[]>(input, output.ToArray());
    }

    private IEnumerable<Tuple<int, int, int>> CreateCasesAroundZero(int n) {
      int start, step, end;
      for (int i = 0; i < n; i++) {
        step = rand.Next(1, 10);
        start = rand.Next(-(step * 20) + 1, -1);
        end = rand.Next(1, start + (20 * step));
        yield return new Tuple<int, int, int>(start, end, step);
      }
    }

    private IEnumerable<Tuple<int, int, int>> CreateCasesEverywhere(int n) {
      int start, step, end;
      for (int i = 0; i < n; i++) {
        step = rand.Next(1, 10);
        start = rand.Next(-500, 500 - (20 * step));
        end = rand.Next(start + 1, start + (20 * step));
        yield return new Tuple<int, int, int>(start, end, step);
      }
    }
  }
}

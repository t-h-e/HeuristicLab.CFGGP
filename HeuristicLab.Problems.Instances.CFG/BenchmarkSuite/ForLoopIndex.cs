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
  public class ForLoopIndex : CFGArtificialDataDescriptor {
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

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      var start = new List<int>(TestPartitionEnd);
      var end = new List<int>(TestPartitionEnd);
      var step = ValueGenerator.GenerateUniformDistributedValues(TestPartitionEnd, 1, 10).ToList();

      createCasesAroundZero(start, end, step, 0, 10, rand);
      createCasesEverywhere(start, end, step, 10, 100, rand);
      createCasesAroundZero(start, end, step, 100, 2000, rand);
      createCasesEverywhere(start, end, step, 200, 1100, rand);

      var input = step.Zip(start.Zip(end, (first, second) => String.Format("{0}, {1}", first, second)), (third, firstSecond) => String.Format("{0}, {1}", firstSecond, third)).ToArray();
      string[] output = new string[TestPartitionEnd];
      for (int i = 0; i < TestPartitionEnd; i++) {
        output[i] = String.Join(", ", Enumerable.Range(start[i], (end[i] - start[i]) / step[i]));
      }

      return new Tuple<string[], string[]>(input, output);
    }

    private void createCasesAroundZero(List<int> start, List<int> end, List<int> step, int from, int to, IRandom rand) {
      int curStart, curStep, curEnd;
      for (int i = 0; i < 10; i++) {
        curStep = step[i];
        curStart = rand.Next(-(curStep * 20) + 1, -1);
        curEnd = rand.Next(1, curStart + (20 * curStep));
        start.Add(curStart);
        end.Add(curEnd);
      }
    }

    private void createCasesEverywhere(List<int> start, List<int> end, List<int> step, int from, int to, IRandom rand) {
      int curStart, curStep, curEnd;
      for (int i = 10; i < 100; i++) {
        curStep = step[i];
        curStart = rand.Next(-500, 500 - (20 * curStep));
        curEnd = rand.Next(curStart + 1, curStart + (20 * curStep));
        start.Add(curStart);
        end.Add(curEnd);
      }
    }
  }
}

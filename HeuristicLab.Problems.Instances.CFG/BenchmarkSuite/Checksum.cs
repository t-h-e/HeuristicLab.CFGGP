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
using System.Linq;
using System.Collections.Generic;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Checksum : CFGArtificialDataDescriptor {
    public override string Name { get { return "Checksum"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Checksum"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(StringValueGenerator.GetRandomStrings(88, 0, 50, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(StringValueGenerator.GetRandomStrings(1000, 0, 50, rand).ToList());

      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => String.Format("Check sum is {0}.", CalcChecksum(x).ToString().PythonEscape())).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private char CalcChecksum(string x) {
      int sum = 0;
      foreach (int item in x.ToCharArray()) {
        sum += item;
      }
      return (char)(sum % 64);
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() {
        "", "A", "\t", "\n", "B\n", "\n\n",
          String.Concat(Enumerable.Repeat('\n', 50)),
          String.Concat(Enumerable.Repeat(' ', 50)),
          String.Concat(Enumerable.Repeat('s', 50)),
          String.Concat(Enumerable.Repeat("CD\n", 16)) + "CD",
          String.Concat(Enumerable.Repeat("x\ny ", 12)) + "x\n",
          String.Concat(Enumerable.Repeat(" \n", 25)),
      };
    }
  }
}

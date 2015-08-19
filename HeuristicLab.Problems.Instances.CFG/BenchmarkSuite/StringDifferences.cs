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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class StringDifferences : CFGArtificialDataDescriptor {
    public override string Name { get { return "String Differences"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "StringDifferences"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<string>> strings = GetHardcodedTrainingSamples();
      strings.AddRange(StringValueGenerator.GetRandomStringsWithoutSpaces(170, 2, 10, rand).Zip(StringValueGenerator.GetRandomStringsWithoutSpaces(170, 2, 10, rand),
                       (x, y) => new List<string>(2) { x, y }).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(StringValueGenerator.GetRandomStringsWithoutSpaces(2000, 0, 10, rand).Zip(StringValueGenerator.GetRandomStringsWithoutSpaces(2000, 0, 10, rand),
                       (x, y) => new List<string>(2) { x, y }).ToList());

      var input = strings.Select(x => String.Join(", ", x.Select(y => y.PrepareStringForPython()))).ToArray();
      var output = strings.Select(x => CalcStringDifferences(x[0].ToCharArray(), x[1].ToCharArray()).PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcStringDifferences(char[] p1, char[] p2) {
      int length = Math.Min(p1.Length, p2.Length);
      StringBuilder strBuilder = new StringBuilder();
      for (int i = 0; i < length; i++) {
        if (p1[i] != p2[i]) {
          strBuilder.Append(String.Format("{0} {1} {2}\n", i, p1[i], p2[i]));
        }
      }
      return strBuilder.ToString();
    }



    private List<List<string>> GetHardcodedTrainingSamples() {
      return new List<List<string>>() {
        new List<string>() { "", "" },
        new List<string>() { "", "hi" },
        new List<string>() { "ThereWorld", "" },
        new List<string>() { "A", "A" },
        new List<string>() { "B", "C" },
        new List<string>() { "&", "#" },
        new List<string>() { "4", "456789" },
        new List<string>() { "rat", "hat" },
        new List<string>() { "new", "net" },
        new List<string>() { "big", "bag" },
        new List<string>() { "STOP", "SIGN" },
        new List<string>() { "abcde", "a" },
        new List<string>() { "abcde", "abcde" },
        new List<string>() { "abcde", "edcba" },
        new List<string>() { "2n", "nn" },
        new List<string>() { "hi", "zipper" },
        new List<string>() { "dealer", "dollars" },
        new List<string>() { "nacho", "cheese" },
        new List<string>() { "loud", "louder" },
        new List<string>() { "qwertyuiop", "asdfghjkl;" },
        new List<string>() { "LALALALALA", "LLLLLLLLLL" },
        new List<string>() { "!!!!!!", ".?." },
        new List<string>() { "9r2334", "9223d4r" },
        new List<string>() { "WellWell", "wellwell" },
        new List<string>() { "TakeThat!", "TAKETHAT!!" },
        new List<string>() { "CHOCOLATE^", "CHOCOLATE^" },
        new List<string>() { "ssssssssss", "~~~~~~~~~~" },
        new List<string>() { ">_=]>_=]>_", "q_q_q_q_q_" },
        new List<string>() { "()()()()()", "pp)pp)pp)p" },
        new List<string>() { "HaHaHaHaHa", "HiHiHiHiHi" },
      };
    }
  }
}

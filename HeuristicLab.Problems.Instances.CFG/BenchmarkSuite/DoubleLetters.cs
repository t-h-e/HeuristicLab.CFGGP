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
using System.Text;

namespace HeuristicLab.Problems.Instances.CFG {
  public class DoubleLetters : CFGArtificialDataDescriptor {
    public override string Name { get { return "Double Letters"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "DoubleLetters"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(StringValueGenerator.GetRandomStrings(68, 0, 20, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(StringValueGenerator.GetRandomStrings(1000, 0, 20, rand).ToList());

      var input = strings.Select(y => y.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => CalcDoubleLetter(x).PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcDoubleLetter(string x) {
      StringBuilder strBuilder = new StringBuilder();
      foreach (var cur in x.ToCharArray()) {
        if (cur == 33) {
          strBuilder.Append(Enumerable.Repeat(cur, 3).ToArray());
        } else if ((cur >= 65 && cur <= 90) || (cur >= 97 && cur <= 122)) {
          strBuilder.Append(Enumerable.Repeat(cur, 2).ToArray());
        } else {
          strBuilder.Append(cur);
        }
      }
      return strBuilder.ToString();
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() { "", "A", "!", " ", "*", "\t", "\n", "B\n", "\n\n", "CD",
        "ef", "!!", "q!", "!R", "!#", "@!", "!F!", "T$L", "4ps",
        "q\t ", "!!!", "i:!i:!i:!i:!i", "88888888888888888888",
        "                    ", "ssssssssssssssssssss",
        "!!!!!!!!!!!!!!!!!!!!", "Ha Ha Ha Ha Ha Ha Ha",
        "x\ny!x\ny!x\ny!x\ny!x\ny!", "1!1!1!1!1!1!1!1!1!1!",
        "G5G5G5G5G5G5G5G5G5G5", ">_=]>_=]>_=]>_=]>_=]",
        "k!!k!!k!!k!!k!!k!!k!"
      };
    }
  }
}

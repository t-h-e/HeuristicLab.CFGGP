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
  public class PigLatin : CFGArtificialDataDescriptor {
    public override string Name { get { return "Pig Latin"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "PigLatin"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 1200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<string> strings = GetHardcodedTrainingSamples();

      strings.AddRange(GetLetterStrings(167, rand));

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(GetLetterStrings(1000, rand));

      var input = strings.Select(x => String.Join(", ", x)).ToArray();
      var output = strings.Select(x => CalcPigLatin(x)).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<string> GetLetterStrings(int p, FastRandom rand) {
      throw new NotImplementedException();
    }

    private string CalcPigLatin(string x) {
      throw new NotImplementedException();
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() {
        "", "a", "b", "c", "d", "e", "i", "m", "o", "u", "y", "z",
        "hello", "there", "world", "eat", "apple", "yellow",
        "orange", "umbrella", "ouch", "in", "hello there world",
        "out at the plate", "nap time on planets",
        "supercalifragilistic", "expialidocious",
        "uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu",
        "ssssssssssssssssssssssssssssssssssssssssssssssssss",
        "w w w w w w w w w w w w w w w w w w w w w w w w w",
        "e e e e e e e e e e e e e e e e e e e e e e e e e",
        "ha ha ha ha ha ha ha ha ha ha ha ha ha ha ha ha ha",
        "x y x y x y x y x y x y x y x y x y x y x y x y x"
      };
    }
  }
}

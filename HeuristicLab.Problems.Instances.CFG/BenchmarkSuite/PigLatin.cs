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

      strings.AddRange(GetLetterStrings(167, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(GetLetterStrings(1000, rand).ToList());

      var input = strings.Select(x => String.Join(", ", x)).ToArray();
      var output = strings.Select(x => CalcPigLatin(x)).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<string> GetLetterStrings(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        var value = StringValueGenerator.GetRandomLowerCaseString(rand.Next(0, 50), rand).ToCharArray();
        for (int j = 0; j < value.Length; j++) {  // randomly add spaces with 20% probability at each position
          if (rand.NextDouble() < 0.2) value[i] = ' ';
        }
        string valueString = new String(value);
        yield return String.Join(" ", valueString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));  // remove double spaces and spaces in the beginnig and the end of the string
      }
    }

    private string CalcPigLatin(string x) {
      var split = x.Split(new char[] { ' ' }, StringSplitOptions.None);
      StringBuilder strBuilder = new StringBuilder();
      foreach (var word in split) {
        if (!StringValueGenerator.vowel.Contains(word.ElementAt(0))) {
          strBuilder.Append(word.Substring(1, word.Length - 1));
          strBuilder.Append(word.ElementAt(0));
        }
        strBuilder.Append("ay ");
      }
      strBuilder.Length--;
      return strBuilder.ToString();
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

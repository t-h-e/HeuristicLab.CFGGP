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
  public class Syllables : CFGArtificialDataDescriptor {
    public override string Name { get { return "Syllables"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Syllables"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetStringsWithSomeVowels(83, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(GetStringsWithSomeVowels(1000, rand).ToList());

      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => String.Format("The number of syllables is {0}.", x.Count(y => StringValueGenerator.vowel.Contains(y)))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<string> GetStringsWithSomeVowels(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        var value = StringValueGenerator.GetRandomStringWithOnlyPrintableCharactersWithoutUpperCaseCharacters(rand.Next(0, 20), rand).ToCharArray();
        for (int j = 0; j < value.Length; j++) {  // randomly add spaces with 20% probability at each position
          if (rand.NextDouble() < 0.2) value[i] = StringValueGenerator.vowel[rand.Next(0, StringValueGenerator.vowel.Length - 1)];
        }
        yield return new String(value);
      }
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() {
        "", "a", "v", "4", "o", " ", "aei", "ouy", "chf", "quite",
        "a r e9j>", "you are many yay yea", "ssssssssssssssssssss",
        "oooooooooooooooooooo", "wi wi wi wi wi wi wi",
        "x y x y x y x y x y ", "eioyeioyeioyeioyeioy"
      };
    }
  }
}

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
  public class Syllables : BenchmarkSuiteDataDescritpor<string> {
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

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String }; } }

    protected override IEnumerable<string> GenerateTraining() {
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetStringsWithSomeVowels(83, rand));
      return strings;
    }

    protected override IEnumerable<string> GenerateTest() {
      return GetStringsWithSomeVowels(1000, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<string> strings) {
      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => x.Count(y => StringValueGenerator.vowel.Contains(y)).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<string> GetStringsWithSomeVowels(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        var value = StringValueGenerator.GetRandomStringWithOnlyPrintableCharactersWithoutUpperCaseCharacters(rand.Next(0, 20 + 1), rand).ToCharArray();
        for (int j = 0; j < value.Length; j++) {  // randomly add spaces with 20% probability at each position
          if (rand.NextDouble() < 0.2) value[j] = StringValueGenerator.vowel[rand.Next(0, StringValueGenerator.vowel.Length)];
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

    protected override void ModifyGrammar(Grammar g) {
      var partialGrammar = GrammarParser.ReadGrammarBNF("<string_const> ::= \"'aeiou'\"");
      g.Combine(partialGrammar);
      partialGrammar = GrammarParser.ReadGrammarBNF("<string_literal> ::= 'a' | 'e' | 'i' | 'o' | 'u'");
      g.Combine(partialGrammar);
    }
  }
}

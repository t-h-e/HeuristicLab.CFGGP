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

namespace HeuristicLab.Problems.Instances.CFG {
  public class ScrabbleScore : BenchmarkSuiteDataDescritpor<string> {
    public override string Name { get { return "Scrabble Score"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "ScrabbleScore"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 1200; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String, DataType.ListInteger }; } }

    protected override IEnumerable<string> GenerateTraining() {
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetAllLowerCaseLetters());
      strings.AddRange(GetAllUpperCaseLetters());
      strings.AddRange(StringValueGenerator.GetRandomStrings(150, 2, 20, rand));
      return strings;
    }

    protected override IEnumerable<string> GenerateTest() {
      return StringValueGenerator.GetRandomStrings(974, 2, 20, rand);
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<string> strings) {
      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => CalcScrabbleScore(x).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private static Dictionary<char, int> scrabbleDictinary = new Dictionary<char, int>() {
      {'a', 1},
      {'b', 3},
      {'c', 3},
      {'d', 2},
      {'e', 1},
      {'f', 4},
      {'g', 2},
      {'h', 4},
      {'i', 1},
      {'j', 8},
      {'k', 5},
      {'l', 1},
      {'m', 3},
      {'n', 1},
      {'o', 1},
      {'p', 3},
      {'q', 10},
      {'r', 1},
      {'s', 1},
      {'t', 1},
      {'u', 1},
      {'v', 4},
      {'w', 4},
      {'x', 8},
      {'y', 4},
      {'z', 10}
    };

    private int CalcScrabbleScore(string x) {
      int score = 0;
      foreach (var item in x.ToCharArray().Select(y => Char.ToLower(y))) {
        score += scrabbleDictinary.ContainsKey(item) ? scrabbleDictinary[item] : 0;
      }
      return score;
    }

    private IEnumerable<string> GetAllUpperCaseLetters() {
      for (int i = 65; i <= 90; i++) {
        yield return ((char)i).ToString();
      }
    }

    private IEnumerable<string> GetAllLowerCaseLetters() {
      for (int i = 97; i <= 122; i++) {
        yield return ((char)i).ToString();
      }
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() {
        "", "*", " ", "Q ", "zx", " Dw", "ef", "!!", " F@", "ydp",
        "4ps", "abcdefghijklmnopqrst", "ghijklmnopqrstuvwxyz",
        "zxyzxyqQQZXYqqjjawp", "h w h j##r##r\\ n+JJL",
        "i !i !i !i !i", "QQQQQQQQQQQQQQQQQQQQ",
        "$$$$$$$$$$$$$$$$$$$$", "wwwwwwwwwwwwwwwwwwww",
        "1 1 1 1 1 1 1 1 1 1 ", " v v v v v v v v v v",
        "Ha Ha Ha Ha Ha Ha Ha", "x y!x y!x y!x y!x y!",
        "G5G5G5G5G5G5G5G5G5G5"
      };
    }

    protected override void ModifyGrammar(Grammar g) {
      var partialGrammar = GrammarParser.ReadGrammarBNF("<list_int_var> ::= 'scrabblescore'");
      g.Combine(partialGrammar);
    }
  }
}

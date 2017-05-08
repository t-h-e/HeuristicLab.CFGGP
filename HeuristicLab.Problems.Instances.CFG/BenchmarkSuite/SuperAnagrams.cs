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
  public class SuperAnagrams : BenchmarkSuiteDataDescritpor<List<string>> {
    public override string Name { get { return "Super Anagrams"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "SuperAnagrams"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.String, DataType.String }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Boolean }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String }; } }

    protected override IEnumerable<List<string>> GenerateTraining() {
      List<List<string>> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetCloseOrSuperAnagrams(170, rand));
      return strings;
    }

    protected override IEnumerable<List<string>> GenerateTest() {
      return GetCloseOrSuperAnagrams(2000, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<string>> strings) {
      var input = strings.Select(x => String.Join(", ", x.Select(y => y.PrepareStringForPython()))).ToArray();
      var output = strings.Select(x => CalcSuperAnagram(x[0], x[1]) ? "True" : "False").ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private bool CalcSuperAnagram(string x, string y) {
      if (x.Length > y.Length) return false;

      var xChar = x.ToCharArray().OrderBy(a => a).ToList();
      var yChar = y.ToCharArray().OrderBy(a => a).ToList();

      for (int i = 0; i < xChar.Count; i++) {
        char cur = xChar[i];
        int index = yChar.IndexOf(cur);
        if (index < 0) return false;

        while (i < xChar.Count && index < yChar.Count && xChar[i] == yChar[index]) {
          i++;
          index++;
        }

        if (i >= xChar.Count) return true;
        if (i < xChar.Count && cur != xChar[i]) continue;
        return false;
      }
      return true;
    }

    private IEnumerable<List<string>> GetCloseOrSuperAnagrams(int n, FastRandom rand) {
      // string with only letters! no other symbols
      for (int i = 0; i < n; i++) {
        int length = rand.Next(0, 20);
        string value0 = StringValueGenerator.GetRandomLowerCaseString(length, rand);
        string value1 = DropCharsAndShuffle(value0, rand);
        yield return rand.NextDouble() < 0.2  // bias towards value1 first, since value0.Length >= value1.Length
                  ? new List<string>() { value0, value1 }
                  : new List<string>() { value1, value0 };

      }
    }

    private string DropCharsAndShuffle(string original, FastRandom rand) {
      if (String.IsNullOrEmpty(original)) return original;

      int drop = rand.Next(0, original.Length);
      var originalChars = original.ToCharArray();
      var result = originalChars.Shuffle(rand).ToList();
      result = result.SampleRandomWithoutRepetition(rand, original.Length - drop).ToList();
      return new String(result.ToArray());
    }

    private List<List<string>> GetHardcodedTrainingSamples() {
      return new List<List<string>>() {
        new List<string>() { "", ""},
        new List<string>() { "h", ""},
        new List<string>() { "", "i"},
        new List<string>() { "a", "a"},
        new List<string>() { "c", "b"},
        new List<string>() { "nn", "n"},
        new List<string>() { "c", "abcde"},
        new List<string>() { "abcde", "c"},
        new List<string>() { "mnbvccxz", "r"},
        new List<string>() { "aabc", "abc"},
        new List<string>() { "abcde", "aabc"},
        new List<string>() { "edcba", "abcde"},
        new List<string>() { "moo", "mo"},
        new List<string>() { "mo", "moo"},
        new List<string>() { "though", "tree"},
        new List<string>() { "zipper", "rip"},
        new List<string>() { "rip", "flipper"},
        new List<string>() { "zipper", "hi"},
        new List<string>() { "dollars", "dealer"},
        new List<string>() { "louder", "loud"},
        new List<string>() { "ccccc", "ccccccccc"},
        new List<string>() { "oldwestaction", "clinteastwood"},
        new List<string>() { "ldwestaction", "clinteastwood"},
        new List<string>() { "verificationcomplete", "verificationcomplete"},
        new List<string>() { "hhhhhhhhhhaaaaaaaaaa", "hahahahahahahahahaha"},
        new List<string>() { "aahhhh", "hahahahahahahahahaha"},
        new List<string>() { "qwqeqrqtqyquqiqoqpqs", ""},
        new List<string>() { "qazwsxedcrfvtgbyhnuj", "wxyz"},
        new List<string>() { "gggffggfefeededdd", "dddeeefffgggg"},
        new List<string>() { "dddeeefffgggg", "gggffggfefeededdd"},
      };
    }
  }
}

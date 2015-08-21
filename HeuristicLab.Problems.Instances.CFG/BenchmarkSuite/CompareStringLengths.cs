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
  public class CompareStringLengths : BenchmarkSuiteDataDescritpor<List<string>> {
    public override string Name { get { return "Compare String Lengths"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "CompareStringLengths"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<List<string>> GenerateTraining() {
      List<List<string>> strings = new List<List<string>>() { new List<string>() { String.Empty, String.Empty, String.Empty } };
      strings.AddRange(GetDistinctPermutations(new string[] { String.Empty, "a", "bc" }));
      strings.AddRange(GetPermutationsWithTwoEmptyStrings(2, rand));
      strings.AddRange(GetPermutationsWithOneEmptyStrings(3, rand));
      strings.AddRange(GetRepeatedString(3, rand));
      strings.AddRange(GetStringsInSortedLengthOrder(25, rand));
      strings.AddRange(GetStrings(50, rand));
      return strings;
    }

    protected override IEnumerable<List<string>> GenerateTest() {
      var strings = GetRepeatedString(100, rand).ToList();
      strings.AddRange(GetStringsInSortedLengthOrder(200, rand));
      strings.AddRange(GetStrings(700, rand));
      return strings;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<string>> strings) {
      var input = strings.Select(x => String.Join(", ", x.Select(y => y.PrepareStringForPython()))).ToArray();
      var output = strings.Select(x => x[0].Length < x[1].Length && x[1].Length < x[2].Length ? "True" : "False").ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<string>> GetStrings(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<string> strings = new List<string>(3) {
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand),
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand),
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand) };

        yield return strings;
      }
    }

    private IEnumerable<List<string>> GetStringsInSortedLengthOrder(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<string> strings = new List<string>(3) {
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand),
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand),
          StringValueGenerator.GetRandomString(rand.Next(0, 49), rand) };

        yield return strings.OrderBy(x => x.Length).ToList();
      }
    }

    private IEnumerable<List<string>> GetRepeatedString(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return Enumerable.Repeat(StringValueGenerator.GetRandomString(rand.Next(1, 49), rand), 3).ToList();
      }
    }

    private List<List<string>> GetPermutationsWithOneEmptyStrings(int n, FastRandom rand) {
      List<List<string>> strings = new List<List<string>>();
      for (int i = 0; i < n; i++) {
        int length = rand.Next(1, 49);
        string value = StringValueGenerator.GetRandomString(length, rand);
        strings.AddRange(GetDistinctPermutations(new string[] { String.Empty, value, String.Copy(value) }));
      }
      return strings;
    }

    private List<List<string>> GetPermutationsWithTwoEmptyStrings(int n, FastRandom rand) {
      List<List<string>> strings = new List<List<string>>();
      for (int i = 0; i < n; i++) {
        int length = rand.Next(1, 49);
        string value = StringValueGenerator.GetRandomString(length, rand);
        strings.AddRange(GetDistinctPermutations(new string[] { String.Empty, String.Empty, value }));
      }
      return strings;
    }

    private List<List<string>> GetDistinctPermutations(string[] values) {
      var allPermutations = GetPermutations(values);
      allPermutations = allPermutations.Distinct<List<string>>(new EnumerableValueEqualityComparer<string>());
      return allPermutations.ToList();
    }

    private IEnumerable<List<string>> GetPermutations(string[] values) {
      if (values == null || values.Length == 0) {
        yield return new string[0].ToList();
      } else {
        for (int pick = 0; pick < values.Count(); ++pick) {
          string item = values.ElementAt(pick);
          int i = -1;
          string[] rest = Array.FindAll<string>(values, p => ++i != pick);
          foreach (List<string> restPermuted in GetPermutations(rest)) {
            i = -1;
            yield return Array.ConvertAll<string, string>(values, p => ++i == 0 ? item : restPermuted[i - 1]).ToList();
          }
        }
      }
    }
  }
}

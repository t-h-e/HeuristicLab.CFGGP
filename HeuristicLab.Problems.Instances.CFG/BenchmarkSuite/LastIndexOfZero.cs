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
  public class LastIndexOfZero : CFGArtificialDataDescriptor {
    public override string Name { get { return "Last Index of Zero"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "LastIndexOfZero"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 150; } }
    protected override int TestPartitionStart { get { return 150; } }
    protected override int TestPartitionEnd { get { return 1150; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<int>> vectors = GetHardcodedTrainingSamples();
      var zeros = GetHardcodedZeros();
      var trainZeros = zeros.SampleRandomWithoutRepetition(rand, 30).ToList();
      var testZeros = zeros.Except<List<int>>(trainZeros, new EnumerableValueEqualityComparer<int>()).ToList();
      var help1 = GetDistinctPermutations(new int[4] { 0, 5, -8, 9 });
      var trainHelp1 = help1.SampleRandomWithoutRepetition(rand, 20).ToList();
      var testHelp1 = help1.Except<List<int>>(trainHelp1, new EnumerableValueEqualityComparer<int>()).ToList();
      var help2 = GetDistinctPermutations(new int[4] { 0, 0, -8, 9 });
      var trainHelp2 = help2.SampleRandomWithoutRepetition(rand, 10).ToList();
      var testHelp2 = help2.Except<List<int>>(trainHelp2, new EnumerableValueEqualityComparer<int>()).ToList();
      var help3 = GetDistinctPermutations(new int[4] { 0, 0, 0, 9 });

      vectors.AddRange(trainZeros);
      vectors.AddRange(trainHelp1);
      vectors.AddRange(trainHelp2);
      vectors.AddRange(help3);

      vectors.AddRange(GetRandomVectors(78, rand).ToList());

      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(testZeros);
      vectors.AddRange(testHelp1);
      vectors.AddRange(testHelp2);
      vectors.AddRange(GetRandomVectors(974, rand).ToList());

      var input = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x))).ToArray();
      var output = vectors.Select(x => x.LastIndexOf(0).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<int>> GetRandomVectors(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int length = rand.Next(1, 50);
        List<int> cur = new List<int>(length) { 0 };
        for (int j = 0; j < length - 1; j++) {
          cur.Add(rand.Next(-50, 50));
        }
        yield return cur.Shuffle(rand).ToList();
      }
    }

    private IEnumerable<List<int>> GetHardcodedZeros() {
      for (int i = 1; i <= 50; i++) {
        yield return Enumerable.Repeat(0, i).ToList();
      }
    }

    private List<List<int>> GetDistinctPermutations(int[] values) {
      var allPermutations = GetPermutations(values);
      allPermutations = allPermutations.Distinct<List<int>>(new EnumerableValueEqualityComparer<int>());
      return allPermutations.ToList();
    }

    private IEnumerable<List<int>> GetPermutations(int[] values) {
      if (values == null || values.Length == 0) {
        yield return new int[0].ToList();
      } else {
        for (int pick = 0; pick < values.Count(); ++pick) {
          int item = values.ElementAt(pick);
          int i = -1;
          int[] rest = Array.FindAll<int>(values, p => ++i != pick);
          foreach (List<int> restPermuted in GetPermutations(rest)) {
            i = -1;
            yield return Array.ConvertAll<int, int>(values, p => ++i == 0 ? item : restPermuted[i - 1]).ToList();
          }
        }
      }
    }

    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {0, 1},
            new List<int>() {1, 0},
            new List<int>() {7, 0},
            new List<int>() {0, 8},
            new List<int>() {0, -1},
            new List<int>() {-1, 0},
            new List<int>() {-7, 0},
            new List<int>() {0, -8},
      };
    }
  }
}

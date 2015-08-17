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
  public class VectorsSummed : CFGArtificialDataDescriptor {
    public override string Name { get { return "Vectors Summed"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "VectorsSummed"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 150; } }
    protected override int TestPartitionStart { get { return 150; } }
    protected override int TestPartitionEnd { get { return 1650; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<Tuple<List<int>, List<int>>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetVectorOfLength(5, 1, rand).ToList());
      vectors.AddRange(GetVectorOfLength(10, 50, rand).ToList());
      vectors.AddRange(GetRandomTuple(126, rand).ToList());

      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(GetVectorOfLength(100, 50, rand).ToList());
      vectors.AddRange(GetRandomTuple(1400, rand).ToList());

      var input = vectors.Select(x => String.Format("[[{0}], [{1}]]", String.Join(", ", x.Item1), String.Join(", ", x.Item2))).ToArray();
      var output = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x.Item1.Zip(x.Item2, (a, b) => a + b)))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetRandomTuple(int n, FastRandom rand) {
      int length = rand.Next(0, 50);
      return GetVectorOfLength(n, length, rand);
    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetVectorOfLength(int n, int length, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return new Tuple<List<int>, List<int>>(GetRandom(rand, length), GetRandom(rand, length));
      }
    }

    private List<int> GetRandom(FastRandom rand, int length) {
      List<int> vector = new List<int>(length);
      for (int j = 0; j < length; j++) {
        vector.Add(rand.Next(-1000, 1000));
      }
      return vector;
    }

    private List<Tuple<List<int>, List<int>>> GetHardcodedTrainingSamples() {
      return new List<Tuple<List<int>, List<int>>>() {
          new Tuple<List<int>, List<int>>(new List<int>() {}, new List<int>() {}),
          new Tuple<List<int>, List<int>>(new List<int>() {0}, new List<int>() {0}),
          new Tuple<List<int>, List<int>>(new List<int>() {10}, new List<int>() {0}),
          new Tuple<List<int>, List<int>>(new List<int>() {5}, new List<int>() {3}),
          new Tuple<List<int>, List<int>>(new List<int>() {-9}, new List<int>() {7}),
          new Tuple<List<int>, List<int>>(new List<int>() {0, 0}, new List<int>() {0, 0}),
          new Tuple<List<int>, List<int>>(new List<int>() {-4, 2}, new List<int>() {0, 1}),
          new Tuple<List<int>, List<int>>(new List<int>() {-3, 0}, new List<int>() {-1, 0}),
          new Tuple<List<int>, List<int>>(new List<int>() {-323, 49}, new List<int>() {-90, -6}),
        };
    }
  }
}

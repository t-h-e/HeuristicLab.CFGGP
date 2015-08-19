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
  public class MirrorImage : CFGArtificialDataDescriptor {
    public override string Name { get { return "Mirror Image"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "MirrorImage"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<Tuple<List<int>, List<int>>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetMirrorImages(37, rand).ToList());
      vectors.AddRange(GetEqual(10, rand).ToList());
      vectors.AddRange(GetMirrorImagesWithFewChanges(20, rand).ToList());
      vectors.AddRange(GetRandomTuple(10, rand).ToList());

      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(GetMirrorImages(500, rand).ToList());
      vectors.AddRange(GetEqual(100, rand).ToList());
      vectors.AddRange(GetMirrorImagesWithFewChanges(200, rand).ToList());
      vectors.AddRange(GetRandomTuple(200, rand).ToList());
      var input = vectors.Select(x => String.Format("[{0}], [{1}]", String.Join(", ", x.Item1), String.Join(", ", x.Item2))).ToArray();
      var output = vectors.Select(x => x.Item1.SequenceEqual(Enumerable.Reverse(x.Item2)) ? "True" : "False").ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetRandomTuple(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return new Tuple<List<int>, List<int>>(GetRandom(rand), GetRandom(rand));
      }

    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetMirrorImagesWithFewChanges(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<int> vector = GetRandom(rand, 1);
        var mirror = Enumerable.Reverse(vector).ToList();

        // add few changes
        int length = vector.Count;
        int changes = rand.Next(1, 5);
        for (int j = 0; j < changes; j++) {
          if (rand.NextBool()) {
            vector[rand.Next(0, length - 1)] = rand.Next(-1000, 1000);
          } else {
            mirror[rand.Next(0, length - 1)] = rand.Next(-1000, 1000);
          }
        }
        yield return new Tuple<List<int>, List<int>>(vector, mirror);
      }
    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetEqual(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<int> vector = GetRandom(rand);
        yield return new Tuple<List<int>, List<int>>(vector, vector.ToList());
      }
    }

    private IEnumerable<Tuple<List<int>, List<int>>> GetMirrorImages(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<int> vector = GetRandom(rand);
        var mirror = Enumerable.Reverse(vector).ToList();
        yield return new Tuple<List<int>, List<int>>(vector, mirror);
      }
    }

    private List<int> GetRandom(FastRandom rand, int minLength = 0) {
      int length = rand.Next(minLength, 50);
      List<int> vector = new List<int>(length);
      for (int j = 0; j < length; j++) {
        vector.Add(rand.Next(-1000, 1000));
      }
      return vector;
    }

    private List<Tuple<List<int>, List<int>>> GetHardcodedTrainingSamples() {
      return new List<Tuple<List<int>, List<int>>>() {
          new Tuple<List<int>, List<int>>(new List<int>() {}, new List<int>() {}),
          new Tuple<List<int>, List<int>>(new List<int>() {1}, new List<int>() {1}),
          new Tuple<List<int>, List<int>>(new List<int>() {0}, new List<int>() {1}),
          new Tuple<List<int>, List<int>>(new List<int>() {1}, new List<int>() {0}),
          new Tuple<List<int>, List<int>>(new List<int>() {-44}, new List<int>() {16}),
          new Tuple<List<int>, List<int>>(new List<int>() {-13}, new List<int>() {-12}),
          new Tuple<List<int>, List<int>>(new List<int>() {2, 1}, new List<int>() {1, 2}),
          new Tuple<List<int>, List<int>>(new List<int>() {0, 1}, new List<int>() {1, 0}),
          new Tuple<List<int>, List<int>>(new List<int>() {0, 7}, new List<int>() {7, 0}),
          new Tuple<List<int>, List<int>>(new List<int>() {5, 8}, new List<int>() {5, 8}),
          new Tuple<List<int>, List<int>>(new List<int>() {34, 12}, new List<int>() {34, 12}),
          new Tuple<List<int>, List<int>>(new List<int>() {456, 456}, new List<int>() {456, 456}),
          new Tuple<List<int>, List<int>>(new List<int>() {40, 831}, new List<int>() {-431, -680}),
          new Tuple<List<int>, List<int>>(new List<int>() {1, 2, 1}, new List<int>() {1, 2, 1}),
          new Tuple<List<int>, List<int>>(new List<int>() {1, 2, 3, 4, 5, 4, 3, 2, 1}, new List<int>() {1, 2, 3, 4, 5, 4, 3, 2, 1}),
          new Tuple<List<int>, List<int>>(new List<int>() {45, 99, 0, 12, 44, 7, 7, 44, 12, 0, 99, 45}, new List<int>() {45, 99, 0, 12, 44, 7, 7, 44, 12, 0, 99, 45}),
          new Tuple<List<int>, List<int>>(new List<int>() {24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}, new List<int>() {24, 23, 22, 21, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 87, 6, 5, 4, 3, 2, 1, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 1718, 19, 20, 21, 22, 23, 24}),
          new Tuple<List<int>, List<int>>(new List<int>() {33, 45, -941}, new List<int>() {33, 45, -941}),
          new Tuple<List<int>, List<int>>(new List<int>() {33, -941, 45}, new List<int>() {33, 45, -941}),
          new Tuple<List<int>, List<int>>(new List<int>() {45, 33, -941}, new List<int>() {33, 45, -941}),
          new Tuple<List<int>, List<int>>(new List<int>() {45, -941, 33}, new List<int>() {33, 45, -941}),
          new Tuple<List<int>, List<int>>(new List<int>() {-941, 33, 45}, new List<int>() {33, 45, -941}),
          new Tuple<List<int>, List<int>>(new List<int>() {-941, 45, 33}, new List<int>() {33, 45, -941}),
        };
    }
  }
}

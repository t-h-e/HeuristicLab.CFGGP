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
  public class NegativeToZero : CFGArtificialDataDescriptor {
    public override string Name { get { return "Negative To Zero"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "NegativeToZero"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<int>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetLength1(5, rand).ToList());
      vectors.AddRange(GetRandom(9, rand, -1000, -1).ToList());
      vectors.AddRange(GetRandom(9, rand, 1).ToList());
      vectors.AddRange(GetRandom(165, rand).ToList());

      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(GetRandom(100, rand, -1000, -1).ToList());
      vectors.AddRange(GetRandom(100, rand, 1).ToList());
      vectors.AddRange(GetRandom(1800, rand).ToList());

      var input = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x))).ToArray();
      var output = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x.Select(y => y >= 0 ? y : 0)))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<int>> GetRandom(int n, FastRandom rand, int min = -1000, int max = 1000) {
      for (int i = 1; i <= n; i++) {
        int length = rand.Next(0, 50);
        List<int> vector = new List<int>(length);
        for (int j = 0; j < length; j++) {
          vector.Add(rand.Next(min, max));
        }
        yield return vector;
      }
    }

    private IEnumerable<List<int>> GetLength1(int n, FastRandom rand) {
      for (int i = 1; i <= n; i++) {
        yield return new List<int>(1) { rand.Next(-1000, 1000) };
      }
    }

    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {},
            new List<int>() {-10},
            new List<int>() {-1},
            new List<int>() {0},
            new List<int>() {1},
            new List<int>() {10},
            new List<int>() {0, 0},
            new List<int>() {0, 1},
            new List<int>() {-1, 0},
            new List<int>() {-90, -6},
            new List<int>() {-16, 33},
            new List<int>() {412, 111},
      };
    }
  }
}

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
  public class CountOdds : CFGArtificialDataDescriptor {
    public override string Name { get { return "Count Odds"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "CountOdds"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<int>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetAllOdd(9, rand).Select(x => x.ToList()));
      vectors.AddRange(GetAllEven(9, rand).Select(x => x.ToList()));
      vectors.AddRange(GetRandom(150, rand).Select(x => x.ToList()));


      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(GetAllOdd(100, rand).Select(x => x.ToList()));
      vectors.AddRange(GetAllEven(100, rand).Select(x => x.ToList()));
      vectors.AddRange(GetRandom(1800, rand).Select(x => x.ToList()));

      var input = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x))).ToArray();
      var output = vectors.Select(x => x.Count(y => y % 2 == 0).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<int>> GetAllOdd(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int length = rand.Next(0, 50);
        List<int> vector = new List<int>(length);
        for (int j = 0; j < length; j++) {
          vector.Add(rand.Next(0, 999) * 2 - 999);
        }
        yield return vector;
      }
    }

    private IEnumerable<List<int>> GetAllEven(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int length = rand.Next(0, 50);
        List<int> vector = new List<int>(length);
        for (int j = 0; j < length; j++) {
          vector.Add(rand.Next(0, 1000) * 2 - 1000);
        }
        yield return vector;
      }
    }

    private IEnumerable<List<int>> GetRandom(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int length = rand.Next(0, 50);
        List<int> vector = new List<int>(length);
        for (int j = 0; j < length; j++) {
          vector.Add(rand.Next(-1000, 1000));
        }
        yield return vector;
      }
    }


    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {},
            new List<int>() {-10},
            new List<int>() {-9},
            new List<int>() {-8},
            new List<int>() {-7},
            new List<int>() {-6},
            new List<int>() {-5},
            new List<int>() {-4},
            new List<int>() {-3},
            new List<int>() {-2},
            new List<int>() {-1},
            new List<int>() {-0},
            new List<int>() {1},
            new List<int>() {2},
            new List<int>() {3},
            new List<int>() {4},
            new List<int>() {5},
            new List<int>() {6},
            new List<int>() {7},
            new List<int>() {8},
            new List<int>() {9},
            new List<int>() {10},
            new List<int>() {-947},
            new List<int>() {-450},
            new List<int>() {303},
            new List<int>() {886},
            new List<int>() {0, 0},
            new List<int>() {0, 1},
            new List<int>() {7, 1},
            new List<int>() {-9, -1},
            new List<int>() {-11, 40},
            new List<int>() {944, 77},
      };
    }
  }
}

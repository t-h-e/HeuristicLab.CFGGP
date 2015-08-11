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
using HeuristicLab.Core;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Grade : CFGArtificialDataDescriptor {
    public override string Name { get { return "Grade"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Grade"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<int>> grades = GetHardcodedTrainingSamples();
      grades.AddRange(CreateThresholdsAndGrade(159, rand).ToList());
      grades = grades.Shuffle(rand).ToList();

      grades.AddRange(CreateThresholdsAndGrade(2000, rand).ToList());

      var input = grades.Select(x => String.Join(", ", x)).ToArray();
      var output = grades.Select(x => CalcGrade(x)).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcGrade(List<int> x) {
      int score = x.Last();
      if (score > x[0]) return "A";
      else if (score > x[1]) return "B";
      else if (score > x[2]) return "C";
      else if (score > x[3]) return "D";
      else return "F";
    }

    private IEnumerable<List<int>> CreateThresholdsAndGrade(int n, IRandom rand) {
      for (int i = 0; i < n; i++) {
        int a = rand.Next(4, 101);
        int b = rand.Next(3, a);
        int c = rand.Next(2, b);
        int d = rand.Next(1, c);
        int grade = rand.Next(0, 101);
        yield return new List<int>(5) { a, b, c, d, grade };
      }
    }

    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {80, 70, 60, 50, 85},
            new List<int>() {80, 70, 60, 50, 80},
            new List<int>() {80, 70, 60, 50, 79},
            new List<int>() {80, 70, 60, 50, 75},
            new List<int>() {80, 70, 60, 50, 70},
            new List<int>() {80, 70, 60, 50, 69},
            new List<int>() {80, 70, 60, 50, 65},
            new List<int>() {80, 70, 60, 50, 60},
            new List<int>() {80, 70, 60, 50, 59},
            new List<int>() {80, 70, 60, 50, 55},
            new List<int>() {80, 70, 60, 50, 50},
            new List<int>() {80, 70, 60, 50, 49},
            new List<int>() {80, 70, 60, 50, 45},
            new List<int>() {90, 80, 70, 60, 100},
            new List<int>() {90, 80, 70, 60, 0},
            new List<int>() {4, 3, 2, 1, 5},
            new List<int>() {4, 3, 2, 1, 4},
            new List<int>() {4, 3, 2, 1, 3},
            new List<int>() {4, 3, 2, 1, 2},
            new List<int>() {4, 3, 2, 1, 1},
            new List<int>() {4, 3, 2, 1, 0},
            new List<int>() {100, 99, 98, 97, 100},
            new List<int>() {100, 99, 98, 97, 99},
            new List<int>() {100, 99, 98, 97, 98},
            new List<int>() {100, 99, 98, 97, 97},
            new List<int>() {100, 99, 98, 97, 96},
            new List<int>() {98, 48, 27, 3, 55},
            new List<int>() {98, 48, 27, 3, 14},
            new List<int>() {98, 48, 27, 3, 1},
            new List<int>() {45, 30, 27, 0, 1},
            new List<int>() {45, 30, 27, 0, 0},
            new List<int>() {48, 46, 44, 42, 40},
            new List<int>() {48, 46, 44, 42, 41},
            new List<int>() {48, 46, 44, 42, 42},
            new List<int>() {48, 46, 44, 42, 43},
            new List<int>() {48, 46, 44, 42, 44},
            new List<int>() {48, 46, 44, 42, 45},
            new List<int>() {48, 46, 44, 42, 46},
            new List<int>() {48, 46, 44, 42, 47},
            new List<int>() {48, 46, 44, 42, 48},
            new List<int>() {48, 46, 44, 42, 49}
      };
    }
  }
}

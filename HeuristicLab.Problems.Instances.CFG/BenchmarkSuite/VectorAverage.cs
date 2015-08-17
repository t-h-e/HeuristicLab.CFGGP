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
  public class VectorAverage : CFGArtificialDataDescriptor {
    public override string Name { get { return "Vector Average"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "VectorAverage"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<List<double>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetVecotrsOfLenght50(4).Select(x => x.ToList()));
      vectors.AddRange(GetVecotrsOfVariableLenght(90, rand).Select(x => x.ToList()));

      vectors = vectors.Shuffle(rand).ToList();

      vectors.AddRange(GetVecotrsOfLenght50(50).Select(x => x.ToList()));
      vectors.AddRange(GetVecotrsOfVariableLenght(950, rand).Select(x => x.ToList()));


      var input = vectors.Select(x => String.Format("[{0:0.0################}]", String.Join(", ", x))).ToArray();
      var output = vectors.Select(x => x.Average().ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<IEnumerable<double>> GetVecotrsOfVariableLenght(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return ValueGenerator.GenerateUniformDistributedValues(rand.Next(1, 50), -1000.0, 1000.0);
      }
    }

    private IEnumerable<IEnumerable<double>> GetVecotrsOfLenght50(int n) {
      for (int i = 0; i < n; i++) {
        yield return ValueGenerator.GenerateUniformDistributedValues(50, -1000.0, 1000.0);
      }
    }

    private List<List<double>> GetHardcodedTrainingSamples() {
      return new List<List<double>>() {
            new List<double>() {0.0},
            new List<double>() {100.0},
            new List<double>() {-100.0},
            new List<double>() {2.0, 129.0},
            new List<double>() {0.12345, -4.678},
            new List<double>() {999.99, 74.113},
      };
    }
  }
}

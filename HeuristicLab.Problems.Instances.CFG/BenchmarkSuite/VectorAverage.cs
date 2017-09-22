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
  public class VectorAverage : BenchmarkSuiteDataDescritpor<List<double>> {
    public override string Name { get { return "Vector Average"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "VectorAverage"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 250; } }
    protected override int TestPartitionStart { get { return 250; } }
    protected override int TestPartitionEnd { get { return 2800; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.ListFloat }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Float }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Float, DataType.ListFloat }; } }

    protected override IEnumerable<List<double>> GenerateTraining() {
      List<List<double>> vectors = GetHardcodedTrainingSamples();
      vectors.AddRange(GetVectorsOfLenght(45, 1).Select(x => x.ToList())); // Random Length-1 vectors
      vectors.AddRange(GetVectorsOfLenght(45, 2).Select(x => x.ToList()));  // Random Length-2 vectors
      vectors.AddRange(GetVectorsOfLenght(50, rand.Next(3, 5 + 1)).Select(x => x.ToList())); // Random Length-3, -4, and -5 vectors
      vectors.AddRange(GetVectorsOfLenght(5, 50).Select(x => x.ToList())); // Random Length-50 vectors
      vectors.AddRange(GetVecotrsOfVariableLenght(95, rand).Select(x => x.ToList())); // Random length, random floats
      return vectors;
    }

    protected override IEnumerable<List<double>> GenerateTest() {
      var vectors = GetVectorsOfLenght(500, 1).Select(x => x.ToList()).ToList(); // Random Length-1 vectors
      vectors.AddRange(GetVectorsOfLenght(500, 2).Select(x => x.ToList())); // Random Length-2 vectors
      vectors.AddRange(GetVectorsOfLenght(500, rand.Next(3, 5 + 1)).Select(x => x.ToList())); // Random Length-3, -4, and -5 vectors
      vectors.AddRange(GetVectorsOfLenght(50, 50).Select(x => x.ToList())); // Random Length-50 vectors
      vectors.AddRange(GetVecotrsOfVariableLenght(1000, rand).Select(x => x.ToList())); // Random length, random floats
      return vectors;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<double>> vectors) {
      var input = vectors.Select(x => String.Format("[{0}]", String.Join(", ", x.Select(y => String.Format("{0:0.0################}", y))))).ToArray();
      var output = vectors.Select(x => x.Average().ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<IEnumerable<double>> GetVecotrsOfVariableLenght(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return ValueGenerator.GenerateUniformDistributedValues(rand.Next(1, 50 + 1), -1000.0, 1000.0, rand);
      }
    }

    private IEnumerable<IEnumerable<double>> GetVectorsOfLenght(int n, int length) {
      for (int i = 0; i < n; i++) {
        yield return ValueGenerator.GenerateUniformDistributedValues(length, -1000.0, 1000.0, rand);
      }
    }

    private List<List<double>> GetHardcodedTrainingSamples() {
      return new List<List<double>>() {
            new List<double>() {0.0},
            new List<double>() {100.0},
            new List<double>() {-100.0},
            new List<double>() {1000.0},
            new List<double>() {-1000.0},
            new List<double>() {2.0, 129.0},
            new List<double>() {0.12345, -4.678},
            new List<double>() {999.99, 74.113},
            new List<double>() {987.654321, 995.0003},
            new List<double>() {-788.788, -812.19},
      };
    }
  }
}

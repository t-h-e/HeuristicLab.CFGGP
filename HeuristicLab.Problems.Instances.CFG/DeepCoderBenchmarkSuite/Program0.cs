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
  public class Program0 : DeepCoderDataDescritpor<Tuple<int, List<int>>> {
    public override string Name { get { return "Program0"; } }
    public override string Description { get { return ""; } }

    public override string Identifier { get { return "Program0"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer, DataType.ListInteger }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { }; } }

    protected override IEnumerable<Tuple<int, List<int>>> GenerateTraining() {
      var x0 = ValueGenerator.GenerateUniformDistributedValues(100, 1, 19, rand).ToList();
      var x1 = GetRandom(100, rand, x0).ToList();
      return x0.Zip(x1, (first, second) => new Tuple<int, List<int>>(first, second)).ToList();
    }

    protected override IEnumerable<Tuple<int, List<int>>> GenerateTest() {
      var x0 = ValueGenerator.GenerateUniformDistributedValues(1000, 1, 19, rand).ToList();
      var x1 = GetRandom(100, rand, x0).ToList();
      return x0.Zip(x1, (first, second) => new Tuple<int, List<int>>(first, second)).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<Tuple<int, List<int>>> trainingAndTest) {
      var input = trainingAndTest.Select(x => String.Format("{0}, {1}", x.Item1, String.Format("[{0}]", String.Join(", ", x.Item2)))).ToArray();
      var output = trainingAndTest.Select(x => x.Item2.OrderBy(y => y).Take(x.Item1).Sum().ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<int>> GetRandom(int n, FastRandom rand, IEnumerable<int> sizes, int min = -256, int max = 256) {
      foreach (var s in sizes) {
        int length = rand.Next(s + 1, 20 + 1);
        List<int> vector = new List<int>(length);  // vector has to be bigger than s
        for (int j = 0; j < length; j++) {
          vector.Add(rand.Next(min, max + 1));
        }
        yield return vector;
      }
    }
  }
}

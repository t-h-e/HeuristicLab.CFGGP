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

namespace HeuristicLab.Problems.Instances.CFG {
  public class Program4 : DeepCoderDataDescritpor<Tuple<List<int>, List<int>>> {
    public override string Name { get { return "Program4"; } }
    public override string Description { get { return ""; } }

    public override string Identifier { get { return "Program4"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.ListInteger, DataType.ListInteger }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { }; } }

    protected override IEnumerable<Tuple<List<int>, List<int>>> GenerateTraining() {
      var sizes = ValueGenerator.GenerateUniformDistributedValues(100, 1, 20, rand).ToList();
      var x0 = ValueGenerator.GenerateUniformDistributedLists(100, sizes, -256, 255, rand).ToList();
      var x1 = ValueGenerator.GenerateUniformDistributedLists(100, sizes, -256, 255, rand).ToList();
      return x0.Zip(x1, (first, second) => new Tuple<List<int>, List<int>>(first, second)).ToList();
    }

    protected override IEnumerable<Tuple<List<int>, List<int>>> GenerateTest() {
      var sizes = ValueGenerator.GenerateUniformDistributedValues(1000, 1, 20, rand).ToList();
      var x0 = ValueGenerator.GenerateUniformDistributedLists(1000, sizes, -256, 255, rand).ToList();
      var x1 = ValueGenerator.GenerateUniformDistributedLists(1000, sizes, -256, 255, rand).ToList();
      return x0.Zip(x1, (first, second) => new Tuple<List<int>, List<int>>(first, second)).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<Tuple<List<int>, List<int>>> trainingAndTest) {
      var input = trainingAndTest.Select(x => String.Format("{0}, {1}", String.Format("[{0}]", String.Join(", ", x.Item1)), String.Format("[{0}]", String.Join(", ", x.Item2)))).ToArray();
      var output = trainingAndTest.Select(x => x.Item1.OrderBy(y => y).Zip(x.Item2.OrderBy(y => y).Reverse(), (a, b) => a * b).Sum().ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }
  }
}

﻿#region License Information
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
  public class Program3 : DeepCoderDataDescritpor<List<int>> {
    public override string Name { get { return "Program3"; } }
    public override string Description { get { return ""; } }

    public override string Identifier { get { return "Program3"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.ListInteger }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { }; } }

    protected override IEnumerable<List<int>> GenerateTraining() {
      var sizes = ValueGenerator.GenerateUniformDistributedValues(100, 1, 20, rand).ToList();
      var x0 = ValueGenerator.GenerateUniformDistributedLists(100, sizes, -256, 255, rand).ToList();
      return x0;
    }

    protected override IEnumerable<List<int>> GenerateTest() {
      var sizes = ValueGenerator.GenerateUniformDistributedValues(1000, 1, 20, rand).ToList();
      var x0 = ValueGenerator.GenerateUniformDistributedLists(1000, sizes, -256, 255, rand).ToList();
      return x0;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<int>> trainingAndTest) {
      var input = trainingAndTest.Select(x => String.Format("{0}", String.Format("[{0}]", String.Join(", ", x)))).ToArray();
      var output = trainingAndTest.Select(x => x.Zip((new List<int>() { x.First() }).Concat(x.Skip(1).Zip(x.Take(x.Count() - 1), (a, b) => Math.Min(a, b))), (a, b) => a - b).Where(y => y > 0).Sum().ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }
  }
}

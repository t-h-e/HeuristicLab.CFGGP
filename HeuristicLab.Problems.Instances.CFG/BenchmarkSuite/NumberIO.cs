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
  public class NumberIO : BenchmarkSuiteDataDescritpor<Tuple<int, double>> {
    public override string Name { get { return "Number IO"; } }
    public override string Description {
      get {
        return "Given an integer and a oat, print their sum" + Environment.NewLine
          + "Variables x0 is an integer and x1 is a float";
      }
    }
    public override string Identifier { get { return "NumberIO"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 25; } }
    protected override int TestPartitionStart { get { return 25; } }
    protected override int TestPartitionEnd { get { return 1025; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer, DataType.Float }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Float }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Float }; } }

    protected override IEnumerable<Tuple<int, double>> GenerateTraining() {
      var x0 = ValueGenerator.GenerateUniformDistributedValues(25, -100, 100, rand);
      var x1 = ValueGenerator.GenerateUniformDistributedValues(25, -100.0, 100.0, rand);
      return x0.Zip(x1, (first, second) => new Tuple<int, double>(first, second));
    }

    protected override IEnumerable<Tuple<int, double>> GenerateTest() {
      var x0 = ValueGenerator.GenerateUniformDistributedValues(1000, -100, 100, rand);
      var x1 = ValueGenerator.GenerateUniformDistributedValues(1000, -100.0, 100.0, rand);
      return x0.Zip(x1, (first, second) => new Tuple<int, double>(first, second));
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<Tuple<int, double>> trainingAndTest) {
      var input = trainingAndTest.Select(x => String.Format("{0}, {1}", x.Item1, x.Item2)).ToArray();
      var output = trainingAndTest.Select(x => (x.Item1 + x.Item2).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }
  }
}

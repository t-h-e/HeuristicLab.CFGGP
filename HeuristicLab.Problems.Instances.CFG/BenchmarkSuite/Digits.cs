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
using System.Numerics;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Digits : BenchmarkSuiteDataDescritpor<long> {
    public override string Name { get { return "Digits"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Digits"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.ListInteger }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean }; } }

    protected override IEnumerable<long> GenerateTraining() {
      var x0 = new List<long>() { -9495969798, -20008000, -777777, -9876, -482, -97, -20, 0, 19, 620, 24068, 512000, 8313227, 30000000, 9998887776 };
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(85, -9999999999, 9999999999, rand));
      return x0;
    }

    protected override IEnumerable<long> GenerateTest() {
      return ValueGenerator.GenerateUniformDistributedValues(1000, -9999999999, 9999999999, rand);
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<long> x0) {
      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => String.Format("[{0}]", String.Join(", ", CalcDigits(x)))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private List<int> CalcDigits(long x) {
      List<int> digits = new List<int>();
      BigInteger y = new BigInteger(Math.Abs(x));
      while (y > 0) {
        digits.Add((int)(y % 10));
        y = y / 10;
      }
      if (x < 0) {
        digits[digits.Count - 1] *= -1;
      }
      return digits;
    }
  }
}

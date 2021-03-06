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
  public class WallisPi : BenchmarkSuiteDataDescritpor<int> {
    public override string Name { get { return "Wallis Pi"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "WallisPi"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 150; } }
    protected override int TestPartitionStart { get { return 150; } }
    protected override int TestPartitionEnd { get { return 200; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Float }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.Float }; } }

    protected override IEnumerable<int> GenerateTraining() {
      var x0 = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 198, 199, 200 };
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(135, 1, 200, rand));
      return x0;
    }

    protected override IEnumerable<int> GenerateTest() {
      return ValueGenerator.GenerateUniformDistributedValues(50, 1, 200, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<int> x0) {
      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => CalcWallisPi(x).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private double CalcWallisPi(int n) {
      double piHalve = 2.0 / 3.0;
      for (int i = 2; i <= n; i += 2) {
        double cur = 2.0 * (double)i;
        piHalve *= cur / (cur - 1.0);

        if (i + 1 > n) break;
        piHalve *= cur / (cur + 1.0);
      }
      return Math.Round(piHalve, 5);
    }
  }
}

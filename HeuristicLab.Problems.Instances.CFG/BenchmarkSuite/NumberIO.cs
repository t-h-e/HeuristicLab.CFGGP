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
using System.Linq;

namespace HeuristicLab.Problems.Instances.CFG {
  public class NumberIO : CFGArtificialDataDescriptor {
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

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      var x0 = ValueGenerator.GenerateUniformDistributedValues(TestPartitionEnd, -100, 100).ToList();
      var x1 = ValueGenerator.GenerateUniformDistributedValues(TestPartitionEnd, -100.0, 100.0).ToList();

      var input = x0.Zip(x1, (first, second) => String.Format("{0}, {1}", first, second)).ToArray();
      var output = x0.Zip(x1, (first, second) => (first + second).ToString()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }
  }
}

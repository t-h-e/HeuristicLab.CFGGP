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
  public class Checksum : BenchmarkSuiteDataDescritpor<string> {
    public override string Name { get { return "Checksum"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Checksum"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 300; } }
    protected override int TestPartitionStart { get { return 300; } }
    protected override int TestPartitionEnd { get { return 2300; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Boolean, DataType.Integer, DataType.String }; } }

    protected override IEnumerable<string> GenerateTraining() {
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(StringValueGenerator.GetRandomStrings(55, 2, 2, rand)); // Random length-2 inputs
      strings.AddRange(StringValueGenerator.GetRandomStrings(50, 3, 3, rand)); // Random length-3 inputs
      strings.AddRange(StringValueGenerator.GetRandomStrings(89, 2, 50, rand)); // Random >= 2 length inputs
      return strings;
    }

    protected override IEnumerable<string> GenerateTest() {
      List<string> strings = StringValueGenerator.GetRandomStrings(500, 2, 2, rand).ToList(); // Random length-2 inputs
      strings.AddRange(StringValueGenerator.GetRandomStrings(500, 3, 3, rand)); // Random length-3 inputs
      strings.AddRange(StringValueGenerator.GetRandomStrings(1000, 2, 50, rand)); // Random >= 2 length inputs
      return strings;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<string> strings) {
      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => CalcChecksum(x).ToString().PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private char CalcChecksum(string x) {
      int sum = 0;
      foreach (int item in x.ToCharArray()) {
        sum += item;
      }
      return (char)(sum % 64 + (int)' ');
    }

    private List<string> GetHardcodedTrainingSamples() {
      var hardCoded = new List<string>() {
        "", "\t", "\n", "B\n", "\n\n",
          String.Concat(Enumerable.Repeat('\n', 50)),
          String.Concat(Enumerable.Repeat(' ', 50)),
          String.Concat(Enumerable.Repeat('s', 50)),
          String.Concat(Enumerable.Repeat("CD\n", 16)) + "CD",
          String.Concat(Enumerable.Repeat("x\ny ", 12)) + "x\n",
          String.Concat(Enumerable.Repeat(" \n", 25)),
      }; // "Special" inputs covering some base cases
      var everyVisibleCharater = Enumerable.Range(32, 95).Select(x => ((char)x).ToString()).ToList();  // All visible characters once
      return hardCoded.Union(everyVisibleCharater).ToList();
    }
  }
}

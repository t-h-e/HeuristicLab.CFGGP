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
  public class StringLengthsBackwards : BenchmarkSuiteDataDescritpor<List<string>> {
    public override string Name { get { return "String Lengths Backwards"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "StringLengthsBackwards"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.ListString }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.ListInteger }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String, DataType.ListString }; } }

    protected override IEnumerable<List<string>> GenerateTraining() {
      List<List<string>> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetRandomStringsOfStrings(90, rand).ToList());
      return strings;
    }

    protected override IEnumerable<List<string>> GenerateTest() {
      return GetRandomStringsOfStrings(1000, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<string>> strings) {
      var input = strings.Select(x => String.Format("[{0}]", String.Join(", ", x.Select(y => y.PrepareStringForPython())))).ToArray();
      var output = strings.Select(x => String.Format("[{0}]", String.Join(", ", x.Select(y => y.Length).Reverse()))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<string>> GetRandomStringsOfStrings(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int count = rand.Next(0, 50);
        yield return StringValueGenerator.GetRandomStrings(count, 0, 50, rand).ToList();
      }
    }

    private List<List<string>> GetHardcodedTrainingSamples() {
      return new List<List<string>>() {
        new List<string>() { },
        new List<string>() { "" },
        new List<string>() { "", "" },
        new List<string>() { "", "", "" },
        new List<string>() { "", "", "", "", "", "", "", "", "", "" },
        new List<string>() { "abcde" },
        new List<string>() { "1" },
        new List<string>() { "abc", "hi there" },
        new List<string>() { "!@#", "\n\n\t\t", "5552\na r" },
        new List<string>() { "tt", "333", "1", "ccc" }
      };
    }
  }
}

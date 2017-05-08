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
  public class ReplaceSpaceWithNewline : BenchmarkSuiteDataDescritpor<string> {
    public override string Name { get { return "Replace Space with Newline"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "ReplaceSpaceWithNewline"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.String, DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String }; } }

    protected override IEnumerable<string> GenerateTraining() {
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetStringWithSpaces(70, rand));
      return strings;
    }

    protected override IEnumerable<string> GenerateTest() {
      return GetStringWithSpaces(1000, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<string> strings) {
      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      //var output = strings.Select(x => new String(x.Select(y => y == ' ' ? '\n' : y).ToArray()).PrepareStringForPython()).ToArray();
      var output = strings.Select(x => String.Format("{0}, {1}", new String(x.Select(y => y == ' ' ? '\n' : y).ToArray()).PrepareStringForPython(), x.Length - x.Count(Char.IsWhiteSpace))).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<string> GetStringWithSpaces(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        var value = StringValueGenerator.GetRandomStringWithoutSpaces(rand.Next(0, 20), rand).ToCharArray();

        // add 20% spaces
        for (int j = 0; j < value.Length; j++) {
          if (rand.NextDouble() < 0.2) {
            value[j] = ' ';
          }
        }
        yield return new string(value);
      }
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() { "", "A", "*", " ", "s", "B ", "  ", " D", "ef", "!!",
        " F ", "T L", "4ps", "q  ", "   ", "  e", "hi ", "  $  ",
        "      9", "i !i !i !i !i", "88888888888888888888",
        "                    ", "ssssssssssssssssssss",
        "1 1 1 1 1 1 1 1 1 1 ", " v v v v v v v v v v",
        "Ha Ha Ha Ha Ha Ha Ha", "x y!x y!x y!x y!x y!",
        "G5G5G5G5G5G5G5G5G5G5", ">_=]>_=]>_=]>_=]>_=]",
        "^_^ ^_^ ^_^ ^_^ ^_^ "
      };
    }
  }
}

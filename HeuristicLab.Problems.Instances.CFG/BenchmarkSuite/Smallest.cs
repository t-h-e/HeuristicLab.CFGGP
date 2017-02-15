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
using HeuristicLab.Core;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Smallest : BenchmarkSuiteDataDescritpor<List<int>> {
    public override string Name { get { return "Smallest"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Smallest"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer, DataType.Integer, DataType.Integer, DataType.Integer }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean }; } }

    protected override IEnumerable<List<int>> GenerateTraining() {
      List<List<int>> smallest = GetHardcodedTrainingSamples();
      smallest.AddRange(GetQuadrupel(5, rand));
      smallest.AddRange(GetTriple(10, rand));
      smallest.AddRange(GetNonNegativeSingles(20, rand));
      smallest.AddRange(GetRandom(60, rand));
      return smallest;
    }

    protected override IEnumerable<List<int>> GenerateTest() {
      var smallest = GetQuadrupel(100, rand).ToList();
      smallest.AddRange(GetTriple(100, rand));
      smallest.AddRange(GetNonNegativeSingles(200, rand));
      smallest.AddRange(GetRandom(600, rand));
      return smallest;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<int>> smallest) {
      var input = smallest.Select(x => String.Join(", ", x)).ToArray();
      var output = smallest.Select(x => { x.Sort(); return x.First().ToString(); }).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private IEnumerable<List<int>> GetQuadrupel(int n, IRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return Enumerable.Repeat(rand.Next(-100, 100), 4).ToList();
      }
    }

    private IEnumerable<List<int>> GetTriple(int n, IRandom rand) {
      for (int i = 0; i < n; i++) {
        var temp = Enumerable.Repeat(rand.Next(-100, 100), 3).ToList();
        temp.Add(rand.Next(-100, 100));
        yield return temp.Shuffle(rand).ToList();
      }
    }

    private IEnumerable<List<int>> GetNonNegativeSingles(int n, IRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return new List<int>(4) { rand.Next(0, 100), rand.Next(0, 100), rand.Next(0, 100), rand.Next(0, 100) };
      }
    }

    private IEnumerable<List<int>> GetRandom(int n, IRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return new List<int>(4) { rand.Next(-100, 100), rand.Next(-100, 100), rand.Next(-100, 100), rand.Next(-100, 100) };
      }
    }

    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {0, 0, 0, 0},
            new List<int>() {-44, -44, -7, -13},
            new List<int>() {0, 4, -99, -33},
            new List<int>() {-22, -22, -22, -22},
            new List<int>() {99, 100, 99, 100}
      };
    }
  }
}

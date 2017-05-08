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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public static class ValueGenerator {

    //copied from HeuristicLab.Problems.Instances.DataAnalysis.ValueGenerator which is not public
    /// <summary>
    /// Generates uniformly distributed values between start and end (inclusive!) 
    /// </summary>
    /// <param name="n">Number of values to generate.</param>
    /// <param name="start">The lower value (inclusive)</param>
    /// <param name="end">The upper value (inclusive)</param>
    /// <returns>An enumerable including n values in [start, end]</returns>
    public static IEnumerable<double> GenerateUniformDistributedValues(int n, double start, double end, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        // we need to return a random value including end.
        // so we cannot use rand.NextDouble() as it returns a value strictly smaller than 1.
        double r = rand.NextUInt() / (double)uint.MaxValue;    // r \in [0,1]
        yield return r * (end - start) + start;
      }
    }

    public static IEnumerable<int> GenerateUniformDistributedValues(int n, int start, int end, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        yield return rand.Next(start, end + 1);
      }
    }

    public static IEnumerable<long> GenerateUniformDistributedValues(int n, long start, long end, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        double r = rand.NextDouble(); // r \in [0, 1)
        r = r * ((end + 1) - start) + start; // r \in [start, end + 1), the "+ 1" makes sure that it is uniformlly distibuted when using floor in the next line
        yield return (long)Math.Floor(r);
      }
    }

    public static IEnumerable<T> SampleRandomWithoutRepetition<T>(IEnumerable<T> enumerable, int count, FastRandom rand) {
      return enumerable.SampleRandomWithoutRepetition(rand, count);
    }


    public static IEnumerable<List<int>> GenerateUniformDistributedLists(int n, IEnumerable<int> sizes, int start, int end, FastRandom rand) {
      foreach (var s in sizes) {
        List<int> l = new List<int>(s);
        for (int j = 0; j < s; j++) {
          l.Add(rand.Next(start, end + 1));
        }
        yield return l;
      }
    }
  }
}

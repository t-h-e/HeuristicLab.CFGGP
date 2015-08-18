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
using System.Collections.Generic;
using HeuristicLab.Random;
using System.Text;

namespace HeuristicLab.Problems.Instances.CFG {
  public class XWordLines : CFGArtificialDataDescriptor {
    public override string Name { get { return "X-Word Lines"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "XWordLines"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 150; } }
    protected override int TestPartitionStart { get { return 150; } }
    protected override int TestPartitionEnd { get { return 2150; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<Tuple<string, int>> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetRandomTuple(104, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(GetRandomTuple(2000, rand).ToList());

      var input = strings.Select(x => String.Format("\"{0}\", {1}", x.Item1, x.Item2)).ToArray();
      var output = strings.Select(x => CalcXWordLines(x.Item1, x.Item2)).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcXWordLines(string a, int x) {
      var split = a.Split(new char[] { '\n', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
      StringBuilder strBuilder = new StringBuilder();
      int count = 0;
      foreach (var item in split) {
        strBuilder.Append(item);
        count++;
        bool newline = count >= x;
        strBuilder.Append(newline ? '\n' : ' ');
        if (newline) { count = 0; }
      }
      return strBuilder.ToString();
    }

    private static int[] intValues = Enumerable.Range(1, 10).Concat(Enumerable.Range(1, 5).Concat(Enumerable.Range(1, 3))).ToArray();

    private IEnumerable<Tuple<string, int>> GetRandomTuple(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        string strValue = GetRandomString(rand.Next(0, 100), rand);
        int intValue = intValues[rand.Next(0, intValues.Length)];
        yield return new Tuple<string, int>(strValue, intValue);
      }
    }

    private string GetRandomString(int length, FastRandom rand) {
      StringBuilder strBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++) {
        strBuilder.Append(GetRandomChar(rand));
      }
      return strBuilder.ToString();
    }

    private char GetRandomChar(FastRandom rand) {
      if (rand.NextDouble() < 0.15) return ' ';
      if (rand.NextDouble() < 0.2) return '\n';
      int value = rand.Next(0, 94);
      return (char)(value + 32);
    }

    private List<Tuple<string, int>> GetHardcodedTrainingSamples() {
      return new List<Tuple<string, int>>() {
          new Tuple<string, int>("", 1),
          new Tuple<string, int>("", 4),
          new Tuple<string, int>("A", 1),
          new Tuple<string, int>("*", 6),
          new Tuple<string, int>(" ", 7),
          new Tuple<string, int>("\n", 1),
          new Tuple<string, int>("s", 2),
          new Tuple<string, int>("B ", 1),
          new Tuple<string, int>("  ", 1),
          new Tuple<string, int>(" D", 2),
          new Tuple<string, int>("2\n", 1),
          new Tuple<string, int>("ef", 1),
          new Tuple<string, int>("!!", 1),
          new Tuple<string, int>(" F ", 1),
          new Tuple<string, int>("T L", 1),
          new Tuple<string, int>("4 s", 2),
          new Tuple<string, int>("o\n&", 1),
          new Tuple<string, int>("e\ne", 2),
          new Tuple<string, int>("q  ", 1),
          new Tuple<string, int>("\n e", 1),
          new Tuple<string, int>("hi ", 4),
          new Tuple<string, int>("q e\n", 1),
          new Tuple<string, int>("  $  ", 3),
          new Tuple<string, int>("\n\n\nr\n", 1),
          new Tuple<string, int>("9r 2 33 4", 1),
          new Tuple<string, int>("9 22 3d 4r", 2),
          new Tuple<string, int>("9 2a 3 4 g", 2),
          new Tuple<string, int>("9 2a 3 4 g", 10),
          new Tuple<string, int>("  hi   there  \n  world  lots    of\nspace         here !   \n \n", 3),
          new Tuple<string, int>("Well well, what is this?\n211 days in a row that you've stopped by to see ME?\nThen, go away!", 3),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("i !", 25)) + "i", 4),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat(" ", 100)), 6),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("\n", 100)), 1),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("s", 100)), 7),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("$ ", 50)), 1),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("1 ", 50)), 4),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("\nr", 50)), 1),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("\nv", 50)), 10),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("d\n ", 33)) + "d", 10),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("Ha ", 33)) + "H", 9),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("x y!", 25)), 5),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("K h\n", 25)), 1),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("G w\n", 25)), 8),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("  3  \n\n  ", 11)) + " ", 3),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat(">_=]", 25)) + " ", 2),
          new Tuple<string, int>(String.Concat(Enumerable.Repeat("^_^ ", 25)) + " ", 1),
      };
    }
  }
}

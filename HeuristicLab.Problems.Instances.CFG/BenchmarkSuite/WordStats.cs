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
  public class WordStats : CFGArtificialDataDescriptor {
    public override string Name { get { return "Word Stats"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "WordStats"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      FastRandom rand = new FastRandom();
      List<string> strings = GetHardcodedTrainingSamples();
      strings.AddRange(GetRandomString(64, rand).ToList());

      strings = strings.Shuffle(rand).ToList();

      strings.AddRange(GetRandomString(1000, rand).ToList());

      var input = strings.Select(x => x.PrepareStringForPython()).ToArray();
      var output = strings.Select(x => CalcWordStats(x).PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private static readonly char[] terminators = new char[] { '.', '!', '?' };

    private string CalcWordStats(string x) {
      StringBuilder strBuilder = new StringBuilder();

      int sentences = x.Count(y => terminators.Contains(y));

      var words = x.Split(terminators.Concat(new char[] { ' ', '\t', '\n' }).ToArray()).OrderBy(y => y.Length).ToArray();
      int lenght = 1;
      int count = 0;
      int index = 0;

      while (index < words.Length) {
        if (words[index].Length <= lenght) {
          count++;
          index++;
        } else {
          strBuilder.Append(String.Format("words of length {0}: {1}\n", lenght, count));
          lenght++;
          count = 0;
        }
      }

      strBuilder.Append(String.Format("words of length {0}: {1}\n", lenght, count));

      strBuilder.Append(String.Format("number of sentences: {0}\n", sentences));
      strBuilder.Append(String.Format("average sentence length: {0:0.#####}\n", sentences));
      return strBuilder.ToString();
    }

    private IEnumerable<string> GetRandomString(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        int length = rand.Next(1, 100);
        var value = GetRandomChar(length, rand).ToArray();

        if (!value.Any(x => terminators.Contains(x))) {
          value[rand.Next(0, value.Length - 1)] = terminators[rand.Next(0, terminators.Length - 1)];
        }
        yield return new String(value);
      }
    }

    private IEnumerable<char> GetRandomChar(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        double prob = rand.NextDouble();
        double tabProb = 0.01 + 0.02 * rand.NextDouble(); //prob of tab is between 0.01 and 0.03
        double newlineProb = 0.02 + 0.05 * rand.NextDouble(); //prob of newline is between 0.02 and 0.07
        double spaceProb = 0.05 + 0.3 * rand.NextDouble();//prob of space is between 0.05 and 0.35
        double terminatorProb = 0.01 + 0.19 * rand.NextDouble(); //prob of sentence terminator is between 0.01 and 0.2

        if (prob < tabProb) yield return '\t';
        if (prob < tabProb + newlineProb) yield return '\n';
        if (prob < tabProb + newlineProb + spaceProb) yield return ' ';
        if (prob < tabProb + newlineProb + spaceProb + terminatorProb) yield return terminators[rand.Next(0, terminators.Length - 1)];
        else yield return (char)rand.Next(33, 126);
      }
    }

    private List<string> GetHardcodedTrainingSamples() {
      return new List<string>() {
        ".", "!", "?", "\t.", "\n!", " ?", ".#", "A.\n", "! \n", "?\t\n", "\n?\n",
          ".!?.!?", ".txt", "!RACECAR!", "www.google.com",
          "Pirate basketball? Envelope WARS!",
          ".hello there wo.RLD",
          "out. at. the. plate.",
          "nap time on planets!",
          "supercalifragilisticexpialidocious?",
          String.Concat(Enumerable.Repeat('\n', 99)) + '.',
          String.Concat(Enumerable.Repeat('=', 99)) + '?',
          '!' + String.Concat(Enumerable.Repeat(' ', 99)),
          '.' + String.Concat(Enumerable.Repeat('h', 99)),
          String.Concat(Enumerable.Repeat('\t', 99)) + '?',
          String.Concat(Enumerable.Repeat('@', 99)) + '!',
          String.Concat(Enumerable.Repeat('.', 100)),
          String.Concat(Enumerable.Repeat('!', 100)),
          String.Concat(Enumerable.Repeat('?', 100)),
          String.Concat(Enumerable.Repeat(".\n", 50)),
          String.Concat(Enumerable.Repeat("?\n\n", 33)) + '?',
          String.Concat(Enumerable.Repeat("!D\n", 33)) + '!',
          String.Concat(Enumerable.Repeat("! ", 50)),
          String.Concat(Enumerable.Repeat(".\t", 50)),
          String.Concat(Enumerable.Repeat("?\ny ", 25)),
          String.Concat(Enumerable.Repeat("5!", 50)),
      };
    }
  }
}

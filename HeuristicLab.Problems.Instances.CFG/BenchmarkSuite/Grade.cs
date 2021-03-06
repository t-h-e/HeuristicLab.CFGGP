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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.CFG {
  public class Grade : BenchmarkSuiteDataDescritpor<List<int>> {
    public override string Name { get { return "Grade"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "Grade"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 200; } }
    protected override int TestPartitionStart { get { return 200; } }
    protected override int TestPartitionEnd { get { return 2200; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer, DataType.Integer, DataType.Integer, DataType.Integer, DataType.Integer }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String }; } }

    protected override IEnumerable<List<int>> GenerateTraining() {
      List<List<int>> grades = GetHardcodedTrainingSamples();
      grades.AddRange(CreateThresholdsAndGrade(159, rand));
      return grades;
    }

    protected override IEnumerable<List<int>> GenerateTest() {
      return CreateThresholdsAndGrade(2000, rand).ToList();
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<List<int>> grades) {
      var input = grades.Select(x => String.Join(", ", x)).ToArray();
      var output = grades.Select(x => CalcGrade(x).PrepareStringForPython()).ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    private string CalcGrade(List<int> x) {
      int score = x.Last();
      if (score >= x[0]) return "A";
      else if (score >= x[1]) return "B";
      else if (score >= x[2]) return "C";
      else if (score >= x[3]) return "D";
      else return "F";
    }

    private IEnumerable<List<int>> CreateThresholdsAndGrade(int n, FastRandom rand) {
      for (int i = 0; i < n; i++) {
        List<int> grades = Enumerable.Range(0, 101).SampleRandomWithoutRepetition(rand, 4).Reverse().ToList();

        // perfect uniform distribution (as close to perfect as possible)
        if (i % 5 < 3) {
          grades.Add(rand.Next(grades[(i % 5) + 1], grades[i % 5]));
        } else if (i % 5 == 3) {
          grades.Add(rand.Next(0, grades[3]));
        } else {
          grades.Add(rand.Next(grades[0], 101));
        }
        yield return grades;
      }
    }

    private List<List<int>> GetHardcodedTrainingSamples() {
      return new List<List<int>>() {
            new List<int>() {80, 70, 60, 50, 85},
            new List<int>() {80, 70, 60, 50, 80},
            new List<int>() {80, 70, 60, 50, 79},
            new List<int>() {80, 70, 60, 50, 75},
            new List<int>() {80, 70, 60, 50, 70},
            new List<int>() {80, 70, 60, 50, 69},
            new List<int>() {80, 70, 60, 50, 65},
            new List<int>() {80, 70, 60, 50, 60},
            new List<int>() {80, 70, 60, 50, 59},
            new List<int>() {80, 70, 60, 50, 55},
            new List<int>() {80, 70, 60, 50, 50},
            new List<int>() {80, 70, 60, 50, 49},
            new List<int>() {80, 70, 60, 50, 45},
            new List<int>() {90, 80, 70, 60, 100},
            new List<int>() {90, 80, 70, 60, 0},
            new List<int>() {4, 3, 2, 1, 5},
            new List<int>() {4, 3, 2, 1, 4},
            new List<int>() {4, 3, 2, 1, 3},
            new List<int>() {4, 3, 2, 1, 2},
            new List<int>() {4, 3, 2, 1, 1},
            new List<int>() {4, 3, 2, 1, 0},
            new List<int>() {100, 99, 98, 97, 100},
            new List<int>() {100, 99, 98, 97, 99},
            new List<int>() {100, 99, 98, 97, 98},
            new List<int>() {100, 99, 98, 97, 97},
            new List<int>() {100, 99, 98, 97, 96},
            new List<int>() {98, 48, 27, 3, 55},
            new List<int>() {98, 48, 27, 3, 14},
            new List<int>() {98, 48, 27, 3, 1},
            new List<int>() {45, 30, 27, 0, 1},
            new List<int>() {45, 30, 27, 0, 0},
            new List<int>() {48, 46, 44, 42, 40},
            new List<int>() {48, 46, 44, 42, 41},
            new List<int>() {48, 46, 44, 42, 42},
            new List<int>() {48, 46, 44, 42, 43},
            new List<int>() {48, 46, 44, 42, 44},
            new List<int>() {48, 46, 44, 42, 45},
            new List<int>() {48, 46, 44, 42, 46},
            new List<int>() {48, 46, 44, 42, 47},
            new List<int>() {48, 46, 44, 42, 48},
            new List<int>() {48, 46, 44, 42, 49}
      };
    }

    protected override void ModifyGrammar(Grammar g) {
      g.Rules.Remove("<string_literal>");
      g.Rules.Remove("<string_const_part>");
      g.Rules.Remove("<string_const>");
      var partialGrammar = GrammarParser.ReadGrammarBNF("<string_const> ::= \"'\"<string_literal>\"'\"" + Environment.NewLine +
                                                        "<string_literal> ::= 'A' | 'B' | 'C' | 'D' | 'F'");
      g.Combine(partialGrammar);
    }
  }
}

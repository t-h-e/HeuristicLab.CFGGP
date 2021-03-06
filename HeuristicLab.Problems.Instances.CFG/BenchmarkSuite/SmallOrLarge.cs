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
  public class SmallOrLarge : BenchmarkSuiteDataDescritpor<int> {
    public override string Name { get { return "Small Or Large"; } }
    public override string Description {
      get {
        return "";
      }
    }
    public override string Identifier { get { return "SmallOrLarge"; } }
    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return 100; } }
    protected override int TestPartitionStart { get { return 100; } }
    protected override int TestPartitionEnd { get { return 1100; } }

    protected override IEnumerable<DataType> InputDataTypes { get { return new List<DataType>() { DataType.Integer }; } }
    protected override IEnumerable<DataType> OutputDataTypes { get { return new List<DataType>() { DataType.String }; } }
    protected override HashSet<DataType> AdditionalDataTypes { get { return new HashSet<DataType>() { DataType.Integer, DataType.Boolean, DataType.String }; } }

    protected override IEnumerable<int> GenerateTraining() {
      var x0 = new List<int>(1100) { -10000, 0, 980, 1020, 1980, 2020, 10000 };
      x0.AddRange(Enumerable.Range(995, 10));
      x0.AddRange(Enumerable.Range(1995, 10));
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(73, -10000, 10000, rand));
      return x0;
    }

    protected override IEnumerable<int> GenerateTest() {
      var x0 = Enumerable.Range(980, 40).ToList();
      x0.AddRange(Enumerable.Range(1980, 40));
      x0.AddRange(ValueGenerator.GenerateUniformDistributedValues(920, -10000, 10000, rand));
      return x0;
    }

    protected override Tuple<string[], string[]> GenerateInputOutput(IEnumerable<int> x0) {
      var input = x0.Select(x => x.ToString()).ToArray();
      var output = x0.Select(x => x < 1000 ? "\"small\"" : x >= 2000 ? "\"large\"" : "\"\"").ToArray();
      return new Tuple<string[], string[]>(input, output);
    }

    protected override void ModifyGrammar(Grammar g) {
      var partialGrammar = GrammarParser.ReadGrammarBNF("<string_const> ::= \"'small'\" | \"'large'\"");
      g.Combine(partialGrammar);
    }
  }
}

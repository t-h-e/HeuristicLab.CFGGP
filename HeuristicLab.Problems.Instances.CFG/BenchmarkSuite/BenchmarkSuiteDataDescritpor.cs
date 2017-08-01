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
  public abstract class BenchmarkSuiteDataDescritpor<T> : BenchmarkSuiteDataDescritpor {
    protected abstract IEnumerable<T> GenerateTraining();
    protected abstract IEnumerable<T> GenerateTest();

    protected abstract Tuple<string[], string[]> GenerateInputOutput(IEnumerable<T> trainingAndTest);

    protected override Tuple<string[], string[]> GenerateInputOutput() {
      var training = GenerateTraining();

      training = training.Shuffle(rand);

      var test = GenerateTest();

      var all = new List<T>();
      all.AddRange(training);
      all.AddRange(test);

      return GenerateInputOutput(all);
    }
  }

  public abstract class BenchmarkSuiteDataDescritpor : CFGArtificialDataDescriptor {
    protected static FastRandom rand;

    protected abstract IEnumerable<DataType> InputDataTypes { get; }
    protected abstract IEnumerable<DataType> OutputDataTypes { get; }
    protected abstract HashSet<DataType> AdditionalDataTypes { get; }

    public override CFGData GenerateData() {
      return GenerateData(false, 3);
    }

    public CFGData GenerateData(bool treeStructure, int numberOfVariables) {
      // Always generate the same dataset 
      rand = new FastRandom(0);
      var cfgData = base.GenerateData();
      cfgData.Grammar = GenerateGrammar(treeStructure, numberOfVariables);
      return cfgData;
    }

    public string GenerateGrammar(bool treeStructure, int numberOfVariables) {
      var grammarConstructor = new PythonGrammarConstructor();
      Options options = new Options(InputDataTypes, OutputDataTypes, AdditionalDataTypes, treeStructure, numberOfVariables);
      var g = grammarConstructor.CombineDataTypes(options);
      ModifyGrammar(g);
      g.TrimGrammar(true);
      return g.PrintGrammar();
    }

    /// <summary>
    /// Make additional adaptations to the generated grammar
    /// 
    /// Mainly used to add teminal symbols used in the original benchmark suite
    /// </summary>
    protected virtual void ModifyGrammar(Grammar g) {
      /* do nothing*/
    }
  }
}

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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {

  [Item("CFGSortingNetworkEvaluator", "An evaluator that can be programmed for arbitrary CFG problems.")]
  [StorableClass]
  public class CFGSortingNetworkEvaluator : InstrumentedOperator, ICFGEvaluator {
    public virtual bool EnabledByDefault {
      get { return false; }
    }

    #region parameters
    public ILookupParameter<ISymbolicExpressionTree> ProgramParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["Program"]; }
    }
    public ILookupParameter<StringValue> HeaderParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Header"]; }
    }
    public ILookupParameter<StringValue> FooterParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Footer"]; }
    }
    public ILookupParameter<ICFGProblemData> ProblemDataParameter {
      get { return (ILookupParameter<ICFGProblemData>)Parameters["ProblemData"]; }
    }
    public ILookupParameter<BoolArray> SuccessfulCasesParameter {
      get { return (ILookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public ILookupParameter<DoubleValue> QualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters["Quality"]; }
    }
    public IValueLookupParameter<LongMatrix> InputParameter {
      get { return (IValueLookupParameter<LongMatrix>)Parameters["Input"]; }
    }
    public IValueLookupParameter<LongMatrix> OutputParameter {
      get { return (IValueLookupParameter<LongMatrix>)Parameters["Output"]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGSortingNetworkEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGSortingNetworkEvaluator(CFGSortingNetworkEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGSortingNetworkEvaluator()
      : base() {
      Parameters.Add(new LookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("Program", "The program to evaluate."));
      Parameters.Add(new LookupParameter<StringValue>("Header", "The header of the program."));
      Parameters.Add(new LookupParameter<StringValue>("Footer", "The footer of the program."));
      Parameters.Add(new LookupParameter<ICFGProblemData>("ProblemData", "The problem data on which the context free grammer solution should be evaluated."));
      Parameters.Add(new LookupParameter<BoolArray>("Cases", "The training cases that have been successfully executed."));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value aka fitness value of the solution."));
      Parameters.Add(new ValueLookupParameter<LongMatrix>("Input", ""));
      Parameters.Add(new ValueLookupParameter<LongMatrix>("Output", ""));
      Parameters.ForEach(x => x.Hidden = false);
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGSortingNetworkEvaluator(this, cloner);
    }

    public override IOperation InstrumentedApply() {
      int steps = 0;
      int swaps = 400;

      int LONGSIZE = 64;
      int N_SORTING_NETWORK = 10;

      // create data
      if (InputParameter.ActualValue == null || OutputParameter.ActualValue == null) {
        long[,] helpInputs;
        long[,] helpOutputs;
        long fitnessCases = (long)Math.Pow(2, N_SORTING_NETWORK);
        int arrayLength = (int)(fitnessCases / LONGSIZE);
        arrayLength = (fitnessCases % LONGSIZE) != 0
                ? arrayLength + 1
                : arrayLength;
        helpInputs = new long[N_SORTING_NETWORK, arrayLength];
        helpOutputs = new long[N_SORTING_NETWORK, arrayLength];

        for (long i = 0; i < fitnessCases; i++) {
          int arrayPos = (int)(i / LONGSIZE);
          int bitPos = (int)(i % LONGSIZE);

          for (int j = 0; j < N_SORTING_NETWORK; j++) {
            helpInputs[j, arrayPos] |= ((i >> j) & 1L) << bitPos;
          }

          long help = i;
          help = help - ((help >> 1) & 0x5555555555555555);
          help = (help & 0x3333333333333333) + ((help >> 2) & 0x3333333333333333);
          int bitCount = (int)((((help + (help >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);


          for (int j = N_SORTING_NETWORK - bitCount; j < N_SORTING_NETWORK; j++) {
            helpOutputs[j, arrayPos] |= 1L << bitPos;
          }
        }
        InputParameter.Value = new LongMatrix(helpInputs);
        OutputParameter.Value = new LongMatrix(helpOutputs);
      }

      LongMatrix inputs = InputParameter.ActualValue;
      LongMatrix outputs = OutputParameter.ActualValue;

      string phenotype = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(ProgramParameter.ActualValue);

      // minimize sorting network from phenotype
      string[] split = phenotype.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      int[] temp = new int[split.Length];
      int pos = 0;
      for (int i = 0; i < split.Length; i += 2) {
        int first = int.Parse(split[i]);
        int second = int.Parse(split[i + 1]);
        if (first == second) {
          continue;
        }
        if (pos >= 2 && temp[pos - 2] == first && temp[pos - 1] == second) {
          continue;
        }
        temp[pos] = first;
        temp[pos + 1] = second;
        pos += 2;

      }
      int[] sortingNetwork = new int[pos];
      Array.Copy(temp, sortingNetwork, pos);


      // copy porblem data
      LongMatrix sorting = inputs;


      // execute sorting network
      int curswap = 0;
      for (int i = 0; i < sortingNetwork.Length; i += 2) {
        int firstLine = sortingNetwork[i];
        int secondLine = sortingNetwork[i + 1];

        //Todo: check time improvement if sorting[firstLine] and sorting[secondLine] are set to variables
        for (int j = 0; j < sorting.Columns; j++) {
          long max = sorting[firstLine, j] & sorting[secondLine, j];
          long min = sorting[firstLine, j] | sorting[secondLine, j];
          sorting[firstLine, j] = min;
          sorting[secondLine, j] = max;
        }
        steps++;
        curswap++;
        if (curswap > swaps) {
          break;
        }
      }


      // calculate fitness cases
      for (int i = 0; i < outputs.Rows; i++) {
        for (int j = 0; j < outputs.Columns; j++) {
          sorting[i, j] ^= outputs[i, j];
        }
      }

      // starts at 1
      for (int i = 1; i < sorting.Rows; i++) {
        for (int j = 0; j < sorting.Columns; j++) {
          sorting[0, j] |= sorting[i, j];
        }
      }

      double fitness = 0;
      bool[] cases = new bool[N_SORTING_NETWORK * LONGSIZE + LONGSIZE];
      for (int j = 0; j < sorting.Rows; j++) {
        for (int k = 0; k < LONGSIZE; k++) {
          cases[j * LONGSIZE + k] = (sorting[0, j] & (1 << k)) != 0;
          if (cases[j * LONGSIZE + k]) fitness++;

        }
      }

      SuccessfulCasesParameter.ActualValue = new BoolArray(cases);
      QualityParameter.ActualValue = new DoubleValue(fitness);

      return base.InstrumentedApply();
    }

    private int countBits(long help) {
      help = help - ((help >> 1) & 0x5555555555555555);
      help = (help & 0x3333333333333333) + ((help >> 2) & 0x3333333333333333);
      return (int)((((help + (help >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56);
    }
  }
}

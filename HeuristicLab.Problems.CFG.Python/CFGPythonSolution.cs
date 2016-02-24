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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  /// <summary>
  /// Python CFG solution
  /// </summary>
  [StorableClass]
  [Item(Name = "CFGPythonSolution", Description = "Represents a Python context free grammar solution and attributes of the solution like accuracy and complexity.")]
  public class CFGPythonSolution : CFGSolution {
    private const string CodeResultName = "Code";
    private const string ProgramResultName = "Program";

    private const string TrainingException = "Training Exception";
    private const string TrainingQuality = "Training Quality";
    private const string TrainingSolvedCases = "Training Solved Cases";
    private const string TrainingSolvedCasesPercentage = "Training Solved Cases Percentage";
    private const string TrainingSolvedCaseQualities = "Training Case Qualities";

    private const string TestException = "Test Exception";
    private const string TestQuality = "Test Quality";
    private const string TestSolvedCases = "Test Solved Cases";
    private const string TestSolvedCasesPercentage = "Test Solved Cases Percentage";
    private const string TestSolvedCaseQualities = "Test Case Qualities";

    [StorableConstructor]
    protected CFGPythonSolution(bool deserializing) : base(deserializing) { }
    protected CFGPythonSolution(CFGPythonSolution original, Cloner cloner)
      : base(original, cloner) {
      name = original.Name;
      description = original.Description;
    }
    public CFGPythonSolution(ISymbolicExpressionTree tree, ICFGPythonProblemData problemData, IntValue timeout)
      : base(tree, problemData) {
      name = ItemName;
      description = ItemDescription;

      string header = problemData.HelperCode == null
                      ? String.Empty
                      : problemData.HelperCode.Value + Environment.NewLine;
      header += problemData.Header == null
                      ? String.Empty
                      : problemData.Header.Value;
      string footer = problemData.Footer == null
                      ? String.Empty
                      : problemData.Footer.Value;

      string program = PythonHelper.FormatToProgram(tree, header, footer);
      Add(new Result(ProgramResultName, "The program with header and footer", new TextValue(program)));
      string code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      Add(new Result(CodeResultName, "The code that was evolved", new TextValue(code)));

      var trainingTimeout = timeout.Value * 2;  // increase timeout to make sure it finishes
      var training = PythonProcessHelper.EvaluateProgram(program, problemData.Input, problemData.Output, problemData.TrainingIndices, trainingTimeout);

      // test timeout should be proportionally bigger than training timeout
      var testTimeout = (int)((double)problemData.TestIndices.Count() / (double)problemData.TrainingIndices.Count() * trainingTimeout);
      testTimeout = testTimeout > timeout.Value ? testTimeout : timeout.Value;

      var test = PythonProcessHelper.EvaluateProgram(program, problemData.Input, problemData.Output, problemData.TestIndices, testTimeout);

      if (String.IsNullOrEmpty(training.Item4)) {
        Add(new Result(TrainingQuality, "Training quality", new DoubleValue(training.Item3)));
        var cases = training.Item1.ToArray();
        Add(new Result(TrainingSolvedCases, "Training cases which have been solved", new BoolArray(cases)));
        Add(new Result(TrainingSolvedCasesPercentage, "Percentage of training cases which have been solved", new PercentValue((double)cases.Count(x => x) / (double)cases.Length)));
        Add(new Result(TrainingSolvedCaseQualities, "The quality of each training case", new DoubleArray(training.Item2.ToArray())));
      } else {
        Add(new Result(TrainingException, "Exception occured during training", new TextValue(training.Item4)));
        Add(new Result(TrainingQuality, "Training quality", new DoubleValue(Double.NaN)));
        Add(new Result(TrainingSolvedCases, "Training cases which have been solved", new BoolArray(problemData.TrainingIndices.Count())));
        Add(new Result(TrainingSolvedCasesPercentage, "Percentage of training cases which have been solved", new PercentValue(0)));
        Add(new Result(TrainingSolvedCaseQualities, "The quality of each training case", new DoubleArray(Enumerable.Repeat(Double.NaN, problemData.TrainingIndices.Count()).ToArray())));
      }

      if (String.IsNullOrEmpty(test.Item4)) {
        Add(new Result(TestQuality, "Test quality", new DoubleValue(test.Item3)));
        var cases = test.Item1.ToArray();
        Add(new Result(TestSolvedCases, "Test cases which have been solved", new BoolArray(cases)));
        Add(new Result(TestSolvedCasesPercentage, "Percentage of test cases which have been solved", new PercentValue((double)cases.Count(x => x) / (double)cases.Length)));
        Add(new Result(TestSolvedCaseQualities, "The quality of each test case", new DoubleArray(test.Item2.ToArray())));
      } else {
        Add(new Result(TestException, "Exception occured during test", new TextValue(test.Item4)));
        Add(new Result(TestQuality, "Test quality", new DoubleValue(Double.NaN)));
        Add(new Result(TestSolvedCases, "Test cases which have been solved", new BoolArray(problemData.TestIndices.Count())));
        Add(new Result(TestSolvedCasesPercentage, "Percentage of test cases which have been solved", new PercentValue(0)));
        Add(new Result(TestSolvedCaseQualities, "The quality of each test case", new DoubleArray(Enumerable.Repeat(Double.NaN, problemData.TestIndices.Count()).ToArray())));
      }
    }
  }
}

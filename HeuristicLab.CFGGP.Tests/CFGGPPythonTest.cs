#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Analysis;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.Xml;
using HeuristicLab.Problems.CFG.Python;
using HeuristicLab.Problems.Instances.CFG;
using HeuristicLab.Selection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class CFGGPPythonTest {
    private const string SampleFileName = "CFGGP_Python";

    [TestMethod]
    [TestCategory("Samples.Create")]
    [TestProperty("Time", "medium")]
    public void CreateGpSymbolicRegressionSampleTest() {
      var ga = CreateGpSymbolicRegressionSample();
      string path = Path.Combine(SampleFileName + SamplesUtils.SampleFileExtension);
      XmlGenerator.Serialize(ga, path);
    }
    [TestMethod]
    [TestCategory("Samples.Execute")]
    [TestProperty("Time", "long")]
    public void RunGpSymbolicRegressionSampleTest() {
      var ga = CreateGpSymbolicRegressionSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 1409040336;
      SamplesUtils.RunAlgorithm(ga);
      Assert.AreEqual(1131598, SamplesUtils.GetDoubleResult(ga, "BestQuality"));
      Assert.AreEqual(6614861.044, SamplesUtils.GetDoubleResult(ga, "CurrentAverageQuality"));
      Assert.AreEqual(624633080, SamplesUtils.GetDoubleResult(ga, "CurrentWorstQuality"));
      Assert.AreEqual(10480, SamplesUtils.GetIntResult(ga, "EvaluatedSolutions"));
      var bestTrainingSolution = (CFGPythonSolution)ga.Results["Best training solution"].Value;
      Assert.AreEqual(1131598, ((DoubleValue)bestTrainingSolution["Training Quality"].Value).Value);
      Assert.AreEqual(12513702, ((DoubleValue)bestTrainingSolution["Test Quality"].Value).Value);
      Assert.AreEqual(0.07, ((PercentValue)bestTrainingSolution["Training Solved Cases Percentage"].Value).Value);
      Assert.AreEqual(0.0245, ((PercentValue)bestTrainingSolution["Test Solved Cases Percentage"].Value).Value);
      var exceptionTable = (DataTable)ga.Results["Exception frequencies"].Value;
      Assert.AreEqual(1, exceptionTable.Rows.Count);
      Assert.IsTrue(exceptionTable.Rows.ContainsKey("No Exception"));
      Assert.IsTrue(exceptionTable.Rows["No Exception"].Values.All(x => x == 1));
    }

    [TestMethod]
    [TestCategory("Samples.Execute")]
    [TestProperty("Time", "long")]
    public void RunGpSymbolicRegressionSampleTestSolutionFound() {
      var ga = CreateGpSymbolicRegressionSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 1901283838;
      SamplesUtils.RunAlgorithm(ga);
      Assert.AreEqual(0, SamplesUtils.GetDoubleResult(ga, "BestQuality"));
      Assert.AreEqual(10504690.242, SamplesUtils.GetDoubleResult(ga, "CurrentAverageQuality"));
      Assert.AreEqual(1434460000, SamplesUtils.GetDoubleResult(ga, "CurrentWorstQuality"));
      Assert.AreEqual(10480, SamplesUtils.GetIntResult(ga, "EvaluatedSolutions"));
      var bestTrainingSolution = (CFGPythonSolution)ga.Results["Best training solution"].Value;
      Assert.AreEqual(0, ((DoubleValue)bestTrainingSolution["Training Quality"].Value).Value);
      Assert.AreEqual(0, ((DoubleValue)bestTrainingSolution["Test Quality"].Value).Value);
      Assert.AreEqual(1.0, ((PercentValue)bestTrainingSolution["Training Solved Cases Percentage"].Value).Value);
      Assert.AreEqual(1.0, ((PercentValue)bestTrainingSolution["Test Solved Cases Percentage"].Value).Value);
      var exceptionTable = (DataTable)ga.Results["Exception frequencies"].Value;
      Assert.AreEqual(1, exceptionTable.Rows.Count);
      Assert.IsTrue(exceptionTable.Rows.ContainsKey("No Exception"));
      Assert.IsTrue(exceptionTable.Rows["No Exception"].Values.All(x => x == 1));
    }

    [TestMethod]
    [TestCategory("Samples.Execute")]
    [TestProperty("Time", "long")]
    public void DisposePythonProcesses() {
      var ga = CreateGpSymbolicRegressionSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 1901283838;
      var before = Process.GetProcessesByName("python").Length;
      SamplesUtils.RunAlgorithm(ga);
      var after = Process.GetProcessesByName("python").Length;
      Assert.AreEqual(before + Environment.ProcessorCount * 2, after);
      (ga.Problem as CFGPythonProblem).Dispose();
      //Thread.Sleep(1000);
      var afterDispose = Process.GetProcessesByName("python").Length;
      Assert.AreEqual(before, afterDispose);
    }

    private GeneticAlgorithm CreateGpSymbolicRegressionSample() {
      GeneticAlgorithm ga = new GeneticAlgorithm();
      #region Problem Configuration
      CFGPythonProblem cfgPythonProblem = new CFGPythonProblem();
      cfgPythonProblem.Name = "Negative To Zero Problem";
      cfgPythonProblem.Description = "Negative To Zero (described in: http://dl.acm.org/citation.cfm?id=2754769)";
      BenchmarkSuiteInstanceProvider provider = new BenchmarkSuiteInstanceProvider();
      var instance = provider.GetDataDescriptors().Where(x => x.Name.Equals("Negative To Zero")).Single();
      CFGData data = (CFGData)provider.LoadData(instance);
      data.Input = File.ReadAllLines(@"Data\Negative_To_Zero-Input.txt");
      data.Output = File.ReadAllLines(@"Data\Negative_To_Zero-Output.txt");
      cfgPythonProblem.Load(data);

      // configure remaining problem parameters
      cfgPythonProblem.MaximumSymbolicExpressionTreeLengthParameter.Value.Value = 250;
      cfgPythonProblem.MaximumSymbolicExpressionTreeDepthParameter.Value.Value = 100;
      #endregion
      #region Algorithm Configuration
      ga.Problem = cfgPythonProblem;
      ga.Name = "Genetic Programming - CFG Python";
      ga.Description = "A standard genetic programming algorithm to solve a cfg python problem";
      SamplesUtils.ConfigureGeneticAlgorithmParameters<TournamentSelector, SubtreeCrossover, MultiSymbolicExpressionTreeManipulator>(
        ga, 500, 1, 20, 0.05, 5);
      var mutator = (MultiSymbolicExpressionTreeManipulator)ga.Mutator;
      mutator.Operators.SetItemCheckedState(mutator.Operators
        .OfType<FullTreeShaker>()
        .Single(), false);
      mutator.Operators.SetItemCheckedState(mutator.Operators
        .OfType<OnePointShaker>()
        .Single(), false);
      #endregion
      return ga;
    }
  }
}

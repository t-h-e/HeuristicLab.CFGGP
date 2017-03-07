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
using System.Threading;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Analysis;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Persistence.Default.Xml;
using HeuristicLab.Problems.CFG.Python;
using HeuristicLab.Problems.Instances.CFG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class CFGGPPythonTest {
    private const string SampleFileName = "CFGGP_Python";

    [TestMethod]
    [TestCategory("Samples.Create")]
    [TestProperty("Time", "medium")]
    public void CreateCFGGPSampleTest() {
      var ga = CreateCFGGPSample();
      string path = Path.Combine(SampleFileName + SamplesUtils.SampleFileExtension);
      XmlGenerator.Serialize(ga, path);
    }
    [TestMethod]
    [TestCategory("Samples.Execute")]
    [TestProperty("Time", "long")]
    public void RunCFGGPSampleTest() {
      var ga = CreateCFGGPSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 530223337;
      ((CFGPythonProblem)ga.Problem).TimeoutParameter.Value.Value = 10000;
      SamplesUtils.RunAlgorithm(ga);
      Assert.AreEqual(56, SamplesUtils.GetDoubleResult(ga, "BestQuality"));
      Assert.AreEqual(71.2, SamplesUtils.GetDoubleResult(ga, "CurrentAverageQuality"));
      Assert.AreEqual(99, SamplesUtils.GetDoubleResult(ga, "CurrentWorstQuality"));
      Assert.AreEqual(1090, SamplesUtils.GetIntResult(ga, "EvaluatedSolutions"));
      Assert.AreEqual(10, SamplesUtils.GetIntResult(ga, "Best training solution generation"));
      var bestTrainingSolution = (CFGPythonSolution)ga.Results["Best training solution"].Value;
      Assert.AreEqual(56, ((DoubleValue)bestTrainingSolution["Training Quality"].Value).Value);
      Assert.AreEqual(596, ((DoubleValue)bestTrainingSolution["Test Quality"].Value).Value);
      Assert.AreEqual(38, ((IntValue)bestTrainingSolution["Model Depth"].Value).Value);
      Assert.AreEqual(248, ((IntValue)bestTrainingSolution["Model Length"].Value).Value);
      Assert.AreEqual(0.44, ((PercentValue)bestTrainingSolution["Training Solved Cases Percentage"].Value).Value);
      Assert.AreEqual(0.404, ((PercentValue)bestTrainingSolution["Test Solved Cases Percentage"].Value).Value);
      var exceptionTable = (DataTable)ga.Results["Exception frequencies"].Value;
      Assert.AreEqual(1, exceptionTable.Rows.Count);
      Assert.IsTrue(exceptionTable.Rows.ContainsKey("No Exception"));
      Assert.IsTrue(exceptionTable.Rows["No Exception"].Values.All(x => x == 1));
    }

    [TestMethod]
    [TestCategory("Samples.Execute")]
    [TestProperty("Time", "long")]
    public void RunCFGGPSampleTestSolutionFound() {
      var ga = CreateCFGGPSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 461206446;
      ((CFGPythonProblem)ga.Problem).TimeoutParameter.Value.Value = 10000;
      SamplesUtils.RunAlgorithm(ga);
      Assert.AreEqual(0, SamplesUtils.GetDoubleResult(ga, "BestQuality"));
      Assert.AreEqual(13.09, SamplesUtils.GetDoubleResult(ga, "CurrentAverageQuality"));
      Assert.AreEqual(99, SamplesUtils.GetDoubleResult(ga, "CurrentWorstQuality"));
      Assert.AreEqual(1090, SamplesUtils.GetIntResult(ga, "EvaluatedSolutions"));
      Assert.AreEqual(7, SamplesUtils.GetIntResult(ga, "Best training solution generation"));
      var bestTrainingSolution = (CFGPythonSolution)ga.Results["Best training solution"].Value;
      Assert.AreEqual(0, ((DoubleValue)bestTrainingSolution["Training Quality"].Value).Value);
      Assert.AreEqual(0, ((DoubleValue)bestTrainingSolution["Test Quality"].Value).Value);
      Assert.AreEqual(17, ((IntValue)bestTrainingSolution["Model Depth"].Value).Value);
      Assert.AreEqual(62, ((IntValue)bestTrainingSolution["Model Length"].Value).Value);
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
      var ga = CreateCFGGPSample();
      ga.SetSeedRandomly.Value = false;
      ga.Seed.Value = 461206446;
      var before = Process.GetProcessesByName("python").Length;
      SamplesUtils.RunAlgorithm(ga);
      var after = Process.GetProcessesByName("python").Length;
      Assert.AreEqual(before + Environment.ProcessorCount * 2, after);
      (ga.Problem as CFGPythonProblem).Dispose();
      Thread.Sleep(1000); //give it a second
      var afterDispose = Process.GetProcessesByName("python").Length;
      Assert.AreEqual(before, afterDispose);
    }

    private GeneticAlgorithm CreateCFGGPSample() {
      GeneticAlgorithm ga = new GeneticAlgorithm();
      #region Problem Configuration
      CFGPythonProblem cfgPythonProblem = new CFGPythonProblem();
      cfgPythonProblem.Name = "Smallest Problem";
      cfgPythonProblem.Description = "Smallest (described in: http://dl.acm.org/citation.cfm?id=2754769)";
      var provider = new BenchmarkSuiteListInstanceProvider();
      var instance = provider.GetDataDescriptors().Single(x => x.Name.Equals("Smallest"));
      CFGData data = (CFGData)provider.LoadData(instance);
      data.Input = File.ReadAllLines(@"Data\Smallest-Input.txt");
      data.Output = File.ReadAllLines(@"Data\Smallest-Output.txt");
      cfgPythonProblem.Load(data);

      // configure remaining problem parameters
      cfgPythonProblem.MaximumSymbolicExpressionTreeLengthParameter.Value.Value = 250;
      cfgPythonProblem.MaximumSymbolicExpressionTreeDepthParameter.Value.Value = 100;
      #endregion
      #region Algorithm Configuration
      ga.Problem = cfgPythonProblem;
      ga.Name = "Genetic Programming - CFG Python";
      ga.Description = "A standard genetic programming algorithm to solve a cfg python problem";
      SamplesUtils.ConfigureGeneticAlgorithmParameters<LexicaseSelector, SubtreeCrossover, MultiSymbolicExpressionTreeManipulator>(
        ga, 100, 1, 10, 0.05, 5);
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

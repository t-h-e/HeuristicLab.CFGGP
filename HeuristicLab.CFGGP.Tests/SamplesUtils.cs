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
using System.Linq;
using System.Threading;
using HeuristicLab.Algorithms.GeneticAlgorithm;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Selection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.CFGGP.Tests {
  public static class SamplesUtils {
    public const string SamplesDirectory = @"Samples\";
    public const string SampleFileExtension = ".hl";

    public static void RunAlgorithm(IAlgorithm a) {
      var trigger = new EventWaitHandle(false, EventResetMode.ManualReset);
      Exception ex = null;
      a.Stopped += (src, e) => { trigger.Set(); };
      a.ExceptionOccurred += (src, e) => { ex = e.Value; trigger.Set(); };
      a.Prepare();
      a.Start();
      trigger.WaitOne();

      Assert.AreEqual(ex, null);
    }

    public static double GetDoubleResult(IAlgorithm a, string resultName) {
      return ((DoubleValue)a.Results[resultName].Value).Value;
    }

    public static int GetIntResult(IAlgorithm a, string resultName) {
      return ((IntValue)a.Results[resultName].Value).Value;
    }

    public static void ConfigureGeneticAlgorithmParameters<S, C, M>(GeneticAlgorithm ga, int popSize, int elites, int maxGens, double mutationRate, int tournGroupSize = 0)
      where S : ISelector
      where C : ICrossover
      where M : IManipulator {
      ga.Elites.Value = elites;
      ga.MaximumGenerations.Value = maxGens;
      ga.MutationProbability.Value = mutationRate;
      ga.PopulationSize.Value = popSize;
      ga.Seed.Value = 0;
      ga.SetSeedRandomly.Value = true;
      ga.Selector = ga.SelectorParameter.ValidValues
        .OfType<S>()
        .First();

      ga.Crossover = ga.CrossoverParameter.ValidValues
        .OfType<C>()
        .First();

      ga.Mutator = ga.MutatorParameter.ValidValues
        .OfType<M>()
        .First();

      var tSelector = ga.Selector as TournamentSelector;
      if (tSelector != null) {
        tSelector.GroupSizeParameter.Value.Value = tournGroupSize;
      }
      ga.Engine = new ParallelEngine.ParallelEngine();
    }
  }
}

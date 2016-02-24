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

using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Operators;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [Item("Context Free Grammar Problem with Tracking", "The Context Free Grammar Problem with Tracking to analyze Crossover and Mutation on Grammars.")]
  [Creatable(CreatableAttribute.Categories.GeneticProgrammingProblems, Priority = 152)]
  [StorableClass]
  public class CFGProblemWithTracking : CFGProblem<ICFGProblemData, ICFGEvaluator<ICFGProblemData>> {
    [StorableConstructor]
    protected CFGProblemWithTracking(bool deserializing) : base(deserializing) { }

    protected CFGProblemWithTracking(CFGProblemWithTracking original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGProblemWithTracking()
      : base(CFGProblemData.EmptyProblemData, new CFGDummyEvaluator(), new ProbabilisticTreeCreator()) {
      InitializeOperators();
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGProblemWithTracking(this, cloner);
    }

    private void InitializeOperators() {
      // remove unnecessary analyzer
      Operators.RemoveAll(x => x is CFGTrainingBestSolutionAnalyzer<ICFGProblemData>);
      Operators.RemoveAll(x => x is CaseAnalyzer);

      Operators.Add(new SymbolicExpressionTreeCrossoverTrackingAnalyzer());
      Operators.Add(new SymbolicExpressionTreeManipulatorTrackingAnalyzer());
      ParameterizeOperators();
    }

    protected override void ParameterizeAnalyzers() {
      base.ParameterizeAnalyzers();
      ParameterizeOperatorsForTracking();
    }

    protected override void OnSolutionCreatorChanged() {
      base.OnSolutionCreatorChanged();
      ParameterizeOperatorsForTracking();
    }

    protected virtual void ParameterizeOperatorsForTracking() {
      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();

      var trackingCrossover = operators.Where(x => x.GetType()
         .GetInterfaces()
         .Any(i => i.IsGenericType
                   && (i.GetGenericTypeDefinition() == typeof(ICrossoverTrackingAnalyzer<>))
                   && (i.GetGenericArguments().First() == typeof(ISymbolicExpressionTree)))).Cast<ICrossoverTrackingAnalyzer<ISymbolicExpressionTree>>();

      var trackingManipulator = operators.Where(x => x.GetType()
         .GetInterfaces()
         .Any(i => i.IsGenericType
                   && (i.GetGenericTypeDefinition() == typeof(IManipulatorTrackingAnalyzer<>))
                   && (i.GetGenericArguments().First() == typeof(ISymbolicExpressionTree)))).Cast<IManipulatorTrackingAnalyzer<ISymbolicExpressionTree>>();


      if (trackingCrossover.Any()) {
        var beforeXO = new BeforeCrossoverOperator<ISymbolicExpressionTree>();
        beforeXO.ParentsParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.Name;
        foreach (var op in operators.OfType<ISymbolicExpressionTreeCrossover>()) {
          var instrumentedXO = op as InstrumentedOperator;
          if (instrumentedXO != null) {
            instrumentedXO.BeforeExecutionOperators.Clear();
            instrumentedXO.BeforeExecutionOperators.Add(beforeXO);
          }
        }

        var crossover = operators.OfType<ISymbolicExpressionTreeCrossover>().FirstOrDefault();
        if (crossover != null) {
          foreach (var op in trackingCrossover) {
            op.ChildParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
            op.CrossoverParentsParameter.ActualName = beforeXO.CrossoverParentsParameter.Name;

            var crossoverTrackingAnalyzer = op as SymbolicExpressionTreeCrossoverTrackingAnalyzer;
            if (crossoverTrackingAnalyzer != null) {
              crossoverTrackingAnalyzer.SymbolicExpressionGrammarParameter.ActualName = GrammarParameter.Name;
            }
          }
        }
      }

      if (trackingManipulator.Any()) {
        var beforeMU = new BeforeManipulatorOperator<ISymbolicExpressionTree>();
        beforeMU.ChildParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        foreach (var op in operators.OfType<ISymbolicExpressionTreeManipulator>()) {
          var instrumentedMU = op as InstrumentedOperator;
          if (instrumentedMU != null) {
            instrumentedMU.BeforeExecutionOperators.Clear();
            instrumentedMU.BeforeExecutionOperators.Add(beforeMU);
          }
        }

        var manipulator = operators.OfType<ISymbolicExpressionTreeManipulator>().FirstOrDefault();
        if (manipulator != null) {
          foreach (var op in trackingManipulator) {
            op.ChildParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
            op.ManipulatorParentParameter.ActualName = beforeMU.ManipulatorParentParameter.Name;
          }
        }
      }
    }
  }
}

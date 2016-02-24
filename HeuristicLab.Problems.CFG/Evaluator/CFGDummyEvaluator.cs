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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [Item("CFGDummyEvaluator", "An evaluator assigns default values to each individual.")]
  [StorableClass]
  public class CFGDummyEvaluator : InstrumentedOperator, ICFGEvaluator<ICFGProblemData> {
    #region parameters
    public ILookupParameter<ISymbolicExpressionTree> ProgramParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["Program"]; }
    }
    public IValueLookupParameter<ICFGProblemData> ProblemDataParameter {
      get { return (IValueLookupParameter<ICFGProblemData>)Parameters["ProblemData"]; }
    }
    public ILookupParameter<BoolArray> SuccessfulCasesParameter {
      get { return (ILookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public ILookupParameter<DoubleArray> CaseQualitiesParameter {
      get { return (ILookupParameter<DoubleArray>)Parameters["CaseQualities"]; }
    }
    public ILookupParameter<DoubleValue> QualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters["Quality"]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGDummyEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGDummyEvaluator(CFGDummyEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGDummyEvaluator()
      : base() {
      Parameters.Add(new LookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("Program", "The program to evaluate."));
      Parameters.Add(new ValueLookupParameter<ICFGProblemData>("ProblemData", "The problem data on which the context free grammer solution should be evaluated."));
      Parameters.Add(new LookupParameter<BoolArray>("Cases", "The training cases that have been successfully executed."));
      Parameters.Add(new LookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual"));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value aka fitness value of the solution."));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGDummyEvaluator(this, cloner);
    }

    public override IOperation InstrumentedApply() {
      SuccessfulCasesParameter.ActualValue = new BoolArray();
      CaseQualitiesParameter.ActualValue = new DoubleArray();
      QualityParameter.ActualValue = new DoubleValue();

      return base.InstrumentedApply();
    }
  }
}

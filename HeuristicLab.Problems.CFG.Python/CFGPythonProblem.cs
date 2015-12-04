#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  [Item("CFG Python Problem", "Generate python code to solve a problem defined by input and output pairs.")]
  [Creatable(CreatableAttribute.Categories.GeneticProgrammingProblems, Priority = 152)]
  [StorableClass]
  public class CFGPythonProblem : CFGProblem<ICFGPythonEvaluator> {

    private const string TimeoutParameterName = "Timeout";

    #region Parameter Properties
    public IValueParameter<IntValue> TimeoutParameter {
      get { return (IValueParameter<IntValue>)Parameters[TimeoutParameterName]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonProblem(bool deserializing) : base(deserializing) { }
    protected CFGPythonProblem(CFGPythonProblem original, Cloner cloner)
      : base(original, cloner) {
    }

    public CFGPythonProblem()
      : base(new CFGPythonEvaluator(), new ProbabilisticTreeCreator()) {
      Parameters.Add(new FixedValueParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped. (In milliseconds)", new IntValue(1000)));

      InitializeOperators();
      ParameterizeEvaluator();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonProblem(this, cloner);
    }

    #region Events
    protected override void OnEvaluatorChanged() {
      base.OnEvaluatorChanged();
      ParameterizeEvaluator();
    }

    protected override void OnReset() {
      base.OnReset();
      Evaluator.ClearCachedValues();
    }
    #endregion

    #region Helpers
    private void InitializeOperators() {
      Operators.RemoveAll(x => x is CFGTrainingBestSolutionAnalyzer);
      Operators.Add(new CFGPythonTrainingBestSolutionAnalyzer());
      Operators.Add(new CFGPythonExceptionAnalyzer());
      ParameterizeOperators();
    }

    private void ParameterizeEvaluator() {
      if (Evaluator != null) {
        Evaluator.TimeoutParameter.ActualName = TimeoutParameterName;
      }
      var treeEvaluator = Evaluator as ISymbolicExpressionTreeOperator;
      if (treeEvaluator != null) {
        treeEvaluator.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
    }

    protected override void ParameterizeOperators() {
      base.ParameterizeOperators();
      if (!Parameters.ContainsKey(TimeoutParameterName)) return;

      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();
      foreach (var op in operators.OfType<ICFGPythonAnalyzer>()) {
        op.TimeoutParameter.ActualName = TimeoutParameterName;
      }
    }
    #endregion
  }
}

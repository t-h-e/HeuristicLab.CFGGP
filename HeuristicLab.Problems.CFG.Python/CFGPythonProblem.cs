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

using System;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Problems.Instances.CFG;

namespace HeuristicLab.Problems.CFG.Python {
  [Item("CFG Python Problem", "Generate python code to solve a problem defined by input and output pairs.")]
  [Creatable(CreatableAttribute.Categories.GeneticProgrammingProblems, Priority = 152)]
  [StorableClass]
  public class CFGPythonProblem : CFGProblem<ICFGPythonProblemData, ICFGPythonEvaluator<ICFGPythonProblemData>>, IDisposable {

    private const string TimeoutParameterName = "Timeout";

    #region Parameter Properties
    public IValueParameter<IntValue> TimeoutParameter {
      get { return (IValueParameter<IntValue>)Parameters[TimeoutParameterName]; }
    }

    public IFixedValueParameter<PythonProcess> PythonProcessParameter {
      get { return (IFixedValueParameter<PythonProcess>)Parameters["PythonProcess"]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonProblem(bool deserializing) : base(deserializing) { }
    protected CFGPythonProblem(CFGPythonProblem original, Cloner cloner)
      : base(original, cloner) {
    }

    public CFGPythonProblem()
      : base(CFGPythonProblemData.EmptyProblemData, new CFGPythonEvaluator<ICFGPythonProblemData>(), new ProbabilisticTreeCreator()) {
      Parameters.Add(new FixedValueParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped. (In milliseconds)", new IntValue(1000)));
      Parameters.Add(new FixedValueParameter<PythonProcess>("PythonProcess", "Python process", new PythonProcess()));

      SetVariables();

      InitializeOperators();
      ParameterizeEvaluator();

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonProblem(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    #region Events
    private void RegisterEventHandlers() {
      GrammarParameter.ValueChanged += new EventHandler(GrammarParameter_ValueChanged);
      if (GrammarParameter.Value != null) {
        GrammarParameter.Value.Changed += new EventHandler(GrammarParameter_Value_Changed);
      }
    }

    private void GrammarParameter_ValueChanged(object sender, EventArgs e) {
      if (GrammarParameter.Value != null) {
        GrammarParameter.Value.Changed += new EventHandler(GrammarParameter_Value_Changed);
      }
      SetVariables();
    }

    private void GrammarParameter_Value_Changed(object sender, EventArgs e) {
      SetVariables();
    }

    private void SetVariables() {
      ProblemData.Variables.Clear();
      if (Grammar != null && Grammar != CFGExpressionGrammar.Empty) {
        var variableSymbols = Grammar.Symbols.Where(x => x.Enabled && x is GroupSymbol && x.Name.StartsWith("Rule: <") && x.Name.EndsWith("_var>")).Cast<GroupSymbol>();
        foreach (var varSy in variableSymbols) {
          VariableType type = (VariableType)Enum.Parse(typeof(VariableType), varSy.Name.Substring("Rule: <".Length, varSy.Name.Length - "Rule: <".Length - "_var>".Length), true);
          var variables = varSy.Symbols.Where(s => s.Enabled).ToDictionary(s => s.Name.Trim(new char[] { '\'', '"' }), x => type);
          ProblemData.Variables.Add(variables);
        }
      }

      SetVariablesToOperators();
    }

    private void SetVariablesToOperators() {
      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();
      foreach (var op in operators.OfType<ICFGPythonVariableSet>()) {
        op.SetVariables(ProblemData.Variables.GetVariableNames());
      }
    }

    protected override void OnEvaluatorChanged() {
      base.OnEvaluatorChanged();
      ParameterizeEvaluator();
      var eval = Evaluator as ICFGPythonVariableSet;
      if (eval != null) {
        eval.SetVariables(ProblemData.Variables.GetVariableNames());
      }
    }

    protected override void OnReset() {
      base.OnReset();
      Evaluator.ClearCachedValues();
    }
    #endregion

    #region Helpers
    private void InitializeOperators() {
      Operators.RemoveAll(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(CFGTrainingBestSolutionAnalyzer<>));
      Operators.Add(new CFGPythonTrainingBestSolutionAnalyzer());
      Operators.Add(new CFGPythonExceptionAnalyzer());
      ParameterizeOperators();
    }

    private void ParameterizeEvaluator() {
      if (Evaluator != null) {
        Evaluator.TimeoutParameter.ActualName = TimeoutParameter.Name;
        Evaluator.PythonProcessParameter.ActualName = PythonProcessParameter.Name;
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
      foreach (var op in operators.OfType<ICFGPythonAnalyzer<CFGPythonProblemData>>()) {
        op.TimeoutParameter.ActualName = TimeoutParameter.Name;
        op.PythonProcessParameter.ActualName = PythonProcessParameter.Name;
      }
      foreach (var op in operators.OfType<ITimeoutBasedOperator>()) {
        op.TimeoutParameter.ActualName = TimeoutParameter.Name;
      }
    }
    #endregion

    protected override ICFGPythonProblemData LoadProblemData(CFGData data) {
      CFGPythonProblemData problemData = new CFGPythonProblemData(data.Input, data.Output);
      problemData.TrainingPartitionParameter.Value.Start = data.TrainingPartitionStart;
      problemData.TrainingPartitionParameter.Value.End = data.TrainingPartitionEnd;
      problemData.TestPartitionParameter.Value.Start = data.TestPartitionStart;
      problemData.TestPartitionParameter.Value.End = data.TestPartitionEnd;
      problemData.EmbedCode.Value = data.Embed;
      return problemData as ICFGPythonProblemData;
    }

    public void Dispose() {
      PythonProcessParameter.Value.Dispose();
    }
  }
}

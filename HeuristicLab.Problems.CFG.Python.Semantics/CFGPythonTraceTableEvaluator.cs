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
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [StorableClass]
  public class CFGPythonTraceTableEvaluator : CFGPythonEvaluator, ISymbolicExpressionTreeOperator {

    private PythonSemanticHelper pythonSemanticHelper;

    #region paramerters
    public ILookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["SymbolicExpressionTree"]; }
    }
    public IValueParameter<StringArray> TraceVariablesParameter {
      get { return (IValueParameter<StringArray>)Parameters["TraceVariables"]; }
    }
    public ILookupParameter<ItemArray<PythonStatementSemantic>> SemanticParameter {
      get { return (ILookupParameter<ItemArray<PythonStatementSemantic>>)Parameters["Semantic"]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonTraceTableEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGPythonTraceTableEvaluator(CFGPythonTraceTableEvaluator original, Cloner cloner)
      : base(original, cloner) {
      RegisterEventHandlers();
      pythonSemanticHelper = new PythonSemanticHelper(TraceVariablesParameter.Value);
    }
    public CFGPythonTraceTableEvaluator() {
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("SymbolicExpressionTree", ""));
      Parameters.Add(new ValueParameter<StringArray>("TraceVariables", "", new StringArray()));

      Parameters.Add(new LookupParameter<ItemArray<PythonStatementSemantic>>("Semantic", ""));

      pythonSemanticHelper = new PythonSemanticHelper();
      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonTraceTableEvaluator(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
      pythonSemanticHelper = new PythonSemanticHelper(TraceVariablesParameter.Value);
    }

    private void RegisterEventHandlers() {
      TraceVariablesParameter.ValueChanged += new EventHandler(TraceVariablesParameter_ValueChanged);
      if (TraceVariablesParameter.Value != null) TraceVariablesParameter.Value.ItemChanged += new EventHandler<EventArgs<int>>(Value_ItemChanged);
    }

    private void Value_ItemChanged(object sender, EventArgs<int> e) {
      pythonSemanticHelper = new PythonSemanticHelper(TraceVariablesParameter.Value);
    }

    private void TraceVariablesParameter_ValueChanged(object sender, EventArgs e) {
      pythonSemanticHelper = new PythonSemanticHelper(TraceVariablesParameter.Value);
      if (TraceVariablesParameter.Value != null) TraceVariablesParameter.Value.ItemChanged += new EventHandler<EventArgs<int>>(Value_ItemChanged);
    }

    public override IOperation InstrumentedApply() {
      var result = pythonSemanticHelper.EvaluateAndTraceProgram(Program, Input, Output, ProblemData.TrainingIndices, HeaderParameter.ActualValue.Value, SymbolicExpressionTreeParameter.ActualValue, Timeout);

      SuccessfulCasesParameter.ActualValue = new BoolArray(result.Item1.ToArray());
      CaseQualitiesParameter.ActualValue = new DoubleArray(result.Item2.ToArray());
      QualityParameter.ActualValue = new DoubleValue(result.Item3);
      ExceptionParameter.ActualValue = new StringValue(result.Item4);

      SemanticParameter.ActualValue = result.Item5 != null ? new ItemArray<PythonStatementSemantic>(result.Item5) : null;

      return base.InstrumentedApply();
    }
  }
}

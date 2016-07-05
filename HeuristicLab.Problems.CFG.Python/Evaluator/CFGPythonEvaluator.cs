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
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  [StorableClass]
  public class CFGPythonEvaluator<T> : InstrumentedOperator, ICFGPythonEvaluator<T>
  where T : class, ICFGPythonProblemData {

    #region paramerters
    public ILookupParameter<IntValue> TimeoutParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Timeout"]; }
    }
    public ILookupParameter<ISymbolicExpressionTree> ProgramParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["Program"]; }
    }
    public IValueLookupParameter<T> ProblemDataParameter {
      get { return (IValueLookupParameter<T>)Parameters["ProblemData"]; }
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
    public ILookupParameter<StringValue> ExceptionParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Exception"]; }
    }

    public IValueLookupParameter<StringValue> InputParameter {
      get { return (IValueLookupParameter<StringValue>)Parameters["PythonInputCache"]; }
    }
    public IValueLookupParameter<StringValue> OutputParameter {
      get { return (IValueLookupParameter<StringValue>)Parameters["PythonOutputCache"]; }
    }

    public ILookupParameter<PythonProcess> PythonProcessParameter {
      get { return (ILookupParameter<PythonProcess>)Parameters["PythonProcess"]; }
    }
    #endregion

    #region properties
    public T ProblemData { get { return ProblemDataParameter.ActualValue; } }
    public double Timeout { get { return TimeoutParameter.ActualValue.Value / 1000.0; } }
    public string Program {
      get {
        return PythonHelper.FormatToProgram(ProgramParameter.ActualValue, ProblemData.FullHeader, ProblemData.FullFooter);
      }
    }
    public string Input {
      get {
        if (InputParameter.ActualValue == null) {
          InputParameter.Value = new StringValue(PythonHelper.ConvertToPythonValues(ProblemData.Input, ProblemData.TrainingIndices));
        }
        return InputParameter.ActualValue.Value;
      }
    }
    public string Output {
      get {
        if (OutputParameter.ActualValue == null) {
          OutputParameter.Value = new StringValue(PythonHelper.ConvertToPythonValues(ProblemData.Output, ProblemData.TrainingIndices));
        }
        return OutputParameter.ActualValue.Value;
      }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGPythonEvaluator(CFGPythonEvaluator<T> original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGPythonEvaluator() {
      Parameters.Add(new LookupParameter<IntValue>("Timeout", "The amount of time an execution is allowed to take, before it is stopped."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("Program", "The program to evaluate."));
      Parameters.Add(new ValueLookupParameter<T>("ProblemData", "The problem data on which the context free grammer solution should be evaluated."));
      Parameters.Add(new LookupParameter<BoolArray>("Cases", "The training cases that have been successfully executed."));
      Parameters.Add(new LookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual"));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value aka fitness value of the solution."));
      Parameters.Add(new LookupParameter<StringValue>("Exception", "Has the exception if any occured or the timeout."));

      Parameters.Add(new ValueLookupParameter<StringValue>("PythonInputCache", "Cache python input"));
      Parameters.Add(new ValueLookupParameter<StringValue>("PythonOutputCache", "Cache python output"));
      Parameters.Add(new LookupParameter<PythonProcess>("PythonProcess", "Python process"));

      SuccessfulCasesParameter.Hidden = true;
      CaseQualitiesParameter.Hidden = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonEvaluator<T>(this, cloner);
    }

    public override IOperation InstrumentedApply() {
      var result = PythonProcessParameter.ActualValue.EvaluateProgram(Program, Input, Output, ProblemData.TrainingIndices, Timeout);

      SuccessfulCasesParameter.ActualValue = new BoolArray(result.Item1.ToArray());
      CaseQualitiesParameter.ActualValue = new DoubleArray(result.Item2.ToArray());
      QualityParameter.ActualValue = new DoubleValue(result.Item3);
      ExceptionParameter.ActualValue = new StringValue(result.Item4);

      return base.InstrumentedApply();
    }

    public IOperation InstrumentedApplyWithoutEvaluation() {
      return base.InstrumentedApply();
    }

    public virtual void ClearCachedValues() {
      InputParameter.Value = null;
      OutputParameter.Value = null;
    }
  }
}

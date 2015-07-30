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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace HeuristicLab.Problems.CFG {
  [StorableClass]
  public class CFGPythonEvaluator : InstrumentedOperator, ICFGEvaluator {
    #region paramerters
    public IValueParameter<IntValue> TimeoutParameter {
      get { return (IValueParameter<IntValue>)Parameters["Timeout"]; }
    }
    public ILookupParameter<ISymbolicExpressionTree> ProgramParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["Program"]; }
    }
    public ILookupParameter<StringValue> HeaderParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Header"]; }
    }
    public ILookupParameter<StringValue> FooterParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Footer"]; }
    }
    public ILookupParameter<StringArray> InputParameter {
      get { return (ILookupParameter<StringArray>)Parameters["Input"]; }
    }
    public ILookupParameter<StringArray> OutputParameter {
      get { return (ILookupParameter<StringArray>)Parameters["Output"]; }
    }
    public ILookupParameter<BoolArray> SuccessfulCasesParameter {
      get { return (ILookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public ILookupParameter<DoubleValue> QualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters["Quality"]; }
    }
    #endregion

    #region properties
    public int Timeout { get { return TimeoutParameter.Value.Value; } }
    public string Program {
      get {
        StringBuilder strBuilder = new StringBuilder();
        string indent = String.Empty;
        if (HeaderParameter.ActualValue != null) {
          string header = HeaderParameter.ActualValue.Value;
          strBuilder.Append(header);
          int lastNewLine = header.LastIndexOf(Environment.NewLine);
          if (lastNewLine > 0) {
            indent = header.Substring(lastNewLine + Environment.NewLine.Length, header.Length - lastNewLine - Environment.NewLine.Length);
          }
        }

        string program = PythonHelper.convertBracketsToIndent(
                         CFGSymbolicExpressionTreeStringFormatter.StaticFormat(
                         ProgramParameter.ActualValue), indent);

        strBuilder.Append(program);
        if (FooterParameter.ActualValue != null) {
          strBuilder.Append(FooterParameter.ActualValue.Value);
        }
        return strBuilder.ToString();
      }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGPythonEvaluator(CFGPythonEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGPythonEvaluator() {
      Parameters.Add(new ValueParameter<IntValue>("Timeout", "The amount of time an execution is allowed to take, before it is stopped.", new IntValue(1000)));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("Program", "The program to evaluate."));
      Parameters.Add(new LookupParameter<StringValue>("Header", "The header of the program."));
      Parameters.Add(new LookupParameter<StringValue>("Footer", "The footer of the program."));
      Parameters.Add(new LookupParameter<StringArray>("Input", "The input for the program."));
      Parameters.Add(new LookupParameter<StringArray>("Output", "The output the program should produce."));
      Parameters.Add(new LookupParameter<BoolArray>("Cases", "The training cases that have been successfully executed."));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value aka fitness value of the solution."));

      SuccessfulCasesParameter.Hidden = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonEvaluator(this, cloner);
    }

    public override IOperation InstrumentedApply() {
      // create python engine and scope
      ScriptEngine pyEngine = Python.CreateEngine();
      ScriptScope scope = pyEngine.CreateScope();

      // set variables in scope
      scope.SetVariable("stop", false);
      scope.SetVariable("input", InputParameter.ActualValue);
      scope.SetVariable("output", OutputParameter.ActualValue);

      // create thread and execute the code
      ExecutePythonThread pyThread = new ExecutePythonThread(Program, pyEngine, scope);
      Thread thread = new Thread(new ThreadStart(pyThread.Run));
      thread.Start();

      // wait for thread
      // if a timeout occures, set variable stop to true to indicate that the python code should stop
      // then wait until the thread finished (threads cannot be stopped, killed or aboarted)
      thread.Join(Timeout);
      if (thread.IsAlive) {
        scope.SetVariable("stop", true);
        thread.Join();
      }

      // get return values
      IEnumerable<bool> cases;
      if (scope.TryGetVariable<IEnumerable<bool>>("cases", out cases)) {
        SuccessfulCasesParameter.ActualValue = new BoolArray(cases.ToArray());
      }

      double quality;
      if (scope.TryGetVariable<double>("quality", out quality)) {
        QualityParameter.ActualValue = new DoubleValue(quality);
      } else if (cases != null) {
        QualityParameter.ActualValue = new DoubleValue(cases.Where(x => !x).Count());
      } else {
        QualityParameter.ActualValue = new DoubleValue(double.PositiveInfinity);
      }


      return base.InstrumentedApply();
    }

    /**
     * Helper class which is used to executes python code in a separate thread
     **/
    private class ExecutePythonThread {
      private string code;
      private ScriptEngine engine;
      private ScriptScope scope;

      public Exception Exception { get; private set; }

      public ExecutePythonThread(string code, ScriptEngine engine, ScriptScope scope) {
        this.code = code;
        this.engine = engine;
        this.scope = scope;
      }

      public void Run() {
        // execute the script
        try {
          engine.Execute(code, scope);
        }
        catch (Exception e) {
          Exception = e;
        }
      }
    }
  }
}

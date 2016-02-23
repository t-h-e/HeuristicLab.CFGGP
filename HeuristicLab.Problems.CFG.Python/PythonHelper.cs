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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.Problems.CFG.Python {
  public class PythonHelper {
    private const String INDENTSPACE = "  ";
    private static string LINESEPARATOR = Environment.NewLine;

    private const string LOOPBREAK = "loopBreak";
    private const string LOOPBREAKUNNUMBERED = "loopBreak%";
    private const string LOOPBREAK_INITIALISE = "loopBreak% = 0";
    private const string LOOPBREAK_IF = "if loopBreak% >";
    private const string LOOPBREAK_INCREMENT = "loopBreak% += 1";

    private const string FUNCTIONSTART = "def evolve():";

    private const string FORCOUNTER = "forCounter";
    private const string FORCOUNTERUNNUMBERED = "forCounter%";

    public static string ConvertBracketsToIndent(string code, string additionalIndent = "") {
      StringBuilder stringBuilder = new StringBuilder();
      String[] split = code.Split(new string[] { Environment.NewLine }, StringSplitOptions.None); ;

      int indent = 0;

      // break loops individually
      Stack<int> loopBreakStack = new Stack<int>();
      //int absoluteLoopBreak = 0;

      int forCounterNumber = 0;

      bool first = true;

      foreach (String part in split) {
        String line = part.Trim();
        // remove intentation if bracket is at the beginning of the line
        while (line.StartsWith(":}")) {
          indent--;
          line = line.Substring(2, line.Length - 2).Trim();
        }

        // add indent
        if (!first) {
          stringBuilder.Append(additionalIndent);
        } else {
          first = false;
        }
        for (int i = 0; i < indent; i++) {
          stringBuilder.Append(INDENTSPACE);
        }

        // add indentation
        while (line.EndsWith("{:")) {
          indent++;
          line = line.Substring(0, line.Length - 2).Trim();
        }
        // remove indentation if bracket is at the end of the line
        while (line.EndsWith(":}")) {
          indent--;
          line = line.Substring(0, line.Length - 2).Trim();
        }

        // break loops individually
        //            if(line.contains(LOOPBREAKUNNUMBERED)) {
        //                if(line.contains(LOOPBREAK_INITIALISE)) {
        //                    loopBreakStack.push(absoluteLoopBreak);
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + absoluteLoopBreak);
        //                    absoluteLoopBreak++;
        //                } else if (line.contains(LOOPBREAK_IF)) {
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + loopBreakStack.peek());
        //                } else if (line.contains(LOOPBREAK_INCREMENT)) {
        //                    line = line.replace(LOOPBREAKUNNUMBERED, LOOPBREAK + loopBreakStack.pop());
        //                } else {
        //                    throw new IllegalArgumentException("Python 'while break' is malformed.");
        //                }
        //            } else if(line.contains(FORCOUNTERUNNUMBERED)) {
        //                line = line.replace(FORCOUNTERUNNUMBERED, FORCOUNTER + forCounterNumber);
        //                forCounterNumber++;
        //            }

        // break all loops after a certain number of iterations
        //if (line.Contains(FUNCTIONSTART)) {
        //    // initialise while break
        //    line += LINESEPARATOR + INDENTSPACE + LOOPBREAK + " = 0";
        //}
        if (line.Contains(LOOPBREAKUNNUMBERED)) {
          if (line.Contains(LOOPBREAK_INITIALISE)) {
            line = ""; // remove line
          } else if (line.Contains(LOOPBREAK_IF)) {
            line = line.Replace(LOOPBREAKUNNUMBERED, LOOPBREAK);
          } else if (line.Contains(LOOPBREAK_INCREMENT)) {
            line = line.Replace(LOOPBREAKUNNUMBERED, LOOPBREAK);
          } else {
            throw new ArgumentException("Python 'while break' is malformed.");
          }
        } else if (line.Contains(FORCOUNTERUNNUMBERED)) {
          line = line.Replace(FORCOUNTERUNNUMBERED, FORCOUNTER + forCounterNumber);
          forCounterNumber++;
        }

        // add line to code
        stringBuilder.Append(line);
        stringBuilder.Append(LINESEPARATOR);
      }

      return stringBuilder.ToString();
    }

    public static string FormatToProgram(ISymbolicExpressionTree tree) {
      return FormatToProgram(tree, (string)null);
    }

    public static string FormatToProgram(ISymbolicExpressionTree tree, string header, string footer = null) {
      StringBuilder strBuilder = new StringBuilder();
      string indent = String.Empty;
      if (header != null) {
        strBuilder.Append(header);
        int lastNewLine = header.LastIndexOf(Environment.NewLine);
        if (lastNewLine > 0) {
          indent = header.Substring(lastNewLine + Environment.NewLine.Length, header.Length - lastNewLine - Environment.NewLine.Length);
        } else {
          indent = header;
        }
        if (!String.IsNullOrWhiteSpace(indent)) {
          indent = "";
        }
      }

      string program = ConvertBracketsToIndent(
                       CFGSymbolicExpressionTreeStringFormatter.StaticFormat(
                       tree), indent);

      strBuilder.Append(program);
      if (footer != null) {
        strBuilder.Append(footer);
      }
      return strBuilder.ToString();
    }

    public static string FormatToProgram(ISymbolicExpressionTree tree, StringValue HeaderValue, StringValue FooterValue = null) {
      return FormatToProgram(tree, HeaderValue == null ? null : HeaderValue.Value, FooterValue == null ? null : FooterValue.Value);
    }

    public static string ConvertToPythonValues(StringArray array, IEnumerable<int> indices) {
      StringBuilder strBuilder = new StringBuilder("[");
      foreach (int row in indices) {
        strBuilder.Append("[");
        strBuilder.Append(array[row]);
        strBuilder.Append("],");
      }
      strBuilder.Append("]");
      return strBuilder.ToString();
    }

    // should not be instanciated directly
    //protected PythonHelper() { }

    //private static PythonHelper pyHelper;

    //public static PythonHelper GetInstance() {
    //  if (pyHelper == null) {
    //    pyHelper = new PythonHelper();
    //  }
    //  return pyHelper;
    //}

    //protected ScriptEngine pyEngine = IronPython.Hosting.Python.CreateEngine(new Dictionary<string, object>() {
    //  {"LightweightScopes", true}
    //});

    //public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, StringArray input, StringArray output, IEnumerable<int> indices, int timeout = 1000) {
    //  return EvaluateProgram(program, ConvertToPythonValues(input, indices), ConvertToPythonValues(output, indices), indices, timeout);
    //}

    //public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, string input, string output, IEnumerable<int> indices, int timeout = 1000) {
    //  ScriptScope scope = pyEngine.CreateScope();
    //  return EvaluateProgram(program, input, output, indices, scope, timeout);
    //}

    //protected Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, string input, string output, IEnumerable<int> indices, ScriptScope scope, int timeout = 1000) {
    //  // set variables in scope
    //  scope.SetVariable("stop", false);
    //  pyEngine.Execute("inval = " + input, scope);
    //  pyEngine.Execute("outval = " + output, scope);

    //  // create thread and execute the code
    //  ExecutePythonThread pyThread = new ExecutePythonThread(program, pyEngine, scope);
    //  Thread thread = new Thread(new ThreadStart(pyThread.Run));
    //  thread.Start();

    //  // wait for thread
    //  // if a timeout occures, set variable stop to true to indicate that the python code should stop
    //  // then wait until the thread finished (threads cannot be stopped, killed or aboarted)
    //  thread.Join(timeout);
    //  bool timedout = false;
    //  if (thread.IsAlive) {
    //    scope.SetVariable("stop", true);
    //    thread.Join();
    //    timedout = true;
    //  }


    //  string exception = String.Empty;
    //  if (pyThread.Exception != null || timedout) {
    //    exception = pyThread.Exception != null ? pyThread.Exception.Message : "Timeout occurred.";
    //  }

    //  // get return values
    //  IEnumerable<bool> cases;
    //  if (!scope.TryGetVariable<IEnumerable<bool>>("cases", out cases)) {
    //    cases = Enumerable.Repeat(false, indices.Count());
    //  }

    //  IEnumerable<double> caseQualities;
    //  // twice as fast as next if statement, but in overall it just makes a difference by a few milliseconds
    //  //if (!scope.TryGetVariable<IEnumerable<double>>("caseQuality", out caseQualities)) {
    //  //  caseQualities = Enumerable.Repeat(Double.MaxValue, indices.Count()).ToList();
    //  //}

    //  IEnumerable<object> tmpCaseQualities;
    //  if (scope.TryGetVariable<IEnumerable<object>>("caseQuality", out tmpCaseQualities)) {
    //    try {
    //      // call ToList to convert values
    //      caseQualities = tmpCaseQualities.Select(x => Convert.ToDouble(x)).ToList();
    //    }
    //    // catches invalid casts, if the value is to big for double or if it is not numeric
    //    catch (InvalidCastException) {
    //      caseQualities = new List<double>();
    //    }
    //  } else {
    //    caseQualities = new List<double>();
    //  }

    //  double quality;
    //  if (!scope.TryGetVariable<double>("quality", out quality)) {
    //    quality = Double.MaxValue;
    //  }

    //  return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string>(cases, caseQualities, quality, exception);
    //}

    ///**
    // * Helper class which is used to executes python code in a separate thread
    // **/
    //private class ExecutePythonThread {
    //  private string code;
    //  private ScriptEngine engine;
    //  private ScriptScope scope;

    //  public Exception Exception { get; private set; }

    //  public ExecutePythonThread(string code, ScriptEngine engine, ScriptScope scope) {
    //    this.code = code;
    //    this.engine = engine;
    //    this.scope = scope;
    //  }

    //  public void Run() {
    //    // execute the script
    //    try {
    //      engine.Execute(code, scope);
    //    }
    //    catch (Exception e) {
    //      Exception = e;
    //    }
    //  }
    //}
  }
}

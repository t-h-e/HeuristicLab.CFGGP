#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2017 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using Newtonsoft.Json.Linq;


namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public static class SemanticOperatorHelper {
    private static readonly List<string> StatementProductionRules = new List<string>() { "Rule: <predefined>", "Rule: <code>", "Rule: <statement>",
                                                                         "Rule: <simple_stmt>", "Rule: <compound_stmt>",
                                                                         "Rule: <for>", "Rule: <call>", "Rule: <assign>"};

    private static readonly ProgramRootSymbol RootSymbol = new ProgramRootSymbol();
    private static readonly StartSymbol StartSymbol = new StartSymbol();

    /// <summary>
    /// 0 = variable names
    /// 1 = variable settings
    /// 2 = variable settings names
    /// 3 = code
    /// </summary>
    private const string EVAL_TRACE_SCRIPT = @"{0}
variables = [{1}]
{2}

trace = {{}}
for v in variables:
    trace[v] = []

for {3} in zip({4}):
{5}
    for v in variables:
        trace[v].append(locals()[v])

for v in variables:
    locals()[v] = trace[v]";


    public static IEnumerable<string> GetSemanticProductionNames(ISymbolicExpressionTreeGrammar grammar) {
      var symbolNames = grammar.Symbols.Select(x => x.Name);
      var statementProductionNames = new List<string>();
      foreach (var ruleName in SemanticOperatorHelper.StatementProductionRules) {
        if (symbolNames.Contains(ruleName)) {
          statementProductionNames.AddRange(((GroupSymbol)grammar.GetSymbol(ruleName)).Symbols.Select(x => x.Name));
        }
      }
      return statementProductionNames;
    }

    public static ISymbolicExpressionTreeNode GetStatementNode(ISymbolicExpressionTreeNode parent, IEnumerable<string> statementProductionNames) {
      ISymbolicExpressionTreeNode statement = parent;
      while (statement != null && !statementProductionNames.Contains(statement.Symbol.Name)) {
        statement = statement.Parent;
      }
      return statement;
    }

    public static string SemanticToPythonVariableSettings(IDictionary<string, IList> semantic, IDictionary<string, VariableType> variableTypes) {
      StringBuilder strBuilder = new StringBuilder();
      foreach (var setting in semantic) {
        strBuilder.AppendLine(String.Format("{0} = {1}", setting.Key, PythonHelper.SerializeCSToPythonJson(setting.Value)));
      }
      return strBuilder.ToString();
    }

    public static JObject EvaluateStatementNode(ISymbolicExpressionTreeNode statement, PythonProcess pythonProcess, IRandom random, ICFGPythonProblemData problemData, IList<string> variables, string variableSettings, double timeout) {
      var statementParent = statement.Parent;
      EvaluationScript crossoverPointScript0 = new EvaluationScript() {
        Script = FormatScript(CreateTreeFromNode(random, statement, RootSymbol, StartSymbol), problemData, problemData.LoopBreakConst, variables, variableSettings),
        Variables = variables,
        Timeout = timeout
      };
      JObject json0 = pythonProcess.SendAndEvaluateProgram(crossoverPointScript0);
      statement.Parent = statementParent; // restore parent
      return json0;
    }

    private static string FormatScript(ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, int loopBreakConst, IEnumerable<string> variables, string variableSettings) {
      Regex r = new Regex(@"^(.*?)\s*=", RegexOptions.Multiline);
      string variableSettingsSubstitute = r.Replace(variableSettings, "${1}_setting =");
      return String.Format(EVAL_TRACE_SCRIPT, problemData.HelperCode.Value,
                                              String.Format("'{0}'", String.Join("','", variables)),
                                              variableSettingsSubstitute,
                                              String.Join(",", variables),
                                              String.Join(",", variables.Select(x => x + "_setting")),
                                              PythonHelper.FormatToProgram(symbolicExpressionTree, loopBreakConst, "    "));
    }

    //copied from SymbolicDataAnalysisExpressionCrossover<T>
    private static ISymbolicExpressionTree CreateTreeFromNode(IRandom random, ISymbolicExpressionTreeNode node, ISymbol rootSymbol, ISymbol startSymbol) {
      var rootNode = new SymbolicExpressionTreeTopLevelNode(rootSymbol);
      if (rootNode.HasLocalParameters) rootNode.ResetLocalParameters(random);

      var startNode = new SymbolicExpressionTreeTopLevelNode(startSymbol);
      if (startNode.HasLocalParameters) startNode.ResetLocalParameters(random);

      startNode.AddSubtree(node);
      rootNode.AddSubtree(startNode);

      return new SymbolicExpressionTree(rootNode);
    }
  }
}

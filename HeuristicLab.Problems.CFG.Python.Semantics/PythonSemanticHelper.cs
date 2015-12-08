using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonSemanticHelper : PythonHelper {

    private const string traceCode = @"import sys

past_locals = {{}}
variable_list = ['{0}']
traceTable = {{}}

def trace(frame, event, arg_unused):
  global past_locals, variable_list, traceTable

  if frame.f_code.co_name != 'evolve':
    return

  relevant_locals = {{}}
  all_locals = frame.f_locals.copy()

  for k, v in all_locals.items():
    if k in variable_list:
      relevant_locals[k] = v
  if len(relevant_locals) > 0 and past_locals != relevant_locals:
    if frame.f_lineno not in traceTable:
      traceTable[frame.f_lineno] = {{}}
    for var in variable_list:
      if var in relevant_locals:
        if var not in traceTable[frame.f_lineno]:
          traceTable[frame.f_lineno][var] = []
        traceTable[frame.f_lineno][var].append(relevant_locals[var])
    past_locals = relevant_locals
  return trace

sys.settrace(trace)

";

    private string traceCodeWithVariables;

    public PythonSemanticHelper() {
      traceCodeWithVariables = String.Empty;
    }

    public PythonSemanticHelper(StringArray variableNames) {
      if (variableNames == null || variableNames.Length == 0) {
        traceCodeWithVariables = String.Empty;
      } else {
        traceCodeWithVariables = String.Format(traceCode, String.Join("', '", variableNames.Where(x => !String.IsNullOrWhiteSpace(x))));
      }
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> EvaluateAndTraceProgram(string program, string input, string output, IEnumerable<int> indices, string header, ISymbolicExpressionTree tree, int timeout = 1000) {
      string traceProgram = traceCodeWithVariables + program;

      ScriptScope scope = pyEngine.CreateScope();
      var tupel = EvaluateProgram(traceProgram, input, output, indices, scope, timeout);

      if (!scope.ContainsVariable("traceTable")) {
        return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(tupel.Item1, tupel.Item2, tupel.Item3, tupel.Item4, null);
      }

      PythonDictionary traceTablePython = scope.GetVariable<PythonDictionary>("traceTable");
      var traceTable = ConvertPythonDictionary(traceTablePython);

      List<PythonStatementSemantic> semantics = new List<PythonStatementSemantic>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductions = ((GroupSymbol)root.Grammar.GetSymbol("Rule: <statement>")).Symbols;
      var statementProductionNames = statementProductions.Select(x => x.Name);
      //var statementNodes = tree.IterateNodesPrefix().Select(x => statementProductionNames.Contains(x.Symbol.Name));

      IList<int> lineTraces = traceTable.Keys.OrderBy(x => x).ToList();

      // add one, because the values are set after the statement is executed
      int curline = traceCode.Count(c => c == '\n') + header.Count(c => c == '\n') + 1;
      var symbolToLineDict = FormatRecursively(root, statementProductionNames, ref curline);

      foreach (var symbolLine in symbolToLineDict) {
        if (traceTable.ContainsKey(symbolLine.Value)) {
          var variableValuesAfter = traceTable[symbolLine.Value];

          IDictionary<string, IList> variableValuesBefore = null;
          int index = lineTraces.IndexOf(symbolLine.Value);
          if (index != 0) {
            index--;
            variableValuesBefore = traceTable[lineTraces[index]];
          }

          semantics.Add(new PythonStatementSemantic() {
            TreeNode = symbolLine.Key,
            Before = variableValuesBefore,
            After = variableValuesAfter
          });
        }
      }

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(tupel.Item1, tupel.Item2, tupel.Item3, tupel.Item4, semantics);
    }

    private IDictionary<int, IDictionary<string, IList>> ConvertPythonDictionary(PythonDictionary dict) {
      IDictionary<int, IDictionary<string, IList>> convertedDict = new Dictionary<int, IDictionary<string, IList>>(dict.Count);
      foreach (var line in dict) {
        IDictionary<string, IList> lineDict = new Dictionary<string, IList>();
        convertedDict.Add((int)line.Key, lineDict);

        foreach (var variable in (PythonDictionary)line.Value) {
          lineDict.Add((string)variable.Key, ConvertPythonList((IronPython.Runtime.List)variable.Value));
        }
      }
      return convertedDict;
    }

    private IList ConvertPythonList(IronPython.Runtime.List pythonList) {
      if (pythonList.Count == 0) return new List<object>();
      //if (pythonList[0] == null) return Enumerable.Repeat<object>(null, pythonList.Count).ToList();

      if (pythonList[0] != null && pythonList[0].GetType() == typeof(IronPython.Runtime.List)) {
        return pythonList.Select(x => ((List)x).ToList()).ToList();
      } else {
        return pythonList.ToList();
      }

      //if (genericType == typeof(bool)) {
      //  return pythonList.Cast<bool>().ToList();
      //} else if (genericType == typeof(int) || genericType == typeof(long)) {
      //  return pythonList.Select(x => Convert.ToInt64(x)).ToList();
      //} else if (genericType == typeof(BigInteger)) {
      //  return pythonList.Select(x => (long)x).ToList();
      //} else if (genericType == typeof(float) || genericType == typeof(double)) {
      //  return pythonList.Select(x => Convert.ToDouble(x)).ToList();
      //} else if (genericType == typeof(string)) {
      //  return pythonList.Cast<string>().ToList();
      //} else if (genericType == typeof(IronPython.Runtime.List)) {
      //  object item = null;
      //  for (int i = 0; i < pythonList.Count; i++) {
      //    item = ((List)pythonList[i]).FirstOrDefault();
      //    if (item != null) break;
      //  }
      //  if (item == null) return Enumerable.Repeat(new List<object>(), pythonList.Count).ToList();

      //  genericType = item.GetType();
      //  if (genericType == typeof(bool)) {
      //    return pythonList.Select(x => ((List)x).Cast<bool>().ToList()).ToList();
      //  } else if (genericType == typeof(int) || genericType == typeof(float) || genericType == typeof(long) || genericType == typeof(double) || genericType == typeof(BigInteger)) {
      //    return pythonList.Select(x => ((List)x).Select(y => item is BigInteger ? (double)y : Convert.ToDouble(y)).ToList()).ToList();
      //    //} else if (genericType == typeof(float)) {
      //    //  return pythonList.Select(x => ((List)x).Cast<float>().ToList()).ToList();
      //  } else if (genericType == typeof(string)) {
      //    return pythonList.Select(x => ((List)x).Cast<string>().ToList()).ToList();
      //  }
      //}
      //throw new ArgumentException("Type in traceTable is not defined or unknown");
    }


    private Dictionary<ISymbolicExpressionTreeNode, int> FormatRecursively(ISymbolicExpressionTreeNode node, IEnumerable<string> productions, ref int curline) {
      Dictionary<ISymbolicExpressionTreeNode, int> symbolToLineDict = new Dictionary<ISymbolicExpressionTreeNode, int>();
      if (node.Subtrees.Count() > 0) {
        // node
        var symbol = node.Symbol as CFGSymbol;
        if (symbol != null) {
          var partsEnumerator = symbol.GetTerminalParts().GetEnumerator();
          var subtreeEnumerator = node.Subtrees.GetEnumerator();
          if (productions.Contains(symbol.Name)) {
            symbolToLineDict.Add(node, curline);
          }
          while (partsEnumerator.MoveNext() && subtreeEnumerator.MoveNext()) {
            curline += partsEnumerator.Current.Count(c => c == '\n');
            symbolToLineDict = symbolToLineDict.Union(FormatRecursively(subtreeEnumerator.Current, productions, ref curline)).ToDictionary(k => k.Key, v => v.Value);
          }
          curline += partsEnumerator.Current.Count(c => c == '\n');
        } else {
          // ProgramRoot or StartSymbol
          foreach (var subtree in node.Subtrees) {
            symbolToLineDict = symbolToLineDict.Union(FormatRecursively(subtree, productions, ref curline)).ToDictionary(k => k.Key, v => v.Value);
          }
        }
      } else {
        // leaf
        var symbol = node.Symbol as CFGSymbol;
        if (productions.Contains(symbol.Name)) {
          symbolToLineDict.Add(node, curline);
        }
        var parts = symbol.GetTerminalParts();
        curline += parts.First().Count(c => c == '\n');
      }
      return symbolToLineDict;
    }
  }
}

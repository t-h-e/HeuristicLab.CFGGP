using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonSemanticHelper : PythonHelper {

    private const string traceCode = @"import sys

past_locals = {}
variable_list = ['b0', 'res', 'i0']
traceTable = {}

def trace(frame, event, arg_unused):
  global past_locals, variable_list, traceTable

  if frame.f_code.co_name != 'evolve':
    return

  relevant_locals = {}
  all_locals = frame.f_locals.copy()

  for k, v in all_locals.items():
    if k in variable_list:
      relevant_locals[k] = v
  if len(relevant_locals) > 0 and past_locals != relevant_locals:
    if frame.f_lineno not in traceTable:
      traceTable[frame.f_lineno] = {}
    for var in variable_list:
      if var in relevant_locals:
        if var not in traceTable[frame.f_lineno]:
          traceTable[frame.f_lineno][var] = []
        traceTable[frame.f_lineno][var].append(relevant_locals[var])
    past_locals = relevant_locals
  return trace

sys.settrace(trace)

";

    private static PythonSemanticHelper pyHelper;

    public new static PythonSemanticHelper GetInstance() {
      if (pyHelper == null) {
        pyHelper = new PythonSemanticHelper();
      }
      return pyHelper;
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, IDictionary<ISymbolicExpressionTreeNode, Tuple<object, object>>> EvaluateAndTraceProgram(string program, string input, string output, IEnumerable<int> indices, string header, ISymbolicExpressionTree tree, int timeout = 1000) {
      string traceProgram = traceCode + program;

      ScriptScope scope = pyEngine.CreateScope();
      var tupel = EvaluateProgram(traceProgram, input, output, indices, scope, timeout);


      // ToDo: map line to symbol or something

      PythonDictionary traceTable = scope.GetVariable<PythonDictionary>("traceTable");


      Dictionary<ISymbolicExpressionTreeNode, Tuple<object, object>> semantics = new Dictionary<ISymbolicExpressionTreeNode, Tuple<object, object>>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductions = ((GroupSymbol)root.Grammar.GetSymbol("statement")).Symbols;
      var statementProductionNames = statementProductions.Select(x => x.Name);
      //var statementNodes = tree.IterateNodesPrefix().Select(x => statementProductionNames.Contains(x.Symbol.Name));

      IList<int> lineTraces = traceTable.Keys.Cast<int>().OrderBy(x => x).ToList();

      // add one, because the values are set after the statement is executed
      int curline = traceCode.Count(c => c == '\n') + header.Count(c => c == '\n') + 1;
      var symbolToLineDict = FormatRecursively(root, statementProductionNames, ref curline);

      foreach (var symbolLine in symbolToLineDict) {
        if (traceTable.ContainsKey(symbolLine.Value)) {
          var variableValuesAfter = traceTable[symbolLine.Value];

          object variableValuesBefore = null;
          int index = lineTraces.IndexOf(symbolLine.Value);
          if (index != 0) {
            index--;
            variableValuesBefore = traceTable[lineTraces[index]];
          }

          semantics.Add(symbolLine.Key, new Tuple<object, object>(variableValuesBefore, variableValuesAfter));
        }
      }

      //Console.WriteLine(traceTable);

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, IDictionary<ISymbolicExpressionTreeNode, Tuple<object, object>>>(tupel.Item1, tupel.Item2, tupel.Item3, tupel.Item4, semantics);
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

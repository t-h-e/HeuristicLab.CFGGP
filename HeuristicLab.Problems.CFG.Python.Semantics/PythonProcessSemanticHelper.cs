using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using IronPython.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonProcessSemanticHelper : PythonProcessHelper {

    private const string traceCode = @"import sys

past_locals = {{}}
variable_list = ['{0}']
traceTable = {{}}
traceChanges = []

def trace(frame, event, arg_unused):
    global past_locals, variable_list, traceTable

    if frame.f_code.co_name != 'evolve':
        return trace

    relevant_locals = {{}}
    all_locals = frame.f_locals.copy()

    for k, v in all_locals.items():
        if k in variable_list:
            relevant_locals[k] = v

    if len(relevant_locals) > 0 and past_locals != relevant_locals:
        # create dict for line number and all variable dicts
        if frame.f_lineno not in traceTable:
            traceTable[frame.f_lineno] = {{}}
            for var in variable_list:
                traceTable[frame.f_lineno][var] = []
        elif len(traceTable[frame.f_lineno][variable_list[0]]) >= {1}:
            past_locals = relevant_locals
            return trace

        for var in variable_list:
            if var in relevant_locals:
                traceTable[frame.f_lineno][var].append(relevant_locals[var])
            else:
                # actually not needed for IronPython, but added so it works for all python versions
                traceTable[frame.f_lineno][var].append(None)
        past_locals = relevant_locals
    return trace

sys.settrace(trace)

";

    private const string traceTableReduceEntries = @"
lines = sorted(list(traceTable.keys()), reverse=True)
for i in range(1, len(lines)):
  for v in variable_list:
    if v in traceTable[lines[i]] and v in traceTable[lines[i-1]]:
      if traceTable[lines[i]][v] == traceTable[lines[i-1]][v]:
        del traceTable[lines[i-1]][v]

for l in lines:
  if not traceTable[l]:
    del traceTable[l]";

    private string traceCodeWithVariables;

    public PythonProcessSemanticHelper() {
      traceCodeWithVariables = String.Empty;
    }

    public PythonProcessSemanticHelper(StringArray variableNames, int limit) {
      if (variableNames == null || variableNames.Length == 0) {
        traceCodeWithVariables = String.Empty;
      } else {
        traceCodeWithVariables = String.Format(traceCode, String.Join("', '", variableNames.Where(x => !String.IsNullOrWhiteSpace(x))), limit);
      }
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> EvaluateAndTraceProgram(string program, string input, string output, IEnumerable<int> indices, string header, ISymbolicExpressionTree tree, int timeout = 1000) {

      string traceProgram = traceCodeWithVariables + program + traceTableReduceEntries;

      EvaluationScript es = base.CreateEvaluationScript(traceProgram, input, output);
      es.Variables.Add("traceTable");
      es.Variables.Add("traceChanges");

      JObject json = base.SendAndEvaluateProgram(es);
      var baseResult = base.GetVariablesFromJson(json, indices.Count());

      if (json["traceTable"] == null) {
        return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, new List<PythonStatementSemantic>());
      }

      var traceTable = JsonConvert.DeserializeObject<IDictionary<int, IDictionary<string, IList>>>(json["traceTable"].ToString());
      IList<int> traceChanges = json["traceChanges"].Select(x => (int)x).ToList();
      //var traceTable = ConvertPythonDictionary(traceTablePython);

      List<PythonStatementSemantic> semantics = new List<PythonStatementSemantic>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductions = ((GroupSymbol)root.Grammar.GetSymbol("Rule: <statement>")).Symbols;
      var statementProductionNames = statementProductions.Select(x => x.Name);

      IList<int> lineTraces = traceTable.Keys.OrderBy(x => x).ToList();

      // add one, because the values are set after the statement is executed
      int curline = traceCode.Count(c => c == '\n') + header.Count(c => c == '\n') + 1;
      var symbolToLineDict = FindStatementSymbolsInTree(root, statementProductionNames, ref curline);

      var prefixTreeNodes = tree.IterateNodesPrefix().ToList();

      foreach (var symbolLine in symbolToLineDict) {
        Dictionary<string, IList> before = new Dictionary<string, IList>();
        var linesBefore = lineTraces.Where(x => x <= symbolLine.Value).OrderByDescending(x => x);
        foreach (var l in linesBefore) {
          foreach (var change in traceTable[l]) {
            if (!before.ContainsKey(change.Key)) {
              before.Add(change.Key, change.Value);
            }
          }
        }

        int after = -1;
        int pos = traceChanges.IndexOf(linesBefore.Max());
        if (pos + 1 < traceChanges.Count && traceTable.ContainsKey(traceChanges[pos + 1])) {
          after = traceChanges[pos + 1];
        }

        if (after >= 0) {
          semantics.Add(new PythonStatementSemantic() {
            TreeNodePrefixPos = prefixTreeNodes.IndexOf(symbolLine.Key),
            Before = before,
            After = traceTable[after],
          });
        }
      }

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, semantics);
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


    private Dictionary<ISymbolicExpressionTreeNode, int> FindStatementSymbolsInTree(ISymbolicExpressionTreeNode node, IEnumerable<string> productions, ref int curline) {
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
            symbolToLineDict = symbolToLineDict.Union(FindStatementSymbolsInTree(subtreeEnumerator.Current, productions, ref curline)).ToDictionary(k => k.Key, v => v.Value);
          }
          curline += partsEnumerator.Current.Count(c => c == '\n');
        } else {
          // ProgramRoot or StartSymbol
          foreach (var subtree in node.Subtrees) {
            symbolToLineDict = symbolToLineDict.Union(FindStatementSymbolsInTree(subtree, productions, ref curline)).ToDictionary(k => k.Key, v => v.Value);
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

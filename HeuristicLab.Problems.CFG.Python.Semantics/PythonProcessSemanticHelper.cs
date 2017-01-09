using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonProcessSemanticHelper {

    private const string traceCode = @"import sys

past_locals = {{}}
variable_list = ['{0}']
traceTable = {{}}
executedLines = set()

def trace(frame, event, arg_unused):
    global past_locals, traceTable, variable_list

    if frame.f_code.co_name != 'evolve':
        return None

    relevant_locals = {{}}
    all_locals = frame.f_locals.copy()

    executedLines.add(frame.f_lineno)
    for k, v in all_locals.items():
        if k in variable_list:
            relevant_locals[k] = v

    if len(relevant_locals) > 0 and past_locals != relevant_locals:
        # create dict for line number and all variable dicts
        if frame.f_lineno not in traceTable:
            traceTable[frame.f_lineno] = {{}}
            for v in variable_list:
                traceTable[frame.f_lineno][v] = []
        else:
            if len(traceTable[frame.f_lineno][variable_list[0]]) >= {1}:
                past_locals = relevant_locals
                return trace

        for v in variable_list:
            if v in relevant_locals:
                traceTable[frame.f_lineno][v].append(relevant_locals[v])
            else:
                # actually not needed for IronPython, but added so it works for all python versions
                traceTable[frame.f_lineno][v].append(None)
        past_locals = relevant_locals
    return trace

sys.settrace(trace)

";

    private const string traceTableReduceEntries = @"
sys.settrace(None)

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

    public PythonProcessSemanticHelper(IEnumerable<string> variableNames, int limit) {
      if (variableNames == null || variableNames.Count() == 0 || limit <= 0) {
        traceCodeWithVariables = String.Empty;
      } else {
        traceCodeWithVariables = String.Format(traceCode, String.Join("', '", variableNames.Where(x => !String.IsNullOrWhiteSpace(x))), limit);
      }
    }

    // TODO: Simplify the semantic extraction. There should be an easier way to do this. It takes too long to understand the code.
    // More comments and better variable naming would also help
    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> EvaluateAndTraceProgram(PythonProcess pythonProcess, string program, string input, string output, IEnumerable<int> indices, string header, string footer, ISymbolicExpressionTree tree, double timeout = 1) {
      string traceProgram = traceCodeWithVariables
                          + program;
      traceProgram += traceCodeWithVariables == String.Empty
                    ? String.Empty
                    : traceTableReduceEntries;

      EvaluationScript es = pythonProcess.CreateEvaluationScript(traceProgram, input, output, timeout);
      es.Variables.Add("traceTable");
      es.Variables.Add("executedLines");

      JObject json = pythonProcess.SendAndEvaluateProgram(es);
      var baseResult = pythonProcess.GetVariablesFromJson(json, indices.Count());

      if (json["traceTable"] == null) {
        return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, new List<PythonStatementSemantic>());
      }

      var traceTable = JsonConvert.DeserializeObject<IDictionary<int, IDictionary<string, IList>>>(json["traceTable"].ToString());
      var executedLines = JsonConvert.DeserializeObject<List<int>>(json["executedLines"].ToString());
      executedLines.Sort();

      IList<int> traceChanges = traceTable.Keys.OrderBy(x => x).ToList();

      List<PythonStatementSemantic> semantics = new List<PythonStatementSemantic>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductions = ((GroupSymbol)root.Grammar.GetSymbol("Rule: <code>")).Symbols.Union(
                                 ((GroupSymbol)root.Grammar.GetSymbol("Rule: <statement>")).Symbols).Union(
                                 ((GroupSymbol)root.Grammar.GetSymbol("Rule: <predefined>")).Symbols);
      var statementProductionNames = statementProductions.Select(x => x.Name);

      // calculate the correct line the semantic evaluation starts from
      var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      int curline = es.Script.Count(c => c == '\n') - code.Count(c => c == '\n') - footer.Count(c => c == '\n') - traceTableReduceEntries.Count(c => c == '\n');

      var symbolToLineDict = FindStatementSymbolsInTree(root, statementProductionNames, ref curline);
      var symbolLinesBegin = symbolToLineDict.Select(x => x.Value[0]).Distinct().OrderBy(x => x).ToList();

      #region fix Before line for <predefined> to have all variables initialised
      int minBefore = symbolLinesBegin.Min();
      symbolLinesBegin.Remove(minBefore);
      int newMinBefore = symbolLinesBegin.Min();
      foreach (var symbolToLine in symbolToLineDict) {
        if (symbolToLine.Value[0] == minBefore) {
          symbolToLine.Value[0] = newMinBefore;
        }
      }
      #endregion
      #region set Before line to an actual line that has been executed, so that the effect of the code is shown
      foreach (var symbolToLine in symbolToLineDict) {
        if (!executedLines.Contains(symbolToLine.Value[0])) {
          var actualBefore = executedLines.First(x => x > symbolToLine.Value[0]);
          symbolToLine.Value.RemoveAt(0);
          symbolToLine.Value.Insert(0, actualBefore);
        }
      }
      #endregion

      var prefixTreeNodes = tree.IterateNodesPrefix().ToList();

      foreach (var symbolLine in symbolToLineDict) {
        Dictionary<string, IList> before = new Dictionary<string, IList>();
        var linesBefore = traceChanges.Where(x => x <= symbolLine.Value[0]).OrderByDescending(x => x);
        foreach (var l in linesBefore) {
          foreach (var change in traceTable[l]) {
            if (!before.ContainsKey(change.Key)) {
              before.Add(change.Key, change.Value);
            }
          }
        }

        Dictionary<string, IList> after = new Dictionary<string, IList>();
        IEnumerable<int> changesOfSnippet;
        var linesAfterSnippet = traceChanges.Where(x => x > symbolLine.Value[1]);
        // if there are changes after the snippet and there is no other snippet inbetween the last line of the snippet and the change then this change belongs to the current snippet
        if (linesAfterSnippet.Count() > 0 && !symbolLinesBegin.Any(x => x > symbolLine.Value[1] && x < linesAfterSnippet.Min())) {
          changesOfSnippet = traceChanges.Where(x => x > symbolLine.Value[0] && x <= linesAfterSnippet.Min()).OrderByDescending(x => x);
        } else {
          changesOfSnippet = traceChanges.Where(x => x > symbolLine.Value[0] && x <= symbolLine.Value[1]).OrderByDescending(x => x);
        }
        foreach (var c in changesOfSnippet) {
          foreach (var change in traceTable[c]) {
            if (!after.ContainsKey(change.Key)) {
              after.Add(change.Key, change.Value);
            }
          }
        }

        semantics.Add(new PythonStatementSemantic() {
          TreeNodePrefixPos = prefixTreeNodes.IndexOf(symbolLine.Key),
          Before = before,
          After = after,
        });
      }

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, semantics);
    }

    /// <summary>
    /// </summary>
    /// <returns>Is a Dictionary which contains a List of values for every node, where index 0 is the line number where the code of the node starts and index 1 is the line number where the code ends</returns>
    private Dictionary<ISymbolicExpressionTreeNode, List<int>> FindStatementSymbolsInTree(ISymbolicExpressionTreeNode node, IEnumerable<string> productions, ref int curline) {
      Dictionary<ISymbolicExpressionTreeNode, List<int>> symbolToLineDict = new Dictionary<ISymbolicExpressionTreeNode, List<int>>();
      if (node.Subtrees.Count() > 0) {
        // node
        var symbol = node.Symbol as CFGSymbol;
        if (symbol != null) {
          var partsEnumerator = symbol.GetTerminalParts().GetEnumerator();
          var subtreeEnumerator = node.Subtrees.GetEnumerator();
          if (productions.Contains(symbol.Name)) {
            symbolToLineDict.Add(node, new List<int>() { curline });  // add beginning
          }
          while (partsEnumerator.MoveNext() && subtreeEnumerator.MoveNext()) {
            curline += partsEnumerator.Current.Count(c => c == '\n');
            symbolToLineDict = symbolToLineDict.Union(FindStatementSymbolsInTree(subtreeEnumerator.Current, productions, ref curline)).ToDictionary(k => k.Key, v => v.Value);
          }
          if (productions.Contains(symbol.Name)) {
            symbolToLineDict[node].Add(curline);   // add end
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
          symbolToLineDict.Add(node, new List<int>() { curline, curline });
        }
        var parts = symbol.GetTerminalParts();
        curline += parts.First().Count(c => c == '\n');
      }
      return symbolToLineDict;
    }
  }
}

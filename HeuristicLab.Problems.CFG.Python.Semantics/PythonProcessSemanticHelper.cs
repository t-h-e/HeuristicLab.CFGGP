using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonProcessSemanticHelper {
    #region traceCodeWithComments
    private const string traceCodeWithComments = @"import sys

past_locals = {{}}
variable_list = ['{0}']
traceTable = {{}}
executedLines = set()

# http://stackoverflow.com/questions/1645028/trace-table-for-python-programs/1645159#1645159
def trace(frame, event, arg_unused):
    global past_locals, traceTable, variable_list
    # only trace changes within evolve
    if frame.f_code.co_name != 'evolve':
        return None
    # event has to be line to avoid using the return statement twice
    if event != 'line':
        if event == 'call':  # reset past_locals as this is a new call to evolve
            past_locals = {{}}
        return trace

    # if past_locals is empty, us the current frame to get input variables
    # ToDo: if the first statement is a function call, this could lead to problems, as the variable changes of the function call might be saved as well
    # use past locals, as the variables have already been set
    # cannot use variables of current frame, as they are not set correctly yet, as the line has not yet been executed
    # using the current variables has different effects, when executing a simple assignment compared to a method call
    if past_locals:
        past_locals = frame.f_locals
    # use current line number to indicate how the variables were set before executing this line
    current_lineno = frame.f_lineno

    relevant_locals = {{}}
    executedLines.add(current_lineno)
    for k, v in past_locals.items():
        if k in variable_list:
            relevant_locals[k] = v

    if len(relevant_locals) > 0:
        # create dict for line number and all variable dicts
        if current_lineno not in traceTable:
            traceTable[current_lineno] = {{}}
            for v in variable_list:
                traceTable[current_lineno][v] = []
        elif len(traceTable[current_lineno][variable_list[0]]) >= {1}:
            past_locals = frame.f_locals
            return trace

        for v in variable_list:
            if v in relevant_locals:
                traceTable[current_lineno][v].append(relevant_locals[v])
            else:
                # actually not needed for IronPython, but added so it works for all python versions
                traceTable[current_lineno][v].append(None)
    past_locals = frame.f_locals
    return trace

sys.settrace(trace)

";
    #endregion

    #region traceCode
    private const string traceCode = @"import sys

past_locals = {{}}
variable_list = ['{0}']
traceTable = {{}}
traceTableBefore = {{}}
executedLines = {{}}
newcall = 0
limit = {1}
prev_line = -1

def trace(frame, event, arg_unused):
    global past_locals, traceTableBefore, traceTable, variable_list, newcall, executedLines, limit, prev_line
    if frame.f_code.co_name != 'evolve':
        return None

    if event != 'line':
        if event == 'call':
            past_locals = {{}}
            newcall += 1
            prev_line = frame.f_lineno
        return trace

    if past_locals:
        past_locals = frame.f_locals
    current_lineno = frame.f_lineno

    relevant_locals = {{}}
    if current_lineno not in executedLines:
        executedLines[current_lineno] = set()
    executedLines[current_lineno].add(newcall - 1)
    for k, v in past_locals.items():
        if k in variable_list:
            relevant_locals[k] = v

    if len(relevant_locals) > 0:
        if prev_line not in traceTable:
            traceTable[prev_line] = {{}}
            for v in variable_list:
                traceTable[prev_line][v] = [None] * (newcall - 1)
        if len(traceTable[prev_line][variable_list[0]]) < limit:
            for v in variable_list:
                if len(traceTable[prev_line][v]) < newcall - 1:
                    traceTable[prev_line][v] += [None] * (newcall - 1 - len(traceTable[prev_line][v]))
                if len(traceTable[prev_line][v]) < newcall:
                    if v in relevant_locals:
                        traceTable[prev_line][v].append(relevant_locals[v])
                    else:
                        traceTable[prev_line][v].append(None)

        if current_lineno not in traceTableBefore:
            traceTableBefore[current_lineno] = {{}}
            for v in variable_list:
                traceTableBefore[current_lineno][v] = [None] * (newcall - 1)
        if len(traceTableBefore[current_lineno][variable_list[0]]) < limit:
            for v in variable_list:
                if len(traceTableBefore[current_lineno][v]) < newcall - 1:
                    traceTableBefore[current_lineno][v] += [None] * (newcall - 1 - len(traceTableBefore[current_lineno][v]))
                if len(traceTableBefore[current_lineno][v]) < newcall:
                    if v in relevant_locals:
                        traceTableBefore[current_lineno][v].append(relevant_locals[v])
                    else:
                        traceTableBefore[current_lineno][v].append(None)

    past_locals = frame.f_locals
    prev_line = current_lineno
    return trace

sys.settrace(trace)

";

    private const string traceTableReduceEntries = @"
sys.settrace(None)

def fix_tracetable(traceTable):
    global limit
    lines = sorted(list(traceTable.keys()), reverse=True)
    training_cases = min(limit, len(inval))
    # trace will be filled up to be of the same length
    for i in range(0, len(lines)):
      for v in variable_list:
        if len(traceTable[lines[i]][v]) < training_cases:
          traceTable[lines[i]][v] += [None] * (training_cases - len(traceTable[lines[i]][v]))

    # correct the traceTable
    # None values are placeholders in case a statement has not been called for every training case
    # None values will be replaced with values that have been set previously
    for i in range(0, len(lines) - 1):
      for v in variable_list:
        for j in range(0, len(traceTable[lines[i]][v])):
          if traceTable[lines[i]][v][j] is None:
            for l in lines[i + 1:]:
              if traceTable[l][v][j] is not None:
                traceTable[lines[i]][v][j] = traceTable[l][v][j]
                break
    return traceTable

def clean_traceTable(traceTable, traceTableBefore):
    # data that has been set before has not changed
    # remove it
    for l in traceTableBefore.keys():
        if l in traceTable:
            for v in traceTableBefore[l].keys():
                if v in traceTable[l] and traceTableBefore[l][v] == traceTable[l][v]:
                    del traceTable[l][v]
            if len(traceTable[l]) == 0:
                del traceTable[l]
    return traceTable, traceTableBefore

def remove_redundant_data(traceTable):
    lines = sorted(list(traceTable.keys()), reverse=True)
    # remove data that has not changed
    for i in range(1, len(lines)):
      for v in variable_list:
        if v in traceTable[lines[i]] and v in traceTable[lines[i - 1]]:
          if traceTable[lines[i]][v] == traceTable[lines[i - 1]][v]:
            del traceTable[lines[i - 1]][v]

    for l in lines:
      if not traceTable[l]:
        del traceTable[l]
    return traceTable

traceTableBefore = fix_tracetable(traceTableBefore)
traceTable = fix_tracetable(traceTable)
traceTable, traceTableBefore = clean_traceTable(traceTable, traceTableBefore)
traceTableBefore = remove_redundant_data(traceTableBefore)
traceTable = remove_redundant_data(traceTable)
executedLines = {k: sorted(v) for k, v in executedLines.items()}
";
    #endregion

    private readonly string traceCodeWithVariables;

    public PythonProcessSemanticHelper() {
      traceCodeWithVariables = String.Empty;
    }

    public PythonProcessSemanticHelper(IEnumerable<string> variableNames, int limit) {
      if (variableNames == null || !variableNames.Any() || limit <= 0) {
        traceCodeWithVariables = String.Empty;
      } else {
        traceCodeWithVariables = String.Format(traceCode, String.Join("', '", variableNames.Where(x => !String.IsNullOrWhiteSpace(x))), limit);
      }
    }

    // ToDo: Remove workaround; use executed lines in PythonStatementSemantic
    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> EvaluateAndTraceProgram(PythonProcess pythonProcess, string program, string input, string output, IEnumerable<int> indices, string header, string footer, ISymbolicExpressionTree tree, double timeout = 1) {
      string traceProgram = traceCodeWithVariables
                          + program;
      traceProgram += traceCodeWithVariables == String.Empty
                    ? String.Empty
                    : traceTableReduceEntries;

      EvaluationScript es = pythonProcess.CreateEvaluationScript(traceProgram, input, output, timeout);
      es.Variables.Add("traceTable");
      es.Variables.Add("traceTableBefore");
      es.Variables.Add("executedLines");

      JObject json = pythonProcess.SendAndEvaluateProgram(es);
      var baseResult = pythonProcess.GetVariablesFromJson(json, indices.Count());

      if (json["traceTable"] == null) {
        return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, new List<PythonStatementSemantic>());
      }

      var traceTable = json["traceTable"].ToObject<IDictionary<int, IDictionary<string, IList>>>();
      var traceTableBefore = json["traceTableBefore"].ToObject<IDictionary<int, IDictionary<string, IList>>>();
      var executedLines = json["executedLines"].ToObject<IDictionary<int, List<int>>>();

      List<PythonStatementSemantic> semantics = new List<PythonStatementSemantic>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(root.Grammar);

      // calculate the correct line the semantic evaluation starts from
      var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      int curline = es.Script.Count(c => c == '\n') - code.Count(c => c == '\n') - footer.Count(c => c == '\n') - traceTableReduceEntries.Count(c => c == '\n');

      var symbolToLineDict = FindStatementSymbolsInTree(root, statementProductionNames, ref curline);

      #region workaround: empty line problem with while, can't fix, otherwise FindStatementSymbolsInTree won't work
      string[] sciptLines = es.Script.Split('\n');
      var beginLineNumbers = symbolToLineDict.Values.Select(x => x[0]).Distinct().ToList();
      var moveLines = new Dictionary<int, int>(beginLineNumbers.Count);
      foreach (var l in beginLineNumbers) {
        // decrease by one, as sciptLines is an array and start from zero, where lineNumbers started counting from 1
        if (String.IsNullOrWhiteSpace(sciptLines[l - 1]) || sciptLines[l - 1].TrimStart().StartsWith("#")) {
          // empty line or comment
          var i = l + 1;
          while (i - 1 < sciptLines.Length && (String.IsNullOrWhiteSpace(sciptLines[i - 1]) || sciptLines[i - 1].TrimStart().StartsWith("#"))) {
            i++;
          }
          moveLines.Add(l, i);
        } else {
          moveLines.Add(l, l);
        }
      }
      foreach (var symbolLine in symbolToLineDict) {
        symbolLine.Value[0] = moveLines[symbolLine.Value[0]];
      }
      #endregion
      #region fix Before line for <predefined> to have all variables initialised
      // not a great way to do it, but the python interpreter does not stop at e.g. 'while False:'
      int newMinBefore = traceTableBefore.OrderBy(x => x.Key).First(x => x.Value.ContainsKey("res0") && !x.Value["res0"].Contains(null)).Key; // first line that changes res0
      foreach (var symbolToLine in symbolToLineDict) {
        symbolToLine.Value.Add(symbolToLine.Value[0]); // original beginning at [2], which is needed for executed lines
        if (symbolToLine.Value[0] < newMinBefore) {
          symbolToLine.Value[0] = newMinBefore;
        }
      }
      #endregion

      var prefixTreeNodes = tree.IterateNodesPrefix().ToList();

      foreach (var symbolLine in symbolToLineDict) {
        // Before
        Dictionary<string, IList> before = new Dictionary<string, IList>();
        foreach (var traceChange in traceTableBefore.Where(x => x.Key <= symbolLine.Value[0]).OrderByDescending(x => x.Key)) {
          foreach (var variableChange in traceChange.Value) {
            if (!before.ContainsKey(variableChange.Key)) {
              before.Add(variableChange.Key, variableChange.Value);
            }
          }
        }
        // After
        Dictionary<string, IList> after = new Dictionary<string, IList>();
        foreach (var traceChange in traceTable.Where(x => x.Key >= symbolLine.Value[0] && x.Key <= symbolLine.Value[1]).OrderByDescending(x => x.Key)) {
          foreach (var variableChange in traceChange.Value) {
            if (!after.ContainsKey(variableChange.Key)) {
              after.Add(variableChange.Key, variableChange.Value);
            }
          }
        }
        // clean after with before                         
        foreach (var key in after.Keys.ToList()) {
          if (PythonSemanticComparer.CompareSequence(after[key], before[key])) {
            after.Remove(key);
          }
        }
        // add semantics
        var executedLinesWithinStatement = executedLines.Where(x => x.Key <= symbolLine.Value[2] && x.Key >= symbolLine.Value[1]);
        semantics.Add(new PythonStatementSemantic() {
          TreeNodePrefixPos = prefixTreeNodes.IndexOf(symbolLine.Key),
          ExecutedCases = executedLinesWithinStatement.Any() ? executedLinesWithinStatement.OrderByDescending(x => x.Value.Count).First().Value : new List<int>(),
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
      if (node.Subtrees.Any()) {
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

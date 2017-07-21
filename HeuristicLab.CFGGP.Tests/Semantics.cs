using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Problems.CFG;
using HeuristicLab.Problems.CFG.Python;
using HeuristicLab.Problems.CFG.Python.Semantics;
using HeuristicLab.Problems.Instances.CFG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class Semantics {
    [TestMethod]
    public void SemanticsTest() {
      BenchmarkSuiteInstanceProvider prov = new BenchmarkSuiteListInstanceProvider();
      var prob = new CFGPythonProblem();
      prob.Load(prov.LoadData(prov.GetDataDescriptors().First(x => x.Name == "Smallest")));

      var grammar = prob.Grammar;

      var root = (SymbolicExpressionTreeTopLevelNode)grammar.ProgramRootSymbol.CreateTreeNode();
      root.SetGrammar(grammar.CreateExpressionTreeGrammar());
      //var root = new SymbolicExpressionTreeNode(new ProgramRootSymbol());
      var start = new SymbolicExpressionTreeNode(new StartSymbol());
      root.AddSubtree(start);

      var gstart = new SymbolicExpressionTreeNode(grammar.StartSymbol);
      start.AddSubtree(gstart);
      var predefined = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'i0 = int(); i1 = int(); i2 = int()'\r\n'b0 = bool(); b1 = bool(); b2 = bool()'\r\n'res0 = int()'\r\n<code>"));
      gstart.AddSubtree(predefined);
      var codestatementSym = GetSymbol(grammar, "<code><statement>'\\n'");
      var code0 = new SymbolicExpressionTreeNode(codestatementSym);
      var code1 = new SymbolicExpressionTreeNode(codestatementSym);
      var code2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      //predefined.AddSubtree(code1);
      predefined.AddSubtree(code0);
      code0.AddSubtree(code1);
      code1.AddSubtree(code2);

      var assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      code2.AddSubtree(assign0);
      //code0.AddSubtree(assign1);

      var if0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<if>"));
      code1.AddSubtree(if0);

      var bool_assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>' = '<bool>"));
      var int_assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
      assign0.AddSubtree(bool_assign0);
      assign1.AddSubtree(int_assign1);

      var bool_var0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>"));
      var int_var1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>"));
      var b0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      var i1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
      bool_assign0.AddSubtree(bool_var0);
      int_assign1.AddSubtree(int_var1);
      bool_var0.AddSubtree(b0);
      int_var1.AddSubtree(i1);

      var bool_const0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_const>"));
      var int_const1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'int('<number>'.0)'"));
      //var num0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
      var num1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
      var boolconstTrue = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'True'"));
      var intconst2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'2'"));

      bool_assign0.AddSubtree(bool_const0);
      int_assign1.AddSubtree(int_const1);
      bool_const0.AddSubtree(boolconstTrue);
      int_const1.AddSubtree(num1);
      num1.AddSubtree(intconst2);

      var ifthenelse0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'if '<bool>':{:\\n'<code>':}else:{:\\n'<code>':}'"));
      var b0_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      if0.AddSubtree(ifthenelse0);
      ifthenelse0.AddSubtree(b0_2);

      var statement1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      var assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var int_assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
      var i1_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
      var intconst7 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'7'"));
      ifthenelse0.AddSubtree(statement1);
      statement1.AddSubtree(assign2);
      assign2.AddSubtree(int_assign2);
      int_assign2.AddSubtree(i1_2);
      int_assign2.AddSubtree(intconst7);

      var statement2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      var assign3 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var int_assign3 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
      var i1_3 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
      var intconst8 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'9'"));
      ifthenelse0.AddSubtree(statement2);
      statement2.AddSubtree(assign3);
      assign3.AddSubtree(int_assign3);
      int_assign3.AddSubtree(i1_3);
      int_assign3.AddSubtree(intconst8);

      var while0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'loopBreak% = 0\\nwhile '<bool>':{:\\n'<code>'\\nif loopBreak% > loopBreakConst or stop.value:{:\\nbreak\\n:}loopBreak% += 1\\n:}'"));
      var b0_3 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      var code4 = new SymbolicExpressionTreeNode(codestatementSym);
      var statement3 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      var bool_assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>' = '<bool>"));
      var bool_var1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>"));
      var b0_4 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      var bool_const1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_const>"));
      var boolconstFalse = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'False'"));

      bool_assign1.AddSubtree(bool_var1);
      bool_var1.AddSubtree(b0_4);
      bool_assign1.AddSubtree(bool_const1);
      bool_const1.AddSubtree(boolconstFalse);
      statement3.AddSubtree(bool_assign1);

      //var statement4 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      var assign4 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var int_assign4 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
      var i1_4 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
      var intconst9 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'1'"));
      //statement4.AddSubtree(assign4);
      assign4.AddSubtree(int_assign4);
      int_assign4.AddSubtree(i1_4);
      int_assign4.AddSubtree(intconst9);

      code4.AddSubtree(statement3);
      code4.AddSubtree(assign4);

      ((List<string>)((CFGProduction)while0.Symbol).parts)[0] = "x = 5\n" + ((List<string>)((CFGProduction)while0.Symbol).parts)[0];
      while0.AddSubtree(b0_3);
      while0.AddSubtree(code4);

      code0.AddSubtree(while0);


      var tree = new SymbolicExpressionTree(root);
      var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      System.Console.WriteLine(code);


      //PythonProcessSemanticHelper ppsh = new PythonProcessSemanticHelper(prob.ProblemData.Variables.GetVariableNames(), 10);
      traceCodeWithVariables = String.Format(traceCode, String.Join("', '", prob.ProblemData.Variables.GetVariableNames().Where(x => !String.IsNullOrWhiteSpace(x))), 10);

      var bla = PythonHelper.FormatToProgram(tree, prob.ProblemData.LoopBreakConst, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter);
      //  var blub = ppsh.EvaluateAndTraceProgram(prob.PythonProcess,
      var blub = EvaluateAndTraceProgram(prob.PythonProcess,
                                             bla,
                                             PythonHelper.ConvertToPythonValues(prob.ProblemData.Input, prob.ProblemData.TrainingIndices),
                                             PythonHelper.ConvertToPythonValues(prob.ProblemData.Output, prob.ProblemData.TrainingIndices),
                                             prob.ProblemData.TrainingIndices, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter, tree);

      Console.WriteLine(blub);

    }

    #region remove!!!
    private string traceCodeWithVariables;

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
            for l in lines[i+1:]:
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
        if v in traceTable[lines[i]] and v in traceTable[lines[i-1]]:
          if traceTable[lines[i]][v] == traceTable[lines[i-1]][v]:
            del traceTable[lines[i-1]][v]

    for l in lines:
      if not traceTable[l]:
        del traceTable[l]
    return traceTable

traceTableBefore = fix_tracetable(traceTableBefore)
traceTable = fix_tracetable(traceTable)
traceTable, traceTableBefore = clean_traceTable(traceTable, traceTableBefore)
traceTableBefore = remove_redundant_data(traceTableBefore)
traceTable = remove_redundant_data(traceTable)
executedLines = {k: list(v) for k, v in executedLines.items()}";
    #endregion
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

      if (json["traceTable"] == null || json["traceTableBefore"] == null) {
        return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>>(baseResult.Item1, baseResult.Item2, baseResult.Item3, baseResult.Item4, new List<PythonStatementSemantic>());
      }

      var traceTable = JsonConvert.DeserializeObject<IDictionary<int, IDictionary<string, IList>>>(json["traceTable"].ToString());
      var traceTableBefore = JsonConvert.DeserializeObject<IDictionary<int, IDictionary<string, IList>>>(json["traceTableBefore"].ToString());
      var executedLinesNew = JsonConvert.DeserializeObject<IDictionary<int, List<int>>>(json["executedLines"].ToString());
      var executedLines = executedLinesNew.Keys.ToList();
      executedLines.Sort();

      IList<int> traceChanges = traceTable.Keys.OrderBy(x => x).ToList();
      IList<int> traceBeforeChanges = traceTableBefore.Keys.OrderBy(x => x).ToList();

      List<PythonStatementSemantic> semantics = new List<PythonStatementSemantic>();
      ISymbolicExpressionTreeNode root = tree.Root;

      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(root.Grammar);

      // calculate the correct line the semantic evaluation starts from
      var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      int curline = es.Script.Count(c => c == '\n') - code.Count(c => c == '\n') - footer.Count(c => c == '\n') - traceTableReduceEntries.Count(c => c == '\n');

      var symbolToLineDict = FindStatementSymbolsInTree(root, statementProductionNames, ref curline);
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
          if (CompareSequence(after[key], before[key])) {
            after.Remove(key);
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

    public bool CompareSequence(IEnumerable first, IEnumerable second) {
      var v1Enumerator = first.GetEnumerator();
      var v2Enumerator = second.GetEnumerator();
      while (v1Enumerator.MoveNext() && v2Enumerator.MoveNext()) {
        if (v1Enumerator.Current is IEnumerable) {
          // then both must be IEnumerable
          if (!CompareSequence(v1Enumerator.Current as IEnumerable, v2Enumerator.Current as IEnumerable)) {
            return false;
          }
        } else if (!v1Enumerator.Current.Equals(v2Enumerator.Current)) {
          return false;
        }
      }
      if (v1Enumerator.MoveNext() || v2Enumerator.MoveNext()) {
        return false;
      }
      return true;
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

    [TestMethod]
    public void TestSemanticSwapInCrossover() {
      var grammar = GenerateSimpleGrammar();
      var parent0 = GenerateSimpleTree(grammar);
      var parent1 = GenerateSimpleTree(grammar);

      var cutPoint = new CutPoint(parent0.Root, parent0.Root.GetSubtree(0));
      var selectedBranch = parent1.Root.GetSubtree(1);

      var p0Nodes = parent0.IterateNodesPrefix().ToList();
      var sem0Enumerable = p0Nodes.Select(x => new PythonStatementSemantic() { Before = new Dictionary<string, IList>() { { "p0_" + p0Nodes.IndexOf(x), null } }, TreeNodePrefixPos = p0Nodes.IndexOf(x) });
      Assert.AreEqual(sem0Enumerable.Count(), 7);

      var p1Nodes = parent1.IterateNodesPrefix().ToList();
      var sem1Enumerable = p1Nodes.Select(x => new PythonStatementSemantic() { Before = new Dictionary<string, IList>() { { "p1_" + p1Nodes.IndexOf(x), null } }, TreeNodePrefixPos = p1Nodes.IndexOf(x) });
      Assert.AreEqual(sem1Enumerable.Count(), 7);

      var sem0 = new ItemArray<PythonStatementSemantic>(sem0Enumerable);
      var sem1 = new ItemArray<PythonStatementSemantic>(sem1Enumerable);

      var crossover = new CFGPythonSemanticEvalCrossover<ICFGPythonProblemData>();
      MethodInfo semanticSwap = crossover.GetType().GetMethod("SemanticSwap", BindingFlags.NonPublic | BindingFlags.Static);

      var methodParams = new object[] { cutPoint, selectedBranch, parent0, parent1, sem0, sem1 };

      var res = semanticSwap.Invoke(this, methodParams);
      var newSemantic = res as ItemArray<PythonStatementSemantic>;
      Assert.IsNotNull(newSemantic);
      Assert.AreEqual(newSemantic.Count(), 7);

      Dictionary<int, string> assertSem = new Dictionary<int, string>() {
        { 0, "p0_0"},
        { 1, "p1_4"},
        { 2, "p1_5"},
        { 3, "p1_6"},
        { 4, "p0_4"},
        { 5, "p0_5"},
        { 6, "p0_6"}
      };
      foreach (var sem in newSemantic) {
        Assert.AreEqual(assertSem[sem.TreeNodePrefixPos], sem.Before.Keys.First());
      }
    }

    private ISymbol GetSymbol(CFGExpressionGrammar grammar, string name) {
      return grammar.Symbols.First(x => x.Name == name);
    }

    private ISymbolicExpressionTreeGrammar GenerateSimpleGrammar() {
      var branch = new SimpleSymbol("branch", "", 2, 2);
      var leaf = new SimpleSymbol("leaf", "", 0, 0);

      var grammar = new SimpleSymbolicExpressionGrammar();
      grammar.AddSymbol(branch);
      grammar.AddSymbol(leaf);
      grammar.AddAllowedChildSymbol(branch, leaf);
      return grammar.CreateExpressionTreeGrammar(); ;
    }

    private ISymbolicExpressionTree GenerateSimpleTree(ISymbolicExpressionTreeGrammar grammar) {
      var branch = new SimpleSymbol("branch", "", 2, 2);
      var leaf = new SimpleSymbol("leaf", "", 0, 0);

      var root = new SymbolicExpressionTreeTopLevelNode(branch);
      root.SetGrammar(grammar);
      var branch0 = new SymbolicExpressionTreeNode(branch);
      var branch1 = new SymbolicExpressionTreeNode(branch);
      var leaf0 = new SymbolicExpressionTreeNode(leaf);
      var leaf1 = new SymbolicExpressionTreeNode(leaf);
      var leaf2 = new SymbolicExpressionTreeNode(leaf);
      var leaf3 = new SymbolicExpressionTreeNode(leaf);
      root.AddSubtree(branch0);
      root.AddSubtree(branch1);
      branch0.AddSubtree(leaf0);
      branch0.AddSubtree(leaf1);
      branch1.AddSubtree(leaf2);
      branch1.AddSubtree(leaf3);

      return new SymbolicExpressionTree(root);
    }
  }
}

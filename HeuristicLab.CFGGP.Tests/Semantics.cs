using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Problems.CFG;
using HeuristicLab.Problems.CFG.Python;
using HeuristicLab.Problems.CFG.Python.Semantics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class Semantics {
    //[TestMethod]
    //public void SemanticsTest() {
    //  BenchmarkSuiteInstanceProvider prov = new BenchmarkSuiteListInstanceProvider();
    //  var prob = new CFGPythonProblem();
    //  prob.Load(prov.LoadData(prov.GetDataDescriptors().First(x => x.Name == "Smallest")));

    //  var grammar = prob.Grammar;

    //  var root = (SymbolicExpressionTreeTopLevelNode)grammar.ProgramRootSymbol.CreateTreeNode();
    //  root.SetGrammar(grammar.CreateExpressionTreeGrammar());
    //  //var root = new SymbolicExpressionTreeNode(new ProgramRootSymbol());
    //  var start = new SymbolicExpressionTreeNode(new StartSymbol());
    //  root.AddSubtree(start);

    //  var gstart = new SymbolicExpressionTreeNode(grammar.StartSymbol);
    //  start.AddSubtree(gstart);

    //  var codestatementSym = GetSymbol(grammar, "<code><statement>'\\n'");
    //  var code0 = new SymbolicExpressionTreeNode(codestatementSym);
    //  var code1 = new SymbolicExpressionTreeNode(codestatementSym);
    //  var code2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
    //  gstart.AddSubtree(code0);
    //  code0.AddSubtree(code1);
    //  code1.AddSubtree(code2);

    //  var assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
    //  var assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
    //  code2.AddSubtree(assign0);
    //  code0.AddSubtree(assign1);

    //  var if0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<if>"));
    //  code1.AddSubtree(if0);

    //  var bool_assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>' = '<bool>"));
    //  var int_assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
    //  assign0.AddSubtree(bool_assign0);
    //  assign1.AddSubtree(int_assign1);

    //  var bool_var0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>"));
    //  var int_var1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>"));
    //  var b0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
    //  var i1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
    //  bool_assign0.AddSubtree(bool_var0);
    //  int_assign1.AddSubtree(int_var1);
    //  bool_var0.AddSubtree(b0);
    //  int_var1.AddSubtree(i1);

    //  var bool_const0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_const>"));
    //  var int_const1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'int('<number>'.0)'"));
    //  //var num0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
    //  var num1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
    //  var boolconstTrue = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'True'"));
    //  var intconst2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'2'"));

    //  bool_assign0.AddSubtree(bool_const0);
    //  int_assign1.AddSubtree(int_const1);
    //  bool_const0.AddSubtree(boolconstTrue);
    //  int_const1.AddSubtree(num1);
    //  num1.AddSubtree(intconst2);

    //  var ifthen0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'if '<bool>':{:\\n'<code>':}'"));
    //  var b0_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
    //  if0.AddSubtree(ifthen0);
    //  ifthen0.AddSubtree(b0_2);

    //  var statement1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
    //  var assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
    //  var int_assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>' = '<int>"));
    //  var i1_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res0'"));
    //  var intconst7 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'7'"));
    //  ifthen0.AddSubtree(statement1);
    //  statement1.AddSubtree(assign2);
    //  assign2.AddSubtree(int_assign2);
    //  int_assign2.AddSubtree(i1_2);
    //  int_assign2.AddSubtree(intconst7);



    //  var tree = new SymbolicExpressionTree(root);
    //  var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
    //  System.Console.WriteLine(code);


    //  PythonProcessSemanticHelper ppsh = new PythonProcessSemanticHelper(prob.ProblemData.Variables.GetVariableNames(), 10);

    //  var bla = PythonHelper.FormatToProgram(tree, prob.ProblemData.LoopBreakConst, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter);
    //  var blub = ppsh.EvaluateAndTraceProgram(prob.PythonProcess,
    //                                         bla,
    //                                         PythonHelper.ConvertToPythonValues(prob.ProblemData.Input, prob.ProblemData.TrainingIndices),
    //                                         PythonHelper.ConvertToPythonValues(prob.ProblemData.Output, prob.ProblemData.TrainingIndices),
    //                                         prob.ProblemData.TrainingIndices, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter, tree);

    //  Console.WriteLine(blub);

    //}

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

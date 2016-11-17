using System;
using System.Linq;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Problems.CFG;
using HeuristicLab.Problems.CFG.Python;
using HeuristicLab.Problems.CFG.Python.Semantics;
using HeuristicLab.Problems.Instances.CFG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class Semantic {
    [TestMethod]
    public void TestMethod1() {
      BenchmarkSuiteInstanceProvider prov = new BenchmarkSuiteInstanceProvider();
      var prob = new CFGPythonProblem();
      prob.Load(prov.LoadData(prov.GetDataDescriptors().Where(x => x.Name == "Smallest").First()));

      var grammar = prob.Grammar;

      var root = (SymbolicExpressionTreeTopLevelNode)grammar.ProgramRootSymbol.CreateTreeNode();
      root.SetGrammar(grammar.CreateExpressionTreeGrammar());
      //var root = new SymbolicExpressionTreeNode(new ProgramRootSymbol());
      var start = new SymbolicExpressionTreeNode(new StartSymbol());
      root.AddSubtree(start);

      var gstart = new SymbolicExpressionTreeNode(grammar.StartSymbol);
      start.AddSubtree(gstart);

      var codestatementSym = GetSymbol(grammar, "<code><statement>'\\n'");
      var code0 = new SymbolicExpressionTreeNode(codestatementSym);
      var code1 = new SymbolicExpressionTreeNode(codestatementSym);
      var code2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      gstart.AddSubtree(code0);
      code0.AddSubtree(code1);
      code1.AddSubtree(code2);

      var assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      code2.AddSubtree(assign0);
      code0.AddSubtree(assign1);

      var if0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<if>"));
      code1.AddSubtree(if0);

      var bool_assign0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>' = '<bool>"));
      var int_assign1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var> '=' <int>"));
      assign0.AddSubtree(bool_assign0);
      assign1.AddSubtree(int_assign1);

      var bool_var0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_var>"));
      var int_var1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var>"));
      var b0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      var i1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res'"));
      bool_assign0.AddSubtree(bool_var0);
      int_assign1.AddSubtree(int_var1);
      bool_var0.AddSubtree(b0);
      int_var1.AddSubtree(i1);

      var bool_const0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<bool_const>"));
      var int_const1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_const>"));
      //var num0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
      var num1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<num>"));
      var boolconstTrue = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'True'"));
      var intconst2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "2"));

      bool_assign0.AddSubtree(bool_const0);
      int_assign1.AddSubtree(int_const1);
      bool_const0.AddSubtree(boolconstTrue);
      int_const1.AddSubtree(num1);
      num1.AddSubtree(intconst2);

      var ifthen0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'if '<bool><block>"));
      var b0_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'b0'"));
      if0.AddSubtree(ifthen0);
      ifthen0.AddSubtree(b0_2);

      var codeblock0 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "':{:\\n'<code>':}'"));
      var statement1 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<statement>'\\n'"));
      var assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<assign>"));
      var int_assign2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "<int_var> '=' <int>"));
      var i1_2 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "'res'"));
      var intconst7 = new SymbolicExpressionTreeNode(GetSymbol(grammar, "7"));
      ifthen0.AddSubtree(codeblock0);
      codeblock0.AddSubtree(statement1);
      statement1.AddSubtree(assign2);
      assign2.AddSubtree(int_assign2);
      int_assign2.AddSubtree(i1_2);
      int_assign2.AddSubtree(intconst7);



      var tree = new SymbolicExpressionTree(root);
      var code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      System.Console.WriteLine(code);


      PythonProcessSemanticHelper ppsh = new PythonProcessSemanticHelper(prob.ProblemData.Variables.GetVariableNames(), 10);

      var bla = PythonHelper.FormatToProgram(tree, prob.ProblemData.LoopBreakConst, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter);
      var blub = ppsh.EvaluateAndTraceProgram(prob.PythonProcess,
                                             bla,
                                             PythonHelper.ConvertToPythonValues(prob.ProblemData.Input, prob.ProblemData.TrainingIndices),
                                             PythonHelper.ConvertToPythonValues(prob.ProblemData.Output, prob.ProblemData.TrainingIndices),
                                             prob.ProblemData.TrainingIndices, prob.ProblemData.FullHeader, prob.ProblemData.FullFooter, tree);

      Console.WriteLine(blub);

    }

    private ISymbol GetSymbol(CFGExpressionGrammar grammar, string name) {
      return grammar.Symbols.Where(x => x.Name == name).First();
    }
  }
}

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
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("CFGPythonSemanticEvalCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class CFGPythonSemanticEvalCrossover : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator, ICFGPythonSemanticsCrossover {
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";
    private const string ProblemDataParameterName = "ProblemData";

    private const string SymbolicExpressionTreeGrammarParameterName = "SymbolicExpressionTreeGrammar";
    private const string SemanticsParameterName = "Semantic";

    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public IValueLookupParameter<PercentValue> CrossoverProbabilityParameter {
      get { return (IValueLookupParameter<PercentValue>)Parameters[CrossoverProbabilityParameterName]; }
    }

    public IValueLookupParameter<ISymbolicExpressionGrammar> SymbolicExpressionTreeGrammarParameter {
      get { return (IValueLookupParameter<ISymbolicExpressionGrammar>)Parameters[SymbolicExpressionTreeGrammarParameterName]; }
    }
    public ILookupParameter<ItemArray<ItemArray<PythonStatementSemantic>>> SemanticsParameter {
      get { return (ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>)Parameters[SemanticsParameterName]; }
    }

    public IValueLookupParameter<ICFGPythonProblemData> ProblemDataParameter {
      get { return (IValueLookupParameter<ICFGPythonProblemData>)Parameters[ProblemDataParameterName]; }
    }
    #endregion
    #region Properties
    public IntValue MaximumSymbolicExpressionTreeLength {
      get { return MaximumSymbolicExpressionTreeLengthParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeDepth {
      get { return MaximumSymbolicExpressionTreeDepthParameter.ActualValue; }
    }
    public PercentValue CrossoverProbability {
      get { return CrossoverProbabilityParameter.ActualValue; }
    }

    public ICFGPythonProblemData ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    private ItemArray<ItemArray<PythonStatementSemantic>> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    #endregion
    [StorableConstructor]
    protected CFGPythonSemanticEvalCrossover(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticEvalCrossover(CFGPythonSemanticEvalCrossover original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonSemanticEvalCrossover()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));

      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));

      Parameters.Add(new ValueLookupParameter<ISymbolicExpressionGrammar>(SymbolicExpressionTreeGrammarParameterName, "Tree grammar"));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>(SemanticsParameterName, ""));

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticEvalCrossover(this, cloner);
    }

    private void RegisterEventHandlers() {
      SymbolicExpressionTreeGrammarParameter.ValueChanged += new EventHandler(SymbolicExpressionTreeGrammarParameter_ValueChanged);
    }

    private void SymbolicExpressionTreeGrammarParameter_ValueChanged(object sender, EventArgs e) {
      if (SymbolicExpressionTreeGrammarParameter.Value == null) return;
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value);

      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) return parent0;

      var p0NodeIndices = semantic0.Select(x => x.TreeNodePrefixPos).ToList();
      var crossoverPoints0 = parent0.IterateNodesPrefix().ToList().Where((value, index) => p0NodeIndices.Contains(index)).Select(x => new CutPoint(x.Parent, x));
      var p1NodeIndices = semantic1.Select(x => x.TreeNodePrefixPos).ToList();
      var p1StatementNodes = parent1.IterateNodesPrefix().ToList().Where((value, index) => p1NodeIndices.Contains(index)).ToList();

      var crossoverPoint0 = crossoverPoints0.SampleRandom(random);
      int level = parent0.Root.GetBranchLevel(crossoverPoint0.Child);
      int length = parent0.Root.GetLength() - crossoverPoint0.Child.GetLength();

      var allowedBranches = new List<ISymbolicExpressionTreeNode>();
      p1StatementNodes.ForEach((n) => {
        if (n.GetDepth() + level <= maxTreeDepth && n.GetLength() + length <= maxTreeLength && crossoverPoint0.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });

      if (allowedBranches.Count == 0)
        return parent0;

      // add variable setting to problem data
      var variableSettings = String.Join("\n", problemData.VariableSettings.Select(x => x.Value));
      var variables = problemData.Variables.CheckedItems.Select(x => x.Value.Value).ToList();

      // create symbols in order to improvize an ad-hoc tree so that the child can be evaluated
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      EvaluationScript crossoverPointScript0 = new EvaluationScript() {
        Script = String.Format("{0}\n", variableSettings, PythonHelper.FormatToProgram(CreateTreeFromNode(random, crossoverPoint0.Child, rootSymbol, startSymbol))),
        Variables = variables //new List<string>() { "cases", "caseQuality", "quality" }
      };
      JObject json0 = PythonProcess.GetInstance().SendAndEvaluateProgram(crossoverPointScript0);
      crossoverPoint0.Child.Parent = crossoverPoint0.Parent; // restore parent
      ISymbolicExpressionTreeNode selectedBranch = null;

      // pick the first node that fulfills the semantic similarity conditions
      foreach (var node in allowedBranches) {
        var parent = node.Parent;
        var tree1 = CreateTreeFromNode(random, node, startSymbol, rootSymbol); // this will affect node.Parent 
        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = String.Format("{0}\n", variableSettings, PythonHelper.FormatToProgram(tree1)),
          Variables = variables //new List<string>() { "cases", "caseQuality", "quality" }
        };
        JObject json1 = PythonProcess.GetInstance().SendAndEvaluateProgram(evaluationScript1);
        node.Parent = parent; // restore parent

        if (DoSimilarityCalculations(json0, json1, variables)) {
          selectedBranch = node;
          break;
        }
      }

      // perform the actual swap
      if (selectedBranch != null)
        Swap(crossoverPoint0, selectedBranch);
      return parent0;
    }

    private bool DoSimilarityCalculations(JObject json0, JObject json1, IList<string> variables) {
      throw new NotImplementedException();
    }

    //copied from SymbolicDataAnalysisExpressionCrossover<T>
    protected static void Swap(CutPoint crossoverPoint, ISymbolicExpressionTreeNode selectedBranch) {
      if (crossoverPoint.Child != null) {
        // manipulate the tree of parent0 in place
        // replace the branch in tree0 with the selected branch from tree1
        crossoverPoint.Parent.RemoveSubtree(crossoverPoint.ChildIndex);
        if (selectedBranch != null) {
          crossoverPoint.Parent.InsertSubtree(crossoverPoint.ChildIndex, selectedBranch);
        }
      } else {
        // child is null (additional child should be added under the parent)
        if (selectedBranch != null) {
          crossoverPoint.Parent.AddSubtree(selectedBranch);
        }
      }
    }
    //copied from SymbolicDataAnalysisExpressionCrossover<T>
    protected static ISymbolicExpressionTree CreateTreeFromNode(IRandom random, ISymbolicExpressionTreeNode node, ISymbol rootSymbol, ISymbol startSymbol) {
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

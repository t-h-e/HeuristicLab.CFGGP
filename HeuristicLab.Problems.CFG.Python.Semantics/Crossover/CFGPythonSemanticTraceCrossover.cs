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
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("CFGPythonSemanticTraceCrossover", "Semantic crossover for program synthesis, which uses the trace table provided by the evaluator to decide on a crossover point.")]
  [StorableClass]
  public class CFGPythonSemanticTraceCrossover : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator, ICFGPythonSemanticsCrossover {
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";

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
    private ItemArray<ItemArray<PythonStatementSemantic>> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    #endregion
    [StorableConstructor]
    protected CFGPythonSemanticTraceCrossover(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticTraceCrossover(CFGPythonSemanticTraceCrossover original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonSemanticTraceCrossover()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));

      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));

      Parameters.Add(new ValueLookupParameter<ISymbolicExpressionGrammar>(SymbolicExpressionTreeGrammarParameterName, "Tree grammar"));
      Parameters.Add(new ScopeTreeLookupParameter<ItemArray<PythonStatementSemantic>>(SemanticsParameterName, ""));

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticTraceCrossover(this, cloner);
    }

    private void RegisterEventHandlers() {
      SymbolicExpressionTreeGrammarParameter.ValueChanged += new EventHandler(SymbolicExpressionTreeGrammarParameter_ValueChanged);
    }

    private void SymbolicExpressionTreeGrammarParameter_ValueChanged(object sender, EventArgs e) {
      if (SymbolicExpressionTreeGrammarParameter.Value == null) return;
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1],
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value);

      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, int maxTreeLength, int maxTreeDepth) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) return parent0;

      var cutpoint0 = semantic0.SampleRandom(random);
      var cutpoint1 = semantic1.SampleRandom(random);

      var treeNode0 = parent0.IterateNodesPrefix().ToList()[cutpoint0.TreeNodePrefixPos];
      var treeNode1 = parent1.IterateNodesPrefix().ToList()[cutpoint1.TreeNodePrefixPos];
      var parentNode0 = treeNode0.Parent;

      var indexTreeNode0 = parentNode0.IndexOfSubtree(treeNode0);

      parentNode0.RemoveSubtree(indexTreeNode0);
      parentNode0.InsertSubtree(indexTreeNode0, treeNode1);

      return parent0;
    }
  }
}

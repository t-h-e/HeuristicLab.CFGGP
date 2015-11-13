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
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;

namespace HeuristicLab.Encodings.SymbolicExpressionTreeEncoding {
  /// <summary>
  /// Takes two parent individuals P0 and P1 each. Selects a random node N0 of P0 and a random node N1 of P1.
  /// And replaces the branch with root0 N0 in P0 with N1 from P1 if the tree-size limits are not violated.
  /// When recombination with N0 and N1 would create a tree that is too large or invalid the operator randomly selects new N0 and N1 
  /// until a valid configuration is found.
  /// </summary>  
  [Item("SelectiveCrossover", "An operator which performs subtree swapping crossover.")]
  [StorableClass]
  public class SelectiveCrossover : SymbolicExpressionTreeCrossover, ISymbolicExpressionTreeSizeConstraintOperator, ISymbolicExpressionTreeGrammarBasedOperator {
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string RemovedBranchParameterName = "CrossoverRemovedBranch";
    private const string AddedBranchParameterName = "CrossoverAddedBranch";
    private const string CutPointSymbol = "CrossoverCutPointSymbol";
    private const string CrossoverProbabilityParameterName = "CrossoverProbability";

    private const string SymbolicExpressionTreeGrammarParameterName = "SymbolicExpressionTreeGrammar";
    private const string ProbabilitiesParameterName = "ProbabilitiesParameterName";

    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public LookupParameter<ISymbolicExpressionTree> RemovedBranchParameter {
      get { return (LookupParameter<ISymbolicExpressionTree>)Parameters[RemovedBranchParameterName]; }
    }
    public LookupParameter<ISymbolicExpressionTree> AddedBranchParameter {
      get { return (LookupParameter<ISymbolicExpressionTree>)Parameters[AddedBranchParameterName]; }
    }
    public LookupParameter<ISymbol> CutPointSymbolParameter {
      get { return (LookupParameter<ISymbol>)Parameters[CutPointSymbol]; }
    }
    public IValueLookupParameter<PercentValue> CrossoverProbabilityParameter {
      get { return (IValueLookupParameter<PercentValue>)Parameters[CrossoverProbabilityParameterName]; }
    }

    public IValueLookupParameter<ISymbolicExpressionGrammar> SymbolicExpressionTreeGrammarParameter {
      get { return (IValueLookupParameter<ISymbolicExpressionGrammar>)Parameters[SymbolicExpressionTreeGrammarParameterName]; }
    }
    public IValueParameter<PercentMatrix> ProbabilitiesParameter {
      get { return (IValueParameter<PercentMatrix>)Parameters[ProbabilitiesParameterName]; }
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
    #endregion
    [StorableConstructor]
    protected SelectiveCrossover(bool deserializing) : base(deserializing) { }
    protected SelectiveCrossover(SelectiveCrossover original, Cloner cloner) : base(original, cloner) { }
    public SelectiveCrossover()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));

      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>(RemovedBranchParameterName, "Branch that has been removed."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>(AddedBranchParameterName, "Branch that has been added."));
      Parameters.Add(new LookupParameter<ISymbol>(CutPointSymbol, "Cut point symbol"));

      Parameters.Add(new ValueLookupParameter<PercentValue>(CrossoverProbabilityParameterName, "Probability of applying crossover", new PercentValue(1.0)));

      Parameters.Add(new ValueLookupParameter<ISymbolicExpressionGrammar>(SymbolicExpressionTreeGrammarParameterName, "Tree grammar"));
      Parameters.Add(new ValueParameter<PercentMatrix>(ProbabilitiesParameterName, "Probability per symbol"));

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SelectiveCrossover(this, cloner);
    }

    private void RegisterEventHandlers() {
      SymbolicExpressionTreeGrammarParameter.ValueChanged += new EventHandler(SymbolicExpressionTreeGrammarParameter_ValueChanged);
    }

    private void SymbolicExpressionTreeGrammarParameter_ValueChanged(object sender, EventArgs e) {
      if (SymbolicExpressionTreeGrammarParameter.Value == null) return;

      SymbolicExpressionTreeGrammarParameter.Value.Changed -= new EventHandler(SymbolicExpressionTreeGrammarParameter_Value_Changed);
      SymbolicExpressionTreeGrammarParameter.Value.Changed += new EventHandler(SymbolicExpressionTreeGrammarParameter_Value_Changed);

      SetInitialProbabilities();
    }

    private void SymbolicExpressionTreeGrammarParameter_Value_Changed(object sender, EventArgs e) {
      SetInitialProbabilities();
    }

    private void SetInitialProbabilities() {
      ISymbolicExpressionGrammar grammar = SymbolicExpressionTreeGrammarParameter.Value;

      var symbols = grammar.AllowedSymbols.Where(x => x != grammar.ProgramRootSymbol).OrderBy(x => x.Name).Select(x => x.Name);

      double[,] props = new double[symbols.Count(), 1];
      for (int i = 0; i < props.Length; i++) {
        props[i, 0] = 1.0;
      }

      ProbabilitiesParameter.Value = new PercentMatrix(props, new List<string>() { "Probability" }, symbols);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random,
      ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, ProbabilitiesParameter.Value);

      return parent0;
    }

    public ISymbolicExpressionTree Cross(IRandom random,
      ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1,
      int maxTreeLength, int maxTreeDepth, PercentMatrix probabilities) {
      // select a random crossover point in the first parent 
      CutPoint crossoverPoint0;
      SelectCrossoverPoint(random, parent0, maxTreeLength, maxTreeDepth, out crossoverPoint0, probabilities);

      int childLength = crossoverPoint0.Child != null ? crossoverPoint0.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = maxTreeLength - (parent0.Length - childLength);
      int maxInsertedBranchDepth = maxTreeDepth - parent0.Root.GetBranchLevel(crossoverPoint0.Parent);

      List<ISymbolicExpressionTreeNode> allowedBranches = new List<ISymbolicExpressionTreeNode>();
      parent1.Root.ForEachNodePostfix((n) => {
        if (n.GetLength() <= maxInsertedBranchLength &&
            n.GetDepth() <= maxInsertedBranchDepth && crossoverPoint0.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });
      // empty branch
      if (crossoverPoint0.IsMatchingPointType(null)) allowedBranches.Add(null);

      if (allowedBranches.Count == 0) {
        return parent0;
      } else {
        CutPointSymbolParameter.ActualValue = (ISymbol)crossoverPoint0.Parent.Symbol.Clone();
        var selectedBranch = SelectRandomBranch(random, allowedBranches);

        if (crossoverPoint0.Child != null) {
          // manipulate the tree of parent0 in place
          // replace the branch in tree0 with the selected branch from tree1
          crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);

          RemovedBranchParameter.ActualValue = new SymbolicExpressionTree((ISymbolicExpressionTreeNode)crossoverPoint0.Child.Clone());
          if (selectedBranch != null) {
            crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, selectedBranch);

            AddedBranchParameter.ActualValue = new SymbolicExpressionTree((ISymbolicExpressionTreeNode)selectedBranch.Clone());
          }
        } else {
          // child is null (additional child should be added under the parent)
          if (selectedBranch != null) {
            crossoverPoint0.Parent.AddSubtree(selectedBranch);

            AddedBranchParameter.ActualValue = new SymbolicExpressionTree((ISymbolicExpressionTreeNode)selectedBranch.Clone());
          }
        }
        return parent0;
      }
    }

    private static void SelectCrossoverPoint(IRandom random, ISymbolicExpressionTree parent0, int maxBranchLength, int maxBranchDepth, out CutPoint crossoverPoint, PercentMatrix probabilities) {
      List<CutPoint> internalCrossoverPoints = new List<CutPoint>();
      List<CutPoint> leafCrossoverPoints = new List<CutPoint>();
      parent0.Root.ForEachNodePostfix((n) => {
        if (n.SubtreeCount > 0 && n != parent0.Root) {
          //avoid linq to reduce memory pressure
          for (int i = 0; i < n.SubtreeCount; i++) {
            var child = n.GetSubtree(i);
            if (child.GetLength() <= maxBranchLength &&
                child.GetDepth() <= maxBranchDepth) {
              if (child.SubtreeCount > 0)
                internalCrossoverPoints.Add(new CutPoint(n, child));
              else
                leafCrossoverPoints.Add(new CutPoint(n, child));
            }
          }

          // add one additional extension point if the number of sub trees for the symbol is not full
          if (n.SubtreeCount < n.Grammar.GetMaximumSubtreeCount(n.Symbol)) {
            // empty extension point
            internalCrossoverPoints.Add(new CutPoint(n, n.SubtreeCount));
          }
        }
      }
    );

      List<CutPoint> allCrossoverPoints = new List<CutPoint>();
      allCrossoverPoints.AddRange(internalCrossoverPoints);
      allCrossoverPoints.AddRange(leafCrossoverPoints);
      var weights = allCrossoverPoints.Select(x => probabilities[probabilities.RowNames.ToList().IndexOf(x.Child.Symbol.Name), 0]);
      crossoverPoint = allCrossoverPoints.SampleProportional(random, 1, weights).First();
      //crossoverPoint = allCrossoverPoints[random.Next(allCrossoverPoints.Count)];
    }

    private static ISymbolicExpressionTreeNode SelectRandomBranch(IRandom random, IEnumerable<ISymbolicExpressionTreeNode> branches) {
      return (from branch in branches
              where branch != null
              select branch).SampleRandom(random);
    }
  }
}

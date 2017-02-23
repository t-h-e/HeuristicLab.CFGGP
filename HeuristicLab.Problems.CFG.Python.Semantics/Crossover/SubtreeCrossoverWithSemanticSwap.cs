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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  /// <summary>
  /// This class is purely used for semantic mutation to make sure the semantics is correct after the crossover
  /// Another way would be to use a BeforeExecutionOperators and AfterExecutionOperators
  /// Or to reevaluate the individual
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Item("SubtreeCrossoverWithSemanticSwap", "Subtree crossover that performs semantic swap.")]
  [StorableClass]
  public class SubtreeCrossoverWithSemanticSwap<T> : CFGPythonSemanticEvalCrossover<T>
  where T : class, ICFGPythonProblemData {

    [StorableConstructor]
    protected SubtreeCrossoverWithSemanticSwap(bool deserializing) : base(deserializing) { }
    protected SubtreeCrossoverWithSemanticSwap(SubtreeCrossoverWithSemanticSwap<T> original, Cloner cloner) : base(original, cloner) { }
    public SubtreeCrossoverWithSemanticSwap() : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SubtreeCrossoverWithSemanticSwap<T>(this, cloner);
    }

    protected override ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability, out ItemArray<PythonStatementSemantic> newSemantics) {
      newSemantics = semantic0;
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        return parent0;
      }
      // select a random crossover point in the first parent 
      CutPoint crossoverPoint0;
      SelectCrossoverPoint(random, parent0, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth, out crossoverPoint0);

      int childLength = crossoverPoint0.Child != null ? crossoverPoint0.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = Math.Max(0, maxTreeLength - (parent0.Length - childLength));
      int maxInsertedBranchDepth = Math.Max(0, maxTreeDepth - parent0.Root.GetBranchLevel(crossoverPoint0.Parent));

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
      }
      var selectedBranch = SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability);

      newSemantics = SemanticSwap(crossoverPoint0, selectedBranch, parent0, parent1, semantic0, semantic1);
      return parent0;
    }
  }
}

﻿#region License Information
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

using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Encodings.SymbolicExpressionTreeEncoding {
  [StorableClass]
  [Item("TrackingReplaceBranchManipulation", "Selects a branch of the tree randomly and replaces it with a newly initialized branch (using PTC2).")]
  public sealed class TrackingReplaceBranchManipulation : SymbolicExpressionTreeManipulator, ISymbolicExpressionTreeSizeConstraintOperator {
    private const int MAX_TRIES = 100;
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string RemovedBranchParameterName = "ManipulatorRemovedBranch";
    private const string AddedBranchParameterName = "ManipulatorAddedBranch";
    private const string CutPointSymbol = "ManipulatorCutPointSymbol";
    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    #endregion
    #region Properties
    public IntValue MaximumSymbolicExpressionTreeLength {
      get { return MaximumSymbolicExpressionTreeLengthParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeDepth {
      get { return MaximumSymbolicExpressionTreeDepthParameter.ActualValue; }
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
    #endregion

    [StorableConstructor]
    private TrackingReplaceBranchManipulation(bool deserializing) : base(deserializing) { }
    private TrackingReplaceBranchManipulation(TrackingReplaceBranchManipulation original, Cloner cloner) : base(original, cloner) { }
    public TrackingReplaceBranchManipulation()
      : base() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));

      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>(RemovedBranchParameterName, "Branch that has been removed."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>(AddedBranchParameterName, "Branch that has been added."));
      Parameters.Add(new LookupParameter<ISymbol>(CutPointSymbol, "Cut point symbol"));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new TrackingReplaceBranchManipulation(this, cloner);
    }

    protected override void Manipulate(IRandom random, ISymbolicExpressionTree symbolicExpressionTree) {
      ReplaceRandomBranch(random, symbolicExpressionTree, MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value);
    }

    public void ReplaceRandomBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, int maxTreeLength, int maxTreeDepth) {
      var allowedSymbols = new List<ISymbol>();
      ISymbolicExpressionTreeNode parent;
      int childIndex;
      int maxLength;
      int maxDepth;
      // repeat until a fitting parent and child are found (MAX_TRIES times)
      int tries = 0;
      do {
#pragma warning disable 612, 618
        parent = symbolicExpressionTree.Root.IterateNodesPrefix().Skip(1).Where(n => n.SubtreeCount > 0).SelectRandom(random);
#pragma warning restore 612, 618

        childIndex = random.Next(parent.SubtreeCount);
        var child = parent.GetSubtree(childIndex);
        maxLength = maxTreeLength - symbolicExpressionTree.Length + child.GetLength();
        maxDepth = maxTreeDepth - symbolicExpressionTree.Root.GetBranchLevel(child);

        allowedSymbols.Clear();
        foreach (var symbol in parent.Grammar.GetAllowedChildSymbols(parent.Symbol, childIndex)) {
          // check basic properties that the new symbol must have
          if ((symbol.Name != child.Symbol.Name || symbol.MinimumArity > 0) &&
            symbol.InitialFrequency > 0 &&
            parent.Grammar.GetMinimumExpressionDepth(symbol) <= maxDepth &&
            parent.Grammar.GetMinimumExpressionLength(symbol) <= maxLength) {
            allowedSymbols.Add(symbol);
          }
        }
        tries++;
      } while (tries < MAX_TRIES && allowedSymbols.Count == 0);

      if (tries < MAX_TRIES) {
        var weights = allowedSymbols.Select(s => s.InitialFrequency).ToList();
#pragma warning disable 612, 618
        var seedSymbol = allowedSymbols.SelectRandom(weights, random);
#pragma warning restore 612, 618

        // replace the old node with the new node
        var seedNode = seedSymbol.CreateTreeNode();
        if (seedNode.HasLocalParameters)
          seedNode.ResetLocalParameters(random);

        CutPointSymbolParameter.ActualValue = (ISymbol)parent.Symbol.Clone();
        RemovedBranchParameter.ActualValue = new SymbolicExpressionTree((ISymbolicExpressionTreeNode)parent.GetSubtree(childIndex).Clone());

        parent.RemoveSubtree(childIndex);
        parent.InsertSubtree(childIndex, seedNode);
        ProbabilisticTreeCreator.PTC2(random, seedNode, maxLength, maxDepth);

        AddedBranchParameter.ActualValue = new SymbolicExpressionTree((ISymbolicExpressionTreeNode)parent.GetSubtree(childIndex).Clone());
      }
    }
  }
}

#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Collections;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Misc {
  [Item("MultiSymbolicExpressionTreeCrossover", "Randomly selects and applies one of its crossover every time it is called.")]
  [StorableClass]
  public sealed class MultiSymbolicExpressionTreeCrossover : StochasticMultiBranch<ISymbolicExpressionTreeCrossover>,
    ISymbolicExpressionTreeCrossover,
    IStochasticOperator,
    ISymbolicExpressionTreeSizeConstraintOperator {
    private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string ParentsParameterName = "Parents";

    public override bool CanChangeName {
      get { return false; }
    }
    protected override bool CreateChildOperation {
      get { return true; }
    }

    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public ILookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<ItemArray<ISymbolicExpressionTree>> ParentsParameter {
      get { return (ILookupParameter<ItemArray<ISymbolicExpressionTree>>)Parameters[ParentsParameterName]; }
    }
    #endregion

    [StorableConstructor]
    private MultiSymbolicExpressionTreeCrossover(bool deserializing) : base(deserializing) { }
    private MultiSymbolicExpressionTreeCrossover(MultiSymbolicExpressionTreeCrossover original, Cloner cloner) : base(original, cloner) { }
    public MultiSymbolicExpressionTreeCrossover()
      : base() {
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression tree on which the operator should be applied."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(ParentsParameterName, "The parent symbolic expression trees which should be crossed."));

      List<ISymbolicExpressionTreeCrossover> list = new List<ISymbolicExpressionTreeCrossover>();
      foreach (Type type in ApplicationManager.Manager.GetTypes(typeof(ISymbolicExpressionTreeCrossover))) {
        if (this.GetType().Assembly != type.Assembly && typeof(SubtreeCrossover).Assembly != type.Assembly) continue;
        if (typeof(IMultiOperator<ISymbolicExpressionTreeCrossover>).IsAssignableFrom(type)) continue;
        list.Add((ISymbolicExpressionTreeCrossover)Activator.CreateInstance(type));
      }
      CheckedItemList<ISymbolicExpressionTreeCrossover> checkedItemList = new CheckedItemList<ISymbolicExpressionTreeCrossover>();
      checkedItemList.AddRange(list.OrderBy(op => op.Name));
      Operators = checkedItemList;//.AsReadOnly(); read only is not used because a problem might add more crossover classes
      Operators_ItemsAdded(this, new CollectionItemsChangedEventArgs<IndexedItem<ISymbolicExpressionTreeCrossover>>(Operators.CheckedItems));

      SelectedOperatorParameter.ActualName = "SelectedCrossoverOperator";
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new MultiSymbolicExpressionTreeCrossover(this, cloner);
    }

    protected override void Operators_ItemsReplaced(object sender, CollectionItemsChangedEventArgs<IndexedItem<ISymbolicExpressionTreeCrossover>> e) {
      base.Operators_ItemsReplaced(sender, e);
      ParameterizeCrossovers();
    }

    protected override void Operators_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<ISymbolicExpressionTreeCrossover>> e) {
      base.Operators_ItemsAdded(sender, e);
      ParameterizeCrossovers();
    }

    private void ParameterizeCrossovers() {
      foreach (IStochasticOperator crossover in Operators.OfType<IStochasticOperator>()) {
        crossover.RandomParameter.ActualName = RandomParameter.Name;
      }
      foreach (ISymbolicExpressionTreeCrossover crossover in Operators.OfType<ISymbolicExpressionTreeCrossover>()) {
        crossover.SymbolicExpressionTreeParameter.ActualName = SymbolicExpressionTreeParameter.Name;
        crossover.ParentsParameter.ActualName = ParentsParameter.Name;
      }
      foreach (ISymbolicExpressionTreeSizeConstraintOperator crossover in Operators.OfType<ISymbolicExpressionTreeSizeConstraintOperator>()) {
        crossover.MaximumSymbolicExpressionTreeDepthParameter.ActualName = MaximumSymbolicExpressionTreeDepthParameter.Name;
        crossover.MaximumSymbolicExpressionTreeLengthParameter.ActualName = MaximumSymbolicExpressionTreeLengthParameter.Name;
      }
    }

    // required to be there due to the ISymbolicExpressionTreeCrossover interface, but never called because it inherits from StochasticMultiBranch
    public ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      throw new NotImplementedException();
    }
  }
}

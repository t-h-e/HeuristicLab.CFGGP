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
using HeuristicLab.Collections;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Misc {
  [Item("MultiSelector", "Randomly selects and applies one of its selectors every time it is called.")]
  [StorableClass]
  public sealed class MultiSelector : StochasticMultiBranch<ISelector>, ICaseSingleObjectiveSelector, IStochasticOperator {

    public override bool CanChangeName {
      get { return false; }
    }
    protected override bool CreateChildOperation {
      get { return true; }
    }

    #region Parameter Properties
    private IValueParameter<BoolValue> CopySelectedParameter {
      get { return (IValueParameter<BoolValue>)Parameters["CopySelected"]; }
    }
    public IValueLookupParameter<IntValue> NumberOfSelectedSubScopesParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["NumberOfSelectedSubScopes"]; }
    }
    public IValueLookupParameter<BoolValue> MaximizationParameter {
      get { return (IValueLookupParameter<BoolValue>)Parameters["Maximization"]; }
    }
    public ILookupParameter<ItemArray<DoubleValue>> QualityParameter {
      get { return (ILookupParameter<ItemArray<DoubleValue>>)Parameters["Quality"]; }
    }
    public ILookupParameter<ItemArray<DoubleArray>> CaseQualitiesParameter {
      get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["CaseQualities"]; }
    }
    #endregion

    #region Properties
    public BoolValue CopySelected {
      get { return CopySelectedParameter.Value; }
      set {
        CopySelectedParameter.Value = value;
        ParameterizeSelector();
      }
    }
    #endregion

    [StorableConstructor]
    private MultiSelector(bool deserializing) : base(deserializing) { }
    private MultiSelector(MultiSelector original, Cloner cloner) : base(original, cloner) { }
    public MultiSelector()
      : base() {
      Parameters.Add(new ValueParameter<BoolValue>("CopySelected", "True if the selected sub-scopes should be copied, otherwise false.", new BoolValue(true)));
      Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfSelectedSubScopes", "The number of sub-scopes which should be selected."));
      Parameters.Add(new ValueLookupParameter<BoolValue>("Maximization", "True if the current problem is a maximization problem, otherwise false."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The quality value contained in each sub-scope which is used for selection."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual."));
      CopySelectedParameter.Hidden = true;

      List<ISelector> list = new List<ISelector>();
      foreach (Type type in ApplicationManager.Manager.GetTypes(typeof(ISelector))) {
        if (typeof(MultiProportionalSelector) == type) continue;
        if (typeof(IMultiOperator<ISelector>).IsAssignableFrom(type)) continue;
        list.Add((ISelector)Activator.CreateInstance(type));
      }
      CheckedItemList<ISelector> checkedItemList = new CheckedItemList<ISelector>();
      checkedItemList.AddRange(list.OrderBy(op => op.Name));
      Operators = checkedItemList.AsReadOnly();
      Operators_ItemsAdded(this, new CollectionItemsChangedEventArgs<IndexedItem<ISelector>>(Operators.CheckedItems));

      SelectedOperatorParameter.ActualName = "SelectedSelectionOperator";
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new MultiSelector(this, cloner);
    }

    protected override void Operators_ItemsReplaced(object sender, CollectionItemsChangedEventArgs<IndexedItem<ISelector>> e) {
      base.Operators_ItemsReplaced(sender, e);
      ParameterizeSelector();
    }

    protected override void Operators_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<ISelector>> e) {
      base.Operators_ItemsAdded(sender, e);
      ParameterizeSelector();
    }

    private void ParameterizeSelector() {
      foreach (IStochasticOperator selector in Operators.OfType<IStochasticOperator>()) {
        selector.RandomParameter.ActualName = RandomParameter.Name;
      }
      foreach (var selector in Operators.OfType<ISelector>()) {
        selector.NumberOfSelectedSubScopesParameter.ActualName = NumberOfSelectedSubScopesParameter.Name;
        selector.CopySelected = CopySelected;
      }
      foreach (var selector in Operators.OfType<ISingleObjectiveSelector>()) {
        selector.QualityParameter.ActualName = QualityParameter.Name;
        selector.MaximizationParameter.ActualName = MaximizationParameter.Name;
      }
      foreach (var selector in Operators.OfType<ICaseSingleObjectiveSelector>()) {
        selector.CaseQualitiesParameter.ActualName = CaseQualitiesParameter.Name;
      }
    }
  }
}

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
using HeuristicLab.Selection;

namespace HeuristicLab.Misc {
  [Item("MultiProportionalSelector", "Applies multiple selector, where every selector selects a proportion of the parents.")]
  [StorableClass]
  public class MultiProportionalSelector : AlgorithmOperator, ICaseSingleObjectiveSelector, IStochasticOperator {
    #region Parameters
    public IValueLookupParameter<BoolValue> MaximizationParameter {
      get { return (IValueLookupParameter<BoolValue>)Parameters["Maximization"]; }
    }
    public ILookupParameter<ItemArray<DoubleValue>> QualityParameter {
      get { return (ILookupParameter<ItemArray<DoubleValue>>)Parameters["Quality"]; }
    }
    public IValueLookupParameter<IntValue> NumberOfSelectedSubScopesParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["NumberOfSelectedSubScopes"]; }
    }
    protected IValueLookupParameter<BoolValue> CopySelectedParameter {
      get { return (IValueLookupParameter<BoolValue>)Parameters["CopySelected"]; }
    }
    public ILookupParameter<ItemArray<DoubleArray>> CaseQualitiesParameter {
      get { return (ILookupParameter<ItemArray<DoubleArray>>)Parameters["CaseQualities"]; }
    }
    public ILookupParameter<IRandom> RandomParameter {
      get { return (ILookupParameter<IRandom>)Parameters["Random"]; }
    }
    public ValueParameter<ICheckedItemList<ISelector>> SelectorsParameter {
      get { return (ValueParameter<ICheckedItemList<ISelector>>)Parameters["Selectors"]; }
    }
    protected ScopeParameter CurrentScopeParameter {
      get { return (ScopeParameter)Parameters["CurrentScope"]; }
    }
    #endregion

    #region Properties
    public BoolValue Maximization {
      get { return MaximizationParameter.Value; }
      set { MaximizationParameter.Value = value; }
    }
    public IntValue NumberOfSelectedSubScopes {
      get { return NumberOfSelectedSubScopesParameter.Value; }
      set { NumberOfSelectedSubScopesParameter.Value = value; }
    }
    public BoolValue CopySelected {
      get { return CopySelectedParameter.Value; }
      set { CopySelectedParameter.Value = value; }
    }
    public ICheckedItemList<ISelector> Selectors {
      get { return SelectorsParameter.Value; }
      set { SelectorsParameter.Value = value; }
    }
    public IEnumerable<ISelector> CheckedSelectors {
      get { return SelectorsParameter.Value.CheckedItems.Select(x => x.Value); }
    }
    public IScope CurrentScope {
      get { return CurrentScopeParameter.ActualValue; }
    }
    #endregion

    [StorableConstructor]
    protected MultiProportionalSelector(bool deserializing) : base(deserializing) { }
    protected MultiProportionalSelector(MultiProportionalSelector original, Cloner cloner)
      : base(original, cloner) {
      Initialize();
    }

    public MultiProportionalSelector()
      : base() {
      #region Create parameters
      Parameters.Add(new ValueLookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The quality of the solutions."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleArray>("CaseQualities", "The quality of every single training case for each individual."));
      Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfSelectedSubScopes", "The number of scopes that should be selected."));
      Parameters.Add(new ValueLookupParameter<BoolValue>("CopySelected", "True if the scopes should be copied, false if they should be moved.", new BoolValue(true)));
      Parameters.Add(new ValueParameter<ICheckedItemList<ISelector>>("Selectors", "The selection operators."));
      Parameters.Add(new LookupParameter<IRandom>("Random", "The random number generator to use."));
      Parameters.Add(new ScopeParameter("CurrentScope", "The current scope from which sub-scopes should be selected."));
      CopySelectedParameter.Hidden = true;
      #endregion

      List<ISelector> list = new List<ISelector>();
      foreach (Type type in ApplicationManager.Manager.GetTypes(typeof(ISelector))) {
        if (this.GetType() == type) continue;
        if (typeof(IMultiOperator<ISelector>).IsAssignableFrom(type)) continue;
        list.Add((ISelector)Activator.CreateInstance(type));
      }
      CheckedItemList<ISelector> checkedItemList = new CheckedItemList<ISelector>();
      checkedItemList.AddRange(list.OrderBy(op => op.Name));
      Selectors = checkedItemList.AsReadOnly();

      Initialize();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new MultiProportionalSelector(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    /// <summary>
    /// Sets how many sub-scopes male and female selectors should select.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="NumberOfSelectedSubScopesParameter"/> returns an odd number.</exception>
    /// <returns>Returns Apply of <see cref="AlgorithmOperator"/>.</returns>
    public override IOperation Apply() {
      #region set number of selected subscopes
      int count = NumberOfSelectedSubScopesParameter.ActualValue.Value;
      int operators = CheckedSelectors.Count();
      int numberOfSelected = count / operators;
      int remaining = count % operators;

      foreach (var selector in CheckedSelectors) {
        int numberOfSelectedSubScopes = numberOfSelected;
        if (remaining > 0) {
          numberOfSelectedSubScopes++;
          remaining--;
        }

        selector.NumberOfSelectedSubScopesParameter.Value = new IntValue(numberOfSelectedSubScopes);
      }
      #endregion

      #region prepare subscopes as a selector would
      List<IScope> scopes = new List<IScope>(CurrentScope.SubScopes);
      CurrentScope.SubScopes.Clear();
      IScope remainingScope = new Scope("Remaining");
      remainingScope.SubScopes.AddRange(scopes);
      CurrentScope.SubScopes.Add(remainingScope);
      IScope selectedScope = new Scope("Selected");
      selectedScope.SubScopes.AddRange(new IScope[0]);
      CurrentScope.SubScopes.Add(selectedScope);
      #endregion
      return base.Apply();
    }

    #region Events
    protected void Selectors_CheckedItemsChanged(object sender, CollectionItemsChangedEventArgs<IndexedItem<ISelector>> e) {
      BuildOperatorGraph();
    }
    #endregion

    #region Helpers
    private void Initialize() {
      BuildOperatorGraph();
      ParameterizeSelector();
      RegisterEventHandlers();
    }

    private void BuildOperatorGraph() {
      OperatorGraph.Operators.Clear();

      SubScopesProcessor firstProcessor = new SubScopesProcessor();
      EmptyOperator firstEmpty = new EmptyOperator();
      RightChildReducer firstRightChildReducer = new RightChildReducer();
      OperatorGraph.InitialOperator = firstProcessor;
      firstProcessor.Operators.Add(CheckedSelectors.FirstOrDefault());
      firstProcessor.Operators.Add(firstEmpty);
      firstProcessor.Successor = firstRightChildReducer;

      SingleSuccessorOperator previous = firstRightChildReducer;
      foreach (var selector in CheckedSelectors.Skip(1)) {
        SubScopesProcessor selectionProcessor = new SubScopesProcessor();
        EmptyOperator empty = new EmptyOperator();
        RightChildReducer rightChildReducer = new RightChildReducer();
        previous.Successor = selectionProcessor;
        selectionProcessor.Operators.Add(selector);
        selectionProcessor.Operators.Add(empty);
        selectionProcessor.Successor = rightChildReducer;
        previous = rightChildReducer;
      }
    }

    private void ParameterizeSelector() {
      foreach (IStochasticOperator selector in Selectors.OfType<IStochasticOperator>()) {
        selector.RandomParameter.ActualName = RandomParameter.Name;
      }
      foreach (var selector in Selectors.OfType<ISelector>()) {
        selector.NumberOfSelectedSubScopesParameter.ActualName = NumberOfSelectedSubScopesParameter.Name;
        selector.CopySelected = CopySelected;
      }
      foreach (var selector in Selectors.OfType<ISingleObjectiveSelector>()) {
        selector.QualityParameter.ActualName = QualityParameter.Name;
        selector.MaximizationParameter.ActualName = MaximizationParameter.Name;
      }
      foreach (var selector in Selectors.OfType<ICaseSingleObjectiveSelector>()) {
        selector.CaseQualitiesParameter.ActualName = CaseQualitiesParameter.Name;
      }
    }

    private void RegisterEventHandlers() {
      Selectors.CheckedItemsChanged += new CollectionItemsChangedEventHandler<IndexedItem<ISelector>>(Selectors_CheckedItemsChanged);
    }
    #endregion
  }
}

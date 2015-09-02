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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Selection;

namespace HeuristicLab.Misc.Selector {
  /// <summary>
  /// A case selection operator which has a fallback operator, if no individual solved a single case.
  /// </summary>
  [Item("CaseWithFallbackSelector", "A case selection operator which has a fallback operator, if no individual solved a single case.")]
  [StorableClass]
  public class CaseWithFallbackSelector : AlgorithmOperator, ISingleObjectiveSelector {
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
    public ILookupParameter<ItemArray<BoolArray>> CasesParameter {
      get { return (ILookupParameter<ItemArray<BoolArray>>)Parameters["Cases"]; }
    }
    public ILookupParameter<IRandom> RandomParameter {
      get { return (ILookupParameter<IRandom>)Parameters["Random"]; }
    }
    public IValueParameter<ICaseSingleObjectiveSelector> CaseSelectorParameter {
      get { return (IValueParameter<ICaseSingleObjectiveSelector>)Parameters["CaseSelector"]; }
    }
    public IValueParameter<ISelector> FallbackSelectorParameter {
      get { return (IValueParameter<ISelector>)Parameters["FallbackSelector"]; }
    }
    public IValueParameter<BoolValue> AtLeastOneCaseSolvedParameter {
      get { return (IValueParameter<BoolValue>)Parameters["AtLeastOneCaseSolved"]; }
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
    public ICaseSingleObjectiveSelector CaseSelector {
      get { return CaseSelectorParameter.Value; }
      set { CaseSelectorParameter.Value = value; }
    }
    public ISelector FallbackSelector {
      get { return FallbackSelectorParameter.Value; }
      set { FallbackSelectorParameter.Value = value; }
    }
    #endregion

    [StorableConstructor]
    protected CaseWithFallbackSelector(bool deserializing) : base(deserializing) { }
    protected CaseWithFallbackSelector(CaseWithFallbackSelector original, Cloner cloner)
      : base(original, cloner) {
      Initialize();
    }

    public CaseWithFallbackSelector()
      : base() {
      #region Create parameters
      Parameters.Add(new ValueLookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>("Quality", "The quality of the solutions."));
      Parameters.Add(new ValueLookupParameter<IntValue>("NumberOfSelectedSubScopes", "The number of scopes that should be selected."));
      Parameters.Add(new ValueLookupParameter<BoolValue>("CopySelected", "True if the scopes should be copied, false if they should be moved.", new BoolValue(true)));
      Parameters.Add(new LookupParameter<IRandom>("Random", "The random number generator to use."));
      Parameters.Add(new ValueParameter<ICaseSingleObjectiveSelector>("CaseSelector", "A Case Selector"));
      Parameters.Add(new ValueParameter<ISelector>("FallbackSelector", "Fallback selector"));
      Parameters.Add(new ScopeTreeLookupParameter<BoolArray>("Cases", "The successful evaluated cases."));
      Parameters.Add(new ValueParameter<BoolValue>("AtLeastOneCaseSolved", "If at least one case has been solved by any individual, lexicase selection will be applied."));
      CopySelectedParameter.Hidden = true;
      AtLeastOneCaseSolvedParameter.Hidden = true;
      #endregion

      #region Create operators
      Placeholder lexicaseSelector = new Placeholder();
      Placeholder fallbackSelector = new Placeholder();
      ConditionalBranch condition = new ConditionalBranch();

      lexicaseSelector.OperatorParameter.ActualName = "LexicaseSelector";
      fallbackSelector.OperatorParameter.ActualName = "FallbackSelector";
      #endregion

      #region Create operator graph
      OperatorGraph.InitialOperator = condition;
      condition.ConditionParameter.ActualName = AtLeastOneCaseSolvedParameter.Name;
      condition.TrueBranch = lexicaseSelector;
      condition.FalseBranch = fallbackSelector;
      #endregion

      Initialize();
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new CaseWithFallbackSelector(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      Initialize();
    }

    public override IOperation Apply() {
      var cases = CasesParameter.ActualValue;
      bool found = false;
      foreach (var individualCases in cases) {
        foreach (var solved in individualCases) {
          if (solved) {
            found = true;
            break;
          }
        }
        if (found) break;
      }

      AtLeastOneCaseSolvedParameter.Value.Value = found;
      return base.Apply();
    }

    #region Events
    private void SelectorParameter_ValueChanged(object sender, EventArgs e) {
      IValueParameter<ISelector> selectorParam = (sender as IValueParameter<ISelector>);
      if (selectorParam != null)
        ParameterizeSelector(selectorParam.Value);
    }
    #endregion

    #region Helpers
    private void Initialize() {
      CaseSelectorParameter.ValueChanged += new EventHandler(SelectorParameter_ValueChanged);
      FallbackSelectorParameter.ValueChanged += new EventHandler(SelectorParameter_ValueChanged);
      if (CaseSelector == null) CaseSelector = new LexicaseSelector();
      if (FallbackSelector == null) FallbackSelector = new ProportionalSelector();
    }
    private void ParameterizeSelector(ISelector selector) {
      selector.CopySelected = new BoolValue(true);
      selector.NumberOfSelectedSubScopesParameter.Value = NumberOfSelectedSubScopes;
      IStochasticOperator stoOp = (selector as IStochasticOperator);
      if (stoOp != null) stoOp.RandomParameter.ActualName = RandomParameter.Name;
      ISingleObjectiveSelector soSelector = (selector as ISingleObjectiveSelector);
      if (soSelector != null) {
        soSelector.MaximizationParameter.ActualName = MaximizationParameter.Name;
        soSelector.QualityParameter.ActualName = QualityParameter.Name;
      }
      ICaseSingleObjectiveSelector caseSelector = (selector as ICaseSingleObjectiveSelector);
      if (caseSelector != null) {
        caseSelector.CasesParameter.ActualName = CasesParameter.Name;
      }
    }
    #endregion
  }
}

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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  [StorableClass]
  [Item("BeforeCrossoverOperator", "A generic operator that can record genealogical relationships between crossover parents and children.")]
  public class BeforeCrossoverOperator<T> : SingleSuccessorOperator, ITrackingCrossoverOperator<T> where T : class,IItem {
    private const string ParentsParameterName = "Parents";
    private const string ChildParameterName = "Child";

    public IScopeTreeLookupParameter<T> ParentsParameter {
      get { return (IScopeTreeLookupParameter<T>)Parameters[ParentsParameterName]; }
    }
    public IValueParameter<IItemArray<T>> CrossoverParentsParameter {
      get { return (IValueParameter<IItemArray<T>>)Parameters["CrossoverParents"]; }
    }

    protected BeforeCrossoverOperator(BeforeCrossoverOperator<T> original, Cloner cloner)
      : base(original, cloner) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new BeforeCrossoverOperator<T>(this, cloner);
    }

    [StorableConstructor]
    protected BeforeCrossoverOperator(bool deserializing) : base(deserializing) { }

    public BeforeCrossoverOperator() {
      Parameters.Add(new ScopeTreeLookupParameter<T>(ParentsParameterName));
      Parameters.Add(new ValueParameter<IItemArray<T>>("CrossoverParents"));
    }

    public override IOperation Apply() {
      ExecutionContext.Scope.Variables.Add(new Variable(CrossoverParentsParameter.Name, (IItemArray<T>)ParentsParameter.ActualValue.Clone()));
      return base.Apply();
    }
  }
}

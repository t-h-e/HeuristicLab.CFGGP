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
  [Item("BeforeManipulatorOperator", "Performs an action before the manipulator operator is applied.")]
  public class BeforeManipulatorOperator<T> : SingleSuccessorOperator, ITrackingManipulatorOperator<T> where T : class, IItem {
    private const string ChildParameterName = "Child";
    private const string ManipulatorParentName = "ManipulatorParent";

    public ILookupParameter<T> ChildParameter {
      get { return (ILookupParameter<T>)Parameters[ChildParameterName]; }
    }
    public IValueParameter<T> ManipulatorParentParameter {
      get { return (IValueParameter<T>)Parameters[ManipulatorParentName]; }
    }

    protected BeforeManipulatorOperator(BeforeManipulatorOperator<T> original, Cloner cloner)
      : base(original, cloner) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new BeforeManipulatorOperator<T>(this, cloner);
    }

    [StorableConstructor]
    protected BeforeManipulatorOperator(bool deserializing) : base(deserializing) { }

    public BeforeManipulatorOperator() {
      Parameters.Add(new LookupParameter<T>(ChildParameterName));
      Parameters.Add(new ValueParameter<T>(ManipulatorParentName));
    }

    public override IOperation Apply() {
      // add a copy of the child before mutation is applied
      ExecutionContext.Scope.Variables.Add(new Variable(ManipulatorParentParameter.Name, (IItem)ChildParameter.ActualValue.Clone()));
      return base.Apply();
    }
  }
}

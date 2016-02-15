﻿#region License Information
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

using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  public class VariableTypeParameterCollection : ParameterizedNamedItem {

    [StorableConstructor]
    protected VariableTypeParameterCollection(bool deserializing) : base(deserializing) { }

    public VariableTypeParameterCollection() { }
    public VariableTypeParameterCollection(IDictionary<string, VariableType> variableTypes) {
      foreach (var v in variableTypes) {
        Parameters.Add(new FixedValueParameter<EnumValue<VariableType>>(v.Key, new EnumValue<VariableType>(v.Value)));
      }
    }

    protected VariableTypeParameterCollection(VariableTypeParameterCollection original, Cloner cloner)
      : base(original, cloner) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new VariableTypeParameterCollection(this, cloner);
    }

    public IEnumerable<string> GetVariableNames() {
      return Parameters.Select(x => x.Name);
    }

    public IDictionary<string, VariableType> GetVariableTypes() {
      return Parameters.ToDictionary(x => x.Name, y => ((EnumValue<VariableType>)y.ActualValue).Value);
    }

    public IDictionary<VariableType, List<string>> GetTypesOfVariables() {
      return Parameters.GroupBy(x => ((EnumValue<VariableType>)x.ActualValue).Value)
                  .ToDictionary(x => x.Key, y => y.Select(r => r.Name).ToList());
    }

    public void Add(IDictionary<string, VariableType> variableTypes) {
      foreach (var v in variableTypes) {
        Parameters.Add(new FixedValueParameter<EnumValue<VariableType>>(v.Key, new EnumValue<VariableType>(v.Value)));
      }
    }

    public void Clear() {
      Parameters.Clear();
    }
  }
}

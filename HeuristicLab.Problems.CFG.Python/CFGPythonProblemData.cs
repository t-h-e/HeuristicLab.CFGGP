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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Misc;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
namespace HeuristicLab.Problems.CFG.Python {
  public class CFGPythonProblemData : CFGProblemData, ICFGPythonProblemData {
    protected const string VariablesParameterName = "Variables";
    protected const string VariableSettingsParameterName = "VariableSettings";

    #region parameter properites
    public IFixedValueParameter<CheckedItemList<StringValue>> VariablesParameter {
      get { return (IFixedValueParameter<CheckedItemList<StringValue>>)Parameters[VariablesParameterName]; }
    }
    public IFixedValueParameter<ItemList<TextValue>> VariableSettingsParameter {
      get { return (IFixedValueParameter<ItemList<TextValue>>)Parameters[VariableSettingsParameterName]; }
    }
    #endregion

    #region properties
    public CheckedItemList<StringValue> Variables {
      get { return VariablesParameter.Value; }
    }
    public ItemList<TextValue> VariableSettings {
      get { return VariableSettingsParameter.Value; }
    }
    #endregion

    protected CFGPythonProblemData(CFGPythonProblemData original, Cloner cloner)
      : base(original, cloner) {
    }

    [StorableConstructor]
    protected CFGPythonProblemData(bool deserializing) : base(deserializing) { }

    public CFGPythonProblemData()
      : base(new List<string>(0), new List<string>(0)) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonProblemData(this, cloner);
    }

  }
}

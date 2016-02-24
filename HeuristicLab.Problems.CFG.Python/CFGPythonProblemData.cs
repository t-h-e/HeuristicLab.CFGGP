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


using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Misc;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
namespace HeuristicLab.Problems.CFG.Python {
  public class CFGPythonProblemData : CFGProblemData, ICFGPythonProblemData {
    protected const string HelperCodeParameterName = "HelperCode";
    protected const string VariablesParameterName = "Variables";
    protected const string VariableSettingsParameterName = "VariableSettings";

    private static readonly CFGPythonProblemData emptyProblemData;
    public static new CFGPythonProblemData EmptyProblemData {
      get { return emptyProblemData; }
    }
    static CFGPythonProblemData() {
      var problemData = new CFGPythonProblemData();
      problemData.Parameters.Clear();
      problemData.Name = "Empty CFG ProblemData";
      problemData.Description = "This ProblemData acts as place holder before the correct problem data is loaded.";
      problemData.isEmpty = true;

      problemData.Parameters.Add(new FixedValueParameter<StringArray>(InputParameterName, "", new StringArray().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<StringArray>(OutputParameterName, "", new StringArray().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TrainingPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TestPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(HelperCodeParameterName, "", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<VariableTypeParameterCollection>(VariablesParameterName, "", new VariableTypeParameterCollection()));
      problemData.Parameters.Add(new FixedValueParameter<ItemList<TextValue>>(VariableSettingsParameterName, "", new ItemList<TextValue>()));
      emptyProblemData = problemData;
    }

    #region parameter properites
    public IFixedValueParameter<TextValue> HelperCodeParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters[HelperCodeParameterName]; }
    }
    public IFixedValueParameter<VariableTypeParameterCollection> VariablesParameter {
      get { return (IFixedValueParameter<VariableTypeParameterCollection>)Parameters[VariablesParameterName]; }
    }
    public IFixedValueParameter<ItemList<TextValue>> VariableSettingsParameter {
      get { return (IFixedValueParameter<ItemList<TextValue>>)Parameters[VariableSettingsParameterName]; }
    }
    #endregion

    #region properties
    public TextValue HelperCode {
      get { return HelperCodeParameter.Value; }
    }
    public VariableTypeParameterCollection Variables {
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
      : this(new List<string>(0), new List<string>(0)) {
    }

    public CFGPythonProblemData(IEnumerable<string> input, IEnumerable<string> output)
      : base(input, output) {
      Parameters.Add(new FixedValueParameter<TextValue>(HelperCodeParameterName, "", new TextValue()));
      Parameters.Add(new FixedValueParameter<VariableTypeParameterCollection>(VariablesParameterName, "", new VariableTypeParameterCollection()));
      Parameters.Add(new FixedValueParameter<ItemList<TextValue>>(VariableSettingsParameterName, "", new ItemList<TextValue>()));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonProblemData(this, cloner);
    }
  }
}

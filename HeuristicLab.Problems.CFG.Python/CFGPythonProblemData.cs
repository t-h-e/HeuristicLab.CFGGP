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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Misc;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
namespace HeuristicLab.Problems.CFG.Python {
  [StorableClass]
  public class CFGPythonProblemData : CFGProblemData, ICFGPythonProblemData {
    protected const string LoopBreakConstParameterName = "LoopBreakConst";
    protected const string HelperCodeParameterName = "HelperCode";
    protected const string VariablesParameterName = "Variables";
    protected const string VariableSettingsParameterName = "VariableSettings";  

    protected const string VariablesParameterDescription = "Collection of variables and their types";
    protected const string VariableSettingsParameterDescription = "Define values set for variables when testing semantics. By default the variable settings from a trace are used.";  

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
      problemData.Parameters.Add(new FixedValueParameter<IntValue>(LoopBreakConstParameterName, "", (IntValue)new IntValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(EmbedCodeParameterName, "Text where code should be embedded to. (Optinal: Does not have to be set.)", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(HeaderParameterName, "", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(FooterParameterName, "", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(HelperCodeParameterName, "", new TextValue().AsReadOnly()));
      // TODO: should be moved to HeuristicLab.Problems.CFG.Python.Semantics
      problemData.Parameters.Add(new FixedValueParameter<VariableTypeParameterCollection>(VariablesParameterName, VariablesParameterDescription, new VariableTypeParameterCollection()));
      problemData.Parameters.Add(new FixedValueParameter<ItemList<TextValue>>(VariableSettingsParameterName, VariableSettingsParameterDescription, new ItemList<TextValue>()));    
      emptyProblemData = problemData;
    }

    #region parameter properites
    public IFixedValueParameter<IntValue> LoopBreakConstParameter {
      get { return (IFixedValueParameter<IntValue>)Parameters[LoopBreakConstParameterName]; }
    }
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
    public int LoopBreakConst {
      get { return LoopBreakConstParameter.Value.Value; }
    }
    public string FullHeader {
      get {
        string fullHeader = Header == null
                          ? String.Empty
                          : Header.Value;
        return fullHeader;
      }
    }
    public string FullFooter {
      get {
        string fullFooter = Footer == null
                          ? String.Empty
                          : Footer.Value;
        return fullFooter;
      }
    }
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

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      // TODO: remove at some point (next release)
      if (!Parameters.ContainsKey(LoopBreakConstParameterName)) {
        Parameters.Add(new FixedValueParameter<IntValue>(LoopBreakConstParameterName, "", new IntValue(1500)));
      }
    }

    [StorableConstructor]
    protected CFGPythonProblemData(bool deserializing) : base(deserializing) { }

    public CFGPythonProblemData()
      : this(new List<string>(0), new List<string>(0)) {
    }

    public CFGPythonProblemData(IEnumerable<string> input, IEnumerable<string> output)
      : base(input, output) {
      Parameters.Add(new FixedValueParameter<IntValue>(LoopBreakConstParameterName, "", new IntValue(1500)));
      Parameters.Add(new FixedValueParameter<TextValue>(HelperCodeParameterName, "", new TextValue()));
      // TODO: should be moved to HeuristicLab.Problems.CFG.Python.Semantics
      Parameters.Add(new FixedValueParameter<VariableTypeParameterCollection>(VariablesParameterName, VariablesParameterDescription, new VariableTypeParameterCollection()));
      Parameters.Add(new FixedValueParameter<ItemList<TextValue>>(VariableSettingsParameterName, VariableSettingsParameterDescription, new ItemList<TextValue>()));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonProblemData(this, cloner);
    }

    private const string HELPER_CODE_HEADER =
@"# *****************************************************************************
# Helper Code
# *****************************************************************************";
    private const string HELPER_CODE_FOOTER =
    "# *****************************************************************************";

    protected override void SetCodeHeaderAndFooter() {
      base.SetCodeHeaderAndFooter();
      int helperCodeHeader = Header.Value.IndexOf(HELPER_CODE_HEADER);
      if (helperCodeHeader < 0) return;

      int helperCodeFooter = Header.Value.IndexOf(HELPER_CODE_FOOTER, helperCodeHeader + HELPER_CODE_HEADER.Length);
      if (helperCodeFooter < 0) return;

      HelperCode.Value = Header.Value.Substring(helperCodeHeader, helperCodeFooter + HELPER_CODE_FOOTER.Length);
    }
  }
}

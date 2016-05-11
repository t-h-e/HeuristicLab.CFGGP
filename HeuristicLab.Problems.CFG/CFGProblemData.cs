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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Misc;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [StorableClass]
  public class CFGProblemData : ParameterizedNamedItem, ICFGProblemData {
    protected const string InputParameterName = "Input";
    protected const string OutputParameterName = "Output";
    protected const string TrainingPartitionParameterName = "TrainingPartition";
    protected const string TestPartitionParameterName = "TestPartition";
    protected const string EmbedCodeParameterName = "EmbedCode";
    protected const string HeaderParameterName = "Header";
    protected const string FooterParameterName = "Footer";

    private const string INSERTCODE = "<insertCodeHere>";

    private static readonly CFGProblemData emptyProblemData;
    public static CFGProblemData EmptyProblemData {
      get { return emptyProblemData; }
    }
    static CFGProblemData() {
      var problemData = new CFGProblemData();
      problemData.Parameters.Clear();
      problemData.Name = "Empty CFG ProblemData";
      problemData.Description = "This ProblemData acts as place holder before the correct problem data is loaded.";
      problemData.isEmpty = true;

      problemData.Parameters.Add(new FixedValueParameter<StringArray>(InputParameterName, "", new StringArray().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<StringArray>(OutputParameterName, "", new StringArray().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TrainingPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TestPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(EmbedCodeParameterName, "Text where code should be embedded to. (Optinal: Does not have to be set.)", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(HeaderParameterName, "", new TextValue().AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<TextValue>(FooterParameterName, "", new TextValue().AsReadOnly()));
      emptyProblemData = problemData;
    }

    #region parameter properites
    public IValueParameter<StringArray> InputParameter {
      get { return (IValueParameter<StringArray>)Parameters[InputParameterName]; }
    }
    public IValueParameter<StringArray> OutputParameter {
      get { return (IValueParameter<StringArray>)Parameters[OutputParameterName]; }
    }
    public IFixedValueParameter<IntRange> TrainingPartitionParameter {
      get { return (IFixedValueParameter<IntRange>)Parameters[TrainingPartitionParameterName]; }
    }
    public IFixedValueParameter<IntRange> TestPartitionParameter {
      get { return (IFixedValueParameter<IntRange>)Parameters[TestPartitionParameterName]; }
    }
    public IFixedValueParameter<TextValue> EmbedCodeParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters[EmbedCodeParameterName]; }
    }
    public IFixedValueParameter<TextValue> HeaderParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters[HeaderParameterName]; }
    }
    public IFixedValueParameter<TextValue> FooterParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters[FooterParameterName]; }
    }
    #endregion

    #region properties
    protected bool isEmpty = false;
    public bool IsEmpty {
      get { return isEmpty; }
    }
    public StringArray Input {
      get { return InputParameter.Value; }
    }
    public StringArray Output {
      get { return OutputParameter.Value; }
    }
    public IntRange TrainingPartition {
      get { return TrainingPartitionParameter.Value; }
    }
    public IntRange TestPartition {
      get { return TestPartitionParameter.Value; }
    }
    public TextValue EmbedCode {
      get { return EmbedCodeParameter.Value; }
    }
    public TextValue Header {
      get { return HeaderParameter.Value; }
    }
    public TextValue Footer {
      get { return FooterParameter.Value; }
    }

    public virtual IEnumerable<int> TrainingIndices {
      get {
        return Enumerable.Range(TrainingPartition.Start, Math.Max(0, TrainingPartition.End - TrainingPartition.Start))
                         .Where(IsTrainingSample);
      }
    }
    public virtual IEnumerable<int> TestIndices {
      get {
        return Enumerable.Range(TestPartition.Start, Math.Max(0, TestPartition.End - TestPartition.Start))
           .Where(IsTestSample);
      }
    }

    public virtual bool IsTrainingSample(int index) {
      return index >= 0 && index < Input.Length &&
        TrainingPartition.Start <= index && index < TrainingPartition.End &&
        (index < TestPartition.Start || TestPartition.End <= index);
    }
    public virtual bool IsTestSample(int index) {
      return index >= 0 && index < Input.Length &&
             TestPartition.Start <= index && index < TestPartition.End;
    }
    #endregion

    protected CFGProblemData(CFGProblemData original, Cloner cloner)
      : base(original, cloner) {
      isEmpty = original.isEmpty;
      RegisterEventHandlers();
    }
    [StorableConstructor]
    protected CFGProblemData(bool deserializing) : base(deserializing) { }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    public CFGProblemData()
      : this(new List<string>(0), new List<string>(0)) {
    }

    public CFGProblemData(IEnumerable<string> input, IEnumerable<string> output) {
      if (input == null) throw new ArgumentNullException("The input must not be null.");
      if (output == null) throw new ArgumentNullException("The input must not be null.");

      int inputEntries = input.Count();
      if (input.Count() != output.Count())
        throw new ArgumentException("Input and output must have the exact same number of entries.");

      int trainingPartitionStart = 0;
      int trainingPartitionEnd = inputEntries / 2;
      int testPartitionStart = inputEntries / 2;
      int testPartitionEnd = inputEntries;


      Parameters.Add(new FixedValueParameter<StringArray>(InputParameterName, "", new StringArray(input.ToArray())));
      Parameters.Add(new FixedValueParameter<StringArray>(OutputParameterName, "", new StringArray(output.ToArray())));
      Parameters.Add(new FixedValueParameter<IntRange>(TrainingPartitionParameterName, "", new IntRange(trainingPartitionStart, trainingPartitionEnd)));
      Parameters.Add(new FixedValueParameter<IntRange>(TestPartitionParameterName, "", new IntRange(testPartitionStart, testPartitionEnd)));
      Parameters.Add(new FixedValueParameter<TextValue>(EmbedCodeParameterName, "Text where code should be embedded to. (Optinal: Does not have to be set.)", new TextValue()));
      Parameters.Add(new FixedValueParameter<TextValue>(HeaderParameterName, "", new TextValue()));
      Parameters.Add(new FixedValueParameter<TextValue>(FooterParameterName, "", new TextValue()));

      RegisterEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      if (this == emptyProblemData) return emptyProblemData;
      return new CFGProblemData(this, cloner);
    }

    private void RegisterEventHandlers() {
      InputParameter.ValueChanged += new EventHandler(Parameter_ValueChanged);
      OutputParameter.ValueChanged += new EventHandler(Parameter_ValueChanged);
      InputParameter.Value.ItemChanged += new EventHandler<EventArgs<int>>(Value_ItemChanged);
      OutputParameter.Value.ItemChanged += new EventHandler<EventArgs<int>>(Value_ItemChanged);
      TrainingPartition.ValueChanged += new EventHandler(Parameter_ValueChanged);
      TestPartition.ValueChanged += new EventHandler(Parameter_ValueChanged);
      EmbedCodeParameter.Value.ValueChanged += new EventHandler(EmbedCodeFilePathParameter_Value_ValueChanged);
    }

    #region Events
    private void EmbedCodeFilePathParameter_Value_ValueChanged(object sender, EventArgs e) {
      SetCodeHeaderAndFooter();
      OnChanged();
    }

    private void Value_ItemChanged(object sender, EventArgs<int> e) {
      OnChanged();
    }

    private void Parameter_ValueChanged(object sender, EventArgs e) {
      OnChanged();
    }

    public event EventHandler Changed;
    protected virtual void OnChanged() {
      var listeners = Changed;
      if (listeners != null) listeners(this, EventArgs.Empty);
    }
    #endregion

    protected virtual void SetCodeHeaderAndFooter() {
      if (String.IsNullOrWhiteSpace(EmbedCode.Value)) {
        Header.Value = String.Empty;
        Footer.Value = String.Empty;
        return;
      }

      string embedCode = EmbedCode.Value;
      int insert = embedCode.IndexOf(INSERTCODE);
      if (insert > 0) {
        Header.Value = embedCode.Substring(0, insert);
        Footer.Value = embedCode.Substring(insert + INSERTCODE.Length, embedCode.Length - insert - INSERTCODE.Length);
      } else {
        Header.Value = embedCode;
        Footer.Value = String.Empty;
      }
    }
  }
}

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
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.Instances;
using HeuristicLab.Problems.Instances.CFG;

namespace HeuristicLab.Problems.CFG {
  [Item("Context Free Grammar Problem", "The Context Free Grammar Problem is a general problem. Any probelm that can be defined as a grammar can be specified with this item.")]
  [Creatable(CreatableAttribute.Categories.GeneticProgrammingProblems, Priority = 140)]
  [StorableClass]
  public class CFGProblem : SingleObjectiveHeuristicOptimizationProblem<ICFGEvaluator, ISymbolicExpressionTreeCreator>, IStorableContent,
    IProblemInstanceConsumer<CFGData> {
    public string Filename { get; set; }

    private const string INSERTCODE = "<insertCodeHere>";

    #region Parameter Properties
    public IFixedValueParameter<TextValue> GrammarBNFParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters["GrammarBNF"]; }
    }
    public IFixedValueParameter<TextValue> EmbedCodeParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters["EmbedCode"]; }
    }
    public IValueParameter<StringArray> InputParameter {
      get { return (IValueParameter<StringArray>)Parameters["Input"]; }
    }

    public IValueParameter<StringArray> OutputParameter {
      get { return (IValueParameter<StringArray>)Parameters["Output"]; }
    }
    public IValueParameter<IntRange> TrainingPartitionParameter {
      get { return (IValueParameter<IntRange>)Parameters["TrainingPartition"]; }
    }
    public IValueParameter<IntRange> TestPartitionParameter {
      get { return (IValueParameter<IntRange>)Parameters["TestPartition"]; }
    }
    public IValueParameter<CFGExpressionGrammar> GrammarParameter {
      get { return (IValueParameter<CFGExpressionGrammar>)Parameters["Grammar"]; }
    }
    public IFixedValueParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IFixedValueParameter<IntValue>)Parameters["MaximumSymbolicExpressionTreeDepth"]; }
    }
    public IFixedValueParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IFixedValueParameter<IntValue>)Parameters["MaximumSymbolicExpressionTreeLength"]; }
    }
    public IFixedValueParameter<StringValue> HeaderFilePathParameter {
      get { return (IFixedValueParameter<StringValue>)Parameters["Header"]; }
    }
    public IFixedValueParameter<StringValue> FooterFilePathParameter {
      get { return (IFixedValueParameter<StringValue>)Parameters["Footer"]; }
    }
    #endregion

    #region properties
    public TextValue GrammarBNF {
      get { return GrammarBNFParameter.Value; }
    }
    public TextValue EmbedCode {
      get { return EmbedCodeParameter.Value; }
    }

    public StringArray Input {
      get { return InputParameter.Value; }
      set { InputParameter.Value = value; }
    }

    public StringArray Output {
      get { return OutputParameter.Value; }
      set { OutputParameter.Value = value; }
    }

    public CFGExpressionGrammar Grammar {
      get { return GrammarParameter.Value; }
      set { GrammarParameter.Value = value; }
    }
    #endregion

    [StorableConstructor]
    protected CFGProblem(bool deserializing) : base(deserializing) { }

    protected CFGProblem(CFGProblem original, Cloner cloner)
      : base(original, cloner) {
      RegisterEventHandlers();
    }

    public CFGProblem()
      : base(new CFGPythonEvaluator(), new ProbabilisticTreeCreator()) {
      Parameters.Add(new FixedValueParameter<TextValue>("GrammarBNF", "Grammar in BNF Form.", new TextValue()));
      Parameters.Add(new FixedValueParameter<TextValue>("EmbedCode", "Text where code should be embedded to. (Optinal: Does not have to be set.)", new TextValue()));
      Parameters.Add(new ValueParameter<StringArray>("Input", "The input used for the CFG program.", new StringArray(new string[] { "a" })));
      Parameters.Add(new ValueParameter<StringArray>("Output", "The input used for the CFG program.", new StringArray(new string[] { "b" })));
      Parameters.Add(new ValueParameter<IntRange>("TrainingPartition", "", new IntRange()));
      Parameters.Add(new ValueParameter<IntRange>("TestPartition", "", new IntRange()));
      Parameters.Add(new FixedValueParameter<IntValue>("MaximumSymbolicExpressionTreeDepth", "Maximal depth of the symbolic expression. The minimum depth needed for the algorithm is 3 because two levels are reserved for the ProgramRoot and the Start symbol.", new IntValue(8)));
      Parameters.Add(new FixedValueParameter<IntValue>("MaximumSymbolicExpressionTreeLength", "Maximal length of the symbolic expression.", new IntValue(25)));
      Parameters.Add(new ValueParameter<CFGExpressionGrammar>("Grammar", "The grammar created from the grammar text file.", CFGExpressionGrammar.Empty));
      Parameters.Add(new FixedValueParameter<StringValue>("Header", "The header of the program.", new StringValue()));
      Parameters.Add(new FixedValueParameter<StringValue>("Footer", "The footer of the program.", new StringValue()));

      Maximization.Value = false;
      MaximizationParameter.Hidden = true;

      GrammarParameter.Hidden = true;
      HeaderFilePathParameter.Hidden = true;
      FooterFilePathParameter.Hidden = true;

      SolutionCreator.SymbolicExpressionTreeParameter.ActualName = "Program";

      CreateTreeFromGrammar();

      RegisterEventHandlers();
      InitializeOperators();
      ParameterizeEvaluator();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGProblem(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }

    #region Events
    private void GrammarBNFParameter_Value_ValueChanged(object sender, EventArgs e) {
      CreateTreeFromGrammar();
    }
    private void EmbedCodeFilePathParameter_Value_ValueChanged(object sender, EventArgs e) {
      SetCodeHeaderAndFooter();
    }
    protected override void OnEvaluatorChanged() {
      ParameterizeEvaluator();
    }
    private void Partition_ValueChanged(object sender, EventArgs e) {
      ParameterizeEvaluator();
    }
    #endregion

    private void RegisterEventHandlers() {
      GrammarBNFParameter.Value.ValueChanged += new EventHandler(GrammarBNFParameter_Value_ValueChanged);
      EmbedCodeParameter.Value.ValueChanged += new EventHandler(EmbedCodeFilePathParameter_Value_ValueChanged);
      TrainingPartitionParameter.Value.ValueChanged += new EventHandler(Partition_ValueChanged);
      TrainingPartitionParameter.Value.ValueChanged += new EventHandler(Partition_ValueChanged);
    }
    #region Helpers
    private void InitializeOperators() {
      Operators.AddRange(ApplicationManager.Manager.GetInstances<ISymbolicExpressionTreeOperator>());
      Operators.Add(new BestSymbolicExpressionTreeAnalyzer());
      Operators.Add(new MinAverageMaxSymbolicExpressionTreeLengthAnalyzer());
      Operators.Add(new SymbolicExpressionSymbolFrequencyAnalyzer());
      Operators.Add(new SymbolicExpressionTreeLengthAnalyzer());
      ParameterizeOperators();
    }
    private void ParameterizeEvaluator() {
      if (Evaluator != null) {
        Evaluator.ProgramParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        Evaluator.OutputParameter.ActualName = OutputParameter.Name;
        Evaluator.InputParameter.ActualName = InputParameter.Name;
        Evaluator.TrainingPartitionParameter.ActualName = TrainingPartitionParameter.Name;
        Evaluator.TestPartitionParameter.ActualName = TestPartitionParameter.Name;
      }
    }

    protected virtual void ParameterizeOperators() {
      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();

      foreach (var op in operators.OfType<ISymbolicExpressionTreeGrammarBasedOperator>()) {
        op.SymbolicExpressionTreeGrammarParameter.ActualName = GrammarBNFParameter.Name;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeSizeConstraintOperator>()) {
        op.MaximumSymbolicExpressionTreeDepthParameter.ActualName = MaximumSymbolicExpressionTreeDepthParameter.Name;
        op.MaximumSymbolicExpressionTreeLengthParameter.ActualName = MaximumSymbolicExpressionTreeLengthParameter.Name;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeCrossover>()) {
        op.ParentsParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeManipulator>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeCreator>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeAnalyzer>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
    }
    #endregion

    private void CreateTreeFromGrammar() {
      if (String.IsNullOrWhiteSpace(GrammarBNF.Value)) {
        GrammarParameter.Value = CFGExpressionGrammar.Empty;
        GrammarParameter.Hidden = true;
        return;
      }

      CFGParser parser = new CFGParser();
      CFGExpressionGrammar grammar = parser.readGrammarBNF(GrammarBNF.Value);

      Grammar = grammar;
      GrammarParameter.Hidden = false;
    }

    private void SetCodeHeaderAndFooter() {
      if (String.IsNullOrWhiteSpace(EmbedCode.Value)) {
        HeaderFilePathParameter.Value.Value = String.Empty;
        FooterFilePathParameter.Value.Value = String.Empty;
        return;
      }

      string embedCode = EmbedCode.Value;
      int insert = embedCode.IndexOf(INSERTCODE);
      if (insert > 0) {
        HeaderFilePathParameter.Value.Value = embedCode.Substring(0, insert);
        FooterFilePathParameter.Value.Value = embedCode.Substring(insert + INSERTCODE.Length, embedCode.Length - insert - INSERTCODE.Length);
      }
    }


    public void Load(CFGData data) {
      Name = data.Name;
      Description = data.Description;
      TrainingPartitionParameter.Value.Start = data.TrainingPartitionStart;
      TrainingPartitionParameter.Value.End = data.TrainingPartitionEnd;
      TestPartitionParameter.Value.Start = data.TestPartitionStart;
      TestPartitionParameter.Value.End = data.TestPartitionEnd;
      InputParameter.Value = new StringArray(data.Input);
      OutputParameter.Value = new StringArray(data.Output);
      GrammarBNF.Value = data.Grammar;
      EmbedCode.Value = data.Embed;
    }
  }
}

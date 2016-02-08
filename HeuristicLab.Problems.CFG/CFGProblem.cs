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
  [StorableClass]
  public abstract class CFGProblem<T, U> : SingleObjectiveHeuristicOptimizationProblem<U, ISymbolicExpressionTreeCreator>, IStorableContent,
    IProblemInstanceConsumer<CFGData>
    where T : class, ICFGProblemData
    where U : class, ICFGEvaluator {

    private const string ProblemDataParameterName = "CFGProblemData";
    private const string ProblemDataParameterDescription = "The data set, target variable and input variables of the context free grammar problem.";

    private const string SolutionName = "Program";

    public string Filename { get; set; }

    private const string INSERTCODE = "<insertCodeHere>";

    #region Parameter Properties
    public IValueParameter<T> ProblemDataParameter {
      get { return (IValueParameter<T>)Parameters[ProblemDataParameterName]; }
    }
    public IFixedValueParameter<TextValue> GrammarBNFParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters["GrammarBNF"]; }
    }
    public IFixedValueParameter<TextValue> EmbedCodeParameter {
      get { return (IFixedValueParameter<TextValue>)Parameters["EmbedCode"]; }
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
    public IFixedValueParameter<StringValue> HeaderParameter {
      get { return (IFixedValueParameter<StringValue>)Parameters["Header"]; }
    }
    public IFixedValueParameter<StringValue> FooterParameter {
      get { return (IFixedValueParameter<StringValue>)Parameters["Footer"]; }
    }
    #endregion

    #region properties
    public T ProblemData {
      get { return ProblemDataParameter.Value; }
      protected set {
        ProblemDataParameter.Value = value;
      }
    }
    public TextValue GrammarBNF {
      get { return GrammarBNFParameter.Value; }
    }
    public TextValue EmbedCode {
      get { return EmbedCodeParameter.Value; }
    }

    public CFGExpressionGrammar Grammar {
      get { return GrammarParameter.Value; }
      set { GrammarParameter.Value = value; }
    }
    #endregion

    [StorableConstructor]
    protected CFGProblem(bool deserializing) : base(deserializing) { }

    protected CFGProblem(CFGProblem<T, U> original, Cloner cloner)
      : base(original, cloner) {
      RegisterEventHandlers();
    }

    public CFGProblem()
      : base(new CFGProgrammableEvaluator() as U, new ProbabilisticTreeCreator()) {
      Initialize();
    }

    public CFGProblem(U evaluator, ISymbolicExpressionTreeCreator creator)
      : base(evaluator, creator) {
      Initialize();
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      RegisterEventHandlers();
    }



    #region Helpers
    private void Initialize() {
      Parameters.Add(new ValueParameter<CFGProblemData>(ProblemDataParameterName, ProblemDataParameterDescription, CFGProblemData.EmptyProblemData));
      Parameters.Add(new FixedValueParameter<TextValue>("GrammarBNF", "Grammar in BNF Form.", new TextValue()));
      Parameters.Add(new FixedValueParameter<TextValue>("EmbedCode", "Text where code should be embedded to. (Optinal: Does not have to be set.)", new TextValue()));
      Parameters.Add(new FixedValueParameter<IntValue>("MaximumSymbolicExpressionTreeDepth", "Maximal depth of the symbolic expression. The minimum depth needed for the algorithm is 3 because two levels are reserved for the ProgramRoot and the Start symbol.", new IntValue(15)));
      Parameters.Add(new FixedValueParameter<IntValue>("MaximumSymbolicExpressionTreeLength", "Maximal length of the symbolic expression.", new IntValue(100)));
      Parameters.Add(new ValueParameter<CFGExpressionGrammar>("Grammar", "The grammar created from the grammar text file.", CFGExpressionGrammar.Empty));
      Parameters.Add(new FixedValueParameter<StringValue>("Header", "The header of the program.", new StringValue()));
      Parameters.Add(new FixedValueParameter<StringValue>("Footer", "The footer of the program.", new StringValue()));

      Maximization.Value = false;
      MaximizationParameter.Hidden = true;

      GrammarParameter.Hidden = true;
      HeaderParameter.Hidden = true;
      FooterParameter.Hidden = true;

      CreateTreeFromGrammar();

      RegisterEventHandlers();

      ParameterizeSolutionCreator();
      ParameterizeEvaluator();
      InitializeOperators();
    }

    private void RegisterEventHandlers() {
      GrammarBNFParameter.Value.ValueChanged += new EventHandler(GrammarBNFParameter_Value_ValueChanged);
      EmbedCodeParameter.Value.ValueChanged += new EventHandler(EmbedCodeFilePathParameter_Value_ValueChanged);
      ProblemDataParameter.ValueChanged += new EventHandler(ProblemDataParameter_ValueChanged);
      if (ProblemDataParameter.Value != null) ProblemDataParameter.Value.Changed += new EventHandler(ProblemData_Changed);
    }

    private void InitializeOperators() {
      Operators.AddRange(ApplicationManager.Manager.GetInstances<ISymbolicExpressionTreeOperator>());
      Operators.Add(new MinAverageMaxSymbolicExpressionTreeLengthAnalyzer());
      Operators.Add(new SymbolicExpressionSymbolFrequencyAnalyzer());
      Operators.Add(new SymbolicExpressionTreeLengthAnalyzer());
      Operators.Add(new CaseAnalyzer());
      Operators.Add(new CFGTrainingBestSolutionAnalyzer());
      ParameterizeOperators();
    }

    private void ParameterizeEvaluator() {
      if (Evaluator != null) {
        Evaluator.ProgramParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        Evaluator.ProblemDataParameter.ActualName = ProblemDataParameterName;
        Evaluator.HeaderParameter.ActualName = HeaderParameter.Name;
        Evaluator.FooterParameter.ActualName = FooterParameter.Name;
      }
    }

    private void ParameterizeSolutionCreator() {
      if (SolutionCreator != null) {
        SolutionCreator.SymbolicExpressionTreeGrammarParameter.ActualName = GrammarParameter.Name;
        SolutionCreator.SymbolicExpressionTreeParameter.ActualName = SolutionName;
        SolutionCreator.MaximumSymbolicExpressionTreeDepthParameter.ActualName = MaximumSymbolicExpressionTreeDepthParameter.Name;
        SolutionCreator.MaximumSymbolicExpressionTreeLengthParameter.ActualName = MaximumSymbolicExpressionTreeLengthParameter.Name;
      }
    }

    protected virtual void ParameterizeOperators() {
      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();

      foreach (var op in operators.OfType<ISymbolicExpressionTreeOperator>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeGrammarBasedOperator>()) {
        op.SymbolicExpressionTreeGrammarParameter.ActualName = GrammarParameter.Name;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeSizeConstraintOperator>()) {
        op.MaximumSymbolicExpressionTreeDepthParameter.ActualName = MaximumSymbolicExpressionTreeDepthParameter.Name;
        op.MaximumSymbolicExpressionTreeLengthParameter.ActualName = MaximumSymbolicExpressionTreeLengthParameter.Name;
      }
      foreach (var op in operators.OfType<ISymbolicExpressionTreeCrossover>()) {
        op.ParentsParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        //op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      //foreach (var op in operators.OfType<ISymbolicExpressionTreeManipulator>()) {
      //  op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      //}
      //foreach (var op in operators.OfType<ISymbolicExpressionTreeCreator>()) {
      //  op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      //}

      ParameterizeAnalyzers();
    }

    protected virtual void ParameterizeAnalyzers() {
      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();

      foreach (var op in operators.OfType<ISymbolicExpressionTreeAnalyzer>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
      }
      foreach (var op in operators.OfType<ICFGAnalyzer>()) {
        op.SymbolicExpressionTreeParameter.ActualName = SolutionCreator.SymbolicExpressionTreeParameter.ActualName;
        op.HeaderParameter.ActualName = HeaderParameter.Name;
        op.FooterParameter.ActualName = FooterParameter.Name;
        op.ProblemDataParameter.ActualName = ProblemDataParameter.Name;
      }
    }
    #endregion

    #region Events
    private void GrammarBNFParameter_Value_ValueChanged(object sender, EventArgs e) {
      CreateTreeFromGrammar();
    }
    private void EmbedCodeFilePathParameter_Value_ValueChanged(object sender, EventArgs e) {
      SetCodeHeaderAndFooter();
    }
    protected override void OnEvaluatorChanged() {
      base.OnEvaluatorChanged();
      ParameterizeEvaluator();
    }

    protected override void OnSolutionCreatorChanged() {
      base.OnSolutionCreatorChanged();
      ParameterizeSolutionCreator();
    }

    private void ProblemDataParameter_ValueChanged(object sender, EventArgs e) {
      ProblemDataParameter.Value.Changed += new EventHandler(ProblemData_Changed);
      OnProblemDataChanged();
      OnReset();
    }

    protected virtual void ProblemData_Changed(object sender, EventArgs e) {
      OnReset();
    }

    public event EventHandler ProblemDataChanged;
    protected virtual void OnProblemDataChanged() {
      var handler = ProblemDataChanged;
      if (handler != null) handler(this, EventArgs.Empty);
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

      var operators = Parameters.OfType<IValueParameter>().Select(p => p.Value).OfType<IOperator>().Union(Operators).ToList();
      foreach (var op in operators.OfType<ISymbolicExpressionTreeGrammarBasedOperator>()) {
        op.SymbolicExpressionTreeGrammarParameter.Value = GrammarParameter.Value;
      }
    }

    private void SetCodeHeaderAndFooter() {
      if (String.IsNullOrWhiteSpace(EmbedCode.Value)) {
        HeaderParameter.Value.Value = String.Empty;
        FooterParameter.Value.Value = String.Empty;
        return;
      }

      string embedCode = EmbedCode.Value;
      int insert = embedCode.IndexOf(INSERTCODE);
      if (insert > 0) {
        HeaderParameter.Value.Value = embedCode.Substring(0, insert);
        FooterParameter.Value.Value = embedCode.Substring(insert + INSERTCODE.Length, embedCode.Length - insert - INSERTCODE.Length);
      }
    }


    public void Load(CFGData data) {
      Name = data.Name;
      Description = data.Description;
      CFGProblemData problemData = new CFGProblemData(data.Input, data.Output);
      problemData.TrainingPartitionParameter.Value.Start = data.TrainingPartitionStart;
      problemData.TrainingPartitionParameter.Value.End = data.TrainingPartitionEnd;
      problemData.TestPartitionParameter.Value.Start = data.TestPartitionStart;
      problemData.TestPartitionParameter.Value.End = data.TestPartitionEnd;
      ProblemDataParameter.Value = problemData as T;
      GrammarBNF.Value = data.Grammar;
      EmbedCode.Value = data.Embed;
    }
  }

  [Item("Context Free Grammar Problem", "The Context Free Grammar Problem is a general problem. Any probelm that can be defined as a grammar can be specified with this item.")]
  [Creatable(CreatableAttribute.Categories.GeneticProgrammingProblems, Priority = 151)]
  [StorableClass]
  public class CFGProblem : CFGProblem<CFGProblemData, ICFGEvaluator> {
    [StorableConstructor]
    protected CFGProblem(bool deserializing) : base(deserializing) { }
    protected CFGProblem(CFGProblem original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGProblem() : base() { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGProblem(this, cloner);
    }
  }
}

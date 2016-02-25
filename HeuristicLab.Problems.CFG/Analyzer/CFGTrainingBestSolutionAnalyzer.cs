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

using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  /// <summary>
  /// An operator that analyzes the training best context free grammar solution.
  /// </summary>
  [Item("CFGTrainingBestSolutionAnalyzer", "An operator that analyzes the training best context free grammar solution.")]
  [StorableClass]
  public class CFGTrainingBestSolutionAnalyzer<T> : SingleSuccessorOperator, IIterationBasedOperator, ICFGAnalyzer<T>
  where T : class, ICFGProblemData {
    private const string TrainingBestSolutionParameterName = "Best training solution";
    private const string TrainingBestSolutionQualityParameterName = "Best training solution quality";
    private const string TrainingBestSolutionGenerationParameterName = "Best training solution generation";
    private const string UpdateAlwaysParameterName = "Always update best solution";
    private const string IterationsParameterName = "Iterations";
    private const string MaximumIterationsParameterName = "Maximum Iterations";

    private const string ProblemDataParameterName = "ProblemData";

    private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";
    private const string ResultCollectionParameterName = "Results";
    private const string QualityParameterName = "Quality";

    #region parameter properties
    public ILookupParameter<CFGSolution> TrainingBestSolutionParameter {
      get { return (ILookupParameter<CFGSolution>)Parameters[TrainingBestSolutionParameterName]; }
    }
    public ILookupParameter<DoubleValue> TrainingBestSolutionQualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters[TrainingBestSolutionQualityParameterName]; }
    }
    public ILookupParameter<IntValue> TrainingBestSolutionGenerationParameter {
      get { return (ILookupParameter<IntValue>)Parameters[TrainingBestSolutionGenerationParameterName]; }
    }
    public IFixedValueParameter<BoolValue> UpdateAlwaysParameter {
      get { return (IFixedValueParameter<BoolValue>)Parameters[UpdateAlwaysParameterName]; }
    }
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters[IterationsParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumIterationsParameterName]; }
    }
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<T> ProblemDataParameter {
      get { return (ILookupParameter<T>)Parameters[ProblemDataParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultCollectionParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters[ResultCollectionParameterName]; }
    }
    public IScopeTreeLookupParameter<DoubleValue> QualityParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[QualityParameterName]; }
    }
    #endregion
    #region properties
    public virtual bool EnabledByDefault {
      get { return true; }
    }
    public CFGSolution TrainingBestSolution {
      get { return TrainingBestSolutionParameter.ActualValue; }
      set { TrainingBestSolutionParameter.ActualValue = value; }
    }
    public DoubleValue TrainingBestSolutionQuality {
      get { return TrainingBestSolutionQualityParameter.ActualValue; }
      set { TrainingBestSolutionQualityParameter.ActualValue = value; }
    }
    public BoolValue UpdateAlways {
      get { return UpdateAlwaysParameter.Value; }
    }
    public ItemArray<ISymbolicExpressionTree> SymbolicExpressionTree {
      get { return SymbolicExpressionTreeParameter.ActualValue; }
    }
    public ResultCollection ResultCollection {
      get { return ResultCollectionParameter.ActualValue; }
    }
    public ItemArray<DoubleValue> Quality {
      get { return QualityParameter.ActualValue; }
    }
    #endregion

    [StorableConstructor]
    protected CFGTrainingBestSolutionAnalyzer(bool deserializing) : base(deserializing) { }
    protected CFGTrainingBestSolutionAnalyzer(CFGTrainingBestSolutionAnalyzer<T> original, Cloner cloner) : base(original, cloner) { }
    public CFGTrainingBestSolutionAnalyzer()
      : base() {
      Parameters.Add(new LookupParameter<T>(ProblemDataParameterName, "The problem data on which the context free grammar solution should be evaluated."));
      Parameters.Add(new LookupParameter<CFGSolution>(TrainingBestSolutionParameterName, "The training best cfg solution."));
      Parameters.Add(new LookupParameter<DoubleValue>(TrainingBestSolutionQualityParameterName, "The quality of the training best cfg solution."));
      Parameters.Add(new LookupParameter<IntValue>(TrainingBestSolutionGenerationParameterName, "The generation in which the best training solution was found."));
      Parameters.Add(new FixedValueParameter<BoolValue>(UpdateAlwaysParameterName, "Determines if the best training solution should always be updated regardless of its quality.", new BoolValue(false)));
      Parameters.Add(new LookupParameter<IntValue>(IterationsParameterName, "The number of performed iterations."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumIterationsParameterName, "The maximum number of performed iterations.") { Hidden = true });
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression trees that should be analyzed."));
      Parameters.Add(new LookupParameter<ResultCollection>(ResultCollectionParameterName, "The result collection to store the analysis results."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, "The qualities of the trees that should be analyzed."));
      UpdateAlwaysParameter.Hidden = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGTrainingBestSolutionAnalyzer<T>(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      if (!Parameters.ContainsKey(UpdateAlwaysParameterName)) {
        Parameters.Add(new FixedValueParameter<BoolValue>(UpdateAlwaysParameterName, "Determines if the best training solution should always be updated regardless of its quality.", new BoolValue(false)));
        UpdateAlwaysParameter.Hidden = true;
      }
      if (!Parameters.ContainsKey(TrainingBestSolutionGenerationParameterName))
        Parameters.Add(new LookupParameter<IntValue>(TrainingBestSolutionGenerationParameterName, "The generation in which the best training solution was found."));
      if (!Parameters.ContainsKey(IterationsParameterName))
        Parameters.Add(new LookupParameter<IntValue>(IterationsParameterName, "The number of performed iterations."));
      if (!Parameters.ContainsKey(MaximumIterationsParameterName))
        Parameters.Add(new ValueLookupParameter<IntValue>(MaximumIterationsParameterName, "The maximum number of performed iterations.") { Hidden = true });
    }

    public override IOperation Apply() {
      #region find best tree
      double bestQuality = double.PositiveInfinity;
      ISymbolicExpressionTree bestTree = null;
      ISymbolicExpressionTree[] tree = SymbolicExpressionTree.ToArray();
      double[] quality = Quality.Select(x => x.Value).ToArray();
      for (int i = 0; i < tree.Length; i++) {
        if (IsBetter(quality[i], bestQuality, false)) {
          bestQuality = quality[i];
          bestTree = tree[i];
        }
      }
      #endregion

      var results = ResultCollection;
      if (bestTree != null && (UpdateAlways.Value || TrainingBestSolutionQuality == null ||
        IsBetter(bestQuality, TrainingBestSolutionQuality.Value, false))) {
        TrainingBestSolution = CreateCFGSolution(bestTree);
        TrainingBestSolutionQuality = new DoubleValue(bestQuality);
        if (IterationsParameter.ActualValue != null)
          TrainingBestSolutionGenerationParameter.ActualValue = new IntValue(IterationsParameter.ActualValue.Value);

        if (!results.ContainsKey(TrainingBestSolutionParameter.Name)) {
          results.Add(new Result(TrainingBestSolutionParameter.Name, TrainingBestSolutionParameter.Description, TrainingBestSolution));
          results.Add(new Result(TrainingBestSolutionQualityParameter.Name, TrainingBestSolutionQualityParameter.Description, TrainingBestSolutionQuality));
          if (TrainingBestSolutionGenerationParameter.ActualValue != null)
            results.Add(new Result(TrainingBestSolutionGenerationParameter.Name, TrainingBestSolutionGenerationParameter.Description, TrainingBestSolutionGenerationParameter.ActualValue));
        } else {
          results[TrainingBestSolutionParameter.Name].Value = TrainingBestSolution;
          results[TrainingBestSolutionQualityParameter.Name].Value = TrainingBestSolutionQuality;
          if (TrainingBestSolutionGenerationParameter.ActualValue != null)
            results[TrainingBestSolutionGenerationParameter.Name].Value = TrainingBestSolutionGenerationParameter.ActualValue;

        }
      }
      return base.Apply();
    }

    protected virtual CFGSolution CreateCFGSolution(ISymbolicExpressionTree bestTree) {
      return new CFGSolution(bestTree, ProblemDataParameter.ActualValue);
    }

    private bool IsBetter(double lhs, double rhs, bool maximization) {
      if (maximization) return lhs > rhs;
      else return lhs < rhs;
    }
  }
}

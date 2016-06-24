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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("SemanticAnalyzationCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class SemanticCrossoverAnalyzationCrossover<T> : CFGPythonSemanticEvalCrossover2<T>, IIterationBasedOperator
  where T : class, ICFGPythonProblemData {
    private const string NumberOfPossibleBranchesSelectedParameterName = "NumberOfPossibleBranchesSelected";
    private const string NumberOfNoChangeDetectedParameterName = "NumberOfNoChangeDetected";

    private const string TypeSelectedForSimilarityParameterName = "TypeSelectedForSimilarity";
    private const string TypeDifferencesParameterName = "TypeDifferences";

    private const string ParentSimilarityAveragePerTypeParameterName = "ParentSimilarityAveragePerType";
    private const string ParentSimilarityAverageParameterName = "ParentSimilarityAverage";
    private const string NewSimilarityAveragePerTypeParameterName = "NewSimilarityAveragePerType";
    private const string NewSimilarityAverageParameterName = "NewSimilarityAverage";

    private const string NewSimilarityCrossoverCountParameterName = "NewSimilarityCrossoverCount";
    private const string ParentsSimilarityCrossoverCountParameterName = "ParentsSimilarityCrossoverCount";  // how often has the similarity crossover been applied (only counts when it really has been used. A similarity > 0 has been found.)

    private const string SimilarityParameterName = "Similarity";

    private const string QualityParameterName = "Quality";
    private const string ParentQualityParameterName = "ParentQuality";

    #region Parameter Properties
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters["Iterations"]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters["MaximumIterations"]; }
    }
    public ILookupParameter<IntValue> NumberOfPossibleBranchesSelectedParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfPossibleBranchesSelectedParameterName]; }
    }
    public ILookupParameter<IntValue> NumberOfNoChangeDetectedParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfNoChangeDetectedParameterName]; }
    }
    public ILookupParameter<StringValue> TypeSelectedForSimilarityParameter {
      get { return (ILookupParameter<StringValue>)Parameters[TypeSelectedForSimilarityParameterName]; }
    }
    public ILookupParameter<ItemList<StringValue>> TypeDifferencesParameter {
      get { return (ILookupParameter<ItemList<StringValue>>)Parameters[TypeDifferencesParameterName]; }
    }
    public ILookupParameter<ItemDictionary<StringValue, DoubleValue>> NewSimilarityAveragePerTypeParameter {
      get { return (ILookupParameter<ItemDictionary<StringValue, DoubleValue>>)Parameters[NewSimilarityAveragePerTypeParameterName]; }
    }
    public ILookupParameter<DoubleValue> NewSimilarityAverageParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters[NewSimilarityAverageParameterName]; }
    }
    public ILookupParameter<ItemArray<ItemDictionary<StringValue, DoubleValue>>> ParentSimilarityPerTypeParameter {
      get { return (ScopeTreeLookupParameter<ItemDictionary<StringValue, DoubleValue>>)Parameters[ParentSimilarityAveragePerTypeParameterName]; }
    }
    public ILookupParameter<ItemArray<DoubleValue>> ParentSimilarityAverageParameter {
      get { return (ScopeTreeLookupParameter<DoubleValue>)Parameters[ParentSimilarityAverageParameterName]; }
    }
    public ILookupParameter<ItemArray<IntValue>> ParentsSimilarityCrossoverCountParameter {
      get { return (ScopeTreeLookupParameter<IntValue>)Parameters[ParentsSimilarityCrossoverCountParameterName]; }
    }
    public ILookupParameter<IntValue> NewSimilarityCrossoverCountParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NewSimilarityCrossoverCountParameterName]; }
    }
    public ILookupParameter<DoubleValue> SimilarityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters[SimilarityParameterName]; }
    }
    public IScopeTreeLookupParameter<DoubleValue> QualityParameter {
      get { return (IScopeTreeLookupParameter<DoubleValue>)Parameters[QualityParameterName]; }
    }
    public ILookupParameter<ItemArray<DoubleValue>> ParentQualityParameter {
      get { return (ILookupParameter<ItemArray<DoubleValue>>)Parameters[ParentQualityParameterName]; }
    }
    #endregion

    #region Properties
    public int Iterations {
      get { return IterationsParameter.ActualValue.Value; }
    }
    public int NumberOfPossibleBranchesSelected {
      set { NumberOfPossibleBranchesSelectedParameter.ActualValue = new IntValue(value); }
    }
    public int NumberOfNoChangeDetected {
      set { NumberOfNoChangeDetectedParameter.ActualValue = new IntValue(value); }
    }
    public string TypeSelectedForSimilarity {
      set { TypeSelectedForSimilarityParameter.ActualValue = new StringValue(value); }
    }
    public ItemList<StringValue> TypeDifferences {
      set { TypeDifferencesParameter.ActualValue = value; }
    }
    public ItemDictionary<StringValue, DoubleValue> NewSimilarityAveragePerType {
      set { NewSimilarityAveragePerTypeParameter.ActualValue = value; }
    }
    public double NewSimilarityAverage {
      set { NewSimilarityAverageParameter.ActualValue = new DoubleValue(value); }
    }
    public ItemArray<ItemDictionary<StringValue, DoubleValue>> ParentSimilarityPerType {
      get { return ParentSimilarityPerTypeParameter.ActualValue; }
    }
    public ItemArray<DoubleValue> ParentSimilarityAverage {
      get { return ParentSimilarityAverageParameter.ActualValue; }
    }
    public ItemArray<IntValue> ParentsSimilarityCrossoverCount {
      get { return ParentsSimilarityCrossoverCountParameter.ActualValue; }
    }
    public int NewSimilarityCrossoverCount {
      get { return NewSimilarityCrossoverCountParameter.ActualValue.Value; }
      set { NewSimilarityCrossoverCountParameter.ActualValue = new IntValue(value); }
    }
    #endregion
    [StorableConstructor]
    protected SemanticCrossoverAnalyzationCrossover(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzationCrossover(SemanticCrossoverAnalyzationCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticCrossoverAnalyzationCrossover()
      : base() {
      Parameters.Add(new LookupParameter<IntValue>("Iterations", "Optional: A value indicating the current iteration."));
      Parameters.Add(new ValueLookupParameter<IntValue>("MaximumIterations", "Unused", new IntValue(-1)));

      Parameters.Add(new LookupParameter<IntValue>(NumberOfPossibleBranchesSelectedParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(NumberOfNoChangeDetectedParameterName, ""));
      Parameters.Add(new LookupParameter<StringValue>(TypeSelectedForSimilarityParameterName, ""));
      Parameters.Add(new LookupParameter<ItemList<StringValue>>(TypeDifferencesParameterName, ""));
      Parameters.Add(new LookupParameter<ItemDictionary<StringValue, DoubleValue>>(NewSimilarityAveragePerTypeParameterName, ""));
      Parameters.Add(new LookupParameter<DoubleValue>(NewSimilarityAverageParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<ItemDictionary<StringValue, DoubleValue>>(ParentSimilarityAveragePerTypeParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(ParentSimilarityAverageParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<IntValue>(ParentsSimilarityCrossoverCountParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(NewSimilarityCrossoverCountParameterName, ""));

      Parameters.Add(new LookupParameter<DoubleValue>(SimilarityParameterName, ""));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, "The qualities of the trees that should be analyzed."));
      Parameters.Add(new LookupParameter<ItemArray<DoubleValue>>(ParentQualityParameterName, ""));

      ParentSimilarityPerTypeParameter.ActualName = NewSimilarityAveragePerTypeParameter.Name;
      ParentSimilarityAverageParameter.ActualName = NewSimilarityAverageParameter.Name;

      ParentsSimilarityCrossoverCountParameter.ActualName = NewSimilarityCrossoverCountParameter.Name;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzationCrossover<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      ParentQualityParameter.ActualValue = QualityParameter.ActualValue;
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value);

      AddStatistics();
      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        AddStatistics();
        return parent0;
      }

      // select a random crossover point in the first parent 
      CutPoint crossoverPoint0;
      SelectCrossoverPoint(random, parent0, internalCrossoverPointProbability, maxTreeLength, maxTreeDepth, out crossoverPoint0);

      int childLength = crossoverPoint0.Child != null ? crossoverPoint0.Child.GetLength() : 0;
      // calculate the max length and depth that the inserted branch can have 
      int maxInsertedBranchLength = maxTreeLength - (parent0.Length - childLength);
      int maxInsertedBranchDepth = maxTreeDepth - parent0.Root.GetBranchLevel(crossoverPoint0.Child);

      List<ISymbolicExpressionTreeNode> allowedBranches = new List<ISymbolicExpressionTreeNode>();
      parent1.Root.ForEachNodePostfix((n) => {
        if (n.GetLength() <= maxInsertedBranchLength &&
            n.GetDepth() <= maxInsertedBranchDepth && crossoverPoint0.IsMatchingPointType(n))
          allowedBranches.Add(n);
      });
      // empty branch
      if (crossoverPoint0.IsMatchingPointType(null)) allowedBranches.Add(null);

      if (allowedBranches.Count == 0) {
        AddStatistics();
        return parent0;
      }

      // select MaxCompares random crossover points
      // Use set to avoid having the same node multiple times
      HashSet<ISymbolicExpressionTreeNode> compBranches;
      if (allowedBranches.Count < MaxComparesParameter.Value.Value) {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>(allowedBranches);
      } else {
        compBranches = new HashSet<ISymbolicExpressionTreeNode>();
        for (int i = 0; i < MaxComparesParameter.Value.Value; i++) {
          var possibleBranch = SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability);
          allowedBranches.Remove(possibleBranch);
          compBranches.Add(possibleBranch);
        }
      }

      // set NumberOfPossibleBranchesSelected
      NumberOfPossibleBranchesSelected = compBranches.Count;

      // get possible semantic positions
      var statementProductions = ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <code>")).Symbols;
      var statementProductionNames = statementProductions.Select(x => x.Name);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = statementProductionNames.Contains(crossoverPoint0.Child.Symbol.Name) ? crossoverPoint0.Child : crossoverPoint0.Parent;
      while (statement != null && !statementProductionNames.Contains(statement.Symbol.Name)) {
        statement = statement.Parent;
      }

      if (statement == null) {
        Swap(crossoverPoint0, compBranches.SampleRandom(random));
        AddStatistics();
        return parent0;
      }

      var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
      string variableSettings;
      if (problemData.VariableSettings.Count == 0) {
        variableSettings = SemanticToPythonVariableSettings(semantic0.First(x => x.TreeNodePrefixPos == statementPos0).Before);
      } else {
        variableSettings = String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));
      }
      var variables = problemData.Variables.GetVariableNames().ToList();

      // create symbols in order to improvize an ad-hoc tree so that the child can be evaluated
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      JObject jsonOriginal = PyProcess.SendAndEvaluateProgram(new EvaluationScript() {
        Script = FormatScript(new SymbolicExpressionTree(new SymbolicExpressionTreeTopLevelNode(rootSymbol)), variables, variableSettings),
        Variables = variables,
        Timeout = Timeout
      });

      var statementParent = statement.Parent;
      EvaluationScript crossoverPointScript0 = new EvaluationScript() {
        Script = FormatScript(CreateTreeFromNode(random, statement, rootSymbol, startSymbol), variables, variableSettings),
        Variables = variables,
        Timeout = Timeout
      };
      JObject json0 = PyProcess.SendAndEvaluateProgram(crossoverPointScript0);
      statement.Parent = statementParent; // restore parent

      ISymbolicExpressionTreeNode selectedBranch = SelectBranch(statement, crossoverPoint0, compBranches, random, variables, variableSettings, json0, jsonOriginal, problemData.Variables.GetTypesOfVariables());

      // perform the actual swap
      if (selectedBranch != null)
        Swap(crossoverPoint0, selectedBranch);

      AddStatistics();
      return parent0;
    }

    protected void AddStatistics() {
      double sum = 0;
      int count = 0;
      for (int i = 0; i < ParentSimilarityAverage.Length; i++) {
        sum += ParentSimilarityAverage[i].Value * ParentsSimilarityCrossoverCount[i].Value;
        count += ParentsSimilarityCrossoverCount[i].Value;
      }
      if (NewSimilarityAverageParameter.ActualValue != null) {
        SimilarityParameter.ActualValue = new DoubleValue(NewSimilarityAverageParameter.ActualValue.Value);
        sum += NewSimilarityAverageParameter.ActualValue.Value;
        count++;
      } else {
        SimilarityParameter.ActualValue = new DoubleValue(0.0);
      }
      if (count != 0) {
        NewSimilarityAverage = sum / count;
        NewSimilarityCrossoverCount = count;
      } else {
        NewSimilarityAverage = 0;
        NewSimilarityCrossoverCount = 0;
      }
    }

    protected string SemanticToPythonVariableSettings(IDictionary<string, IList> semantic) {
      StringBuilder strBuilder = new StringBuilder();
      foreach (var setting in semantic) {
        strBuilder.AppendLine(String.Format("{0} = {1}", setting.Key,
                                                         JsonConvert.SerializeObject(setting.Value)));
      }
      return strBuilder.ToString();
    }

    protected ISymbolicExpressionTreeNode SelectBranch(ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, IEnumerable<ISymbolicExpressionTreeNode> compBranches, IRandom random, List<string> variables, string variableSettings, JObject jsonParent0, JObject jsonOriginal, IDictionary<VariableType, List<string>> variablesPerType) {
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      var statementNodetParent = statementNode.Parent; // save statement parent
      List<JObject> evaluationPerNode = new List<JObject>();
      List<double> similarity = new List<double>();

      var evaluationTree = CreateTreeFromNode(random, statementNode, rootSymbol, startSymbol); // this will affect statementNode.Parent
      foreach (var node in compBranches) {
        var parent = node.Parent; // save parent

        crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);
        crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, node); // this will affect node.Parent

        EvaluationScript evaluationScript1 = new EvaluationScript() {
          Script = FormatScript(evaluationTree, variables, variableSettings),
          Variables = variables,
          Timeout = Timeout
        };
        JObject json = PyProcess.SendAndEvaluateProgram(evaluationScript1);
        node.Parent = parent; // restore parent

        evaluationPerNode.Add(json);
        similarity.Add(0);
      }
      statementNode.Parent = statementNodetParent; // restore statement parent
      crossoverPoint0.Parent.RemoveSubtree(crossoverPoint0.ChildIndex);
      crossoverPoint0.Parent.InsertSubtree(crossoverPoint0.ChildIndex, crossoverPoint0.Child); // restore crossoverPoint0

      Dictionary<VariableType, List<string>> differencesPerType = new Dictionary<VariableType, List<string>>();
      List<string> differences;
      foreach (var entry in variablesPerType) {
        differences = new List<string>();
        foreach (var variableName in entry.Value) {
          if (evaluationPerNode.Any(x => !JToken.EqualityComparer.Equals(jsonOriginal[variableName], x[variableName]))
          || !JToken.EqualityComparer.Equals(jsonOriginal[variableName], jsonParent0[variableName])) {
            differences.Add(variableName);
          }
        }

        if (differences.Count > 0) {
          differencesPerType.Add(entry.Key, differences);
        }
      }

      // set TypeDifferences
      TypeDifferences = new ItemList<StringValue>(differencesPerType.Select(x => new StringValue(x.Key.ToString())));

      if (differencesPerType.Count == 0) return compBranches.SampleRandom(random); // no difference found, crossover with any branch

      var typeDifference = differencesPerType.SampleRandom(random);

      //set TypeSelectedForSimilarity
      TypeSelectedForSimilarity = typeDifference.Key.ToString();

      foreach (var variableName in typeDifference.Value) {
        var variableSimilarity = CalculateDifference(jsonParent0[variableName], evaluationPerNode.Select(x => x[variableName]), typeDifference.Key, true);
        similarity = similarity.Zip(variableSimilarity, (x, y) => x + y).ToList();
      }
      similarity = similarity.Select(x => x / typeDifference.Value.Count).ToList(); // normalize between 0 and 1 again (actually not necessary)

      // set NumberOfNoChangeDetected
      NumberOfNoChangeDetected = similarity.Count(x => x.IsAlmost(0.0));

      double best = Double.MaxValue;
      int pos = -1;
      for (int i = 0; i < similarity.Count; i++) {
        if (similarity[i] > 0 && similarity[i] < best) {
          best = similarity[i];
          pos = i;
        }
      }

      // set
      if (pos >= 0) {
        NewSimilarityAverage = best;
        NewSimilarityCrossoverCount = 1;
      }

      return pos >= 0 ? compBranches.ElementAt(pos) : compBranches.SampleRandom(random);
    }
  }
}

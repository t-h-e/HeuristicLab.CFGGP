#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2017 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("AbstractSemanticAnalyzationManipulator", "Base class for semantic analyzation manipulator.")]
  [StorableClass]
  public abstract class AbstractSemanticAnalyzationManipulator<T> : CFGPythonSemanticManipulator<T>
    where T : class, ICFGPythonProblemData {
    protected const int MAX_TRIES = 100; // as used in other manipulators
    private const string NumberOfTriesParameterName = "NumberOfTriesMutation";
    private const string MutationTypeParameterName = "MutationType";

    private const string SemanticallyEquivalentMutationParameterName = "SemanticallyEquivalentMutation";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParentMutation";
    private const string SemanticLocalityParameterName = "SemanticLocalityMutation";
    private const string ConstructiveEffectParameterName = "ConstructiveEffectMutation";

    private const string MutationExceptionsParameterName = "MutationExceptions";

    public const int NoMutation = 0;
    public const int SemanticMutation = 1;
    public const int RandomMutation = 2;

    //public const int NoMutation = 0;
    public const int Equvivalent = 1;
    public const int Different = 2;

    #region parameter properties
    public ILookupParameter<IntValue> NumberOfTriesParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfTriesParameterName]; }
    }
    public ILookupParameter<IntValue> MutationTypeParameter {
      get { return (ILookupParameter<IntValue>)Parameters[MutationTypeParameterName]; }
    }
    public ILookupParameter<IntValue> SemanticallyEquivalentMutationParameter {
      get { return (ILookupParameter<IntValue>)Parameters[SemanticallyEquivalentMutationParameterName]; }
    }
    public ILookupParameter<BoolValue> SemanticallyDifferentFromRootedParentParameter {
      get { return (ILookupParameter<BoolValue>)Parameters[SemanticallyDifferentFromRootedParentParameterName]; }
    }
    public ILookupParameter<DoubleValue> SemanticLocalityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters[SemanticLocalityParameterName]; }
    }
    public ILookupParameter<IntValue> ConstructiveEffectParameter {
      get { return (ILookupParameter<IntValue>)Parameters[ConstructiveEffectParameterName]; }
    }
    public ILookupParameter<ItemCollection<StringValue>> MutationExceptionsParameter {
      get { return (ILookupParameter<ItemCollection<StringValue>>)Parameters[MutationExceptionsParameterName]; }
    }
    #endregion

    [StorableConstructor]
    protected AbstractSemanticAnalyzationManipulator(bool deserializing) : base(deserializing) { }
    protected AbstractSemanticAnalyzationManipulator(AbstractSemanticAnalyzationManipulator<T> original, Cloner cloner)
      : base(original, cloner) { }
    public AbstractSemanticAnalyzationManipulator() {
      Parameters.Add(new LookupParameter<IntValue>(NumberOfTriesParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(MutationTypeParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(SemanticallyEquivalentMutationParameterName, ""));
      Parameters.Add(new LookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new LookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(ConstructiveEffectParameterName, ""));
      Parameters.Add(new LookupParameter<ItemCollection<StringValue>>(MutationExceptionsParameterName, ""));
    }

    protected override void Manipulate(IRandom random, ISymbolicExpressionTree symbolicExpressionTree) {
      var pythonSemanticHelper = new PythonProcessSemanticHelper(ProblemData.Variables.GetVariableNames(), 1000); // hardcoded value!!! // TODO: object created for every mutation

      var input = PythonHelper.ConvertToPythonValues(ProblemData.Input, ProblemData.TrainingIndices);
      var output = PythonHelper.ConvertToPythonValues(ProblemData.Output, ProblemData.TrainingIndices);
      var beforeResults = pythonSemanticHelper.EvaluateAndTraceProgram(PythonProcessParameter.ActualValue,
                                             PythonHelper.FormatToProgram(symbolicExpressionTree, ProblemData.LoopBreakConst, ProblemData.FullHeader, ProblemData.FullFooter),
                                             input,
                                             output,
                                             ProblemData.TrainingIndices,
                                             ProblemData.FullHeader,
                                             ProblemData.FullFooter,
                                             symbolicExpressionTree,
                                             Timeout);

      ReplaceBranch(random, symbolicExpressionTree, ProblemData, Semantics, PythonProcess, Timeout, MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, MaxCompares.Value);

      var afterResults = pythonSemanticHelper.EvaluateAndTraceProgram(PythonProcessParameter.ActualValue,
                                             PythonHelper.FormatToProgram(symbolicExpressionTree, ProblemData.LoopBreakConst, ProblemData.FullHeader, ProblemData.FullFooter),
                                             input,
                                             output,
                                             ProblemData.TrainingIndices,
                                             ProblemData.FullHeader,
                                             ProblemData.FullFooter,
                                             symbolicExpressionTree,
                                             Timeout);

      if (SemanticallyEquivalentMutationParameter.ActualValue.Value == NoMutation) {
        AddStatisticsNoMutation();
      } else {
        AddStatistics(beforeResults, afterResults);
      }
    }

    public abstract void ReplaceBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, ItemArray<PythonStatementSemantic> semantics, PythonProcess pythonProcess, double timeout, int maxTreeLength, int maxTreeDepth, int maximumSemanticTries);

    private void AddStatistics(Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> beforeResults, Tuple<IEnumerable<bool>, IEnumerable<double>, double, string, List<PythonStatementSemantic>> afterResults) {
      if (String.IsNullOrEmpty(beforeResults.Item4) && !String.IsNullOrEmpty(afterResults.Item4)) {
        SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(true);
      } else {
        CheckDifference(beforeResults.Item5.First(), afterResults.Item5.First()); // first semantic statement is <predefined> which contains all code and therefore all changes to res*
      }
      SemanticLocalityParameter.ActualValue = new DoubleValue(Math.Abs(beforeResults.Item3 - afterResults.Item3));
      ConstructiveEffectParameter.ActualValue = new IntValue(afterResults.Item3 < beforeResults.Item3 ? 1 : 0); // 1 == contructive; 0 == not
    }

    private void AddStatisticsNoMutation() {
      SemanticallyEquivalentMutationParameter.ActualValue = new IntValue(0);  // 0 == no mutation
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);
      SemanticLocalityParameter.ActualValue = new DoubleValue(0.0);
      ConstructiveEffectParameter.ActualValue = new IntValue(0);
    }

    // copied from AbstractSemanticAnalyzationCrossover
    private void CheckDifference(PythonStatementSemantic beforeSemantics, PythonStatementSemantic afterSemantics) {
      SemanticallyDifferentFromRootedParentParameter.ActualValue = new BoolValue(false);
      // check all results
      var resKeys = beforeSemantics.Before.Keys.Where(x => x.StartsWith("res"));
      foreach (var resKey in resKeys) {
        var beforeRes = beforeSemantics.After.Keys.Contains(resKey) ? beforeSemantics.After[resKey] : beforeSemantics.Before[resKey];
        var afterRes = afterSemantics.After.Keys.Contains(resKey) ? afterSemantics.After[resKey] : afterSemantics.Before[resKey];

        var enumParent = beforeRes.GetEnumerator();
        var enumChild = afterRes.GetEnumerator();

        var type = ProblemData.Variables.GetTypesOfVariables().First(x => x.Value.Contains(resKey)).Key;
        if (type.IsListType()) {
          // always move forward both enumerators (do not use short-circuit evaluation!)
          while (enumParent.MoveNext() & enumChild.MoveNext()) {
            if (!JToken.EqualityComparer.Equals((JArray)enumParent.Current, (JArray)enumChild.Current)) {
              SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
              break;
            }
          }
          if (enumParent.MoveNext() || enumChild.MoveNext()) {
            SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
          }
        } else {
          // always move forward both enumerators (do not use short-circuit evaluation!)
          while (enumParent.MoveNext() & enumChild.MoveNext()) {
            if (!enumParent.Current.Equals(enumChild.Current)) {
              SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
              break;
            }
          }
          if (enumParent.MoveNext() || enumChild.MoveNext()) {
            SemanticallyDifferentFromRootedParentParameter.ActualValue.Value = true;
          }
        }
        // break if a change has already been found
        if (SemanticallyDifferentFromRootedParentParameter.ActualValue.Value) { break; }
      }
    }
  }
}

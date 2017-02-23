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
  [StorableClass]
  [Item("SemanticAnalyzationManipulator",
    "Removes a random sub-tree of the input tree and fixes the tree by generating random subtrees if necessary..")]
  public class SemanticAnalyzationManipulator<T> : CFGPythonSemanticManipulator<T>
    where T : class, ICFGPythonProblemData {
    private const int MAX_TRIES = 100; // as used in other manipulators
    private const string NumberOfTriesParameterName = "NumberOfTriesMutation";
    private const string SemanticMutationParameterName = "SemanticMutation";

    private const string SemanticallyEquivalentMutationParameterName = "SemanticallyEquivalentMutation";
    private const string SemanticallyDifferentFromRootedParentParameterName = "SemanticallyDifferentFromRootedParentMutation";
    private const string SemanticLocalityParameterName = "SemanticLocalityMutation";
    private const string ConstructiveEffectParameterName = "ConstructiveEffectMutation";

    private const string MutationExceptionsParameterName = "MutationExceptions";

    public const int SemanticMutation = 0;
    public const int RandomMutation = 1;
    public const int NoMutation = 2;

    #region parameter properties
    public ILookupParameter SemanticMutationParameter {
      get { return (ILookupParameter<IntValue>)Parameters[SemanticMutationParameterName]; }
    }
    public ILookupParameter<IntValue> NumberOfTriesParameter {
      get { return (ILookupParameter<IntValue>)Parameters[NumberOfTriesParameterName]; }
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

    #region properties

    #endregion

    [StorableConstructor]
    protected SemanticAnalyzationManipulator(bool deserializing) : base(deserializing) { }
    protected SemanticAnalyzationManipulator(SemanticAnalyzationManipulator<T> original, Cloner cloner)
      : base(original, cloner) { }
    public SemanticAnalyzationManipulator() {
      Parameters.Add(new LookupParameter<IntValue>(NumberOfTriesParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(SemanticMutationParameterName, ""));

      Parameters.Add(new LookupParameter<IntValue>(SemanticallyEquivalentMutationParameterName, ""));
      Parameters.Add(new LookupParameter<BoolValue>(SemanticallyDifferentFromRootedParentParameterName, ""));
      Parameters.Add(new LookupParameter<DoubleValue>(SemanticLocalityParameterName, ""));
      Parameters.Add(new LookupParameter<IntValue>(ConstructiveEffectParameterName, ""));
      Parameters.Add(new LookupParameter<ItemCollection<StringValue>>(MutationExceptionsParameterName, ""));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticAnalyzationManipulator<T>(this, cloner);
    }

    protected override void Manipulate(IRandom random, ISymbolicExpressionTree symbolicExpressionTree) {
      ReplaceSemanticallyDifferentBranch(random, symbolicExpressionTree, ProblemData, Semantics, PythonProcess, Timeout, MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, MaxCompares.Value);
      throw new ArgumentException("Reevaluate parent anyway to get new quality value!!!");
    }

    /// <summary>
    /// 1. find a mutation point
    /// 2. generate new random tree
    /// 3. calculate semantic of old and new subtree (or of the closest parent)
    /// 4. do mutation if semantically different
    /// 5. retry until a certain number of tries is reached
    /// 
    /// if no mutation has happened, do random mutation
    /// </summary>
    public new void ReplaceSemanticallyDifferentBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, ItemArray<PythonStatementSemantic> semantics, PythonProcess pythonProcess, double timeout, int maxTreeLength, int maxTreeDepth, int maximumSemanticTries) {
      var statementProductionNames = SemanticOperatorHelper.GetSemanticProductionNames(symbolicExpressionTree.Root.Grammar);
      var variables = problemData.Variables.GetVariableNames().ToList();
      string variableSettings = problemData.VariableSettings.Count == 0 ? String.Empty : String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));

      var allowedSymbols = new List<ISymbol>();
      // repeat until a fitting parent and child are found (MAX_TRIES times)
      int tries = 0;
      int semanticTries = 0;
      do {
        #region find mutation point
#pragma warning disable 612, 618
        ISymbolicExpressionTreeNode parent = symbolicExpressionTree.Root.IterateNodesPrefix().Skip(1).Where(n => n.SubtreeCount > 0).SelectRandom(random);
#pragma warning restore 612, 618

        int childIndex = random.Next(parent.SubtreeCount);
        var child = parent.GetSubtree(childIndex);
        int maxLength = maxTreeLength - symbolicExpressionTree.Length + child.GetLength();
        int maxDepth = maxTreeDepth - symbolicExpressionTree.Root.GetBranchLevel(child);

        allowedSymbols.Clear();
        foreach (var symbol in parent.Grammar.GetAllowedChildSymbols(parent.Symbol, childIndex)) {
          // check basic properties that the new symbol must have
          if ((symbol.Name != child.Symbol.Name || symbol.MinimumArity > 0) &&
            symbol.InitialFrequency > 0 &&
            parent.Grammar.GetMinimumExpressionDepth(symbol) <= maxDepth &&
            parent.Grammar.GetMinimumExpressionLength(symbol) <= maxLength) {
            allowedSymbols.Add(symbol);
          }
        }
        #endregion

        #region check for semantic difference with a new random tree
        if (allowedSymbols.Count > 0) {
          if (semanticTries <= maximumSemanticTries) {
            // do semantic mutation
            #region calculate original json output
            ISymbolicExpressionTreeNode statement = SemanticOperatorHelper.GetStatementNode(child, statementProductionNames);
            var statementPos0 = symbolicExpressionTree.IterateNodesPrefix().ToList().IndexOf(statement);
            if (String.IsNullOrEmpty(variableSettings)) {
              variableSettings = SemanticOperatorHelper.SemanticToPythonVariableSettings(semantics.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
            }

            var jsonOriginal = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
            #endregion

            var seedNode = GenerateAndInsertNewSubtree(random, parent, allowedSymbols, childIndex, maxLength, maxDepth);

            #region calculate new json output
            JObject jsonReplaced;
            if (child == statement) {
              // child is executable, so is the new child
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(seedNode, pythonProcess, random, problemData, variables, variableSettings, timeout);
            } else {
              jsonReplaced = SemanticOperatorHelper.EvaluateStatementNode(statement, pythonProcess, random, problemData, variables, variableSettings, timeout);
            }

            if (JToken.EqualityComparer.Equals(jsonOriginal, jsonReplaced)) {
              // semantically equivalent. undo mutation
              parent.RemoveSubtree(childIndex);
              parent.InsertSubtree(childIndex, child);
              allowedSymbols.Clear();
            } else {
              SemanticMutationParameter.ActualValue = new IntValue(SemanticMutation);
              AddStatisticsSemanticCrossover()
              Console.WriteLine("never happens?");
            }

            if (problemData.VariableSettings.Count == 0) {
              // reset variableSettings
              variableSettings = String.Empty;
            }
            semanticTries++;
            #endregion
          } else {
            // do random mutation
            GenerateAndInsertNewSubtree(random, parent, allowedSymbols, childIndex, maxLength, maxDepth);
            SemanticMutationParameter.ActualValue = new IntValue(RandomMutation);
          }
        }
        #endregion
        tries++;
      } while (tries < MAX_TRIES && allowedSymbols.Count == 0 && semanticTries <= maximumSemanticTries);

      NumberOfTriesParameter.ActualValue = new IntValue(semanticTries);
      if (SemanticMutationParameter.ActualValue == null) { SemanticMutationParameter.ActualValue = new IntValue(NoMutation); }
    }

    protected void AddStatistics(ItemArray<PythonStatementSemantic> semantic0, ISymbolicExpressionTree child, ISymbolicExpressionTreeNode statementNode, CutPoint crossoverPoint0, JObject jsonOriginal, ISymbolicExpressionTreeNode swapedBranch, IRandom random, T problemData, List<string> variables, string variableSettings) {
      if (SemanticallyEquivalentCrossoverParameter.ActualValue == null) {
        JObject jsonNow = SemanticOperatorHelper.EvaluateStatementNode(statementNode, PyProcess, random, problemData, variables, variableSettings, Timeout);
        SemanticallyEquivalentCrossoverParameter.ActualValue = new IntValue(JToken.EqualityComparer.Equals(jsonOriginal, jsonNow) ? 1 : 2);
      }
      AddStatistics(semantic0, child);
    }
  }
}

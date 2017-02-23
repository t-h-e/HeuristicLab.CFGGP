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
  [Item("SemanticManipulation",
    "Removes a random sub-tree of the input tree and fixes the tree by generating random subtrees if necessary..")]
  public class CFGPythonSemanticManipulator<T> : SymbolicExpressionTreeManipulator, ICFGPythonSemanticsManipulator<T>, ISymbolicExpressionTreeSizeConstraintOperator
    where T : class, ICFGPythonProblemData {
    private const int MAX_TRIES = 100; // as used in other manipulators
    private const string MaximumSymbolicExpressionTreeLengthParameterName = "MaximumSymbolicExpressionTreeLength";
    private const string MaximumSymbolicExpressionTreeDepthParameterName = "MaximumSymbolicExpressionTreeDepth";
    private const string ProblemDataParameterName = "ProblemData";
    private const string MaxComparesParameterName = "MaxCompares";
    private const string TimeoutParameterName = "Timeout";
    private const string PythonProcessParameterName = "PythonProcess";
    private const string NewSemanticParameterName = "NewSemantic";

    #region Parameter Properties
    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeLengthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeLengthParameterName]; }
    }

    public IValueLookupParameter<IntValue> MaximumSymbolicExpressionTreeDepthParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumSymbolicExpressionTreeDepthParameterName]; }
    }
    public ILookupParameter<ItemArray<PythonStatementSemantic>> SemanticsParameter {
      get { return (ILookupParameter<ItemArray<PythonStatementSemantic>>)Parameters[NewSemanticParameterName]; }
    }
    public ILookupParameter<T> ProblemDataParameter {
      get { return (ILookupParameter<T>)Parameters[ProblemDataParameterName]; }
    }
    public IValueParameter<IntValue> MaxComparesParameter {
      get { return (IValueParameter<IntValue>)Parameters[MaxComparesParameterName]; }
    }
    public ILookupParameter<IntValue> TimeoutParameter {
      get { return (ILookupParameter<IntValue>)Parameters[TimeoutParameterName]; }
    }
    public ILookupParameter<PythonProcess> PythonProcessParameter {
      get { return (ILookupParameter<PythonProcess>)Parameters[PythonProcessParameterName]; }
    }
    #endregion

    #region Properties
    public IntValue MaximumSymbolicExpressionTreeLength {
      get { return MaximumSymbolicExpressionTreeLengthParameter.ActualValue; }
    }
    public IntValue MaximumSymbolicExpressionTreeDepth {
      get { return MaximumSymbolicExpressionTreeDepthParameter.ActualValue; }
    }
    public IntValue MaxCompares {
      get { return MaxComparesParameter.Value; }
    }
    public ICFGPythonProblemData ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    public ItemArray<PythonStatementSemantic> Semantics {
      get { return SemanticsParameter.ActualValue; }
    }
    public double Timeout { get { return TimeoutParameter.ActualValue.Value / 1000.0; } }
    public PythonProcess PythonProcess { get { return PythonProcessParameter.ActualValue; } }
    #endregion

    [StorableConstructor]
    protected CFGPythonSemanticManipulator(bool deserializing) : base(deserializing) { }
    protected CFGPythonSemanticManipulator(CFGPythonSemanticManipulator<T> original, Cloner cloner)
      : base(original, cloner) { }
    public CFGPythonSemanticManipulator() {
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeLengthParameterName, "The maximal length (number of nodes) of the symbolic expression tree."));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumSymbolicExpressionTreeDepthParameterName, "The maximal depth of the symbolic expression tree (a tree with one node has depth = 0)."));
      Parameters.Add(new LookupParameter<ItemArray<PythonStatementSemantic>>(NewSemanticParameterName, ""));
      Parameters.Add(new LookupParameter<T>(ProblemDataParameterName, "Problem data"));
      Parameters.Add(new ValueParameter<IntValue>(MaxComparesParameterName, "Maximum number of branches that ae going to be compared for crossover.", new IntValue(10)));
      Parameters.Add(new LookupParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped. (In milliseconds)"));
      Parameters.Add(new LookupParameter<PythonProcess>("PythonProcess", "Python process"));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonSemanticManipulator<T>(this, cloner);
    }

    protected override void Manipulate(IRandom random, ISymbolicExpressionTree symbolicExpressionTree) {
      ReplaceSemanticallyDifferentBranch(random, symbolicExpressionTree, ProblemData, Semantics, PythonProcess, Timeout, MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, MaxCompares.Value);
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
    public static void ReplaceSemanticallyDifferentBranch(IRandom random, ISymbolicExpressionTree symbolicExpressionTree, ICFGPythonProblemData problemData, ItemArray<PythonStatementSemantic> semantics, PythonProcess pythonProcess, double timeout, int maxTreeLength, int maxTreeDepth, int maximumSemanticTries) {
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
          }
        }
        #endregion
        tries++;
      } while (tries < MAX_TRIES && allowedSymbols.Count == 0 && semanticTries <= maximumSemanticTries);
    }

    protected static ISymbolicExpressionTreeNode GenerateAndInsertNewSubtree(IRandom random, ISymbolicExpressionTreeNode parent, List<ISymbol> allowedSymbols, int childIndex, int maxLength, int maxDepth) {
      var weights = allowedSymbols.Select(s => s.InitialFrequency).ToList();
#pragma warning disable 612, 618
      var seedSymbol = allowedSymbols.SelectRandom(weights, random);
#pragma warning restore 612, 618

      // replace the old node with the new node
      var seedNode = seedSymbol.CreateTreeNode();
      if (seedNode.HasLocalParameters) {
        seedNode.ResetLocalParameters(random);
      }
      parent.RemoveSubtree(childIndex);
      parent.InsertSubtree(childIndex, seedNode);
      ProbabilisticTreeCreator.PTC2(random, seedNode, maxLength, maxDepth);
      return seedNode;
    }
  }
}

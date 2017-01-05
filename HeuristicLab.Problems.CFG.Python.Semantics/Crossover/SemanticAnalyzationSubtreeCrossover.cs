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
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Random;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [Item("SemanticAnalyzationSubtreeCrossover", "Semantic crossover for program synthesis, which evaluates statements to decide on a crossover point.")]
  [StorableClass]
  public class SemanticCrossoverAnalyzationSubtreeCrossover<T> : SemanticCrossoverAnalyzationCrossover<T>
  where T : class, ICFGPythonProblemData {
    [StorableConstructor]
    protected SemanticCrossoverAnalyzationSubtreeCrossover(bool deserializing) : base(deserializing) { }
    protected SemanticCrossoverAnalyzationSubtreeCrossover(SemanticCrossoverAnalyzationSubtreeCrossover<T> original, Cloner cloner) : base(original, cloner) { }
    public SemanticCrossoverAnalyzationSubtreeCrossover()
      : base() {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SemanticCrossoverAnalyzationSubtreeCrossover<T>(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      if (Semantics.Length == 2 && random.NextDouble() < CrossoverProbability.Value)
        return Cross(random, parent0, parent1, Semantics[0], Semantics[1], ProblemData,
          MaximumSymbolicExpressionTreeLength.Value, MaximumSymbolicExpressionTreeDepth.Value, InternalCrossoverPointProbability.Value);

      AddStatisticsNoCrossover(NoXoProbability);
      return parent0;
    }

    private ISymbolicExpressionTree Cross(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1, ItemArray<PythonStatementSemantic> semantic0, ItemArray<PythonStatementSemantic> semantic1, ICFGPythonProblemData problemData, int maxTreeLength, int maxTreeDepth, double internalCrossoverPointProbability) {
      if (semantic0 == null || semantic1 == null || semantic0.Length == 0 || semantic1.Length == 0) {
        AddStatisticsNoCrossover(NoXoNoSemantics);
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

      // set NumberOfAllowedBranches
      NumberOfAllowedBranches = allowedBranches.Count;

      if (allowedBranches.Count == 0) {
        AddStatisticsNoCrossover(NoXoNoAllowedBranch);
        return parent0;
      }

      var selectedBranch = (ISymbolicExpressionTreeNode)SelectRandomBranch(random, allowedBranches, internalCrossoverPointProbability).Clone();

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
      var statementProductions = ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <code>")).Symbols.Union(
                                 ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <statement>")).Symbols).Union(
                                 ((GroupSymbol)crossoverPoint0.Parent.Grammar.GetSymbol("Rule: <predefined>")).Symbols);
      var statementProductionNames = statementProductions.Select(x => x.Name);

      // find first node that can be used for evaluation in parent0
      ISymbolicExpressionTreeNode statement = statementProductionNames.Contains(crossoverPoint0.Child.Symbol.Name) ? crossoverPoint0.Child : crossoverPoint0.Parent;
      while (statement != null && !statementProductionNames.Contains(statement.Symbol.Name)) {
        statement = statement.Parent;
      }

      if (statement == null) {
        Swap(crossoverPoint0, compBranches.SampleRandom(random));
        AddStatisticsNoCrossover(NoXoNoStatement);
        return parent0;
      }

      var statementPos0 = parent0.IterateNodesPrefix().ToList().IndexOf(statement);
      string variableSettings;
      if (problemData.VariableSettings.Count == 0) {
        variableSettings = SemanticToPythonVariableSettings(semantic0.First(x => x.TreeNodePrefixPos == statementPos0).Before, problemData.Variables.GetVariableTypes());
      } else {
        variableSettings = String.Join(Environment.NewLine, problemData.VariableSettings.Select(x => x.Value));
      }
      var variables = problemData.Variables.GetVariableNames().ToList();

      // create symbols in order to improvize an ad-hoc tree so that the child can be evaluated
      var rootSymbol = new ProgramRootSymbol();
      var startSymbol = new StartSymbol();
      var statementParent = statement.Parent;
      EvaluationScript crossoverPointScript0 = new EvaluationScript() {
        Script = FormatScript(CreateTreeFromNode(random, statement, rootSymbol, startSymbol), problemData.LoopBreakConst, variables, variableSettings),
        Variables = variables,
        Timeout = Timeout
      };
      JObject json0 = PyProcess.SendAndEvaluateProgram(crossoverPointScript0);
      statement.Parent = statementParent; // restore parent

      // perform the actual swap
      if (selectedBranch != null) {
        Swap(crossoverPoint0, selectedBranch);
        AddStatistics(semantic0, parent0, statement, crossoverPoint0, json0, selectedBranch, random, variables, variableSettings, problemData.LoopBreakConst, problemData.Variables.GetTypesOfVariables()); // parent zero has been changed is now considered the child
      } else {
        AddStatisticsNoCrossover(NoXoNoSelectedBranch);
      }

      return parent0;
    }
  }
}

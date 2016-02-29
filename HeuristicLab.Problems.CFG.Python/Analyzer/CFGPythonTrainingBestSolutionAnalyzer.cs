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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  /// <summary>
  /// An operator that analyzes the training best context free grammar solution.
  /// </summary>
  [Item("CFGPythonTrainingBestSolutionAnalyzer", "An operator that analyzes the training best context free grammar solution.")]
  [StorableClass]
  public class CFGPythonTrainingBestSolutionAnalyzer : CFGTrainingBestSolutionAnalyzer<ICFGPythonProblemData>, ICFGPythonAnalyzer<ICFGPythonProblemData> {
    private const string TimeoutParameterName = "Timeout";

    #region parameter properties
    public ILookupParameter<IntValue> TimeoutParameter {
      get { return (ILookupParameter<IntValue>)Parameters[TimeoutParameterName]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonTrainingBestSolutionAnalyzer(bool deserializing) : base(deserializing) { }
    protected CFGPythonTrainingBestSolutionAnalyzer(CFGPythonTrainingBestSolutionAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public CFGPythonTrainingBestSolutionAnalyzer()
      : base() {
      Parameters.Add(new LookupParameter<IntValue>(TimeoutParameterName, "The amount of time an execution is allowed to take, before it is stopped."));
      UpdateAlwaysParameter.Hidden = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonTrainingBestSolutionAnalyzer(this, cloner);
    }

    protected override CFGSolution CreateCFGSolution(ISymbolicExpressionTree bestTree) {
      return new CFGPythonSolution(bestTree, ProblemDataParameter.ActualValue, TimeoutParameter.ActualValue);
    }
  }
}

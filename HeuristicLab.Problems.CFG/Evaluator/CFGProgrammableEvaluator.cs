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
using HeuristicLab.Operators.Programmable;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {

  [Item("CFGProgrammableEvaluator", "An evaluator that can be programmed for arbitrary CFG problems.")]
  [StorableClass]
  public class CFGProgrammableEvaluator : ProgrammableSingleSuccessorOperator, ICFGEvaluator {
    public virtual bool EnabledByDefault {
      get { return false; }
    }

    #region parameters
    public ILookupParameter<ISymbolicExpressionTree> ProgramParameter {
      get { return (ILookupParameter<ISymbolicExpressionTree>)Parameters["Program"]; }
    }
    public ILookupParameter<StringValue> HeaderParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Header"]; }
    }
    public ILookupParameter<StringValue> FooterParameter {
      get { return (ILookupParameter<StringValue>)Parameters["Footer"]; }
    }
    public ILookupParameter<ICFGProblemData> ProblemDataParameter {
      get { return (ILookupParameter<ICFGProblemData>)Parameters["ProblemData"]; }
    }
    public ILookupParameter<BoolArray> SuccessfulCasesParameter {
      get { return (ILookupParameter<BoolArray>)Parameters["Cases"]; }
    }
    public ILookupParameter<DoubleValue> QualityParameter {
      get { return (ILookupParameter<DoubleValue>)Parameters["Quality"]; }
    }
    #endregion

    [StorableConstructor]
    protected CFGProgrammableEvaluator(bool deserializing) : base(deserializing) { }
    protected CFGProgrammableEvaluator(CFGProgrammableEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGProgrammableEvaluator()
      : base() {
      Parameters.Add(new LookupParameter<BoolValue>("Maximization", "True if the problem is a maximization problem."));
      Parameters.Add(new LookupParameter<ISymbolicExpressionTree>("Program", "The program to evaluate."));
      Parameters.Add(new LookupParameter<StringValue>("Header", "The header of the program."));
      Parameters.Add(new LookupParameter<StringValue>("Footer", "The footer of the program."));
      Parameters.Add(new LookupParameter<ICFGProblemData>("ProblemData", "The problem data on which the context free grammer solution should be evaluated."));
      Parameters.Add(new LookupParameter<BoolArray>("Cases", "The training cases that have been successfully executed."));
      Parameters.Add(new LookupParameter<DoubleValue>("Quality", "The quality value aka fitness value of the solution."));
      Parameters.ForEach(x => x.Hidden = false);

      SelectNamespace("HeuristicLab.Encodings.SymbolicExpressionTreeEncoding");
      SelectNamespace("HeuristicLab.Problems.CFG");
      SelectNamespace("HeuristicLab.Optimization");
      SelectNamespace("HeuristicLab.Optimization.Operators");
      Assemblies[typeof(HeuristicLab.Optimization.ResultCollection).Assembly] = true;
      Assemblies[typeof(HeuristicLab.Problems.CFG.CFGProgrammableEvaluator).Assembly] = true;
      Assemblies[typeof(HeuristicLab.Encodings.SymbolicExpressionTreeEncoding.SymbolicExpressionTree).Assembly] = true;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGProgrammableEvaluator(this, cloner);
    }
  }
}

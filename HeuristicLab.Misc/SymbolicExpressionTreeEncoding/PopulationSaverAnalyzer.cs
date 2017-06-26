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
using System.IO;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  /// <summary>
  /// An operator that tracks the solved cases.
  /// </summary>
  [Item("PopulationSaverAnalyzer", "An operator that saves the Population to a file.")]
  [StorableClass]
  public class PopulationSaverAnalyzer : SingleSuccessorOperator, ISymbolicExpressionTreeAnalyzer, IIterationBasedOperator {
    private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";

    private const string IterationsParameterName = "Iterations";
    private const string MaximumIterationsParameterName = "Maximum Iterations";

    #region parameter properties
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public IValueParameter<StringValue> FileNameParameter {
      get { return (IValueParameter<StringValue>)Parameters["FileName"]; }
    }
    public IValueParameter<StringValue> FilePathParameter {
      get { return (IValueParameter<StringValue>)Parameters["FilePath"]; }
    }
    public IValueParameter<IntArray> SaveGenerationsParameter {
      get { return (IValueParameter<IntArray>)Parameters["SaveGenerations"]; }
    }
    public ILookupParameter<IntValue> IterationsParameter {
      get { return (ILookupParameter<IntValue>)Parameters[IterationsParameterName]; }
    }
    public IValueLookupParameter<IntValue> MaximumIterationsParameter {
      get { return (IValueLookupParameter<IntValue>)Parameters[MaximumIterationsParameterName]; }
    }
    #endregion

    #region properties
    public virtual bool EnabledByDefault {
      get { return false; }
    }
    public string FileName { get { return FileNameParameter.Value.Value; } }
    public string FilePath { get { return FilePathParameter.Value.Value; } }
    public int Generation { get { return IterationsParameter.ActualValue.Value; } }
    #endregion

    [StorableConstructor]
    protected PopulationSaverAnalyzer(bool deserializing) : base(deserializing) { }
    protected PopulationSaverAnalyzer(PopulationSaverAnalyzer original, Cloner cloner)
      : base(original, cloner) {
    }
    public PopulationSaverAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, ""));
      Parameters.Add(new ValueParameter<StringValue>("FileName", "", new StringValue()));
      Parameters.Add(new ValueParameter<StringValue>("FilePath", "", new StringValue()));
      Parameters.Add(new ValueParameter<IntArray>("SaveGenerations", "", new IntArray()));
      Parameters.Add(new LookupParameter<IntValue>(IterationsParameterName, ""));
      Parameters.Add(new ValueLookupParameter<IntValue>(MaximumIterationsParameterName, ""));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new PopulationSaverAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      if (!SaveGenerationsParameter.Value.Contains(Generation)) return base.Apply();
      var res = new Result("Population at Generation " + Generation, SymbolicExpressionTreeParameter.ActualValue);
      ContentManager.Save(res, String.Format("{0}-{1}-{2}.hl", Path.Combine(FilePath, FileName), Generation, DateTime.Now.ToString("yyyyMMddHHmmssfff")), true);
      return base.Apply();
    }
  }
}

﻿#region License Information
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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [StorableClass]
  [Item("Semantic1TestCrossover", "PartialChangeInAtLeastOneVariable")]
  public class Semantic1TestCrossover<T> : SemanticTestAnalyzationCrossover<T>
     where T : class, ICFGPythonProblemData {

    [StorableConstructor]
    protected Semantic1TestCrossover(bool deserializing) : base(deserializing) { }
    protected Semantic1TestCrossover(Semantic1TestCrossover<T> original, Cloner cloner) : base(original, cloner) {
    }
    public Semantic1TestCrossover() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new Semantic1TestCrossover<T>(this, cloner);
    }

    protected override bool SemanticMeasure(JObject original, JObject replaced) {
      return PythonSemanticComparer.PartialChangeInAtLeastOneVariable(original, replaced);
    }
  }
}

﻿#region License Information
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
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Misc {
  /// <summary>
  /// Dummy class which does nothing. Is only used when running a GA without using a real crossover.
  /// </summary>  
  [Item("DummyCrossover", "An operator which performs NO crossover.")]
  [StorableClass]
  public class SymbolicExpressionTreeDummyCrossover : SymbolicExpressionTreeCrossover {
    [StorableConstructor]
    protected SymbolicExpressionTreeDummyCrossover(bool deserializing) : base(deserializing) { }
    protected SymbolicExpressionTreeDummyCrossover(SymbolicExpressionTreeDummyCrossover original, Cloner cloner) : base(original, cloner) { }
    public SymbolicExpressionTreeDummyCrossover()
      : base() {
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicExpressionTreeDummyCrossover(this, cloner);
    }

    public override ISymbolicExpressionTree Crossover(IRandom random, ISymbolicExpressionTree parent0, ISymbolicExpressionTree parent1) {
      return parent0;
    }
  }
}

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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [StorableClass]
  [Item("CFGSymbol", "Generic CFG symbol.")]
  public class CFGSymbol : Symbol {
    [Storable]
    private int arity;
    public override int MinimumArity {
      get { return arity; }
    }
    public override int MaximumArity {
      get { return arity; }
    }

    [StorableConstructor]
    protected CFGSymbol(bool deserializing) : base(deserializing) { }
    protected CFGSymbol(CFGSymbol original, Cloner cloner) : base(original, cloner) {
      arity = original.arity;
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGSymbol(this, cloner);
    }
    public CFGSymbol(string name, int arity)
      : base(name, "Generic CFG symbol.") {
      this.arity = arity;
    }

    public virtual IEnumerable<string> GetTerminalParts() {
      return new string[] { String.Empty, String.Empty };
    }
  }
}

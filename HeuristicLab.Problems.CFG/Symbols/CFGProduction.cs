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

using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [StorableClass]
  [Item("CFGProduction", "CFG production symbol.")]
  public class CFGProduction : CFGSymbol {

    [Storable]
    public IEnumerable<string> parts { get; protected set; }
    [StorableConstructor]
    private CFGProduction(bool deserializing) : base(deserializing) { }
    private CFGProduction(CFGProduction original, Cloner cloner) : base(original, cloner) {
      parts = original.parts.ToList();
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGProduction(this, cloner);
    }

    /**
     *  The lenght of parts -1 is the arity of this symbol
     **/
    public CFGProduction(string name, IEnumerable<string> parts)
      : base(name, parts.Count() - 1) {
        this.parts = parts.ToList();
    }

    public override IEnumerable<string> GetTerminalParts() {
      return parts;
    }
  }
}

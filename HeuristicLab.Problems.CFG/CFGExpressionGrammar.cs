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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [StorableClass]
  [Item("CFGExpressionGrammar", "Represents the tree grammar for any cfg problem.")]
  public class CFGExpressionGrammar : SymbolicExpressionGrammar {

    #region Properties
    private static CFGExpressionGrammar empty = new CFGExpressionGrammar();
    public static CFGExpressionGrammar Empty { get { return empty; } }
    #endregion

    [StorableConstructor]
    protected CFGExpressionGrammar(bool deserializing) : base(deserializing) { }
    protected CFGExpressionGrammar(CFGExpressionGrammar original, Cloner cloner) : base(original, cloner) { }
    public CFGExpressionGrammar()
      : base(ItemAttribute.GetName(typeof(CFGExpressionGrammar)), ItemAttribute.GetDescription(typeof(CFGExpressionGrammar))) {
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGExpressionGrammar(this, cloner);
    }

    public new virtual void AddSymbol(ISymbol symbol) {
      base.AddSymbol(symbol);
    }
    public new virtual void SetSubtreeCount(ISymbol symbol, int minimumSubtreeCount, int maximumSubtreeCount) {
      base.SetSubtreeCount(symbol, minimumSubtreeCount, maximumSubtreeCount);
    }
    public new virtual void AddAllowedChildSymbol(ISymbol parent, ISymbol child) {
      base.AddAllowedChildSymbol(parent, child);
    }

    public new virtual void AddAllowedChildSymbol(ISymbol parent, ISymbol child, int argumentIndex) {
      base.AddAllowedChildSymbol(parent, child, argumentIndex);
    }
  }
}

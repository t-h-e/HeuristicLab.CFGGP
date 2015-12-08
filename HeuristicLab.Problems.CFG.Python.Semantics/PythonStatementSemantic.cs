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

using System.Collections;
using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  [StorableClass]
  public class PythonStatementSemantic : Item {
    [Storable]
    public ISymbolicExpressionTreeNode TreeNode { get; set; }

    [Storable]
    public IDictionary<string, IList> Before { get; set; }
    [Storable]
    public IDictionary<string, IList> After { get; set; }

    [StorableConstructor]
    protected PythonStatementSemantic(bool deserializing) : base(deserializing) { }

    protected PythonStatementSemantic(PythonStatementSemantic original, Cloner cloner)
      : base(original, cloner) {
    }
    public PythonStatementSemantic() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new PythonStatementSemantic(this, cloner);
    }
  }
}

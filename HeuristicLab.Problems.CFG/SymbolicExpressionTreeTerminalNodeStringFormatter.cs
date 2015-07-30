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
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using System.Linq;
using System.Text;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.Problems.CFG {
  [Item("Terminal Node String Formatter", "A string formatter for symbolic expression trees.")]
  [StorableClass]
  public class SymbolicExpressionTreeTerminalNodeStringFormatter : NamedItem, ISymbolicExpressionTreeStringFormatter {
    public bool Spaces { get; set; }
    [StorableConstructor]
    protected SymbolicExpressionTreeTerminalNodeStringFormatter(bool deserializing) : base(deserializing) { }
    protected SymbolicExpressionTreeTerminalNodeStringFormatter(SymbolicExpressionTreeTerminalNodeStringFormatter original, Cloner cloner)
      : base(original, cloner) {
      Spaces = original.Spaces;
    }
    public SymbolicExpressionTreeTerminalNodeStringFormatter()
      : base("Terminal Node String Formatter", "A string formatter for symbolic expression trees.") {
      Spaces = false;
    }
    public string Format(ISymbolicExpressionTree symbolicExpressionTree) {
      return FormatRecursively(symbolicExpressionTree.Root);
    }

    public static string StaticFormat(ISymbolicExpressionTree symbolicExpressionTree) {
      if (symbolicExpressionTree == null || symbolicExpressionTree.Root == null) {
        System.Console.WriteLine("asdf");
      }
      return FormatRecursively(symbolicExpressionTree.Root);
    }

    private static string FormatRecursively(ISymbolicExpressionTreeNode node) {
      StringBuilder strBuilder = new StringBuilder();
      if (node.Subtrees.Count() > 0) {
        // node
        foreach (var subtree in node.Subtrees) {
          strBuilder.Append(FormatRecursively(subtree));
        }
      } else {
        // leaf
        strBuilder.Append(node.ToString());
      }
      return strBuilder.ToString();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicExpressionTreeTerminalNodeStringFormatter(this, cloner);
    }
  }
}

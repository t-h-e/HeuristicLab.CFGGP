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

using System.Linq;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  [Item("CFG String Formatter", "A string formatter for CFG symbolic expression trees.")]
  [StorableClass]
  public class CFGSymbolicExpressionTreeStringFormatter : NamedItem, ISymbolicExpressionTreeStringFormatter {
    public bool Spaces { get; set; }
    [StorableConstructor]
    protected CFGSymbolicExpressionTreeStringFormatter(bool deserializing) : base(deserializing) { }
    protected CFGSymbolicExpressionTreeStringFormatter(CFGSymbolicExpressionTreeStringFormatter original, Cloner cloner)
      : base(original, cloner) {
      Spaces = original.Spaces;
    }
    public CFGSymbolicExpressionTreeStringFormatter()
      : base("CFG String Formatter", "A string formatter for CFG symbolic expression trees.") {
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

    //private static HashSet<string> NewLine = new HashSet<string>() { "\n", "\r\n", "\r"};
    private static string FormatRecursively(ISymbolicExpressionTreeNode node) {
      StringBuilder strBuilder = new StringBuilder();
      if (node.Subtrees.Count() > 0) {
        // node
        var symbol = node.Symbol as CFGSymbol;
        if (symbol != null) {
          var partsEnumerator = symbol.GetTerminalParts().GetEnumerator();
          var subtreeEnumerator = node.Subtrees.GetEnumerator();
          while (partsEnumerator.MoveNext() && subtreeEnumerator.MoveNext()) {
            strBuilder.Append(partsEnumerator.Current);
            strBuilder.Append(FormatRecursively(subtreeEnumerator.Current));
          }
          strBuilder.Append(partsEnumerator.Current);
        } else {
          // ProgramRoot or StartSymbol
          foreach (var subtree in node.Subtrees) {
            strBuilder.Append(FormatRecursively(subtree));
          }
        }
      } else {
        // leaf
        var symbol = node.Symbol as CFGSymbol;
          var parts = symbol.GetTerminalParts();
          strBuilder.Append(parts.First());
      }
      return strBuilder.ToString();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGSymbolicExpressionTreeStringFormatter(this, cloner);
    }
  }
}

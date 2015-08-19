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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HeuristicLab.Problems.Instances.CFG {
  public static class PythonStringHelper {

    public static string PrepareStringForPython(this string x) {
      return String.Format("\"{0}\"", Regex.Escape(x));
    }

    /// <summary>
    /// The code below escapes strings to work with python
    /// </summary>
    private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()    {
        {"\"", @"\"""},
        {"\'", @"\'"},
        {"\\", @"\\"},
        {"\a", @"\a"},
        {"\b", @"\b"},
        {"\f", @"\f"},
        {"\n", @"\n"},
        {"\r", @"\r"},
        {"\t", @"\t"},
        {"\v", @"\v"},
    };

    private static Regex escapeRegex = new Regex(String.Format("[{0}]", String.Concat(escapeMapping.Values.ToArray())));

    public static string PythonEscape(this string s) {
      return escapeRegex.Replace(s, EscapeMatchEval);
    }

    private static string EscapeMatchEval(Match m) {
      if (escapeMapping.ContainsKey(m.Value)) {
        return escapeMapping[m.Value];
      }
      throw new NotSupportedException();
    }
  }
}

#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonSemanticComparer {

    public static double Compare<T>(IEnumerable<T> first, IEnumerable<T> second, Func<T, T, double> distance) {
      double sum = 0;
      double max = 0;
      int count = 0;
      var firstEnumerator = first.GetEnumerator();
      var secondEnumerator = second.GetEnumerator();
      while (firstEnumerator.MoveNext() && secondEnumerator.MoveNext()) {
        double cur = distance(firstEnumerator.Current, secondEnumerator.Current);
        sum += cur;
        if (cur > max) max = cur;
        count++;
      }
      if (firstEnumerator.MoveNext() || secondEnumerator.MoveNext()) throw new ArgumentException("One enumerator had more elements than the other");

      return sum / (max * count);
    }

    public static double Compare(IEnumerable<bool> first, IEnumerable<bool> second) {
      return Compare<bool>(first, second, (x, y) => x == y ? 0 : 1);
    }

    public static double Compare(IEnumerable<int> first, IEnumerable<int> second) {
      return Compare<int>(first, second, (x, y) => Math.Abs(x - y));
    }

    public static double Compare(IEnumerable<double> first, IEnumerable<double> second) {
      return Compare<double>(first, second, (x, y) => Math.Abs(x - y));
    }

    public static double Compare(IEnumerable<string> first, IEnumerable<string> second) {
      return Compare<string>(first, second, LevenshteinDistance);
    }

    // from: http://rosettacode.org/wiki/Levenshtein_distance#C.23
    // which is faster than https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance#C.24
    // and equal to http://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
    private static double LevenshteinDistance(string s, string t) {
      int n = s.Length;
      int m = t.Length;
      int[,] d = new int[n + 1, m + 1];

      if (n == 0) {
        return m;
      }

      if (m == 0) {
        return n;
      }

      for (int i = 0; i <= n; i++)
        d[i, 0] = i;
      for (int j = 0; j <= m; j++)
        d[0, j] = j;

      for (int j = 1; j <= m; j++)
        for (int i = 1; i <= n; i++)
          if (s[i - 1] == t[j - 1])
            d[i, j] = d[i - 1, j - 1];  //no operation
          else
            d[i, j] = Math.Min(Math.Min(
                d[i - 1, j] + 1,    //a deletion
                d[i, j - 1] + 1),   //an insertion
                d[i - 1, j - 1] + 1 //a substitution
                );
      return d[n, m];
    }

    public Tuple<int, int> Compare(IEnumerable<IList<bool>> x, IEnumerable<IList<bool>> y) {
      throw new NotImplementedException();
    }

    public Tuple<int, int> Compare(IEnumerable<IList<int>> x, IEnumerable<IList<int>> y) {
      throw new NotImplementedException();
    }

    public Tuple<int, double> Compare(IEnumerable<IList<double>> x, IEnumerable<IList<double>> y) {
      throw new NotImplementedException();
    }


    public Tuple<int, int> Compare(IEnumerable<IList<string>> x, IEnumerable<IList<string>> y) {
      throw new NotImplementedException();
    }
  }
}

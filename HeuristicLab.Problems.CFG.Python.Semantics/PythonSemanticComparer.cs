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
using System.Linq;
using HeuristicLab.Common;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonSemanticComparer {

    #region base data types
    //---------------------------------------------------------------------------------------------------------------
    // Base Data Types
    //---------------------------------------------------------------------------------------------------------------

    public static double Compare(IEnumerable<bool> first, IEnumerable<bool> second, bool normalize) {
      return Compare(first, second, (x, y) => x == y ? 0 : 1, normalize);
    }

    public static double Compare(IEnumerable<double> first, IEnumerable<double> second, bool normalize) {
      return Compare(first, second, (x, y) => Math.Abs(x - y), normalize);
    }

    public static double Compare(IEnumerable<string> first, IEnumerable<string> second, bool normalize) {
      return Compare(first, second, LevenshteinDistance, normalize);
    }

    private static double Compare<T>(IEnumerable<T> first, IEnumerable<T> second, Func<T, T, double> distance, bool normalize) {
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

      if (!normalize) return sum;

      double denom = max * count;
      if (sum == 0) return 0;
      if (denom.IsAlmost(0)) return 1;
      sum /= denom;
      return sum == 0 ? Double.Epsilon : sum;
    }
    #endregion

    #region list data types
    //---------------------------------------------------------------------------------------------------------------
    // List Data Types
    //---------------------------------------------------------------------------------------------------------------

    public static double Compare<T>(IEnumerable<IList<T>> first, IEnumerable<IList<T>> second, bool normalize) {
      return CompareList<T>(first, second, LevenshteinDistanceList<T>, normalize);
    }

    private static double CompareList<T>(IEnumerable<IList<T>> first, IEnumerable<IList<T>> second, Func<IList<T>, IList<T>, double> distance, bool normalize) {
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

      if (!normalize) return sum;

      double denom = max * count;
      if (sum == 0) return 0;
      if (denom.IsAlmost(0)) return 1;
      sum /= denom;
      return sum == 0 ? Double.Epsilon : sum;
    }
    #endregion

    #region base data types multiple
    public static IEnumerable<double> Compare(IEnumerable<bool> first, IEnumerable<IEnumerable<bool>> second, bool normalize) {
      return Compare(first, second, (x, y) => x == y ? 0 : 1, normalize);
    }

    public static IEnumerable<double> Compare(IEnumerable<double> first, IEnumerable<IEnumerable<double>> second, bool normalize) {
      return Compare(first, second, (x, y) => Math.Abs(x - y), normalize);
    }

    public static IEnumerable<double> Compare(IEnumerable<string> first, IEnumerable<IEnumerable<string>> second, bool normalize) {
      return Compare(first, second, LevenshteinDistance, normalize);
    }

    private static IEnumerable<double> Compare<T>(IEnumerable<T> first, IEnumerable<IEnumerable<T>> second, Func<T, T, double> distance, bool normalize) {
      List<double> sum = new List<double>();
      List<IEnumerator<T>> enumerators = new List<IEnumerator<T>>();
      foreach (var item in second) {
        enumerators.Add(item.GetEnumerator());
        sum.Add(0);
      }
      double max = 0;
      int count = 0;
      var firstEnumerator = first.GetEnumerator();
      while (firstEnumerator.MoveNext() && enumerators.All(x => x.MoveNext())) {
        for (int i = 0; i < enumerators.Count; i++) {
          double cur = distance(firstEnumerator.Current, enumerators[i].Current);
          sum[i] += cur;
          if (cur > max) max = cur;
        }
        count++;
      }
      if (firstEnumerator.MoveNext() || enumerators.Any(x => x.MoveNext())) throw new ArgumentException("One enumerator had more elements than the other");

      if (!normalize) return sum;

      double denom = sum.Max();
      for (int i = 0; i < sum.Count; i++) {
        if (sum[i] == 0) continue;
        if (denom.IsAlmost(0)) sum[i] = 1;
        else {
          sum[i] /= denom;
          sum[i] = sum[i] == 0 ? Double.Epsilon : sum[i];
        }
      }
      return sum;
    }
    #endregion

    #region list data types multiple
    public static IEnumerable<double> Compare<T>(IEnumerable<IList<T>> first, IEnumerable<IEnumerable<IList<T>>> second, bool normalize) {
      return CompareList<T>(first, second, LevenshteinDistanceList<T>, normalize);
    }

    private static IEnumerable<double> CompareList<T>(IEnumerable<IList<T>> first, IEnumerable<IEnumerable<IList<T>>> second, Func<IList<T>, IList<T>, double> distance, bool normalize) {
      List<double> sum = new List<double>();
      List<IEnumerator<IList<T>>> enumerators = new List<IEnumerator<IList<T>>>();
      foreach (var item in second) {
        enumerators.Add(item.GetEnumerator());
        sum.Add(0);
      }
      double max = 0;
      int count = 0;
      var firstEnumerator = first.GetEnumerator();
      while (firstEnumerator.MoveNext() && enumerators.All(x => x.MoveNext())) {
        for (int i = 0; i < enumerators.Count; i++) {
          double cur = distance(firstEnumerator.Current, enumerators[i].Current);
          sum[i] += cur;
          if (cur > max) max = cur;
        }
        count++;
      }
      if (firstEnumerator.MoveNext() || enumerators.Any(x => x.MoveNext())) throw new ArgumentException("One enumerator had more elements than the other");

      if (!normalize) return sum;

      double denom = sum.Max();
      for (int i = 0; i < sum.Count; i++) {
        if (sum[i] == 0) continue;
        if (denom.IsAlmost(0)) sum[i] = 1;
        else {
          sum[i] /= denom;
          sum[i] = sum[i] == 0 ? Double.Epsilon : sum[i];
        }
      }
      return sum;
    }
    #endregion

    #region Levenshtein Distance
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

    private static double LevenshteinDistanceList<T>(IList<T> first, IList<T> second) {
      int n = first.Count;
      int m = second.Count;
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
          if (first[i - 1].Equals(second[j - 1]))
            d[i, j] = d[i - 1, j - 1];  //no operation
          else
            d[i, j] = Math.Min(Math.Min(
                d[i - 1, j] + 1,    //a deletion
                d[i, j - 1] + 1),   //an insertion
                d[i - 1, j - 1] + 1 //a substitution
                );
      return d[n, m];
    }
    #endregion
  }
}

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python.Semantics {
  public class PythonSemanticComparer {

    /// <summary>
    /// Compares to variable settings if they are equal
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool CompareSequence(IEnumerable first, IEnumerable second) {
      var v1Enumerator = first.GetEnumerator();
      var v2Enumerator = second.GetEnumerator();
      while (v1Enumerator.MoveNext() && v2Enumerator.MoveNext()) {
        if (v1Enumerator.Current is IList) {
          // then both must be IEnumerable
          if (!CompareSequence((IList)v1Enumerator.Current, (IList)v2Enumerator.Current)) {
            return false;
          }
        } else if (!v1Enumerator.Current.Equals(v2Enumerator.Current)) {
          return false;
        }
      }
      if (v1Enumerator.MoveNext() || v2Enumerator.MoveNext()) {
        return false;
      }
      return true;
    }

    public static bool AnyChange(IDictionary<string, JToken> sem1, IDictionary<string, JToken> sem2) {
      var allKeys = sem1.Keys.Union(sem2.Keys);
      if (allKeys.Except(sem1.Keys).Any() || allKeys.Except(sem2.Keys).Any()) return true;

      foreach (var key in allKeys) {
        if (ChangesPerVariable(sem1[key], sem2[key]).Any(x => x)) return true;
      }

      return false;
    }

    public static bool PartialChangeInAtLeastOneVariable(IDictionary<string, JToken> sem1, IDictionary<string, JToken> sem2) {
      var keysInBoth = sem1.Keys.Intersect(sem2.Keys);
      if (!keysInBoth.Any()) return false;

      foreach (var key in keysInBoth) {
        var changes = ChangesPerVariable(sem1[key], sem2[key]).ToList();
        if (!(changes.All(x => x) || changes.All(y => !y))) {
          return true;
        }
      }
      return false;
    }
    public static IEnumerable<bool> ChangesPerVariable(IEnumerable<JToken> values1, IEnumerable<JToken> values2) {
      var v1Enumerator = values1.GetEnumerator();
      var v2Enumerator = values2.GetEnumerator();
      while (v1Enumerator.MoveNext() && v2Enumerator.MoveNext()) {
        if (v1Enumerator.Current.Any()) {
          yield return v1Enumerator.Current.SequenceEqual(v2Enumerator.Current);
        } else {
          yield return !v1Enumerator.Current.Equals(v2Enumerator.Current);
        }
      }

      if (!v1Enumerator.MoveNext() && !v2Enumerator.MoveNext()) yield break;
      for (int i = 0; i < Math.Abs(values1.Count() - values2.Count()); i++) {
        yield return true;
      }
      v1Enumerator.Dispose();
      v2Enumerator.Dispose();
    }

    private static IEnumerable<bool> ChangesPerVariable(IEnumerable values1, IEnumerable values2) {
      var v1Enumerator = values1.GetEnumerator();
      var v2Enumerator = values2.GetEnumerator();
      while (v1Enumerator.MoveNext() && v2Enumerator.MoveNext()) {
        if (v1Enumerator.Current is IList) {
          yield return !CompareSequence((IList)v1Enumerator.Current, (IList)v2Enumerator.Current);
        } else {
          yield return v1Enumerator.Current == null ? v2Enumerator.Current == null : !v1Enumerator.Current.Equals(v2Enumerator.Current);
        }

        if (!v1Enumerator.MoveNext() && !v2Enumerator.MoveNext()) yield break;
        while (v1Enumerator.MoveNext() || v2Enumerator.MoveNext()) {
          yield return true;
        }
      }
    }

    public static JObject ReplaceNotExecutedCases(JObject semantics, IDictionary<string, IList> before, List<int> executedCases) {
      var converted = semantics.ToObject<IDictionary<string, IList>>();
      foreach (var key in converted.Keys.ToList()) {
        if (executedCases.Count == converted[key].Count) continue;
        int pos = 0;
        for (int i = 0; i < converted[key].Count; i++) {
          if (pos >= executedCases.Count || (pos < executedCases.Count && executedCases[pos] != i)) {
            converted[key][i] = before[key][i];
          } else {
            pos++;
          }
        }
      }
      return JObject.FromObject(converted);
    }
    public static JObject ProduceDifference(JObject evaluatedSemantics, IDictionary<string, IList> oldSemanticsBefore) {
      var converted = evaluatedSemantics.ToObject<IDictionary<string, IList>>();
      var tmp = new Dictionary<string, IList>();
      foreach (var key in oldSemanticsBefore.Keys) {
        if (ChangesPerVariable(converted[key], oldSemanticsBefore[key]).Any(x => x)) {
          tmp[key] = converted[key];
        }
      }
      return JObject.FromObject(tmp);
    }

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

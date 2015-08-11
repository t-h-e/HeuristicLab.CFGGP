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

namespace HeuristicLab.Problems.Instances.CFG {
  public class EnumerableValueEqualityComparer<T> : IEqualityComparer<IEnumerable<T>> {
    #region IEqualityComparer<IEnumerable<T>> Members

    public bool Equals(IEnumerable<T> x, IEnumerable<T> y) {
      return x.SequenceEqual(y);
    }

    public int GetHashCode(IEnumerable<T> obj) {
      int hashcode = obj.Count();
      foreach (var item in obj) {
        hashcode = unchecked(hashcode * 314159 + item.GetHashCode());
      }
      return hashcode;
    }

    #endregion
  }
}

#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2018 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

namespace HeuristicLab.Problems.Instances.CFG {
  public enum DataType {
    Boolean, Integer, Float, String, Char, ListBoolean, ListInteger, ListFloat, ListString
  }

  public static class DataTypeExtensions {
    public static IEnumerable<DataType> Requires(this DataType dt) {
      switch (dt) {
        case DataType.ListBoolean:
          return new List<DataType>() { DataType.Boolean };
        case DataType.ListInteger:
          return new List<DataType>() { DataType.Integer };
        case DataType.ListFloat:
          return new List<DataType>() { DataType.Float };
        case DataType.ListString:
          return new List<DataType>() { DataType.String };
        default:
          break;
      }
      return new List<DataType>();
    }
  }
}

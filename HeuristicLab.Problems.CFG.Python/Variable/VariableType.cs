﻿#region License Information
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

using System.Collections.Generic;

namespace HeuristicLab.Problems.CFG.Python {
  public static class Extensions {
    private static List<VariableType> listTypes = new List<VariableType>() { VariableType.List_Bool, VariableType.List_Float, VariableType.List_Int, VariableType.List_String };
    public static bool IsListType(this VariableType type) {
      return listTypes.Contains(type);
    }
  }
  public enum VariableType {
    Bool, Char, Int, Float, String, List_Bool, List_Int, List_Float, List_String
  }
}

﻿#region License Information
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

namespace HeuristicLab.Problems.Instances.CFG {
  /// <summary>
  /// Describes instances of the Context Free Grammar Problem.
  /// Todo: belongs in the HeuristicLab.Problems.Instances project
  /// </summary>
  public class CFGData {
    /// <summary>
    /// The name of the instance
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Optional! The description of the instance
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// String containing the path to a file with the grammar
    /// </summary>
    public string Grammar { get; set; }
    /// <summary>
    /// Optional! String containing the path to a file where code should be embeded
    /// </summary>
    public string Embed { get; set; }
    /// <summary>
    /// Input values
    /// </summary>
    public string[] Input { get; set; }
    /// <summary>
    /// Output values
    /// </summary>
    public string[] Output { get; set; }

    public int TrainStart { get; set; }
    public int TrainEnd { get; set; }
    public int TestStart { get; set; }
    public int TestEnd { get; set; }

  }
}

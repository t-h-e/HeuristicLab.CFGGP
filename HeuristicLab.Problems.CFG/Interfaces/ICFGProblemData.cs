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

using System;
using System.Collections.Generic;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Misc;

namespace HeuristicLab.Problems.CFG {
  public interface ICFGProblemData : INamedItem {

    StringArray Input { get; }
    StringArray Output { get; }
    IntRange TrainingPartition { get; }
    IntRange TestPartition { get; }
    TextValue Header { get; }
    TextValue Footer { get; }

    IEnumerable<int> TrainingIndices { get; }
    IEnumerable<int> TestIndices { get; }

    bool IsTrainingSample(int index);
    bool IsTestSample(int index);

    event EventHandler Changed;
  }
}

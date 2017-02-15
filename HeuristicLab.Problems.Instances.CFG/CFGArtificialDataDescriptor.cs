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
namespace HeuristicLab.Problems.Instances.CFG {
  public abstract class CFGArtificialDataDescriptor : IDataDescriptor {
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Identifier { get; }
    protected abstract int TrainingPartitionStart { get; }
    protected abstract int TrainingPartitionEnd { get; }
    protected abstract int TestPartitionStart { get; }
    protected abstract int TestPartitionEnd { get; }

    public virtual CFGData GenerateData() {
      CFGData cfgData = new CFGData();
      cfgData.Name = Name;
      cfgData.Description = Description;
      cfgData.TrainingPartitionStart = TrainingPartitionStart;
      cfgData.TrainingPartitionEnd = TrainingPartitionEnd;
      cfgData.TestPartitionStart = TestPartitionStart;
      cfgData.TestPartitionEnd = TestPartitionEnd;
      var inputOutput = GenerateInputOutput();
      cfgData.Input = inputOutput.Item1;
      cfgData.Output = inputOutput.Item2;
      return cfgData;
    }

    protected abstract Tuple<string[], string[]> GenerateInputOutput();
  }
}

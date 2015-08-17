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

namespace HeuristicLab.Problems.Instances.CFG {
  public class BenchmarkSuiteInstanceProvider : CFGArtificialInstanceProvider {
    public override string Name {
      get { return "General Program Synthesis Benchmark Suite"; }
    }
    public override string Description {
      get { return ""; }
    }
    public override Uri WebLink {
      get { return new Uri("https://web.cs.umass.edu/publication/docs/2015/UM-CS-2015-006.pdf"); }
    }
    public override string ReferencePublication {
      get { return "T. Helmuth and L. Spector, \"Detailed Problem Descriptions for General Program Synthesis Benchmark Suite\", Technical Report UM-CS-2015-006, School of Computer Science, University of Massachusetts Amherst, 2015."; }
    }

    protected override string FileName { get { return "BenchmarkSuite"; } }

    public override IEnumerable<IDataDescriptor> GetDataDescriptors() {
      List<IDataDescriptor> descriptorList = new List<IDataDescriptor>();
      descriptorList.Add(new NumberIO());
      descriptorList.Add(new SmallOrLarge());
      descriptorList.Add(new ForLoopIndex());
      descriptorList.Add(new CompareStringLengths());
      descriptorList.Add(new DoubleLetters());
      descriptorList.Add(new CollatzNumbers());
      descriptorList.Add(new ReplaceSpaceWithNewline());
      descriptorList.Add(new StringDifferences());
      descriptorList.Add(new EvenSquares());
      descriptorList.Add(new WallisPi());
      descriptorList.Add(new StringLengthsBackwards());
      descriptorList.Add(new LastIndexOfZero());
      descriptorList.Add(new VectorAverage());
      descriptorList.Add(new CountOdds());
      descriptorList.Add(new MirrorImage());


      descriptorList.Add(new SumOfSquares());
      descriptorList.Add(new Digits());
      descriptorList.Add(new Grade());
      descriptorList.Add(new Median());
      descriptorList.Add(new Smallest());
      descriptorList.Add(new VectorsSummed());
      descriptorList.Add(new NegativeToZero());
      return descriptorList;
    }
  }
}

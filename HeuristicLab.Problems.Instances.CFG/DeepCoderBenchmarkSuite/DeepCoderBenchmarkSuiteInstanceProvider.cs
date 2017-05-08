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
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace HeuristicLab.Problems.Instances.CFG {
  public class DeepCoderBenchmarkSuiteInstanceProvider : CFGArtificialInstanceProvider {
    public override string Name {
      get { return "DeepCoder Benchmark Suite"; }
    }
    public override string Description {
      get { return ""; }
    }
    public override Uri WebLink {
      get { return new Uri("https://arxiv.org/abs/1611.01989"); }
    }
    public override string ReferencePublication {
      get { return ""; }
    }

    protected override string FileName { get { return "DeepCoder"; } }

    public override CFGData LoadData(IDataDescriptor id) {
      CFGArtificialDataDescriptor descriptor = (CFGArtificialDataDescriptor)id;
      CFGData cfgData = descriptor.GenerateData();
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      using (var instancesZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        IEnumerable<ZipArchiveEntry> entries = instancesZipFile.Entries.Where(e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));

        var embedEntry = entries.FirstOrDefault(x => x.Name.EndsWith(".py"));
        if (embedEntry != null) {
          using (var stream = new StreamReader(embedEntry.Open())) {
            cfgData.Embed = stream.ReadToEnd();
          }
        }
      }
      return cfgData;
    }

    public override IEnumerable<IDataDescriptor> GetDataDescriptors() {
      List<IDataDescriptor> descriptorList = new List<IDataDescriptor>();
      descriptorList.Add(new Program0());
      descriptorList.Add(new Program1());
      descriptorList.Add(new Program2());
      descriptorList.Add(new Program3());
      descriptorList.Add(new Program4());
      descriptorList.Add(new Program5());
      descriptorList.Add(new Program6());
      descriptorList.Add(new Program7());
      descriptorList.Add(new Program8());
      return descriptorList;
    }
  }
}

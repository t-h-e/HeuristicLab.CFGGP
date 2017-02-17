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
using System.Reflection;
using System.Text.RegularExpressions;

namespace HeuristicLab.Problems.Instances.CFG {
  public abstract class CFGArtificialInstanceProvider : CFGInstanceProvider {

    public override bool CanExportData {
      get { return true; }
    }

    public override void ExportData(CFGData instance, string path) { throw new NotImplementedException(); }

    protected abstract string FileName { get; }

    public override CFGData LoadData(IDataDescriptor id) {
      CFGArtificialDataDescriptor descriptor = (CFGArtificialDataDescriptor)id;
      CFGData cfgData = descriptor.GenerateData();
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      using (var instancesZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        IEnumerable<ZipArchiveEntry> entries = instancesZipFile.Entries.Where(e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));

        var bnfEntry = entries.Where(x => x.Name.EndsWith(".bnf")).FirstOrDefault();
        if (bnfEntry != null) {
          using (var stream = new StreamReader(bnfEntry.Open())) {
            cfgData.Grammar = stream.ReadToEnd();
          }
        }

        var embedEntry = entries.Where(x => x.Name.EndsWith("Embed.txt")).FirstOrDefault();
        if (embedEntry != null) {
          using (var stream = new StreamReader(embedEntry.Open())) {
            cfgData.Embed = stream.ReadToEnd();
          }
        }
      }
      return cfgData;
    }

    protected virtual string GetResourceName(string fileName) {
      return Assembly.GetExecutingAssembly().GetManifestResourceNames()
              .Where(x => Regex.Match(x, @".*\.Data\." + fileName).Success).SingleOrDefault();
    }
  }
}

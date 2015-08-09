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
  public class CFGZipInstanceProvider : CFGInstanceProvider {

    public override string Name {
      get { return "Program Synthesis"; }
    }

    public override string Description {
      get { return "Program Synthesis Problems"; }
    }

    public override Uri WebLink {
      get { return new Uri("http://doi.acm.org/10.1145/2739480.2754769"); }
    }

    public override string ReferencePublication {
      get {
        return @"Thomas Helmuth and Lee Spector. 2015.
General Program Synthesis Benchmark Suite.
In Proceedings of the 2015 on Genetic and Evolutionary Computation Conference (GECCO '15), Sara Silva (Ed.). ACM, New York, NY, USA, 1039-1046.";
      }
    }

    protected virtual string FileName { get { return "ProgramSynthesis"; } }

    public override IEnumerable<IDataDescriptor> GetDataDescriptors() {
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      if (String.IsNullOrEmpty(instanceArchiveName)) yield break;

      using (var instanceStream = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        foreach (var entry in instanceStream.Entries.Where(
                        e => String.IsNullOrWhiteSpace(e.Name)  //has to be a directory
                     && instanceStream.Entries.Any(x => x.FullName.StartsWith(e.FullName) && x.Name.EndsWith(".bnf")) //must contain grammar
                     && instanceStream.Entries.Any(x => x.FullName.StartsWith(e.FullName) && x.Name.EndsWith("-Data.txt")) //must contain data
                     ).Select(x => x.FullName.Substring(0, x.FullName.Length - 1)).OrderBy(x => x)) {
          yield return new CFGZipDataDescriptor(Path.GetFileNameWithoutExtension(entry), GetDescription(), entry);
        }
      }
    }

    public override CFGData LoadData(IDataDescriptor id) {
      CFGData cfgData = new CFGData();
      var descriptor = (CFGZipDataDescriptor)id;
      cfgData.Name = descriptor.Identifier;
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      using (var instancesZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        IEnumerable<ZipArchiveEntry> entries = instancesZipFile.Entries.Where(e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));

        using (var stream = new StreamReader(entries.Where(x => x.Name.EndsWith(".bnf")).First().Open())) {
          cfgData.Grammar = stream.ReadToEnd();
        }

        var embedEntry = entries.Where(x => x.Name.EndsWith("Embed.txt")).FirstOrDefault();
        if (embedEntry != null) {
          using (var stream = new StreamReader(embedEntry.Open())) {
            cfgData.Embed = stream.ReadToEnd();
          }
        }

        ZipArchiveEntry data = entries.Where(x => x.Name.EndsWith("-Data.txt")).First();
        using (StreamReader stream = new StreamReader(data.Open())) {
          int help;
          var train = stream.ReadLine().Split(new char[] { '-' });
          if (int.TryParse(train[0], out help)) { cfgData.TrainingPartitionStart = help; } else { throw new ArgumentException("Expected int."); }
          if (int.TryParse(train[1], out help)) { cfgData.TrainingPartitionEnd = help; } else { throw new ArgumentException("Expected int."); }
          var test = stream.ReadLine().Split(new char[] { '-' });
          if (int.TryParse(test[0], out help)) { cfgData.TestPartitionStart = help; } else { throw new ArgumentException("Expected int."); }
          if (int.TryParse(test[1], out help)) { cfgData.TestPartitionEnd = help; } else { throw new ArgumentException("Expected int."); }

          if (!stream.ReadLine().Equals("//in")) { throw new ArgumentException("Expected input data."); }

          List<string> input = new List<string>();
          string line;
          while ((line = stream.ReadLine()) != null && !line.Equals("//out")) {
            input.Add(line);
          }
          cfgData.Input = input.ToArray();

          if (!line.Equals("//out")) { throw new ArgumentException("Expected output data."); }
          List<string> output = new List<string>();
          while ((line = stream.ReadLine()) != null) {
            output.Add(line);
          }
          cfgData.Output = output.ToArray();

          if (cfgData.Input.Length != cfgData.Output.Length) { throw new ArgumentException("Input and output do not have the same length."); }
        }
      }

      return cfgData;
    }

    private string GetDescription() {
      return "Embedded instance of plugin version " + Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).Cast<AssemblyFileVersionAttribute>().First().Version + ".";
    }

    protected virtual string GetResourceName(string fileName) {
      return Assembly.GetExecutingAssembly().GetManifestResourceNames()
              .Where(x => Regex.Match(x, @".*\.Data\." + fileName).Success).SingleOrDefault();
    }
  }
}

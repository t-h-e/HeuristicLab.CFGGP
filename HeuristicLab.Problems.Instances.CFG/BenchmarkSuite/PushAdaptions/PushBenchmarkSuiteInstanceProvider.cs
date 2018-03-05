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
  public class PushBenchmarkSuiteInstanceProvider : BenchmarkSuiteListInstanceProvider {

    private const string HeaderSeparation = "# *****************************************************************************";

    public override string Name {
      get { return "Push Related Grammars General Program Synthesis Benchmark Suite"; }
    }

    public override CFGData LoadDataLocal(IDataDescriptor id, bool treeStructure, int numberOfVariables = 3, bool recursion = false) {
      BenchmarkSuiteDataDescritpor descriptor = (BenchmarkSuiteDataDescritpor)id;
      CFGData cfgData = descriptor.GenerateData(treeStructure, numberOfVariables, true, new PythonPushRelatedGrammarConstructor());  // set recursion to true. it should always be used for the Push Related Grammars
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      using (
        var instancesZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName), ZipArchiveMode.Read)) {
        IEnumerable<ZipArchiveEntry> entries = instancesZipFile.Entries.Where(e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));

        var embedEntry = entries.FirstOrDefault(x => x.Name.EndsWith("Embed.txt"));
        if (embedEntry != null) {
          using (var stream = new StreamReader(embedEntry.Open())) {
            cfgData.Embed = stream.ReadToEnd();
          }
        }
      }

      var pushRelatedArchiveName = GetType().Assembly.GetManifestResourceNames().First(x => x.EndsWith("PythonPushRelated.PushRelated.zip"));
      using (
        var pushRelatedZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(pushRelatedArchiveName), ZipArchiveMode.Read)) {
        ZipArchiveEntry headerEntry = pushRelatedZipFile.Entries.First(e => e.Name == "Embed-Header.txt");

        using (var stream = new StreamReader(headerEntry.Open())) {
          var realHeader = stream.ReadToEnd();
          var endOfOriginalHeader = cfgData.Embed.LastIndexOf(HeaderSeparation);
          cfgData.Embed = realHeader + cfgData.Embed.Substring(endOfOriginalHeader + HeaderSeparation.Length);
        }

        ZipArchiveEntry additionalEntry = pushRelatedZipFile.Entries.FirstOrDefault(e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));
        if (additionalEntry != null) {
          using (var stream = new StreamReader(additionalEntry.Open())) {
            var additionalHeader = stream.ReadToEnd();
            var endOfHeader = cfgData.Embed.LastIndexOf(HeaderSeparation);
            cfgData.Embed = cfgData.Embed.Substring(0, endOfHeader) + additionalHeader + cfgData.Embed.Substring(endOfHeader, cfgData.Embed.Length - endOfHeader);
          }
        }
      }

      return cfgData;
    }

    public override CFGData GenerateGrammar(Options options) {
      var python = new PythonPushRelatedGrammarConstructor();
      var grammar = python.CombineDataTypes(options);
      grammar.TrimGrammar(true);

      var data = new CFGData {
        Name = String.Format("Generated Grammar (Types: {0})", String.Join(", ", options.Datatypes)),
        Grammar = grammar.PrintGrammar()
      };
      return data;
    }

    public override IEnumerable<IDataDescriptor> GetDataDescriptors() {
      List<IDataDescriptor> descriptorList = new List<IDataDescriptor>();
      descriptorList.Add(new NumberIO());
      descriptorList.Add(new SmallOrLarge());
      descriptorList.Add(new ForLoopIndex());
      descriptorList.Add(new CompareStringLengths());
      descriptorList.Add(new PushDoubleLetters());
      descriptorList.Add(new CollatzNumbers());
      descriptorList.Add(new PushReplaceSpaceWithNewline());
      descriptorList.Add(new PushStringDifferences());
      descriptorList.Add(new EvenSquares());
      descriptorList.Add(new WallisPi());
      descriptorList.Add(new StringLengthsBackwards());
      descriptorList.Add(new LastIndexOfZero());
      descriptorList.Add(new VectorAverage());
      descriptorList.Add(new CountOdds());
      descriptorList.Add(new MirrorImage());
      descriptorList.Add(new PushSuperAnagrams());
      descriptorList.Add(new SumOfSquares());
      descriptorList.Add(new VectorsSummed());
      descriptorList.Add(new PushXWordLines());
      descriptorList.Add(new PushPigLatin());
      descriptorList.Add(new NegativeToZero());
      descriptorList.Add(new PushScrabbleScore());
      descriptorList.Add(new PushWordStats());
      descriptorList.Add(new PushChecksum());
      descriptorList.Add(new PushDigits());
      descriptorList.Add(new Grade());
      descriptorList.Add(new Median());
      descriptorList.Add(new Smallest());
      descriptorList.Add(new PushSyllables());

      return descriptorList;
    }
  }
}

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
  public abstract class BenchmarkSuiteInstanceProvider : CFGArtificialInstanceProvider {

    public override bool CanImportData {
      get { return true; }
    }

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
      get {
        return
          "T. Helmuth and L. Spector, \"Detailed Problem Descriptions for General Program Synthesis Benchmark Suite\", Technical Report UM-CS-2015-006, School of Computer Science, University of Massachusetts Amherst, 2015.";
      }
    }

    protected override string FileName {
      get { return "BenchmarkSuite"; }
    }

    public CFGData LoadDataLocal(IDataDescriptor id, bool treeStructure, int numberOfVariables = 3) {
      BenchmarkSuiteDataDescritpor descriptor = (BenchmarkSuiteDataDescritpor)id;
      CFGData cfgData = descriptor.GenerateData(treeStructure, numberOfVariables);
      var instanceArchiveName = GetResourceName(FileName + @"\.zip");
      using (
        var instancesZipFile = new ZipArchive(GetType().Assembly.GetManifestResourceStream(instanceArchiveName),
          ZipArchiveMode.Read)) {
        IEnumerable<ZipArchiveEntry> entries =
          instancesZipFile.Entries.Where(
            e => e.FullName.StartsWith(descriptor.Identifier) && !String.IsNullOrWhiteSpace(e.Name));

        var embedEntry = entries.FirstOrDefault(x => x.Name.EndsWith("Embed.txt"));
        if (embedEntry != null) {
          using (var stream = new StreamReader(embedEntry.Open())) {
            cfgData.Embed = stream.ReadToEnd();
          }
        }
      }
      return cfgData;
    }

    public CFGData GenerateGrammar(Options options) {
      var python = new PythonGrammarConstructor();
      var grammar = python.CombineDataTypes(options);
      grammar.TrimGrammar(true);

      var data = new CFGData {
        Name = String.Format("Generated Grammar (Types: {0})", String.Join(", ", options.Datatypes)),
        Grammar = grammar.PrintGrammar()
      };
      return data;
    }

    public override CFGData ImportData(string path) {
      var data = new CFGData();
      data.Name = Path.GetFileNameWithoutExtension(path);
      using (var reader = new StreamReader(path)) {
        data.Grammar = reader.ReadToEnd();
      }
      return data;
    }

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
      descriptorList.Add(new SuperAnagrams());
      descriptorList.Add(new SumOfSquares());
      descriptorList.Add(new VectorsSummed());
      descriptorList.Add(new XWordLines());
      descriptorList.Add(new PigLatin());
      descriptorList.Add(new NegativeToZero());
      descriptorList.Add(new ScrabbleScore());
      descriptorList.Add(new WordStats());
      descriptorList.Add(new Checksum());
      descriptorList.Add(new Digits());
      descriptorList.Add(new Grade());
      descriptorList.Add(new Median());
      descriptorList.Add(new Smallest());
      descriptorList.Add(new Syllables());

      return descriptorList;
    }
  }
  public class BenchmarkSuiteListInstanceProvider : BenchmarkSuiteInstanceProvider {
    public override CFGData LoadData(IDataDescriptor id) {
      return LoadDataLocal(id, false);
    }
  }

  /// <summary>
  /// Has been added to be able to use the CreateExperiment dialog to generate experiments with tree grammars as well
  /// </summary>
  public class BenchmarkSuiteTreeInstanceProvider : BenchmarkSuiteInstanceProvider {
    public override string Name {
      get { return "Tree Grammars General Program Synthesis Benchmark Suite"; }
    }

    public override CFGData LoadData(IDataDescriptor id) {
      return LoadDataLocal(id, true);
    }
  }
}

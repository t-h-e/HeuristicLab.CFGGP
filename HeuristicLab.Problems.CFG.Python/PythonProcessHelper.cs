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
using System.Diagnostics;
using System.Linq;
using HeuristicLab.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace HeuristicLab.Problems.CFG.Python {
  public class PythonProcessHelper {

    private Process python;
    private PythonProcessHelper() {
      python = new Process {
        StartInfo = new ProcessStartInfo {
          FileName = "python",
          //Arguments = @"E:\coding\codewarsPython\python_script_evaluation.py",
          Arguments = @"python_script_evaluation.py",
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          CreateNoWindow = true
        }
      };
      python.Start();
    }

    private static PythonProcessHelper instance;
    public static PythonProcessHelper GetInstance() {
      if (instance == null) {
        instance = new PythonProcessHelper();
      }
      return instance;
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, StringArray input, StringArray output, IEnumerable<int> indices, int timeout = 1000) {
      return EvaluateProgram(program, PythonHelper.ConvertToPythonValues(input, indices), PythonHelper.ConvertToPythonValues(output, indices), indices, timeout);
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, string input, string output, IEnumerable<int> indices, int timeout = 1000) {
      EvaluationScript es = new EvaluationScript() {
        Script = String.Format("inval = {0}\noutval = {1}\n{2}", input, output, program),
        Variables = new List<string>() { "cases", "caseQuality", "quality" }
      };

      string send = JsonConvert.SerializeObject(es);

      python.StandardInput.WriteLine(send);
      python.StandardInput.Flush();
      JObject result = JObject.Parse(python.StandardOutput.ReadLine());

      string exception = !String.IsNullOrWhiteSpace((string)result["exception"]) ? (string)result["exception"] : String.Empty;

      // get return values
      IEnumerable<bool> cases = result["cases"] != null
                              ? cases = result["cases"].Select(x => (bool)x)
                              : cases = Enumerable.Repeat(false, indices.Count());

      IEnumerable<double> caseQualities = result["caseQualities"] != null
                                        ? caseQualities = result["caseQualities"].Select(x => (double)x)
                                        : caseQualities = new List<double>();

      double quality = result["quality"] != null
                     ? (double)result["quality"]
                     : Double.MaxValue;

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string>(cases, caseQualities, quality, exception);
    }
  }
}

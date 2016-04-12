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
using System.Linq;
using HeuristicLab.Data;
using Newtonsoft.Json.Linq;
namespace HeuristicLab.Problems.CFG.Python {
  public class PythonProcessHelper {

    public static Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, StringArray input, StringArray output, IEnumerable<int> indices, double timeout = 1) {
      return EvaluateProgram(program, PythonHelper.ConvertToPythonValues(input, indices), PythonHelper.ConvertToPythonValues(output, indices), indices, timeout);
    }

    public static Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, string input, string output, IEnumerable<int> indices, double timeout = 1) {
      EvaluationScript es = CreateEvaluationScript(program, input, output, timeout);
      JObject json = PythonProcess.GetInstance().SendAndEvaluateProgram(es);
      return GetVariablesFromJson(json, indices.Count());
    }

    protected static EvaluationScript CreateEvaluationScript(string program, string input, string output, double timeout) {
      return new EvaluationScript() {
        Script = String.Format("inval = {0}\noutval = {1}\n{2}", input, output, program),
        Variables = new List<string>() { "cases", "caseQuality", "quality" },
        Timeout = timeout
      };
    }

    protected static Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> GetVariablesFromJson(JObject json, int numberOfCases) {
      string exception = !String.IsNullOrWhiteSpace((string)json["exception"]) ? (string)json["exception"] : String.Empty;

      // get return values
      IEnumerable<bool> cases = json["cases"] != null
                              ? cases = json["cases"].Select(x => (bool)x)
                              : cases = Enumerable.Repeat(false, numberOfCases);

      IEnumerable<double> caseQualities = json["caseQualities"] != null
                                        ? caseQualities = json["caseQualities"].Select(x => (double)x)
                                        : caseQualities = new List<double>();

      double quality = json["quality"] == null || Double.IsInfinity((double)json["quality"])
                     ? Double.MaxValue
                     : (double)json["quality"];

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string>(cases, caseQualities, quality, exception);
    }
  }
}

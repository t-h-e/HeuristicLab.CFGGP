#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python {
  public class PythonProcess {
    private static Process python;
    private static PythonProcess instance;

    private PythonProcess(string pathToPython, string pythonArguments) {
      python = new Process {
        StartInfo = new ProcessStartInfo {
          FileName = pathToPython,
          Arguments = pythonArguments + "python_script_evaluation.py",
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          CreateNoWindow = true
        }
      };
      python.Start();
    }

    public static PythonProcess GetInstance(string pathToPython = "python", string pythonArguments = "") {
      if (instance == null) {
        instance = new PythonProcess(pathToPython, pythonArguments);
      }
      return instance;
    }

    public bool SetNewPythonPathOrArguments(string pathToPython = "python", string pythonArguments = "") {
      lock (python) {
        python.Kill();
        python = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = pathToPython,
            Arguments = pythonArguments + " python_script_evaluation.py",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
          }
        };
        return python.Start();
      }
    }

    public JObject SendAndEvaluateProgram(EvaluationScript es) {
      lock (python) {
        string send = JsonConvert.SerializeObject(es);

        python.StandardInput.WriteLine(send);
        python.StandardInput.Flush();
        return JObject.Parse(python.StandardOutput.ReadLine());
      }
    }
  }
}

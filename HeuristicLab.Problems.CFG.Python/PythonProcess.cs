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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python {
  public class PythonProcess : IDisposable {
    private const string EVALSCRIPT = "python_script_evaluation.py";

    private static Process python;
    private static PythonProcess instance;

    private PythonProcess() {
      SetNewPythonPathOrArguments();
    }

    public static PythonProcess GetInstance() {
      if (instance == null) {
        instance = new PythonProcess();
      }
      return instance;
    }

    public bool SetNewPythonPathOrArguments(string pathToPython = "python", string pythonArguments = "") {
      CheckIfResourceIsNewer(EVALSCRIPT);

      if (python != null) python.Kill();
      python = new Process {
        StartInfo = new ProcessStartInfo {
          FileName = pathToPython,
          Arguments = String.Format("{0} {1}", pythonArguments, EVALSCRIPT),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          CreateNoWindow = true
        }
      };
      try {
        return python.Start();
      }
      catch (Win32Exception) {
        python = null;
      }
      catch (InvalidOperationException) {
        python = null;
      }
      return false;
    }

    public JObject SendAndEvaluateProgram(EvaluationScript es) {
      if (python == null) { throw new ArgumentException("No python process has been started."); }
      lock (python) {
        if (python.HasExited) { throw new ArgumentException("Python process has already exited."); }
        try {
          string send = JsonConvert.SerializeObject(es);
          python.StandardInput.WriteLine(send);
          python.StandardInput.Flush();
          return JObject.Parse(python.StandardOutput.ReadLine());
        }
        catch (JsonReaderException e) {
          JObject json = new JObject();
          json.Add("exception", new JValue(e.Message));
          return json;
        }
      }
    }

    private void CheckIfResourceIsNewer(string scriptName) {
      Assembly assembly = GetType().Assembly;
      if (File.Exists(scriptName) && File.GetLastWriteTime(scriptName) >= File.GetLastWriteTime(assembly.Location)) return;

      Stream scriptStream = assembly.GetManifestResourceStream(String.Format("{0}.{1}", GetType().Namespace, scriptName));
      using (var fileStream = File.Create(scriptName)) {
        scriptStream.CopyTo(fileStream);
      }
    }

    public void Dispose() {
      python.Dispose();
    }
  }
}

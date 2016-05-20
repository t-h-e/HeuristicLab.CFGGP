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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.Problems.CFG.Python {
  [Item("Python Process", "Item that runs a Python process")]
  [StorableClass]
  public class PythonProcess : NamedItem, IDisposable {
    private const string EVALSCRIPT = "python_script_evaluation.py";

    private const string CASES = "cases";
    private const string CASEQUALITY = "caseQuality";
    private const string QUALITY = "quality";


    #region Fields & Properties
    private Object pythonLock = new Object();
    /// <summary>
    /// Process should normally be closed, but the process created closes itself automatically if HeuristicLab is closed
    /// </summary>
    private Process python;
    [Storable]
    private string executable;
    public string Executable {
      get { return executable; }
      set {
        if (value == executable) return;
        executable = value;
        UpdateName();
        TestPythonStart();
        OnExecutableChanged();
      }
    }
    [Storable]
    private string arguments;
    public string Arguments {
      get { return arguments; }
      set {
        if (value == arguments) return;
        arguments = value;
        UpdateName();
        TestPythonStart();
        OnArgumentsChanged();
      }
    }
    #endregion

    private bool HasPythonBeenStartedBefore = false;

    [StorableConstructor]
    protected PythonProcess(bool deserializing) : base(deserializing) { }
    protected PythonProcess(PythonProcess original, Cloner cloner)
      : base(original, cloner) {
      executable = original.executable;
      arguments = original.arguments;
      UpdateName();
      python = CreatePythonProcess();
    }

    public PythonProcess() : this("python", String.Empty) { }
    public PythonProcess(string executable, string arguments)
      : base() {
      this.executable = executable;
      this.arguments = arguments;
      UpdateName();
      this.python = CreatePythonProcess();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new PythonProcess(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      UpdateName();
      lock (pythonLock) {
        python = CreatePythonProcess();
      }
    }

    private Process CreatePythonProcess() {
      return new Process {
        StartInfo = new ProcessStartInfo {
          FileName = executable,
          Arguments = String.Format("{0} {1}", arguments, EVALSCRIPT),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardInput = true,
          CreateNoWindow = true,
        }
      };
    }

    public bool TestPythonStart() {
      CheckIfResourceIsNewer(EVALSCRIPT);

      lock (pythonLock) {
        if (python != null && HasPythonBeenStartedBefore && !python.HasExited) python.Kill();
        python = CreatePythonProcess();
        try {
          python.Start();
          OnProcessStarted();
          python.Kill();

          // reset flag after testing if the process can be started
          HasPythonBeenStartedBefore = false;
          return true;
        } catch (Win32Exception ex) {
          python = null;
          OnProcessException(ex);
        } catch (InvalidOperationException ex) {
          python = null;
          OnProcessException(ex);
        }
      }
      return false;
    }

    public void StartPython() {
      CheckIfResourceIsNewer(EVALSCRIPT);

      lock (pythonLock) {
        if (python != null && HasPythonBeenStartedBefore && !python.HasExited) python.Kill();
        python = CreatePythonProcess();
        try {
          python.Start();
          OnProcessStarted();

          // Process has been started
          HasPythonBeenStartedBefore = true;
        } catch (Win32Exception ex) {
          python = null;
          OnProcessException(ex);
        } catch (InvalidOperationException ex) {
          python = null;
          OnProcessException(ex);
        }
      }
    }

    private void UpdateName() {
      name = string.Format("Process {0} {1}", Path.GetFileNameWithoutExtension(executable), arguments);
      OnNameChanged();
    }

    #region Events
    public event EventHandler ExecutableChanged;
    protected void OnExecutableChanged() {
      EventHandler handler = ExecutableChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    public event EventHandler ArgumentsChanged;
    protected void OnArgumentsChanged() {
      EventHandler handler = ArgumentsChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    public event EventHandler ProcessStarted;
    private void OnProcessStarted() {
      EventHandler handler = ProcessStarted;
      if (handler != null) handler(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs<Exception>> ProcessException;
    private void OnProcessException(Exception e) {
      EventHandler<EventArgs<Exception>> handler = ProcessException;
      if (handler != null) handler(this, new EventArgs<Exception>(e));
    }
    #endregion


    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, StringArray input, StringArray output, IEnumerable<int> indices, double timeout = 1) {
      return EvaluateProgram(program, PythonHelper.ConvertToPythonValues(input, indices), PythonHelper.ConvertToPythonValues(output, indices), indices, timeout);
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> EvaluateProgram(string program, string input, string output, IEnumerable<int> indices, double timeout = 1) {
      EvaluationScript es = CreateEvaluationScript(program, input, output, timeout);
      JObject json = SendAndEvaluateProgram(es);
      return GetVariablesFromJson(json, indices.Count());
    }

    public EvaluationScript CreateEvaluationScript(string program, string input, string output, double timeout) {
      return new EvaluationScript() {
        Script = String.Format("inval = {1}{0}outval = {2}{0}{3}", Environment.NewLine, input, output, program),
        Variables = new List<string>() { CASES, CASEQUALITY, QUALITY },
        Timeout = timeout
      };
    }

    public Tuple<IEnumerable<bool>, IEnumerable<double>, double, string> GetVariablesFromJson(JObject json, int numberOfCases) {
      string exception = !String.IsNullOrWhiteSpace((string)json["exception"]) ? (string)json["exception"] : String.Empty;

      // get return values
      IEnumerable<bool> cases = json[CASES] != null
                              ? cases = json[CASES].Select(x => (bool)x)
                              : cases = Enumerable.Repeat(false, numberOfCases);

      IEnumerable<double> caseQualities = json[CASEQUALITY] != null
                                        ? caseQualities = json[CASEQUALITY].Select(x => (double)x)
                                        : caseQualities = new List<double>();

      double quality = json[QUALITY] == null || Double.IsInfinity((double)json[QUALITY])
                     ? Double.MaxValue
                     : (double)json[QUALITY];

      return new Tuple<IEnumerable<bool>, IEnumerable<double>, double, string>(cases, caseQualities, quality, exception);
    }

    private ConcurrentDictionary<string, ManualResetEventSlim> waitDict = new ConcurrentDictionary<string, ManualResetEventSlim>();
    private ConcurrentDictionary<string, JObject> resultDict = new ConcurrentDictionary<string, JObject>();

    public JObject SendAndEvaluateProgram(EvaluationScript es) {
      ManualResetEventSlim wh = new ManualResetEventSlim(false);
      es.Id = Guid.NewGuid().ToString();
      waitDict.TryAdd(es.Id, wh);
      string send = JsonConvert.SerializeObject(es);
      lock (pythonLock) {
        if (!HasPythonBeenStartedBefore && python != null) {
          StartPython();
        }
        if (python == null || python.HasExited) {
          ArgumentException e = new ArgumentException(python == null ? "No python process has been started." : "Python process has already exited.");
          OnProcessException(e);
          throw e;
        }

        python.StandardInput.WriteLine(send);
        python.StandardInput.Flush();
      }

      IncrementPythonWait();

      JObject res;
      if (wh.WaitHandle.WaitOne((int)(es.Timeout * 1000 * 30))) {
        if (!resultDict.TryRemove(es.Id, out res)) {
          res = new JObject();
          res.Add("exception", new JValue("Something went wrong."));
          Console.WriteLine("JSON sent (Something went wrong.):");
          Console.WriteLine(send);
        }
      } else {
        res = new JObject();
        res.Add("exception", new JValue("Timeout while waiting for python."));
        Console.WriteLine("JSON sent (Timeout while waiting for python.):");
        Console.WriteLine(send);
      }

      waitDict.TryRemove(es.Id, out wh);
      return res;
    }

    private Object readerThreadLock = new Object();
    private Thread readerThread;

    private void IncrementPythonWait() {
      lock (readerThreadLock) {
        if (readerThread == null || !readerThread.IsAlive) {
          lock (deadFlagLock) { deadFlag = false; }
          readerThread = new Thread(this.ReadPythonOutput);
          readerThread.Start();
        } else {
          readCounterSemaphore.Release();
          lock (deadFlagLock) {
            if (deadFlag && readCounterSemaphore.CurrentCount != 0) {
              readCounterSemaphore = new SemaphoreSlim(0, int.MaxValue);
              deadFlag = false;
              readerThread.Join();
              readerThread = new Thread(this.ReadPythonOutput);
              readerThread.Start();
            }
          }
        }
      }
    }

    object deadFlagLock = new Object();
    bool deadFlag = false;
    SemaphoreSlim readCounterSemaphore = new SemaphoreSlim(0, int.MaxValue);

    private static string idInJSONPattern = @"\""id\""\s*:\s*\""(?<id>[\S]*?)\""";
    private static Regex idInJSONRegex = new Regex(idInJSONPattern);

    private void ReadPythonOutput() {

      do {
        // not locked, otherwise there is no concurrency
        // only one thread should read StandardOutput
        // should only be read when python process is running and is not being restarted
        string readJSON = null;
        try {
          readJSON = python.StandardOutput.ReadLine();
        } catch (InvalidOperationException e) {
          Console.WriteLine("Pipe might have been broken!?");
          Console.WriteLine(e);
          break;
        }

        if (readJSON != null) {
          JObject res = null;
          try {
            res = JObject.Parse(readJSON);
          } catch (JsonReaderException e) {
            Match idMatch = idInJSONRegex.Match(readJSON);
            if (idMatch.Success) {
              res = new JObject();
              res["id"] = idMatch.Groups["id"].Value;
              res["exception"] = e.Message;
            }
          }

          if (res != null) {
            string id = res["id"].Value<string>();
            resultDict.TryAdd(id, res);

            ManualResetEventSlim wh;
            if (waitDict.TryGetValue(id, out wh)) {
              wh.Set();
            }
          }
        }

        bool wait = !readCounterSemaphore.Wait(10000);
        if (wait) {
          lock (deadFlagLock) {
            if (readCounterSemaphore.CurrentCount == 0) {
              deadFlag = true;
            } else {
              readCounterSemaphore.Wait();
            }
          }
        }
      } while (!deadFlag);

      // clear all results that have been read from python, but not been processed due to a timeout
      resultDict.Clear();
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
      python.Kill();
      python.Dispose();
    }
  }
}

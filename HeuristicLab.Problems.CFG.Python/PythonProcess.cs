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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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


    private EvaluationThreadPool evaluationThreadPool;

    #region Fields & Properties
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

    [StorableConstructor]
    protected PythonProcess(bool deserializing) : base(deserializing) { }
    protected PythonProcess(PythonProcess original, Cloner cloner)
      : base(original, cloner) {
      executable = original.executable;
      arguments = original.arguments;
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, Environment.ProcessorCount);
    }

    public PythonProcess() : this("python", String.Empty) { }
    public PythonProcess(string executable, string arguments)
      : base() {
      this.executable = executable;
      this.arguments = arguments;
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, Environment.ProcessorCount);
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new PythonProcess(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, Environment.ProcessorCount);
    }

    public bool TestPythonStart() {
      CheckIfResourceIsNewer(EVALSCRIPT);
      Process python = CreatePythonProcess(executable, arguments);
      try {
        python.Start();
        OnProcessStarted();
        python.Kill();
        return true;
      } catch (Win32Exception ex) {
        OnProcessException(ex);
      } catch (InvalidOperationException ex) {
        OnProcessException(ex);
      }
      return false;
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

    public JObject SendAndEvaluateProgram(EvaluationScript es) {
      ManualResetEventSlim wh = new ManualResetEventSlim(false);
      es.Id = Guid.NewGuid().ToString();
      string send = JsonConvert.SerializeObject(es);

      var et = new EvalTask(send, wh);

      evaluationThreadPool.EnqueueTask(et);

      wh.Wait();

      return et.Result;
    }

    private static void CheckIfResourceIsNewer(string scriptName) {
      Assembly assembly = typeof(PythonProcess).Assembly;
      if (File.Exists(scriptName) && File.GetLastWriteTime(scriptName) >= File.GetLastWriteTime(assembly.Location)) return;

      Stream scriptStream = assembly.GetManifestResourceStream(String.Format("{0}.{1}", typeof(PythonProcess).Namespace, scriptName));
      using (var fileStream = File.Create(scriptName)) {
        scriptStream.CopyTo(fileStream);
      }
    }

    public void Dispose() {
      evaluationThreadPool.Dispose();
    }

    private static Process CreatePythonProcess(string executable, string arguments) {
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

    private class EvalTask {
      public ManualResetEventSlim WaitHandle { get; }
      public string EvalString { get; }

      public JObject Result { get; set; }

      public EvalTask(string EvalString, ManualResetEventSlim WaitHandle) {
        this.EvalString = EvalString;
        this.WaitHandle = WaitHandle;
      }
    }

    private class EvaluationThreadPool : IDisposable {
      readonly object _locker = new object();
      readonly List<Thread> _workers;
      readonly Queue<EvalTask> _taskQueue = new Queue<EvalTask>();
      readonly int workerCount;

      readonly object hasBeenStarted_locker = new object();
      private bool hasBeenStarted;

      private string executable;
      private string arguments;

      private static string idInJSONPattern = @"\""id\""\s*:\s*\""(?<id>[\S]*?)\""";
      private static Regex idInJSONRegex = new Regex(idInJSONPattern);

      public EvaluationThreadPool(string executable, string arguments, int workerCount) {
        CheckIfResourceIsNewer(EVALSCRIPT);
        this.executable = executable;
        this.arguments = arguments;
        this.workerCount = workerCount;
        _workers = new List<Thread>(workerCount);

        hasBeenStarted = false;
      }

      public void EnqueueTask(EvalTask task) {
        lock (hasBeenStarted_locker) {
          if (!hasBeenStarted) {
            for (int i = 0; i < workerCount; i++) {
              Thread t = new Thread(Consume) { IsBackground = true, Name = string.Format("EvaluationThreadPool worker {0}", i) };
              _workers.Add(t);
              t.Start();
            }
            hasBeenStarted = true;
          }
        }

        lock (_locker) {
          _taskQueue.Enqueue(task);
          Monitor.PulseAll(_locker);
        }
      }

      void Consume() {
        Process python = CreatePythonProcess(executable, arguments);
        python.Start();

        while (true) {
          EvalTask item;
          lock (_locker) {
            while (_taskQueue.Count == 0) Monitor.Wait(_locker);
            item = _taskQueue.Dequeue();
          }
          if (item == null) return;

          python.StandardInput.WriteLine(item.EvalString);
          python.StandardInput.Flush();

          Task<string> read = null;
          try {
            read = python.StandardOutput.ReadLineAsync();
          } catch (InvalidOperationException e) {
            Console.WriteLine("Pipe might have been broken!?");
            Console.WriteLine(e);
            break;
          }

          JObject res = null;

          if (!read.Wait(30 * 1000)) {
            res = new JObject();
            res["exception"] = "Timeout while waiting for python to return";
            item.Result = res;
            item.WaitHandle.Set();
            // read not was successfull
            Console.WriteLine("I killed a process and i liked it.");
            python.Kill();
            python.Dispose();
            python = CreatePythonProcess(executable, arguments);
            python.Start();
            continue;
          }

          // read was successfull
          string readJSON = read.Result;
          try {
            res = JObject.Parse(readJSON);
          } catch (JsonReaderException e) {
            Console.WriteLine("JsonReaderException, received:");
            Console.WriteLine(readJSON);
            Match idMatch = idInJSONRegex.Match(readJSON);
            if (idMatch.Success) {
              res = new JObject();
              res["id"] = idMatch.Groups["id"].Value;
              res["exception"] = e.Message;
            } else {
              Console.WriteLine("Could not find id in read value");
            }
          }

          item.Result = res;
          item.WaitHandle.Set();
        }
      }

      public void Dispose() {
        _workers.ForEach(thread => EnqueueTask(null));
        _workers.ForEach(thread => thread.Join());
      }
    }
  }
}

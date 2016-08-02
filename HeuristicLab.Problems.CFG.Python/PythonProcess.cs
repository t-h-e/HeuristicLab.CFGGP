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

//#define LOG_COMMUNICATION

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        if (evaluationThreadPool != null) { evaluationThreadPool.Dispose(); }
        evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
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
        if (evaluationThreadPool != null) { evaluationThreadPool.Dispose(); }
        evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
      }
    }
    [Storable]
    private int degreeOfParallelism;
    public int DegreeOfParallelism {
      get { return degreeOfParallelism; }
      set {
        if (value == degreeOfParallelism) return;
        degreeOfParallelism = value;
        if (evaluationThreadPool != null) { evaluationThreadPool.Dispose(); }
        evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
      }
    }
    #endregion

    [StorableConstructor]
    protected PythonProcess(bool deserializing) : base(deserializing) { }
    protected PythonProcess(PythonProcess original, Cloner cloner)
      : base(original, cloner) {
      executable = original.executable;
      arguments = original.arguments;
      degreeOfParallelism = original.degreeOfParallelism;
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
    }

    public PythonProcess() : this("python", String.Empty) { }
    public PythonProcess(string executable, string arguments)
      : base() {
      this.executable = executable;
      this.arguments = arguments;
      degreeOfParallelism = -1;
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new PythonProcess(this, cloner);
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      UpdateName();
      evaluationThreadPool = new EvaluationThreadPool(executable, arguments, degreeOfParallelism);
    }

    public bool TestPythonStart() {
      CheckIfResourceIsNewer(EVALSCRIPT);
      Process python = CreatePythonProcess(executable, arguments);
      try {
        python.Start();
        OnProcessStarted();
        python.StandardInput.Close(); // python process terminates by itself if standard input closes
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

    /// <summary>
    /// added evalTaskBag for debugging purposes
    /// </summary>
    private ConcurrentBag<EvalTask> evalTaskBag = new ConcurrentBag<EvalTask>();
    public IEnumerable<string> GetIndidividuals() {
      return evalTaskBag.Select(x => x.EvalString);
    }

    public JObject SendAndEvaluateProgram(EvaluationScript es) {
      ManualResetEventSlim wh = new ManualResetEventSlim(false);
      es.Id = Guid.NewGuid().ToString();
      string send = JsonConvert.SerializeObject(es);

      var et = new EvalTask(send, wh);

      evalTaskBag.Add(et);
      evaluationThreadPool.EnqueueTask(et);

      wh.Wait();
      EvalTask help;
      if (!evalTaskBag.TryTake(out help)) {
        Console.WriteLine("Could not remove item from bag!");
      }

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
      private readonly object _locker = new object();
      private readonly List<Thread> _workers;
      private readonly Queue<EvalTask> _taskQueue = new Queue<EvalTask>();
      private readonly int workerCount;

      private readonly object hasBeenStarted_locker = new object();
      private bool hasBeenStarted;

      private readonly string executable;
      private readonly string arguments;

      public EvaluationThreadPool(string executable, string arguments, int workerCount) {
        CheckIfResourceIsNewer(EVALSCRIPT);
        this.executable = executable;
        this.arguments = arguments;
        this.workerCount = workerCount <= 0
                         ? Environment.ProcessorCount
                         : workerCount;
        _workers = new List<Thread>(this.workerCount);

        lock (hasBeenStarted_locker) {
          hasBeenStarted = false;
        }
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
        python.StandardInput.AutoFlush = true;

        while (true) {
          EvalTask item;
          lock (_locker) {
            while (_taskQueue.Count == 0) Monitor.Wait(_locker);
            item = _taskQueue.Dequeue();
          }
          if (item == null) {
            python.StandardInput.Close(); // python process terminates by itself if standard input closes
            return;
          }
#if LOG_COMMUNICATION
          lock (_locker) { using (StreamWriter file = new StreamWriter(@"HL_log.txt", true)) { file.WriteLine(String.Format("{0} {1} {2}", Thread.CurrentThread.ManagedThreadId, python.Id, item.EvalString)); } }
#endif

          python.StandardInput.WriteLine(item.EvalString);
#if LOG_COMMUNICATION
          lock (_locker) { using (StreamWriter file = new StreamWriter(@"HL_log.txt", true)) { file.WriteLine(String.Format("{0} {1} {2}", Thread.CurrentThread.ManagedThreadId, python.Id, "Sent")); } }
#endif

          string readJSON = python.StandardOutput.ReadLine();
#if LOG_COMMUNICATION
          lock (_locker) { using (StreamWriter file = new StreamWriter(@"HL_log.txt", true)) { file.WriteLine(String.Format("{0} {1} {2}", Thread.CurrentThread.ManagedThreadId, python.Id, "Received")); } }
#endif

          JObject res = null;
          try {
            res = JObject.Parse(readJSON);
          } catch (JsonReaderException e) {
            res = new JObject();
            if (e.Message.StartsWith("JSON integer")) {
              res["exception"] = "JSON integer";
            } else {
              Console.WriteLine(readJSON);
              res["exception"] = e.Message;
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

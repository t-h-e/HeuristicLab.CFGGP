using System.Collections.Generic;
using System.IO;
using HeuristicLab.Problems.CFG.Python;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace HeuristicLab.CFGGP.Tests {
  [TestClass]
  public class PythonEvaluationTest {

    private PythonProcess pp;
    [TestInitialize()]
    public void Initialize() {
      pp = new PythonProcess("python.exe", "");
      pp.DegreeOfParallelism = 1;
    }

    [TestMethod]
    public void PythonProcessStarted() {
      Assert.IsTrue(pp.TestPythonStart());
    }

    [TestMethod]
    public void NormalResult() {
      string script, resultJson;
      using (StreamReader sr = new StreamReader(@"Data\PythonProcessNormal.txt")) {
        script = sr.ReadToEnd();
      }
      EvaluationScript es = new EvaluationScript() {
        Script = script,
        Timeout = 1.0,
        Variables = new List<string>() { "cases", "caseQuality", "quality" }
      };
      var res = pp.SendAndEvaluateProgram(es);

      using (StreamReader sr = new StreamReader(@"Data\NormalResultJson.txt")) {
        resultJson = sr.ReadToEnd();
      }
      var expected = JObject.Parse(resultJson);
      Assert.IsTrue(JObject.EqualityComparer.Equals(expected, res));
    }

    [TestMethod]
    public void TimeoutSimple() {
      EvaluationScript es = new EvaluationScript() {
        Script = @"def fib(x):
    i = 1
    j = 1
    sum = 0
    for _ in range(0, x):
        sum = i + j
        j = i
        i = sum

fib(200000)",
        Timeout = 1.0,
      };
      var res = pp.SendAndEvaluateProgram(es);
      Assert.IsNotNull(res["exception"]);
      Assert.AreEqual("Timeout occurred.", res["exception"]);
    }

    [TestMethod]
    public void TimeoutReal() {
      string script;
      using (StreamReader sr = new StreamReader(@"Data\PythonProcessTimeout.txt")) {
        script = sr.ReadToEnd();
      }
      EvaluationScript es = new EvaluationScript() {
        Script = script,
        Timeout = 1.0,
      };
      var res = pp.SendAndEvaluateProgram(es);
      Assert.IsNotNull(res["exception"]);
      Assert.AreEqual("Timeout occurred.", res["exception"]);
    }

    [TestMethod]
    public void Exception() {
      EvaluationScript es = new EvaluationScript() {
        Script = "raise ValueError('this is an exception')",
        Timeout = 1.0,
      };
      pp.SendAndEvaluateProgram(es);
    }
  }
}

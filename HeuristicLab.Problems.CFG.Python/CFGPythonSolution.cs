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
using System.Drawing;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  /// <summary>
  /// CFG solution
  /// </summary>
  [StorableClass]
  [Item(Name = "CFGSolution", Description = "Represents a context free grammar solution and attributes of the solution like accuracy and complexity.")]
  public class CFGPythonSolution : ResultCollection, INamedItem, IStorableContent {
    private const string ModelLengthResultName = "Model Length";
    private const string ModelDepthResultName = "Model Depth";

    private const string ModelResultName = "Model";
    private const string CodeResultName = "Code";
    private const string ProgramResultName = "Program";

    private const string TrainingException = "Training Exception";
    private const string TrainingQuality = "Training Quality";
    private const string TrainingSolvedCases = "Training Solved Cases";
    private const string TrainingSolvedCasesPercentage = "Training Solved Cases Percentage";

    private const string TestException = "Test Exception";
    private const string TestQuality = "Test Quality";
    private const string TestSolvedCases = "Test Solved Cases";
    private const string TestSolvedCasesPercentage = "Test Solved Cases Percentage";

    public string Filename { get; set; }

    public static new Image StaticItemImage {
      get { return HeuristicLab.Common.Resources.VSImageLibrary.Function; }
    }

    [StorableConstructor]
    protected CFGPythonSolution(bool deserializing) : base(deserializing) { }
    protected CFGPythonSolution(CFGPythonSolution original, Cloner cloner)
      : base(original, cloner) {
      name = original.Name;
      description = original.Description;
    }
    public CFGPythonSolution(ISymbolicExpressionTree tree, ICFGProblemData problemData, IntValue timeout, StringValue header = null, StringValue footer = null)
      : base() {
      name = ItemName;
      description = ItemDescription;

      Add(new Result(ModelLengthResultName, "Length of the symbolic regression model.", new IntValue(tree.Length)));
      Add(new Result(ModelDepthResultName, "Depth of the symbolic regression model.", new IntValue(tree.Depth)));

      Add(new Result(ModelResultName, "The cfg model.", tree));
      string program = PythonHelper.FormatToProgram(tree, header, footer);
      Add(new Result(ProgramResultName, "The program with header and footer", new TextValue(program)));
      string code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
      Add(new Result(CodeResultName, "The code that was evolved", new TextValue(code)));

      var training = PythonHelper.EvaluateProgram(program, problemData.Input, problemData.Output, problemData.TrainingIndices, timeout.Value);
      var test = PythonHelper.EvaluateProgram(program, problemData.Input, problemData.Output, problemData.TestIndices, timeout.Value);

      if (String.IsNullOrEmpty(training.Item3)) {
        Add(new Result(TrainingQuality, "Training quality", new DoubleValue(training.Item2)));
        var cases = training.Item1.ToArray();
        Add(new Result(TrainingSolvedCases, "Training cases which have been solved", new BoolArray(cases)));
        Add(new Result(TrainingSolvedCasesPercentage, "Percentage of training cases which have been solved", new PercentValue((double)cases.Count(x => x) / (double)cases.Length)));
      } else {
        Add(new Result(TrainingException, "Exception occured during training", new TextValue(training.Item3)));
      }

      if (String.IsNullOrEmpty(test.Item3)) {
        Add(new Result(TestQuality, "Test quality", new DoubleValue(test.Item2)));
        var cases = test.Item1.ToArray();
        Add(new Result(TestSolvedCases, "Test cases which have been solved", new BoolArray(cases)));
        Add(new Result(TestSolvedCasesPercentage, "Percentage of test cases which have been solved", new PercentValue((double)cases.Count(x => x) / (double)cases.Length)));
      } else {
        Add(new Result(TestException, "Exception occured during test", new TextValue(test.Item3)));
      }
    }

    #region INamedItem Members
    [Storable]
    protected string name;
    public string Name {
      get { return name; }
      set {
        if (!CanChangeName) throw new NotSupportedException("Name cannot be changed.");
        if (!(name.Equals(value) || (value == null) && (name == string.Empty))) {
          CancelEventArgs<string> e = value == null ? new CancelEventArgs<string>(string.Empty) : new CancelEventArgs<string>(value);
          OnNameChanging(e);
          if (!e.Cancel) {
            name = value == null ? string.Empty : value;
            OnNameChanged();
          }
        }
      }
    }
    public virtual bool CanChangeName {
      get { return true; }
    }
    [Storable]
    protected string description;
    public string Description {
      get { return description; }
      set {
        if (!CanChangeDescription) throw new NotSupportedException("Description cannot be changed.");
        if (!(description.Equals(value) || (value == null) && (description == string.Empty))) {
          description = value == null ? string.Empty : value;
          OnDescriptionChanged();
        }
      }
    }
    public virtual bool CanChangeDescription {
      get { return true; }
    }

    public override string ToString() {
      return Name;
    }

    public event EventHandler<CancelEventArgs<string>> NameChanging;
    protected virtual void OnNameChanging(CancelEventArgs<string> e) {
      var handler = NameChanging;
      if (handler != null) handler(this, e);
    }

    public event EventHandler NameChanged;
    protected virtual void OnNameChanged() {
      var handler = NameChanged;
      if (handler != null) handler(this, EventArgs.Empty);
      OnToStringChanged();
    }

    public event EventHandler DescriptionChanged;
    protected virtual void OnDescriptionChanged() {
      var handler = DescriptionChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    #endregion
  }
}

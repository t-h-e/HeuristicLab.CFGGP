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
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Misc;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG.Python {
  /// <summary>
  /// An operator that saves all individuals that have thrown an exceptions during the evaluation.
  /// </summary>
  [Item("CFGPythonIndividualExceptionAnalyzer", "An operator that saves all individuals that have thrown an exceptions during the evaluation.")]
  [StorableClass]
  public class CFGPythonIndividualExceptionAnalyzer : SingleSuccessorOperator, ICFGAnalyzer<ICFGPythonProblemData> {
    private const string ExceptionTreeParameterName = "Exception";
    private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";
    private const string ProblemDataParameterName = "ProblemData";
    private const string IndividualExceptionCollectionParameterName = "IndividualExceptionCollection";
    private const string ResultCollectionParameterName = "Results";
    private const string ExcludeTimeoutParameterName = "ExcludeTimeout";
    private const string IncludeTreeParameterName = "IncludeTree";

    #region parameter properties
    public IScopeTreeLookupParameter<StringValue> ExceptionTreeParameter {
      get { return (IScopeTreeLookupParameter<StringValue>)Parameters[ExceptionTreeParameterName]; }
    }
    public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymbolicExpressionTreeParameter {
      get { return (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName]; }
    }
    public ILookupParameter<ICFGPythonProblemData> ProblemDataParameter {
      get { return (ILookupParameter<ICFGPythonProblemData>)Parameters[ProblemDataParameterName]; }
    }
    public ILookupParameter<ItemCollection<BrokenIndividual>> IndividualExceptionCollectionParameter {
      get { return (ILookupParameter<ItemCollection<BrokenIndividual>>)Parameters[IndividualExceptionCollectionParameterName]; }
    }
    public ILookupParameter<ResultCollection> ResultCollectionParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters[ResultCollectionParameterName]; }
    }
    public IValueParameter<BoolValue> ExcludeTimeoutParameter {
      get { return (IValueParameter<BoolValue>)Parameters[ExcludeTimeoutParameterName]; }
    }
    public IValueParameter<BoolValue> IncludeTreeParameter {
      get { return (IValueParameter<BoolValue>)Parameters[IncludeTreeParameterName]; }
    }
    #endregion

    #region properties
    public virtual bool EnabledByDefault {
      get { return false; }
    }
    public ICFGPythonProblemData ProblemData {
      get { return ProblemDataParameter.ActualValue; }
    }
    public ResultCollection ResultCollection {
      get { return ResultCollectionParameter.ActualValue; }
    }
    public ItemCollection<BrokenIndividual> IndividualExceptionCollection {
      get { return IndividualExceptionCollectionParameter.ActualValue; }
      set { IndividualExceptionCollectionParameter.ActualValue = value; }
    }
    public bool ExcludeTimeout {
      get { return ExcludeTimeoutParameter.Value.Value; }
    }
    public bool IncludeTree {
      get { return IncludeTreeParameter.Value.Value; }
    }
    #endregion

    [StorableConstructor]
    protected CFGPythonIndividualExceptionAnalyzer(bool deserializing) : base(deserializing) { }
    protected CFGPythonIndividualExceptionAnalyzer(CFGPythonIndividualExceptionAnalyzer original, Cloner cloner)
      : base(original, cloner) {
    }
    public CFGPythonIndividualExceptionAnalyzer()
      : base() {
      Parameters.Add(new ScopeTreeLookupParameter<StringValue>(ExceptionTreeParameterName, "The symbolic expression trees to analyze."));
      Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, "The symbolic expression trees that should be analyzed."));
      Parameters.Add(new LookupParameter<ICFGPythonProblemData>(ProblemDataParameterName, "The problem data on which the context free grammar solution should be evaluated."));
      Parameters.Add(new LookupParameter<ItemCollection<BrokenIndividual>>(IndividualExceptionCollectionParameterName, "Collection of individuals which threw an exception."));
      Parameters.Add(new LookupParameter<ResultCollection>(ResultCollectionParameterName, "The result collection where the exception frequencies should be stored."));
      Parameters.Add(new ValueParameter<BoolValue>(ExcludeTimeoutParameterName, "Parameter defines if individuals which only threw a timeout exception should be excluded.", new BoolValue(true)));
      Parameters.Add(new ValueParameter<BoolValue>(IncludeTreeParameterName, "Parameter defines if the symbolic expression tree should be included. Set to false, to save space.", new BoolValue(true)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new CFGPythonIndividualExceptionAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      var results = ResultCollection;
      if (IndividualExceptionCollection == null) {
        IndividualExceptionCollection = new ItemCollection<BrokenIndividual>();
        results.Add(new Result("Individual exception collection", IndividualExceptionCollection));
      }

      string[] exceptions = ExceptionTreeParameter.ActualValue.Select(x => x.Value).ToArray();
      ISymbolicExpressionTree[] tree = SymbolicExpressionTreeParameter.ActualValue.ToArray();
      for (int i = 0; i < tree.Length; i++) {
        if ((!String.IsNullOrEmpty(exceptions[i]) && exceptions[i] != "Timeout occurred.")
          || (exceptions[i] == "Timeout occurred." && !ExcludeTimeout)) {
          IndividualExceptionCollection.Add(CreateBrokenIndividual(tree[i], exceptions[i], IncludeTree));
        }
      }
      return base.Apply();
    }

    public BrokenIndividual CreateBrokenIndividual(ISymbolicExpressionTree tree, string exception, bool includeTree) {
      return new BrokenIndividual(tree, exception, includeTree, ProblemData);
    }

    [StorableClass]
    [Item(Name = "Broken Individual", Description = "An individual that threw an exception during evaluation.")]
    public class BrokenIndividual : ResultCollection, INamedItem, IStorableContent {
      private const string ProgramResultName = "Program";
      private const string CodeResultName = "Code";
      private const string ExceptiontName = "Exception";

      private const string ModelLengthResultName = "Model Length";
      private const string ModelDepthResultName = "Model Depth";
      private const string ModelResultName = "Model";

      public string Filename { get; set; }

      [StorableConstructor]
      protected BrokenIndividual(bool deserializing) : base(deserializing) { }

      protected BrokenIndividual(BrokenIndividual original, Cloner cloner) : base(original, cloner) {
        name = original.Name;
        description = original.Description;
      }

      public BrokenIndividual(ISymbolicExpressionTree tree, string exception, bool includeTree, ICFGPythonProblemData problemData) {
        name = String.Format("{0}: {1}", ItemName, exception);
        description = ItemDescription;
        Add(new Result(ModelLengthResultName, "Length of the symbolic regression model.", new IntValue(tree.Length)));
        Add(new Result(ModelDepthResultName, "Depth of the symbolic regression model.", new IntValue(tree.Depth)));
        if (includeTree) {
          Add(new Result(ModelResultName, "The CFG model.", tree));
        }
        Add(new Result(ExceptiontName, "The CFG model.", new StringValue(exception)));

        string program = PythonHelper.FormatToProgram(tree, problemData.LoopBreakConst, problemData.FullHeader, problemData.FullFooter);
        Add(new Result(ProgramResultName, "The program with header and footer", new TextValue(program)));
        string code = CFGSymbolicExpressionTreeStringFormatter.StaticFormat(tree);
        Add(new Result(CodeResultName, "The code that was evolved", new TextValue(code)));
      }

      public override IDeepCloneable Clone(Cloner cloner) {
        return new BrokenIndividual(this, cloner);
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
}

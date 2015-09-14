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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.CFG {
  /// <summary>
  /// CFG solution
  /// </summary>
  [StorableClass]
  [Item(Name = "CFGSolution", Description = "Represents a context free grammar solution and attributes of the solution like accuracy and complexity.")]
  public class CFGSolution : ResultCollection, INamedItem, IStorableContent {
    private const string ModelLengthResultName = "Model Length";
    private const string ModelDepthResultName = "Model Depth";

    private const string ModelResultName = "Model";

    public string Filename { get; set; }

    public static new Image StaticItemImage {
      get { return HeuristicLab.Common.Resources.VSImageLibrary.Function; }
    }

    [StorableConstructor]
    protected CFGSolution(bool deserializing) : base(deserializing) { }
    protected CFGSolution(CFGSolution original, Cloner cloner)
      : base(original, cloner) {
      name = original.Name;
      description = original.Description;
    }
    public CFGSolution(ISymbolicExpressionTree tree, ICFGProblemData problemData)
      : base() {
      name = ItemName;
      description = ItemDescription;

      Add(new Result(ModelLengthResultName, "Length of the symbolic regression model.", new IntValue(tree.Length)));
      Add(new Result(ModelDepthResultName, "Depth of the symbolic regression model.", new IntValue(tree.Depth)));

      Add(new Result(ModelResultName, "The CFG model.", tree));
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

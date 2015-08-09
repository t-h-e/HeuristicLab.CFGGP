#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.Windows.Forms;
using HeuristicLab.MainForm;
using HeuristicLab.MainForm.WindowsForms;

namespace HeuristicLab.Misc.Views {
  [View("TextValue View")]
  [Content(typeof(TextValue), true)]
  public partial class TextValueView : AsynchronousContentView {

    public new TextValue Content {
      get { return (TextValue)base.Content; }
      set { base.Content = value; }
    }

    public override bool ReadOnly {
      get {
        if ((Content != null) && Content.ReadOnly) return true;
        return base.ReadOnly;
      }
      set { base.ReadOnly = value; }
    }

    public bool LabelVisible {
      get { return !splitContainer.Panel1Collapsed; }
      set { splitContainer.Panel1Collapsed = !value; }
    }

    public TextValueView() {
      InitializeComponent();
      errorProvider.SetIconAlignment(valueTextBox, ErrorIconAlignment.MiddleLeft);
      errorProvider.SetIconPadding(valueTextBox, 2);
    }

    protected override void DeregisterContentEvents() {
      Content.ValueChanged -= new EventHandler(Content_ValueChanged);
      base.DeregisterContentEvents();
    }

    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.ValueChanged += new EventHandler(Content_ValueChanged);
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {
        valueTextBox.Text = string.Empty;
      } else
        valueTextBox.Text = Content.Value;
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      valueTextBox.Enabled = Content != null;
      valueTextBox.ReadOnly = ReadOnly;
    }

    protected virtual void Content_ValueChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Content_ValueChanged), sender, e);
      else
        valueTextBox.Text = Content.Value;
    }

    protected virtual void valueTextBox_KeyDown(object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Escape) {
        valueTextBox.Text = Content.Value;
        valueLabel.Select();  // select label to validate data
      } else if (e.Control & e.KeyCode == Keys.A) {
        valueTextBox.SelectAll();
      }
    }
    protected virtual void valueTextBox_Validated(object sender, EventArgs e) {
      if (ReadOnly) return;
      if (!Content.ReadOnly) Content.Value = valueTextBox.Text;
      errorProvider.SetError(valueTextBox, string.Empty);
      valueTextBox.Text = Content.Value;
    }
  }
}

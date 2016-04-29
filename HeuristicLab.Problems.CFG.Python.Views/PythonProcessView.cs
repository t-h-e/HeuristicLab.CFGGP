using System;
using System.Windows.Forms;
using HeuristicLab.Common;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;

namespace HeuristicLab.Problems.CFG.Python.Views {
  [View("Python process View")]
  [Content(typeof(PythonProcess), IsDefaultView = true)]
  public partial class PythonProcessView : NamedItemView {
    public new PythonProcess Content {
      get { return (PythonProcess)base.Content; }
      set { base.Content = value; }
    }

    public PythonProcessView() {
      InitializeComponent();
    }

    protected override void DeregisterContentEvents() {
      Content.ExecutableChanged -= new EventHandler(Content_ExecutableChanged);
      Content.ArgumentsChanged -= new EventHandler(Content_ArgumentsChanged);
      Content.ProcessStarted -= new EventHandler(Content_ProcessStarted);
      Content.ProcessException -= new EventHandler<EventArgs<Exception>>(Content_ProcessException);
      base.DeregisterContentEvents();
    }

    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.ExecutableChanged += new EventHandler(Content_ExecutableChanged);
      Content.ArgumentsChanged += new EventHandler(Content_ArgumentsChanged);
      Content.ProcessStarted += new EventHandler(Content_ProcessStarted);
      Content.ProcessException += new EventHandler<EventArgs<Exception>>(Content_ProcessException);
    }

    #region Event Handlers (Content)
    private void Content_ExecutableChanged(object sender, EventArgs e) {
      executableTextBox.Text = Content.Executable;
    }
    private void Content_ArgumentsChanged(object sender, EventArgs e) {
      argumentsTextBox.Text = Content.Arguments;
    }
    private void Content_ProcessStarted(object sender, EventArgs e) {
      if (InvokeRequired) Invoke(new Action<object, EventArgs>(Content_ProcessStarted), sender, e);
      else {
        SetEnabledStateOfControls();
        statusPictureBox.Image = HeuristicLab.Common.Resources.VSImageLibrary.Default;
        toolTip.SetToolTip(statusPictureBox, string.Empty);
      }
    }
    private void Content_ProcessException(object sender, EventArgs<Exception> e) {
      if (InvokeRequired) Invoke(new Action<object, EventArgs<Exception>>(Content_ProcessException), sender, e);
      else {
        SetEnabledStateOfControls();
        statusPictureBox.Image = HeuristicLab.Common.Resources.VSImageLibrary.Error;
        toolTip.SetToolTip(statusPictureBox, e.Value.Message);
      }
    }
    #endregion

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {
        executableTextBox.Text = String.Empty;
        argumentsTextBox.Text = String.Empty;
      } else {
        executableTextBox.Text = Content.Executable;
        argumentsTextBox.Text = Content.Arguments;
        Content.TestPythonStart(); // Process will be started for testing. This will invoke Content_ProcessStarted or Content_ProcessException;
      }
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      bool readOnlyorNull = ReadOnly || Content == null;
      browseExecutableButton.Enabled = !readOnlyorNull;
      executableTextBox.Enabled = !readOnlyorNull;
      argumentsTextBox.Enabled = !readOnlyorNull;
    }

    #region Event Handlers (child controls)
    private void browseExecutableButton_Click(object sender, EventArgs e) {
      if (openFileDialog.ShowDialog() == DialogResult.OK) {
        try {
          Content.Executable = openFileDialog.FileName;
        }
        catch (InvalidOperationException ex) {
          MessageBox.Show(ex.Message);
        }
      }
    }

    private void executableTextBox_Validated(object sender, EventArgs e) {
      if (Content != null) {
        Content.Executable = executableTextBox.Text;
      }
    }

    private void argumentsTextBox_Validated(object sender, EventArgs e) {
      if (Content != null) {
        Content.Arguments = argumentsTextBox.Text;
      }
    }
    #endregion
  }
}

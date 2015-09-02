namespace HeuristicLab.Misc.Views {
  partial class TextValueView {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.valueTextBox = new System.Windows.Forms.TextBox();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.valueLabel = new System.Windows.Forms.Label();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // valueTextBox
      // 
      this.valueTextBox.AcceptsReturn = true;
      this.valueTextBox.AcceptsTab = true;
      this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.valueTextBox.Location = new System.Drawing.Point(3, 3);
      this.valueTextBox.Multiline = true;
      this.valueTextBox.Name = "valueTextBox";
      this.valueTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.valueTextBox.Size = new System.Drawing.Size(128, 61);
      this.valueTextBox.TabIndex = 2;
      this.valueTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.valueTextBox_KeyDown);
      this.valueTextBox.Validated += new System.EventHandler(this.valueTextBox_Validated);
      // 
      // errorProvider
      // 
      this.errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
      this.errorProvider.ContainerControl = this;
      // 
      // valueLabel
      // 
      this.valueLabel.AutoSize = true;
      this.valueLabel.Location = new System.Drawing.Point(3, 3);
      this.valueLabel.Name = "valueLabel";
      this.valueLabel.Size = new System.Drawing.Size(37, 13);
      this.valueLabel.TabIndex = 1;
      this.valueLabel.Text = "&Value:";
      // 
      // splitContainer
      // 
      this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer.IsSplitterFixed = true;
      this.splitContainer.Location = new System.Drawing.Point(0, 0);
      this.splitContainer.Name = "splitContainer";
      this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      this.splitContainer.Panel1.Controls.Add(this.valueLabel);
      this.splitContainer.Panel1MinSize = 25;
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.valueTextBox);
      this.splitContainer.Size = new System.Drawing.Size(134, 96);
      this.splitContainer.SplitterDistance = 25;
      this.splitContainer.SplitterWidth = 1;
      this.splitContainer.TabIndex = 0;
      this.splitContainer.TabStop = false;
      // 
      // TextValueView
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.splitContainer);
      this.Name = "TextValueView";
      this.Size = new System.Drawing.Size(134, 96);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel1.PerformLayout();
      this.splitContainer.Panel2.ResumeLayout(false);
      this.splitContainer.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    protected System.Windows.Forms.TextBox valueTextBox;
    protected System.Windows.Forms.ErrorProvider errorProvider;
    protected System.Windows.Forms.Label valueLabel;
    protected System.Windows.Forms.SplitContainer splitContainer;
  }
}

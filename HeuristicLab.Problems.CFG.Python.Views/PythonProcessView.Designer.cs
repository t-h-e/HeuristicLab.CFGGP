namespace HeuristicLab.Problems.CFG.Python.Views {
  partial class PythonProcessView {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PythonProcessView));
      this.executableTextBox = new System.Windows.Forms.TextBox();
      this.argumentsLabel = new System.Windows.Forms.Label();
      this.argumentsTextBox = new System.Windows.Forms.TextBox();
      this.executableLabel = new System.Windows.Forms.Label();
      this.browseExecutableButton = new System.Windows.Forms.Button();
      this.statusPictureBox = new System.Windows.Forms.PictureBox();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.statusPictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // nameTextBox
      // 
      this.errorProvider.SetIconAlignment(this.nameTextBox, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
      this.errorProvider.SetIconPadding(this.nameTextBox, 2);
      this.nameTextBox.Location = new System.Drawing.Point(72, 0);
      this.nameTextBox.Size = new System.Drawing.Size(266, 20);
      // 
      // infoLabel
      // 
      this.infoLabel.Location = new System.Drawing.Point(350, 3);
      // 
      // executableTextBox
      // 
      this.executableTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.executableTextBox.Location = new System.Drawing.Point(72, 26);
      this.executableTextBox.Name = "executableTextBox";
      this.executableTextBox.Size = new System.Drawing.Size(266, 20);
      this.executableTextBox.TabIndex = 8;
      this.executableTextBox.Validated += new System.EventHandler(this.executableTextBox_Validated);
      // 
      // argumentsLabel
      // 
      this.argumentsLabel.AutoSize = true;
      this.argumentsLabel.Location = new System.Drawing.Point(3, 55);
      this.argumentsLabel.Name = "argumentsLabel";
      this.argumentsLabel.Size = new System.Drawing.Size(60, 13);
      this.argumentsLabel.TabIndex = 11;
      this.argumentsLabel.Text = "Arguments:";
      // 
      // argumentsTextBox
      // 
      this.argumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.argumentsTextBox.Location = new System.Drawing.Point(72, 52);
      this.argumentsTextBox.Name = "argumentsTextBox";
      this.argumentsTextBox.Size = new System.Drawing.Size(266, 20);
      this.argumentsTextBox.TabIndex = 10;
      this.argumentsTextBox.Validated += new System.EventHandler(this.argumentsTextBox_Validated);
      // 
      // executableLabel
      // 
      this.executableLabel.AutoSize = true;
      this.executableLabel.Location = new System.Drawing.Point(3, 29);
      this.executableLabel.Name = "executableLabel";
      this.executableLabel.Size = new System.Drawing.Size(63, 13);
      this.executableLabel.TabIndex = 12;
      this.executableLabel.Text = "Executable:";
      // 
      // browseExecutableButton
      // 
      this.browseExecutableButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.browseExecutableButton.Image = HeuristicLab.Common.Resources.VSImageLibrary.Open;
      this.browseExecutableButton.Location = new System.Drawing.Point(344, 24);
      this.browseExecutableButton.Name = "browseExecutableButton";
      this.browseExecutableButton.Size = new System.Drawing.Size(26, 23);
      this.browseExecutableButton.TabIndex = 9;
      this.browseExecutableButton.UseVisualStyleBackColor = true;
      this.browseExecutableButton.Click += new System.EventHandler(this.browseExecutableButton_Click);
      // 
      // statusPictureBox
      // 
      this.statusPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.statusPictureBox.Image = HeuristicLab.Common.Resources.VSImageLibrary.Error;
      this.statusPictureBox.Location = new System.Drawing.Point(350, 55);
      this.statusPictureBox.Name = "statusPictureBox";
      this.statusPictureBox.Size = new System.Drawing.Size(17, 17);
      this.statusPictureBox.TabIndex = 13;
      this.statusPictureBox.TabStop = false;
      // 
      // openFileDialog
      // 
      this.openFileDialog.Filter = "Executables|*.exe|All files|*.*";
      this.openFileDialog.Title = "Select the executable of the python process";
      // 
      // PythonProcessView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.statusPictureBox);
      this.Controls.Add(this.executableTextBox);
      this.Controls.Add(this.argumentsLabel);
      this.Controls.Add(this.argumentsTextBox);
      this.Controls.Add(this.executableLabel);
      this.Controls.Add(this.browseExecutableButton);
      this.Name = "PythonProcessView";
      this.Size = new System.Drawing.Size(373, 82);
      this.Controls.SetChildIndex(this.nameLabel, 0);
      this.Controls.SetChildIndex(this.nameTextBox, 0);
      this.Controls.SetChildIndex(this.infoLabel, 0);
      this.Controls.SetChildIndex(this.browseExecutableButton, 0);
      this.Controls.SetChildIndex(this.executableLabel, 0);
      this.Controls.SetChildIndex(this.argumentsTextBox, 0);
      this.Controls.SetChildIndex(this.argumentsLabel, 0);
      this.Controls.SetChildIndex(this.executableTextBox, 0);
      this.Controls.SetChildIndex(this.statusPictureBox, 0);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.statusPictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox executableTextBox;
    private System.Windows.Forms.Label argumentsLabel;
    private System.Windows.Forms.TextBox argumentsTextBox;
    private System.Windows.Forms.Label executableLabel;
    private System.Windows.Forms.Button browseExecutableButton;
    private System.Windows.Forms.PictureBox statusPictureBox;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
  }
}

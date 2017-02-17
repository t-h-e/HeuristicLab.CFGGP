namespace HeuristicLab.Problems.Instances.CFG.Views {
  partial class CFGBenchmarkSuiteProvider {
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
      this.treeCheckBox = new System.Windows.Forms.CheckBox();
      this.numberOfTempVarUpDown = new System.Windows.Forms.NumericUpDown();
      this.numberOfVaribalesLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.SuspendLayout();
      // 
      // instancesComboBox
      // 
      this.instancesComboBox.Location = new System.Drawing.Point(217, 2);
      this.instancesComboBox.Size = new System.Drawing.Size(383, 21);
      // 
      // instanceLabel
      // 
      this.instanceLabel.Location = new System.Drawing.Point(160, 6);
      // 
      // splitContainer2
      // 
      // 
      // splitContainer2.Panel2
      //
      this.splitContainer2.Panel2.Controls.Add(this.treeCheckBox);
      this.splitContainer2.Panel2.Controls.Add(this.numberOfVaribalesLabel);
      this.splitContainer2.Panel2.Controls.Add(this.numberOfTempVarUpDown);
      // 
      // treeCheckBox
      // 
      this.treeCheckBox.AutoSize = true;
      this.treeCheckBox.Location = new System.Drawing.Point(5, 4);
      this.treeCheckBox.Name = "treeCheckBox";
      this.treeCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.treeCheckBox.Size = new System.Drawing.Size(48, 17);
      this.treeCheckBox.TabIndex = 8;
      this.treeCheckBox.Text = "Tree";
      this.treeCheckBox.UseVisualStyleBackColor = true;
      // 
      // numberOfVaribalesLabel
      // 
      this.numberOfVaribalesLabel.AutoSize = true;
      this.numberOfVaribalesLabel.Location = new System.Drawing.Point(59, 5);
      this.numberOfVaribalesLabel.Name = "numberOfVaribalesLabel";
      this.numberOfVaribalesLabel.Size = new System.Drawing.Size(50, 13);
      this.numberOfVaribalesLabel.TabIndex = 10;
      this.numberOfVaribalesLabel.Text = "# of Vars";
      // 
      // numberOfTempVarUpDown
      // 
      //this.numberOfTempVarUpDown.Location = new System.Drawing.Point(114, 1);
      //this.numberOfTempVarUpDown.Name = "textBox1";
      //this.numberOfTempVarUpDown.Size = new System.Drawing.Size(29, 20);
      //this.numberOfTempVarUpDown.TabIndex = 9;
      //this.numberOfTempVarUpDown.Text = "3";
      this.numberOfTempVarUpDown.Location = new System.Drawing.Point(115, 3);
      this.numberOfTempVarUpDown.Name = "numberOfTempVarUpDown";
      this.numberOfTempVarUpDown.Size = new System.Drawing.Size(39, 20);
      this.numberOfTempVarUpDown.TabIndex = 23;
      this.numberOfTempVarUpDown.Value = new decimal(3);
      // 
      // CFGBenchmarkSuiteProvider
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Name = "CFGBenchmarkSuiteProvider";
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      this.splitContainer2.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    protected System.Windows.Forms.CheckBox treeCheckBox;
    protected System.Windows.Forms.NumericUpDown numberOfTempVarUpDown;
    protected System.Windows.Forms.Label numberOfVaribalesLabel;
  }
}

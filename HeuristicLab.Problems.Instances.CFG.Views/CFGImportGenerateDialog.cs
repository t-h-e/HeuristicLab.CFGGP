using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HeuristicLab.Problems.Instances.CFG.Views {
  public partial class CFGImportGenerateDialog : Form {


    public string Path {
      get { return BNFFileTextBox.Text; }
    }

    public Options GenerateOptions {
      get { return GenerateOptionsHelper(); }
    }

    public CFGImportGenerateDialog() {
      InitializeComponent();
    }

    protected virtual void OpenButtonClick(object sender, System.EventArgs e) {
      if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

      // do some checks
    }

    protected Options GenerateOptionsHelper() {
      var input = new List<DataType>();
      var output = new List<DataType>();
      var tempVarDataTypes = new HashSet<DataType>();
      // Input
      AddTypeRepeat(input, BoolInputUpDown, DataType.Boolean);
      AddTypeRepeat(input, IntegerInputUpDown, DataType.Integer);
      AddTypeRepeat(input, FloatInputUpDown, DataType.Float);
      AddTypeRepeat(input, StringInputUpDown, DataType.String);
      AddTypeRepeat(input, ListOfBoolInputUpDown, DataType.ListBoolean);
      AddTypeRepeat(input, ListOfIntegerInputUpDown, DataType.ListInteger);
      AddTypeRepeat(input, ListOfFloatInputUpDown, DataType.ListFloat);
      AddTypeRepeat(input, ListOfStringInputUpDown, DataType.ListString);

      // Output
      AddTypeRepeat(output, BoolOutputUpDown, DataType.Boolean);
      AddTypeRepeat(output, IntegerOutputUpDown, DataType.Integer);
      AddTypeRepeat(output, FloatOutputUpDown, DataType.Float);
      AddTypeRepeat(output, StringOutputUpDown, DataType.String);
      AddTypeRepeat(output, ListOfBoolOutputUpDown, DataType.ListBoolean);
      AddTypeRepeat(output, ListOfIntegerOutputUpDown, DataType.ListInteger);
      AddTypeRepeat(output, ListOfFloatOutputUpDown, DataType.ListFloat);
      AddTypeRepeat(output, ListOfStringOutputUpDown, DataType.ListString);

      // Temp Variables
      AddTypeIfChecked(tempVarDataTypes, BoolCheckbox, DataType.Boolean);
      AddTypeIfChecked(tempVarDataTypes, IntegerCheckbox, DataType.Integer);
      AddTypeIfChecked(tempVarDataTypes, FloatCheckbox, DataType.Float);
      AddTypeIfChecked(tempVarDataTypes, StringCheckbox, DataType.String);
      AddTypeIfChecked(tempVarDataTypes, ListOfBoolCheckbox, DataType.ListBoolean);
      AddTypeIfChecked(tempVarDataTypes, ListOfIntegerCheckbox, DataType.ListInteger);
      AddTypeIfChecked(tempVarDataTypes, ListOfFloatCheckbox, DataType.ListFloat);
      AddTypeIfChecked(tempVarDataTypes, ListOfStringCheckbox, DataType.ListString);

      return new Options(input, output, tempVarDataTypes, TreeStructureCheckbox.Checked, (int)NumberOfTempVariablesUpDown.Value);
    }

    private void AddTypeRepeat(List<DataType> list, NumericUpDown upDown, DataType dataType) {
      if (upDown.Value > 0) { list.AddRange(Enumerable.Repeat(dataType, (int)upDown.Value)); }
    }

    private void AddTypeIfChecked(HashSet<DataType> set, CheckBox checkBox, DataType dataType) {
      if (checkBox.Checked) set.Add(dataType);
    }
  }
}

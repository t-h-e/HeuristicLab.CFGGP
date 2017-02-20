using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeuristicLab.MainForm;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.Instances.Views;

namespace HeuristicLab.Problems.Instances.CFG.Views {
  [View("Regression InstanceProvider View")]
  [Content(typeof(BenchmarkSuiteInstanceProvider), IsDefaultView = true)]
  public partial class CFGBenchmarkSuiteProvider : ProblemInstanceProviderView<CFGData> {

    public new BenchmarkSuiteInstanceProvider Content {
      get { return (BenchmarkSuiteInstanceProvider)base.Content; }
      set { base.Content = value; }
    }

    public CFGBenchmarkSuiteProvider() {
      InitializeComponent();
    }

    protected override void importButton_Click(object sender, EventArgs e) {
      var importTypeDialog = new CFGImportGenerateDialog();
      if (importTypeDialog.ShowDialog() == DialogResult.OK) {
        CFGData instance = null;
        try {
          if (importTypeDialog.ImportButtonClick) {
            instance = Content.ImportData(importTypeDialog.Path);
          } else {
            instance = Content.GenerateGrammar(importTypeDialog.GenerateOptions);
          }
        } catch (IOException ex) {
          ErrorWhileParsing(ex);
          return;
        }
        try {
          GenericConsumer.Load(instance);
          instancesComboBox.SelectedIndex = -1;
        } catch (IOException ex) {
          ErrorWhileLoading(ex, importTypeDialog.Path);
        }
      }
    }

    protected void ErrorWhileParsing(Exception ex) {
      MessageBox.Show(String.Format("There was an error parsing the file: {0}", Environment.NewLine + ex.Message), "Error while parsing", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    protected void ErrorWhileLoading(Exception ex, string path) {
      MessageBox.Show(String.Format("This problem does not support loading the instance {0}: {1}", Path.GetFileName(path), Environment.NewLine + ex.Message), "Cannot load instance");
    }

    protected override void instancesComboBox_SelectionChangeCommitted(object sender, EventArgs e) {
      toolTip.SetToolTip(instancesComboBox, String.Empty);
      if (instancesComboBox.SelectedIndex >= 0) {
        var descriptor = (IDataDescriptor)instancesComboBox.SelectedItem;

        IContentView activeView = (IContentView)MainFormManager.MainForm.ActiveView;
        var mainForm = (MainForm.WindowsForms.MainForm)MainFormManager.MainForm;
        // lock active view and show progress bar
        mainForm.AddOperationProgressToContent(activeView.Content, "Loading problem instance.");

        Task.Factory.StartNew(() => {
          CFGData data;
          try {
            data = Content.LoadDataLocal(descriptor, treeCheckBox.Checked, (int)numberOfTempVarUpDown.Value);
          } catch (Exception ex) {
            ErrorHandling.ShowErrorDialog(String.Format("Could not load the problem instance {0}", descriptor.Name), ex);
            mainForm.RemoveOperationProgressFromContent(activeView.Content);
            return;
          }
          try {
            GenericConsumer.Load(data);
          } catch (Exception ex) {
            ErrorHandling.ShowErrorDialog(String.Format("This problem does not support loading the instance {0}", descriptor.Name), ex);
          } finally {
            mainForm.RemoveOperationProgressFromContent(activeView.Content);
          }
        });
      }
    }
  }
}

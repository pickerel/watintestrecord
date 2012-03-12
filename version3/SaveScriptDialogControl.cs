using System.Text.RegularExpressions;
using FileDialogExtenders;
using TestRecorder.Core.CodeGenerators;

namespace TestRecorder
{
    public partial class SaveScriptDialogControl : FileDialogControlBase
    {
        public SaveScriptDialogControl()
        {
            InitializeComponent();
        }

        private void SaveScriptDialogControlEventFilterChanged(System.Windows.Forms.IWin32Window sender, int index)
        {
            ddlTemplate.Items.Clear();
            if (index == 1) return;
            string selectedFilter = GetSelectedFilter(index);
            var templateList = CodeGenerator.GetAvailableTemplates(selectedFilter);
            templateList.ForEach(t => ddlTemplate.Items.Add(t));
            if (ddlTemplate.Items.Count > 0)
                ddlTemplate.SelectedIndex = 0;
        }

        public string GetSelectedFilter(int index)
        {
            var regexObj = new Regex(@"\|(\*\.[a-z0-9*]+)", RegexOptions.IgnoreCase);
            MatchCollection saveFilters = regexObj.Matches(FileDlgFilter);
            return saveFilters[index-1].Groups[1].Value;
        }
    }
}

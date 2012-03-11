using System.Collections.Specialized;
using System.Windows.Forms;
using TestRecorder.Core.Actions;

namespace TestRecorder
{
    public partial class frmModifyFinder : Form
    {
        private NameValueCollection _finder;

        public frmModifyFinder()
        {
            InitializeComponent();
        }

        public void SetCheckList(ActionElementBase element)
        {
            _finder = element.AllAttributes;
            clbAttributes.Items.Clear();
            foreach (string attributekey in _finder.AllKeys)
            {
                clbAttributes.Items.Add(attributekey + " = " + _finder[attributekey], 
                    element.ActionFinder.KeyExists(attributekey)?CheckState.Checked : CheckState.Unchecked);
            }
        }

        public NameValueCollection GetChecked()
        {
            var newFinder = new NameValueCollection();
            foreach (int indexChecked in clbAttributes.CheckedIndices)
            {
                string key = _finder.GetKey(indexChecked);
                newFinder.Add(key, _finder[key]);
            }
            return newFinder;
        }
    }
}

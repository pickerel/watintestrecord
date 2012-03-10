using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestRecorder
{
    public partial class frmAnchor : Form
    {
        //A simple form to allow insertion of a hyperlink into the document
        //Other features such as TargetFrame, parameters, ... can be added

        public frmAnchor()
        {
            InitializeComponent();
            this.comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FormClosing += new FormClosingEventHandler(frmAnchor_FormClosing);
            this.btnOk.Click += new EventHandler(btnOk_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);
            this.chkTargetFrame.AutoCheck = true;
        }

        void frmAnchor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public DialogResult ShowDialogInternal(IWin32Window owner, string anchortext)
        {
            m_Success = false;
            this.textBox1.Text = ""; //title
            this.textBox2.Text = anchortext; //text
            this.comboBox1.Text = ""; //Address
            this.chkTargetFrame.Checked = false;
            this.comboBox2.SelectedIndex = 0; //_self Default
            this.ShowDialog(owner);
            return (m_Success) ? DialogResult.OK : DialogResult.Cancel;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            m_Success = true;
            this.Hide();
        }

        private bool m_Success = false;
        public string ScreenTip
        {
            get { return this.textBox1.Text; }
        }
        public string AnchorText
        {
            get { return this.textBox2.Text; }
        }
        public string Address
        {
            get { return this.comboBox1.Text; }
        }
        public bool TargetFrameChecked
        {
            get { return this.chkTargetFrame.Checked; }
        }
        public string TargetFrame
        {
            get { return this.comboBox2.Text; }
        }

    }
}
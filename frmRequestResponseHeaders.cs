using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TestRecorder
{
    public partial class frmRequestResponseHeaders : Form
    {
        public delegate void AddTextItem(String myString);
        public AddTextItem myDelegate;
        public void AddToTextBox(string text)
        {
            richTextBox1.AppendText(Environment.NewLine + text + Environment.NewLine);
        }

        public frmRequestResponseHeaders()
        {
            InitializeComponent();
            this.Text = "HTTP + HTTPS Request Response Headers!";
            this.Load += new EventHandler(frmRequestResponseHeaders_Load);
            this.FormClosing += new FormClosingEventHandler(frmRequestResponseHeaders_FormClosing);
            this.toolStripButtonClear.Click += new EventHandler(toolStripButtonClear_Click);
            this.toolStripButtonClose.Click += new EventHandler(toolStripButtonClose_Click);
            this.toolStripButtonSave.Click += new EventHandler(toolStripButtonSave_Click);
        }

        void frmRequestResponseHeaders_Load(object sender, EventArgs e)
        {
            myDelegate = new AddTextItem(AddToTextBox);
        }

        //Maybe coming from another thread other than the main
        //Throws System.InvalidOperationException when attempting to access
        //a textbox, ... Not allowed in .NET. Cross thread issues
        public void AppendToLogI(string Text)
        {
            if (richTextBox1.InvokeRequired)
            {
                object[] args = { Text };
                Invoke(myDelegate, args);
            }
            else
                richTextBox1.AppendText(Environment.NewLine + Text + Environment.NewLine);
        }

        public void AppendToLog(string Text)
        {
            richTextBox1.AppendText(Environment.NewLine + Text + Environment.NewLine);
        }

        void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            if (AllForms.ShowStaticSaveDialogForText(this) == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(AllForms.m_dlgSave.FileName))
                {
                    sw.Write(richTextBox1.Text);
                }
            }
        }

        void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        void toolStripButtonClear_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        void frmRequestResponseHeaders_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                toolStripButtonClose_Click(this, EventArgs.Empty);
            }
        }
    }
}
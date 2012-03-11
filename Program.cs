using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestRecorder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var mainform = new frmMain();

            try
            {
                Application.Run(mainform);
            }
            catch (Exception ex)
            {
                EmailException(ex);
            }

        }

        private static void EmailException(Exception e)
        {
            var frm = new frmException
            {
                lblError = { Text = e.Message },
                rtbStack = { Text = e.StackTrace }
            };
            if (frm.ShowDialog() == DialogResult.OK)
            {
                var email = new EmailException();
                string strAddress = frm.txtEmail.Text.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(strAddress.ToUpper(), @"^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$"))
                {
                    strAddress = "";
                }
                email.SendMail(e, strAddress, frm.txtComments.Text, frm.chkCopy.Checked, false);
            }
        }
    }
}
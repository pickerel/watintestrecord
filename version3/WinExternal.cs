using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Permissions;

namespace TestRecorder
{
    #region WinExternal class
    /// <summary>
    /// Simple class to demonstrate WindowExternal functionality
    /// Used in frmWindowExternal.cs form
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]

    public class WinExternal
    {
        private string m_Msg = "Original value of AMessageFromHome property.";
        public WinExternal()
        {

        }

        [DispId(0)]
        public void ClickedItem(object sender)
        {
            System.Diagnostics.Debug.WriteLine("Clicked");
        }

        public void SaySomething()
        {
            System.Windows.Forms.MessageBox.Show("SaySomething method of WinExternal called!");
        }

        public string PassValueBack(string text)
        {
            text += "- Return value from windowexternal class!";
            return text;
        }

        public string AMessageFromHome
        {
            get
            {
                return m_Msg;
            }
            set
            {
                System.Windows.Forms.MessageBox.Show("Setting value of AMessageFromHome property via window.external from:\r\n" + m_Msg + "\r\nTo:\r\n" + value);
                m_Msg = value;
            }
        }
    }
    #endregion
}

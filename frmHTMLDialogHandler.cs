using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using IfacesEnumsStructsClasses;

namespace DemoApp
{
    /// <summary>
    /// To demonstrate how to receive events from
    /// an HTML dialog's document and window objects launched
    /// using showModelessDialog() and showModalDialog() methods!
    /// 
    /// Sample call:
    /// 1) Load (dragdrop) ModalModelessDialogs.htm onto frmMain.
    /// 2) In an event handler call:
    /// <code>
    /// m_HtmlDlgHandler.StartHandling();
    /// m_HtmlDlgHandler.Show();
    /// </code>
    /// 3) launch a modal or modaless dialog by using one of the buttons 
    /// in the ModalModelessDialogs.htm page.
    /// </summary>
    public partial class frmHTMLDialogHandler : Form
    {
        #region Variables
        private ComUtilitiesLib.UtilManClass m_csexwbCOMLib = new ComUtilitiesLib.UtilManClass();
        //private CSEXWBDLMANLib.csDLManClass m_csexwbCOMLib = new CSEXWBDLMANLib.csDLManClass();
        private WindowsHookUtil.HookInfo m_CBT = new WindowsHookUtil.HookInfo(ComUtilitiesLib.WINDOWSHOOK_TYPES.WHT_CBT);
        //private WindowsHookUtil.HookInfo m_CBT = new WindowsHookUtil.HookInfo(CSEXWBDLMANLib.WINDOWSHOOK_TYPES.WHT_CBT);
        private const string m_HTMLDlgClassName = "Internet Explorer_TridentDlgFrame";
        private string m_strTemp = string.Empty;
        private int m_NCode = 0;
        private WindowEnumerator winenum = new WindowEnumerator();
        private IHTMLDocument2 m_pDoc2 = null;
        private IHTMLWindow2 m_pWin2 = null;
        private System.Collections.ArrayList m_Ctls = new System.Collections.ArrayList();
        private int m_Count = 0;
        private int m_Counter = 0;
        private Timer m_timer = new Timer();
        private IntPtr m_Dialog = IntPtr.Zero;
        private IntPtr m_IE = IntPtr.Zero;
        private bool m_IsEventsConnected = false;

        private csExWB.HTMLElementEvents m_docelemevents = new csExWB.HTMLElementEvents();
        private csExWB.HTMLWindowEvents m_docwinevents = new csExWB.HTMLWindowEvents();

        #endregion

        #region Form events
        public frmHTMLDialogHandler()
        {
            InitializeComponent();

            m_docelemevents.elemonclick += new csExWB.HTMLElementEventHandler(m_docelemevents_elemonclick);
            m_docelemevents.elemonkeyup += new csExWB.HTMLElementEventHandler(m_docelemevents_elemonkeyup);
            m_docwinevents.winunload += new csExWB.HTMLWindowEventHandler(m_docwinevents_winunload);

            m_timer.Enabled = false;
            m_timer.Interval = 150;
            m_timer.Tick += new EventHandler(m_timer_Tick);

            this.FormClosing += new FormClosingEventHandler(frmHTMLDialogHandler_FormClosing);
        }

        void frmHTMLDialogHandler_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                m_CBT.IsHooked = false;
                //m_csexwbCOMLib.SetupWindowsHook(
                    //CSEXWBDLMANLib.WINDOWSHOOK_TYPES.WHT_CBT,
                    //(int)this.Handle.ToInt32(),
                    //m_CBT.IsHooked,
                    //ref m_CBT.UniqueMsgID);
                m_csexwbCOMLib.SetupWindowsHook(
                    ComUtilitiesLib.WINDOWSHOOK_TYPES.WHT_CBT,
                    (int)this.Handle.ToInt32(),
                    m_CBT.IsHooked,
                    ref m_CBT.UniqueMsgID);

                m_Dialog = IntPtr.Zero;
                m_IsEventsConnected = false;
                if (m_docelemevents.m_IsCOnnected)
                    m_docelemevents.DisconnectHtmlElementEvents();
                if (m_docwinevents.m_IsCOnnected)
                    m_docwinevents.DisconnectHtmlWindowEvents();
                this.Hide();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == m_CBT.UniqueMsgID)
            {
                m_csexwbCOMLib.HookProcNCode(ComUtilitiesLib.WINDOWSHOOK_TYPES.WHT_CBT, ref m_NCode);
                //m_csexwbCOMLib.HookProcNCode(CSEXWBDLMANLib.WINDOWSHOOK_TYPES.WHT_CBT, ref m_NCode);
                if (m_NCode == WindowsHookUtil.HCBT_CREATEWND)
                {
                    m_strTemp = WinApis.GetWindowClass(m.WParam); //Marshal.PtrToStringAnsi(m_CREATESTRUCT.lpszClass);
                    if (!string.IsNullOrEmpty(m_strTemp))
                    {
                        if ((m_strTemp.Equals(m_HTMLDlgClassName, StringComparison.CurrentCultureIgnoreCase)) &&
                            (m_Dialog == IntPtr.Zero))
                        {
                            m_Dialog = m.WParam; //got it
                        }
                    }
                }
                else if (m_NCode == WindowsHookUtil.HCBT_ACTIVATE)
                {
                    //Already have this window?
                    if ((m_Dialog != IntPtr.Zero) && (m_IsEventsConnected == false) &&
                        (m_Dialog == m.WParam))
                    {
                        //m.WParam contains handle to the new window
                        m_IsEventsConnected = true;
                        m_timer.Start();
                    }
                }
            }
            else
                base.WndProc(ref m);
        } 
        #endregion

        void m_timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();
            this.richTextBox1.Clear();
            //Enum child windows
            winenum.enumerate(m_Dialog);
            //Get the control names
            m_Ctls = winenum.GetControlsClassNames();
            m_Count = m_Ctls.Count;

            this.richTextBox1.AppendText("HTMLDialog total child windows count =" + m_Count.ToString() + "\r\n");

            for (m_Counter = 0; m_Counter < m_Count; m_Counter++)
            {
                this.richTextBox1.AppendText(m_Ctls[m_Counter].ToString() + "\r\n");
                //Find IE_Server
                if ((m_Ctls[m_Counter] != null) &&
                    (m_Ctls[m_Counter].ToString().Equals("Internet Explorer_Server", StringComparison.CurrentCultureIgnoreCase))
                    )
                {
                    //subscribe to documentelement events
                    //so we can handle key + unload events
                    m_IE = (IntPtr)Int32.Parse(winenum.GetControlsHwnds()[m_Counter].ToString());
                    this.richTextBox1.AppendText("Internet Explorer_Server HWND =" + m_IE.ToString() + "\r\n");
                    if (m_IE != IntPtr.Zero)
                    {
                        m_pDoc2 = winenum.GetIEHTMLDocument2FromWindowHandle(m_IE);
                        IHTMLDocument3 doc3 = m_pDoc2 as IHTMLDocument3;
                        if (doc3 != null)
                        {
                            if(m_docelemevents.ConnectToHtmlElementEvents(doc3.documentElement))
                                this.richTextBox1.AppendText("Subscribed to Document events = OK\r\n");
                            else
                                this.richTextBox1.AppendText("Subscribed to Document events = FAILED\r\n");
                        }
                        if (m_pDoc2 != null)
                        {
                            m_pWin2 = m_pDoc2.parentWindow as IHTMLWindow2;
                            if (m_pWin2 != null)
                            {
                                if (m_docwinevents.ConnectToHtmlWindowEvents(m_pWin2))
                                    this.richTextBox1.AppendText("Subscribed to Window events = OK\r\n");
                                else
                                    this.richTextBox1.AppendText("Subscribed to Window events = FAILED\r\n");
                            }
                        }
                    }
                    break;
                }
            }
            winenum.clear();
        }

        public void StartHandling()
        {
            m_CBT.IsHooked = true;
            //m_csexwbCOMLib.SetupWindowsHook(
            //    CSEXWBDLMANLib.WINDOWSHOOK_TYPES.WHT_CBT,
            //    (int)this.Handle.ToInt32(),
            //    m_CBT.IsHooked,
            //    ref m_CBT.UniqueMsgID);
            m_csexwbCOMLib.SetupWindowsHook(
                ComUtilitiesLib.WINDOWSHOOK_TYPES.WHT_CBT,
                (int)this.Handle.ToInt32(),
                m_CBT.IsHooked,
                ref m_CBT.UniqueMsgID);
        }

        #region DOCUMENT+WINDOW EVENTS Members
        void m_docelemevents_elemonkeyup(object sender, csExWB.HTMLElementEventArgs e)
        {
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
                this.richTextBox1.AppendText("\r\nHTML_Document_Event==>tagName ="
                    + e.EventObj.SrcElement.tagName);
        }

        void m_docwinevents_winunload(object sender, csExWB.HTMLWindowEventArgs e)
        {
            m_docelemevents.DisconnectHtmlElementEvents();
            m_docwinevents.DisconnectHtmlWindowEvents();
            this.richTextBox1.AppendText("\r\nWindow Closed!");
        }

        void m_docelemevents_elemonclick(object sender, csExWB.HTMLElementEventArgs e)
        {
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
                this.richTextBox1.AppendText("\r\nHTML_Document_Event==>tagName ="
                    + e.EventObj.SrcElement.tagName);
        }
        #endregion
    }
}
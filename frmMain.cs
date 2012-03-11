using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Web;
using IfacesEnumsStructsClasses;
using System.IO;
using TestRecorder.Core;
using TestRecorder.Core.Actions;
using TestRecorder.Core.CodeGenerators;
using csExWB;

namespace TestRecorder
{

    /// <summary>
    /// A multi tab/thumb simulated webbrowser control which
    /// demonstrates the usage of csEXWB control with some extras.
    /// 
    /// The demo pretty much covers all the basics and most of the advanced
    /// functionality that the control offers. It also includes a complete DOM
    /// viewer which works independent of the control, cache and cookie explorers, thumb navigation,
    /// document information viewer, loading and displaying favorites in dynamic
    /// menu, Popup, authentication and find handlers,
    /// a functional HTML editor, ....
    /// 
    /// A bit of details:
    /// In the frmMain, each browser has a corresponding toolstripbutton
    /// acting as a tab and a menu item for tab switching which displays
    /// number of open tabs as well.
    /// 
    /// The name of all the webbrowser corresponding controls are identical!
    /// All use the webbrowser instance Name.
    /// Any new browser can be deleted but the first one which was placed
    /// on the form in design time. Kept for making things a bit easier!
    /// 
    /// The rest is a matter of synchronizing addition, removal, and switching
    /// among webbrowser instances. Simple enough.
    /// 
    /// All the images used in this project 
    /// are coming from IeToolbar.bmp image strip. They are loade in 
    /// SetupImages() method into a static imagelist
    /// which then is shared by all project forms and controls.
    /// 
    /// 
    /// To Test InvokeScript
    /// Add this code to onclick event of a control called tsBtn
    /// <code>
    /// if (tsBtn.Tag == null) //First click, loads the script into browser
    /// {
    ///     tsBtn.Tag = 1;
// ReSharper disable CSharpWarnings::CS1570
    ///     string html = "<HTML><HEAD><SCRIPT language=Javascript>function InvokeMethod (str){ myDiv.innerHTML = '<font size=8>' + str + '</font>';}</Script></Head><Body><H2>Call from App via InvokeScript</h2><div id=\"myDiv\" style=\"font-size: 100%; vertical-align: middle; width: 100%; direction: ltr; font-family: Fantasy, Sans-Serif, Serif; height: 200px; text-align: center\"></div></Body></HTML>";
// ReSharper restore CSharpWarnings::CS1570
    ///     m_CurWB.LoadHtmlIntoBrowser(html, "http://www.dummy.com");
    /// }
    /// else //Second click invoke script
    /// {
    ///     tsBtn.Tag = null;
    ///     m_CurWB.InvokeScript("InvokeMethod", new object[] { "Nice !" });
    /// }
    /// </code>
    /// </summary>
// ReSharper disable InconsistentNaming
    public partial class frmMain : Form
// ReSharper restore InconsistentNaming
    {
        #region Local Variables

        private const string MAboutBlank = "about:blank";
        private const string MBlank = "Blank";
        private cEXWB _mCurWb; //Current WB
        private int _mICurTab; //Current Tab index
        private int _mICurMenu; //Current WB count menu
        private int _mICountWb = 1; //WB Count
        private const int MMaxTextLen = 15; //Maimum len of text displayed in tabs,...
        private ToolStripButton _mTsBtnFirstTab; //for reference
        //For reference when rClicked on a toolstripbutton
        private ToolStripButton _mTsBtnctxMnu; 
        //Is used in browser context menu event to hold a ref
        //to the HTML element under the mouse
        private object _mOHtmlCtxMenu;
        //To capture file download name in FileDownload event
        //private string m_Status = string.Empty;

        //private PictureBox m_thumbPic = new PictureBox();
        //private ToolStripDropDown m_thumbPopup = new ToolStripDropDown();

        //Images for statusbar, ....
        private Image _mImgLock;
        private Image _mImgUnLock;
        private Image _mBlankImage;

        //Forms
        private readonly frmPopup _mFrmPopup = new frmPopup();
        private readonly frmFind _mFrmFind = new frmFind();
        private readonly frmCacheCookie _mFrmCacheCookie = new frmCacheCookie();
        private readonly frmDOM _mFrmDom = new frmDOM();
        private readonly frmDocInfo _mFrmDocInfo = new frmDocInfo();
        private readonly frmAuthenticate _mFrmAuth = new frmAuthenticate();
        private readonly frmHTMLeditor _mFrmHtmlEditor = new frmHTMLeditor();
        private readonly frmFileDownload _mFrmFileDownload = new frmFileDownload();
        private readonly WinExternal _mExternal = new WinExternal();

        //private frmHTMLDialogHandler m_HtmlDlgHandler = new frmHTMLDialogHandler();
        private readonly frmWindowExternal _mFrmWindowExternal = new frmWindowExternal();
        private frmAutomation _mFrmAutomation;
        //private frmHTMLParser _mFrmHtmlParser = new frmHTMLParser();

        private readonly ScriptFactoryManager _scriptManager = new ScriptFactoryManager();
        private readonly Dictionary<int, int> _selectLister = new Dictionary<int, int>();
        private readonly List<IHTMLElement> _inputList = new List<IHTMLElement>(); 
        internal bool Recording;

        #endregion

        #region Form Events

        public frmMain()
        {
            InitializeComponent();
        }

        private void FrmMainLoad(object sender, EventArgs e)
        {
            try
            {
                mainToolStripMenuItem.Visible = false;
                SetupImages();

                lvActions.Items.Clear();
                _scriptManager.OnActionAdded += ActionAdded;
                _scriptManager.OnActionRemoved += ActionRemoved;
                _scriptManager.OnActionModified += ActionModified;

                //Restricted
                //cEXWB1.WBDOCDOWNLOADCTLFLAG = (int)(DOCDOWNLOADCTLFLAG.NO_DLACTIVEXCTLS |
                //DOCDOWNLOADCTLFLAG.NO_FRAMEDOWNLOAD | DOCDOWNLOADCTLFLAG.NO_JAVA |
                //DOCDOWNLOADCTLFLAG.NO_RUNACTIVEXCTLS | DOCDOWNLOADCTLFLAG.NO_SCRIPTS |
                //DOCDOWNLOADCTLFLAG.NOFRAMES | DOCDOWNLOADCTLFLAG.PRAGMA_NO_CACHE |
                //DOCDOWNLOADCTLFLAG.NO_BEHAVIORS | DOCDOWNLOADCTLFLAG.NO_CLIENTPULL |
                //DOCDOWNLOADCTLFLAG.SILENT);

            //    //To activate autocomplete
            //    cEXWB1.WBDOCHOSTUIFLAG = (int)(DOCHOSTUIFLAG.NO3DBORDER |
            //DOCHOSTUIFLAG.FLAT_SCROLLBAR | DOCHOSTUIFLAG.THEME |
            //DOCHOSTUIFLAG.ENABLE_FORMS_AUTOCOMPLETE);

                //cEXWB1.SendSourceOnDocumentCompleteWBEx = true;
                cEXWB1 = new cEXWB();
                toolStripContainer1.ContentPanel.Controls.Add(cEXWB1);
                cEXWB1.Dock = DockStyle.Fill;
                
                cEXWB1.DownloadComplete += CExwb1DownloadComplete;
                cEXWB1.DocumentComplete += CExwb1DocumentComplete;
                cEXWB1.WBSecondaryOnLoad += cEXWB1_WBSecondaryOnLoad;
                cEXWB1.TitleChange += CExwb1TitleChange;
                cEXWB1.NewWindow2 += CExwb1NewWindow2;
                cEXWB1.ScriptError += CExwb1ScriptError;
                cEXWB1.WBKeyDown += CExwb1WbKeyDown;
                 
                cEXWB1.WindowClosing += CExwb1WindowClosing;
                cEXWB1.DocumentCompleteEX += cEXWB1_DocumentCompleteEX;
                cEXWB1.NewWindow3 += CExwb1NewWindow3;
                cEXWB1.WBSecurityProblem += CExwb1WbSecurityProblem;
                cEXWB1.WBKeyUp += cEXWB1_WBKeyUp;
                cEXWB1.WBContextMenu += CExwb1WbContextMenu;
                cEXWB1.FileDownload += cEXWB1_FileDownload;
                cEXWB1.StatusTextChange += CExwb1StatusTextChange;
                cEXWB1.WBDragDrop += CExwb1WbDragDrop;
                cEXWB1.WBDocHostShowUIShowMessage += CExwb1WbDocHostShowUiShowMessage;
                cEXWB1.SetSecureLockIcon += CExwb1SetSecureLockIcon;
                cEXWB1.DownloadBegin += CExwb1DownloadBegin;
                cEXWB1.NavigateComplete2 += CExwb1NavigateComplete2;
                cEXWB1.WBEvaluteNewWindow += CExwb1WbEvaluteNewWindow;
                cEXWB1.WBAuthenticate += CExwb1WbAuthenticate;
                cEXWB1.RefreshEnd += cEXWB1_RefreshEnd;
                cEXWB1.NavigateError += cEXWB1_NavigateError;
                cEXWB1.BeforeNavigate2 += CExwb1BeforeNavigate2;
                cEXWB1.RefreshBegin += cEXWB1_RefreshBegin;
                cEXWB1.CommandStateChange += CExwb1CommandStateChange;
                cEXWB1.ProgressChange += CExwb1ProgressChange;
                cEXWB1.FileDownloadExStart += CExwb1FileDownloadExStart;
                cEXWB1.FileDownloadExEnd += CExwb1FileDownloadExEnd;
                cEXWB1.FileDownloadExProgress += CExwb1FileDownloadExProgress;
                cEXWB1.FileDownloadExError += CExwb1FileDownloadExError;
                cEXWB1.FileDownloadExAuthenticate += CExwb1FileDownloadExAuthenticate;
                cEXWB1.FileDownloadExDownloadFileFullyWritten += cEXWB1_FileDownloadExDownloadFileFullyWritten;
                cEXWB1.ObjectForScripting = _mExternal;
                
                 
                cEXWB1.WBStop += cEXWB1_WBStop;
                //cEXWB1.ProcessUrlAction += new csExWB.ProcessUrlActionEventHandler(cEXWB1_ProcessUrlAction);

                cEXWB1.WBLButtonDown += CExwb1WblButtonDown;
                cEXWB1.WBLButtonUp += CExwb1WblButtonUp;
                //cEXWB1.WBMouseMove += new csExWB.HTMLMouseEventHandler(cEXWB1_WBMouseMove);

                cEXWB1.RegisterAsBrowser = true;
                cEXWB1.FileDownloadDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

                cEXWB1.NavToBlank();
                //cEXWB1.Navigate2("file:///C:/Work/TestRecorder3/tests/html/FramesetWithinFrameset.html");

                //Add first tab
                string sname = cEXWB1.Name;
                _mTsBtnFirstTab = new ToolStripButton
                                      {
                                          ImageScaling = ToolStripItemImageScaling.None,
                                          ImageAlign = ContentAlignment.MiddleCenter,
                                          TextAlign = ContentAlignment.TopLeft,
                                          TextImageRelation = TextImageRelation.TextAboveImage,
                                          Name = sname,
                                          Text = MBlank,
                                          Image = _mBlankImage,
                                          ToolTipText = MAboutBlank,
                                          Checked = true
                                      };
                _mTsBtnFirstTab.MouseUp += tsWBTabs_ToolStripButtonCtxMenuHandler;

                tsWBTabs.Items.Add(_mTsBtnFirstTab);
                // current WB and first toolstripbutton index
                _mCurWb = cEXWB1;
                _mICurTab = tsWBTabs.Items.Count - 1;
                //Add menu
                var menu = new ToolStripMenuItem(MBlank, _mImgUnLock) {Name = sname, Checked = true};
                ctxMnuOpenWBs.Items.Add(menu);
                _mICurMenu = ctxMnuOpenWBs.Items.Count - 1;

                _mFrmFind.Icon = AllForms.BitmapToIcon(30);
                //form find callback
                _mFrmFind.FindInPageEvent += MFrmFindFindInPageEvent;
                
                //Start watching favorites folder
                fswFavorites.Path = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);

                //Load favorites
                LoadFavoriteMenuItems();

                if (cEXWB1.Name == "")
                {
                    cEXWB1.Name = "Browser" + cEXWB1.GetHashCode();
                }
                _scriptManager.AddBrowserWindow(cEXWB1.Name, cEXWB1.LocationUrl, cEXWB1.DocumentTitle);
            }
            catch (Exception ee)
            {
                MessageBox.Show(@"frmMain_Load Failed\r\n" + ee);
            }
        }

        void cEXWB1_WBSecondaryOnLoad(object sender, SecondaryOnloadEventArgs e)
        {
        }

        private void FrmMainFormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if( (e.CloseReason == CloseReason.ApplicationExitCall)
                || (e.CloseReason == CloseReason.UserClosing) )
            {
                if (!AllForms.AskForConfirmation("Proceed to exit application?", this))
                {
                    e.Cancel = true;
                    //Refocus
                    try
                    {
                        if (_mCurWb != null)
                            _mCurWb.SetFocus();
                    }
                    catch (Exception ee)
                    {
                        AllForms.m_frmLog.AppendToLog("frmMain_FormClosing\r\n" + ee);
                    }
                }
            }
            */
        }

        #endregion

        #region Local methods

        /// <summary>
        /// Called from HTML Editor in response to loading
        /// current browser contents into the HTML Editor
        /// </summary>
        public cEXWB CurrentBrowserControl
        {
            get
            {
                return _mCurWb;
            }
        }

        private bool CheckWbPointer()
        {
            return _mCurWb != null;
        }

        private cEXWB FindBrowser(string name)
        {
            try
            {
                foreach (Control ctl in toolStripContainer1.ContentPanel.Controls)
                {
                    if (ctl.Name == name)
                    {
                        var findBrowser = ctl as cEXWB;
                        if (findBrowser != null)
                            return findBrowser;
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            return null;
        }

        private ToolStripButton FindTab(string name)
        {
            try
            {
                foreach (ToolStripItem item in tsWBTabs.Items)
                {
                    if (item.Name == name)
                    {
                        var toolStripButton = item as ToolStripButton;
                        if (toolStripButton != null)
                            return toolStripButton;
                    }
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
            return null;
        }

        private ToolStripMenuItem FindWbMenu(string name)
        {
            return ctxMnuOpenWBs.Items.Cast<ToolStripItem>().Where(item => item.Name == name).OfType<ToolStripMenuItem>().FirstOrDefault();
        }

        private void AddNewBrowser(string tabText, string tabTooltip, string url, bool bringToFront)
        {
            //Copy flags
            var iDochostUiFlag = (int)(DOCHOSTUIFLAG.NO3DBORDER | DOCHOSTUIFLAG.FLAT_SCROLLBAR | DOCHOSTUIFLAG.THEME);
                        
            var iDocDlCltFlag = (int)(DOCDOWNLOADCTLFLAG.DLIMAGES | DOCDOWNLOADCTLFLAG.BGSOUNDS | DOCDOWNLOADCTLFLAG.VIDEOS);

            if (_mCurWb != null)
            {
                iDochostUiFlag = _mCurWb.WBDOCHOSTUIFLAG;
                iDocDlCltFlag = _mCurWb.WBDOCDOWNLOADCTLFLAG;
            }

            int i = _mICountWb + 1;
            string sname = "cEXWB" + i.ToString(CultureInfo.InvariantCulture);

            try
            {
                var btn = new ToolStripButton
                              {
                                  ImageAlign = ContentAlignment.MiddleCenter,
                                  ImageScaling = ToolStripItemImageScaling.None,
                                  TextAlign = ContentAlignment.TopLeft,
                                  TextImageRelation = TextImageRelation.TextAboveImage,
                                  Name = sname,
                                  Text = tabText.Length > 0 ? tabText : MBlank,
                                  Image = _mBlankImage
                              };
                if (tabTooltip.Length > 0)
                    btn.ToolTipText = tabTooltip;
                btn.AutoToolTip = true;
                btn.MouseUp += tsWBTabs_ToolStripButtonCtxMenuHandler;
                tsWBTabs.Items.Add(btn);

                //Create and setup browser
                var pWB = new cEXWB
                                       {
                                           Anchor = cEXWB1.Anchor,
                                           Name = sname,
                                           Location = cEXWB1.Location,
                                           Size = cEXWB1.Size,
                                           RegisterAsBrowser = true,
                                           WBDOCDOWNLOADCTLFLAG = iDocDlCltFlag,
                                           WBDOCHOSTUIFLAG = iDochostUiFlag,
                                           FileDownloadDirectory = cEXWB1.FileDownloadDirectory
                                       };

                //pWB.Dock = cEXWB1.Dock;


                //Add events, using the same eventhandlers for all browsers
                pWB.TitleChange += CExwb1TitleChange;
                pWB.StatusTextChange += CExwb1StatusTextChange;
                pWB.CommandStateChange += CExwb1CommandStateChange;
                pWB.WBKeyDown += CExwb1WbKeyDown;
                pWB.WBEvaluteNewWindow += CExwb1WbEvaluteNewWindow;
                pWB.BeforeNavigate2 += CExwb1BeforeNavigate2;
                pWB.ProgressChange += CExwb1ProgressChange;
                pWB.NavigateComplete2 += CExwb1NavigateComplete2;
                pWB.DownloadBegin += CExwb1DownloadBegin;
                pWB.ScriptError += CExwb1ScriptError;
                pWB.DownloadComplete += CExwb1DownloadComplete;
                pWB.StatusTextChange += CExwb1StatusTextChange;
                pWB.DocumentCompleteEX += cEXWB1_DocumentCompleteEX;
                pWB.WBDragDrop += CExwb1WbDragDrop;
                pWB.SetSecureLockIcon += CExwb1SetSecureLockIcon;
                pWB.NavigateError += cEXWB1_NavigateError;
                pWB.WBSecurityProblem += CExwb1WbSecurityProblem;
                pWB.NewWindow2 += CExwb1NewWindow2;
                pWB.DocumentComplete += CExwb1DocumentComplete;
                pWB.NewWindow3 += CExwb1NewWindow3;
                pWB.WBKeyUp += cEXWB1_WBKeyUp;
                pWB.WindowClosing += CExwb1WindowClosing;
                pWB.WBContextMenu += CExwb1WbContextMenu;
                pWB.WBDocHostShowUIShowMessage += CExwb1WbDocHostShowUiShowMessage;
                pWB.FileDownload += cEXWB1_FileDownload;
                pWB.WBAuthenticate += CExwb1WbAuthenticate;
                pWB.WBStop += cEXWB1_WBStop;
                
                pWB.FileDownloadExStart += CExwb1FileDownloadExStart;
                pWB.FileDownloadExEnd += CExwb1FileDownloadExEnd;
                pWB.FileDownloadExProgress += CExwb1FileDownloadExProgress;
                pWB.FileDownloadExError += CExwb1FileDownloadExError;
                pWB.FileDownloadExAuthenticate += CExwb1FileDownloadExAuthenticate;
                pWB.FileDownloadExDownloadFileFullyWritten += cEXWB1_FileDownloadExDownloadFileFullyWritten;

                pWB.WBStop += cEXWB1_WBStop;
                //pWB.ProcessUrlAction += new csExWB.ProcessUrlActionEventHandler(cEXWB1_ProcessUrlAction);

                pWB.WBLButtonDown += CExwb1WblButtonDown;
                pWB.WBLButtonUp += CExwb1WblButtonUp;
                //pWB.WBMouseMove += new csExWB.HTMLMouseEventHandler(cEXWB1_WBMouseMove);

                pWB.RegisterAsBrowser = true;

                //Add to controls collection
                //Controls.Add(pWB);
                toolStripContainer1.ContentPanel.Controls.Add(pWB);

                var menu = new ToolStripMenuItem(btn.Text, _mImgUnLock) {Name = sname};
                ctxMnuOpenWBs.Items.Add(menu);

                if (bringToFront)
                {
                    //Uncheck last tab
                    ((ToolStripButton)tsWBTabs.Items[_mICurTab]).Checked = false;
                    btn.Checked = true;

                    ((ToolStripMenuItem)ctxMnuOpenWBs.Items[_mICurMenu]).Checked = false;
                    _mICurMenu = ctxMnuOpenWBs.Items.Count - 1;
                    menu.Checked = true;

                    //Adjust current browser pointer
                    _mCurWb = pWB;
                    //Adjust current tab index
                    _mICurTab = tsWBTabs.Items.Count - 1;
                    //Reset and hide progressbar
                    tsProgress.Value = 0;
                    tsProgress.Maximum = 0;
                    tsProgress.Visible = false;
                    //Bring to front
                    pWB.BringToFront();
                }
                //Increase count
                _mICountWb++;
                tsBtnOpenWBs.Text = _mICountWb.ToString(CultureInfo.InvariantCulture) + @" open tab(s)";

                if (url.Length > 0)
                    pWB.Navigate(url);

                if (pWB.Name == "")
                {
                    pWB.Name = "Browser" + pWB.GetHashCode();
                }

                _scriptManager.AddBrowserWindow(pWB.Name, pWB.LocationUrl, pWB.DocumentTitle);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("AddNewBrowser\r\n" + ee);
                return;
            }

            return;
        }

        /// <summary>
        /// Removes an inactive browser without switching to another
        /// </summary>
        /// <param name="name"></param>
        /// <param name="removeMenu">true, removes corresponding menu item</param>
        /// <returns></returns>
        private void RemoveBrowser2(string name, bool removeMenu)
        {
            try
            {
                //Do not remove the first browser          
                if ((_mICountWb == 1) || (name == _mTsBtnFirstTab.Name))
                    return;

                var pWB = FindBrowser(name);
                //Controls.Remove(pWB);
                toolStripContainer1.ContentPanel.Controls.Remove(pWB);
                
                pWB.Dispose();

                ToolStripButton btn = FindTab(name);
                tsWBTabs.Items.Remove(btn);
                btn.Dispose();

                if (removeMenu)
                {
                    ToolStripMenuItem menu = FindWbMenu(name);
                    ctxMnuOpenWBs.Items.Remove(menu);
                    menu.Dispose();
                }

                _mICountWb--;
                tsBtnOpenWBs.Text = _mICountWb.ToString(CultureInfo.InvariantCulture) + @" open tab(s)";
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser2\r\n" + ee);
            }
            return;
        }

        /// <summary>
        /// Removes the current browser and switches to the one before it
        /// if one is available, else the first one is selected
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private void RemoveBrowser(string name)
        {
            //Do not remove the first browser          
            if ((_mICountWb == 1) || (name == _mTsBtnFirstTab.Name))
                return;

            tsProgress.Value = 0;
            tsProgress.Maximum = 0;
            tsProgress.Visible = false;

            ToolStripButton btn = FindTab(name);
            ToolStripButton nexttab = null;
            try
            {
                //find the first available btn before this one and switch
                foreach (ToolStripItem item in tsWBTabs.Items)
                {
                    if (item.Name == btn.Name)
                    {
                        break;
                    }
                    var toolStripButton = item as ToolStripButton;
                    if (toolStripButton != null)
                        nexttab = toolStripButton;
                }
            }
            catch (Exception eRemoveBrowser)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser\r\n" + eRemoveBrowser);
            }

            try
            {
                tsWBTabs.Items.Remove(btn);
                btn.Dispose();
            }
            catch (Exception eRemoveBrowser1)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser1\r\n" + eRemoveBrowser1);
            }

            try
            {
                var pWB = FindBrowser(name);
                //Controls.Remove(pWB);
                toolStripContainer1.ContentPanel.Controls.Remove(pWB);
                pWB.Dispose();
            }
            catch (Exception eRemoveBrowser2)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser2\r\n" + eRemoveBrowser2);
            }

            ToolStripMenuItem menu = FindWbMenu(name);
            ToolStripMenuItem nextmenu = null;

            try
            {
                foreach (ToolStripItem titem in ctxMnuOpenWBs.Items)
                {
                    if (titem.Name == menu.Name)
                    {
                        break;
                    }
                    var toolStripMenuItem = titem as ToolStripMenuItem;
                    if (toolStripMenuItem != null)
                        nextmenu = toolStripMenuItem;
                }
                ctxMnuOpenWBs.Items.Remove(menu);
                menu.Dispose();
            }
            catch (Exception eRemoveBrowser3)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser3\r\n" + eRemoveBrowser3);
            }

            try
            {
                if (nexttab == null)
                {
                    _mCurWb = cEXWB1;
                    _mICurTab = tsWBTabs.Items.IndexOf(_mTsBtnFirstTab);
                    _mICurMenu = 0;
                    nexttab = _mTsBtnFirstTab;
                }
                else
                {
                    _mCurWb = FindBrowser(nexttab.Name);
                    _mICurTab = tsWBTabs.Items.IndexOf(nexttab);
                    if (nextmenu != null)
                        _mICurMenu = ctxMnuOpenWBs.Items.IndexOf(nextmenu);
                }

                Text = _mCurWb.GetTitle(true);
                if (Text.Length == 0)
                    Text = MBlank;
                comboURL.Text = nexttab.ToolTipText;
                nexttab.Checked = true;
                if (nextmenu != null) nextmenu.Checked = true;
                _mCurWb.BringToFront();

                _mCurWb.SetFocus();

            }
            catch (Exception eRemoveBrowser4)
            {
                AllForms.m_frmLog.AppendToLog("RemoveBrowser4\r\n" + eRemoveBrowser4);
            }

            _mICountWb--;
            tsBtnOpenWBs.Text = _mICountWb.ToString() + @" open tab(s)";

            return;
        }

        private void SwitchTabs(string name, ToolStripButton btn)
        {
            try
            {
                var pWB = FindBrowser(name);
                if (pWB == null)
                    return;

                //Uncheck last one
                if (_mICountWb > 1)
                    ((ToolStripButton)tsWBTabs.Items[_mICurTab]).Checked = false;
                _mICurTab = tsWBTabs.Items.IndexOf(btn);

                _mCurWb = pWB;
                tsBtnBack.Enabled = _mCurWb.CanGoBack;
                tsBtnForward.Enabled = _mCurWb.CanGoForward;
                _mCurWb.BringToFront();
                _mCurWb.SetFocus();
                Text = _mCurWb.GetTitle(true);
                if (Text.Length == 0)
                    Text = MBlank;
                if (btn != null)
                {
                    btn.Checked = true;
                    btn.Text = Text;
                    if (btn.Text.Length > MMaxTextLen)
                        btn.Text = btn.Text.Substring(0, MMaxTextLen) + @"...";
                    btn.ToolTipText = HttpUtility.UrlDecode(_mCurWb.LocationUrl);
                    comboURL.Text = btn.ToolTipText;
                }

                //Uncheck all menu items first
                foreach (ToolStripItem item in ctxMnuOpenWBs.Items)
                {
                    var toolStripMenuItem = item as ToolStripMenuItem;
                    if (toolStripMenuItem != null)
                        (toolStripMenuItem).Checked = false;
                }
                //Find target menu item
                ToolStripMenuItem menu = FindWbMenu(name);
                _mICurMenu = ctxMnuOpenWBs.Items.IndexOf(menu);
                if (menu != null)
                {
                    if (btn != null)
                        menu.Text = btn.Text;
                    menu.Checked = true;
                }
                //Reset and hide progressbar
                //If page is in the process of loading then the progressbar
                //will be adjusted
                tsProgress.Value = 0;
                tsProgress.Maximum = 0;
                tsProgress.Visible = false;

                //update SecureLockIcon state
                UpdateSecureLockIcon(_mCurWb.SecureLockIcon);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("SwitchingTabs\r\n" + ee);
            }

        }

        private void UpdateSecureLockIcon(SecureLockIconConstants slic)
        {
            if (slic == SecureLockIconConstants.secureLockIconUnsecure)
            {
                tsSecurity.Image = _mImgUnLock;
                tsSecurity.Text = @"Not Secure";
            }
            else if (slic == SecureLockIconConstants.secureLockIcon128Bit)
            {
                tsSecurity.Image = _mImgLock;
                tsSecurity.Text = @"128 Bit";
            }
            else if (slic == SecureLockIconConstants.secureLockIcon40Bit)
            {
                tsSecurity.Image = _mImgLock;
                tsSecurity.Text = @"40 Bit";
            }
            else if (slic == SecureLockIconConstants.secureLockIcon56Bit)
            {
                tsSecurity.Image = _mImgLock;
                tsSecurity.Text = @"56 Bit";
            }
            else if (slic == SecureLockIconConstants.secureLockIconFortezza)
            {
                tsSecurity.Image = _mImgLock;
                tsSecurity.Text = @"Fortezza";
            }
            else if (slic == SecureLockIconConstants.secureLockIconMixed)
            {
                tsSecurity.Image = _mImgUnLock;
                tsSecurity.Text = @"Mixed";
            }
            else if (slic == SecureLockIconConstants.secureLockIconUnknownBits)
            {
                tsSecurity.Image = _mImgUnLock;
                tsSecurity.Text = @"UnknownBits";
            }
        }

        /// <summary>
        /// Loads all images from a image strip into a
        /// static imagelist which in turn can 
        /// be used bey any form or control 
        /// capable of using images
        /// </summary>
        private void SetupImages()
        {
            try
            {
                //string[] str = GetType().Assembly.GetManifestResourceNames();
                //foreach (string s in str)
                //{
                //    System.Diagnostics.Debug.Print(s);
                //}
                //DemoApp.Properties.Resources.resources
                //DemoApp.frmPopup.resources
                //DemoApp.frmMain.resources
                //DemoApp.Resources.IeToolbar.bmp
                //....

                Stream file2 =
                    GetType().Assembly.GetManifestResourceStream("TestRecorder.Resources.blanka.bmp");
                if (file2 != null) _mBlankImage = Image.FromStream(file2);

                Stream file =
                    GetType().Assembly.GetManifestResourceStream("TestRecorder.Resources.IeToolbar.bmp");
                if (file != null)
                {
                    Image img = Image.FromStream(file);

                    AllForms.m_imgListMain.TransparentColor = Color.FromArgb(192, 192, 192);
                    AllForms.m_imgListMain.Images.AddStrip(img);
                }

                tsBtnBack.Image = AllForms.m_imgListMain.Images[0];
                tsBtnForward.Image = AllForms.m_imgListMain.Images[1];
                tsBtnStop.Image = AllForms.m_imgListMain.Images[2];
                tsBtnRefresh.Image = AllForms.m_imgListMain.Images[4];
                tsBtnGo.Image = AllForms.m_imgListMain.Images[10];
                tsChkBtnGo.Image = AllForms.m_imgListMain.Images[18];

                tsBtnOpenWBs.Image = AllForms.m_imgListMain.Images[12];
                tsBtnAddWB.Image = AllForms.m_imgListMain.Images[16];
                tsChkBtnAddWB.Image = AllForms.m_imgListMain.Images[18];
                tsBtnRemoveWB.Image = AllForms.m_imgListMain.Images[39];
                tsBtnRemoveAllWBs.Image = AllForms.m_imgListMain.Images[40];

                _mImgLock = AllForms.m_imgListMain.Images[13];
                _mImgUnLock = AllForms.m_imgListMain.Images[32]; //normall ie

                tsLinksLblText.Image = AllForms.m_imgListMain.Images[20];

                tsFileMnuNew.Image = AllForms.m_imgListMain.Images[19];
                tsFileMnuOpen.Image = AllForms.m_imgListMain.Images[43];
                tsFileMnuSave.Image = AllForms.m_imgListMain.Images[21];
                tsFileMnuSaveDocument.Image = AllForms.m_imgListMain.Images[44];
                tsFileMnuSaveDocumentImage.Image = AllForms.m_imgListMain.Images[45];
                tsEditMnuCut.Image = AllForms.m_imgListMain.Images[23];
                tsEditMnuCopy.Image = AllForms.m_imgListMain.Images[24];
                tsEditMnuPaste.Image = AllForms.m_imgListMain.Images[25];
                tsEditMnuSelectAll.Image = AllForms.m_imgListMain.Images[28];
                tsEditMnuFindInPage.Image = AllForms.m_imgListMain.Images[30];
                tsFileMnuPrintPreview.Image = AllForms.m_imgListMain.Images[7];
                tsFileMnuPrint.Image = AllForms.m_imgListMain.Images[8];
                tsFileMnuExit.Image = AllForms.m_imgListMain.Images[37];

                tsHelpMnuHelpAbout.Image = AllForms.m_imgListMain.Images[33];
                tsHelpMnuHelpContents.Image = AllForms.m_imgListMain.Images[9];

                tsOpticalZoom.Image = AllForms.m_imgListMain.Images[30];

                Icon = AllForms.BitmapToIcon(41);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("\r\nError=" + ee);
            }
        }

        private void NavToUrl(string sUrl)
        {
            if (!CheckWbPointer())
                return;
            try
            {
                _scriptManager.AddNavigate(_mCurWb.Name, sUrl);
                _mCurWb.Navigate(sUrl);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("NavToUrl\r\n" + ee);
            }
        }

        #endregion

        #region Event handlers

        private void ToolStripViewMenuClickHandler(object sender, EventArgs e)
        {
            try
            {
                if ((sender == tsViewMnuLogs) && (!AllForms.m_frmLog.Visible))
                    AllForms.m_frmLog.Show(this);
                else if (sender == mainToolStripMenuItem)
                {
                    //tsMenus.Visible = mainToolStripMenuItem.Checked;
                }
                else if (sender == tabsToolStripMenuItem)
                {
                    tsWBTabs.Visible = tabsToolStripMenuItem.Checked;
                }
                else if (sender == linksToolStripMenuItem)
                {
                    tsLinks.Visible = linksToolStripMenuItem.Checked;
                }
                else if (sender == addressToolStripMenuItem)
                {
                    tsGoSearch.Visible = addressToolStripMenuItem.Checked;
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("ToolStripViewMenuClickHandler\r\n" + ee);
            }
        }

        private void BrowserCtxMenuClickHandler(object sender, EventArgs e)
        {
            if (_mOHtmlCtxMenu == null)
                return;
            try
            {
                //A
                if (sender == ctxMnuACopyUrl)
                {
                    var phref = (IHTMLAnchorElement)_mOHtmlCtxMenu;
                    Clipboard.Clear();
                    Clipboard.SetText(HttpUtility.UrlDecode(phref.href));
                }
                else if (sender == ctxMnuACopyUrlText)
                {
                    var pelem = (IHTMLElement)_mOHtmlCtxMenu;
                    Clipboard.Clear();
                    Clipboard.SetText(pelem.outerText);
                }
                else if (sender == ctxMnuAOpenInBack)
                {
                    var phref = (IHTMLAnchorElement)_mOHtmlCtxMenu;
                    AddNewBrowser(MBlank, "", phref.href, false);
                }
                else if (sender == ctxMnuAOpenInFront)
                {
                    var phref = (IHTMLAnchorElement)_mOHtmlCtxMenu;
                    AddNewBrowser(MBlank, "", phref.href, true);
                }
                //Img
                else if (sender == ctxMnuImgCopyImageSource)
                {
                    var pimg = (IHTMLImgElement)_mOHtmlCtxMenu;
                    Clipboard.Clear();
                    Clipboard.SetText(pimg.src);
                }
                else if (sender == ctxMnuImgCopyImageAlt)
                {
                    var pimg = (IHTMLImgElement)_mOHtmlCtxMenu;
                    Clipboard.Clear();
                    Clipboard.SetText(pimg.alt);
                }
                else if (sender == ctxMnuImgCopyUrlText)
                {
                    var pelem = (IHTMLElement)_mOHtmlCtxMenu;
                    var phref = (IHTMLAnchorElement)pelem.parentElement;
                    Clipboard.Clear();
                    Clipboard.SetText(HttpUtility.UrlDecode(phref.href));
                    /*
                    Uri url = new Uri(phref.href);                    
                    string str = "AbsolutePath\r\n" + url.AbsolutePath;
                    str += "\r\nFragment\r\n" + url.Fragment;
                    str += "\r\nHost\r\n" + url.Host;
                    str += "\r\nPathAndQuery\r\n" + url.PathAndQuery;
                    str += "\r\nPort\r\n" + url.Port;
                    str += "\r\nQuery\r\n" + url.Query;
                    str += "\r\nScheme\r\n" + url.Scheme;
                    str += "\r\nUserInfo\r\n" + url.UserInfo;
                    str += "\r\nOriginal\r\n" + url.OriginalString;
                    fLog.AppendToLog("\r\n" + str);
                    
                    //Uncomment the top part

                    AbsolutePath
                    /imgres
                    Fragment

                    Host
                    images.google.ca
                    PathAndQuery
                    /imgres?imgurl=http://www.rixane.com/shots/flight-over-sea-800-1.jpg&imgrefurl=http://www.rixane.com/flight-over-sea/flight-over-sea.html&h=600&w=800&sz=69&hl=en&start=1&tbnid=arcdwYQaOgXKaM:&tbnh=107&tbnw=143&prev=/images%3Fq%3Dsea%26svnum%3D10%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
                    Port
                    80
                    Query
                    ?imgurl=http://www.rixane.com/shots/flight-over-sea-800-1.jpg&imgrefurl=http://www.rixane.com/flight-over-sea/flight-over-sea.html&h=600&w=800&sz=69&hl=en&start=1&tbnid=arcdwYQaOgXKaM:&tbnh=107&tbnw=143&prev=/images%3Fq%3Dsea%26svnum%3D10%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
                    Scheme
                    http
                    UserInfo

                    Original
                    http://images.google.ca/imgres?imgurl=http://www.rixane.com/shots/flight-over-sea-800-1.jpg&imgrefurl=http://www.rixane.com/flight-over-sea/flight-over-sea.html&h=600&w=800&sz=69&hl=en&start=1&tbnid=arcdwYQaOgXKaM:&tbnh=107&tbnw=143&prev=/images%3Fq%3Dsea%26svnum%3D10%26hl%3Den%26lr%3D%26safe%3Doff%26sa%3DG
                     */
                }
                else if (sender == ctxMnuImgOpenInBack)
                {
                    var pelem = (IHTMLElement)_mOHtmlCtxMenu;
                    var phref = (IHTMLAnchorElement)pelem.parentElement;
                    AddNewBrowser(MBlank, "", phref.href, false);
                }
                else if (sender == ctxMnuImgOpenInFront)
                {
                    var pelem = (IHTMLElement)_mOHtmlCtxMenu;
                    var phref = (IHTMLAnchorElement)pelem.parentElement;
                    AddNewBrowser(MBlank, "", phref.href, true);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("BrowserCtxMenuClickHandler\r\n" + ee);
            }

            _mOHtmlCtxMenu = null;
        }

        private void tsWBTabs_ToolStripButtonCtxMenuHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                try
                {
                    tsMnuCloasAllWBs.Enabled = (_mICountWb > 1);
                    ctxMnuCloseWB.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                catch (Exception ee)
                {
                    AllForms.m_frmLog.AppendToLog("TabContextMenuHandler\r\n" + ee);
                }
            }
        }

        private void GoSearchToolStripButtonClickHandler(object sender, EventArgs e)
        {
            if (!CheckWbPointer())
                return;

            try
            {
                if (sender == tsBtnGo)
                {
                    if (tsChkBtnGo.Checked) //Open in a new background browser
                    {
                        AddNewBrowser(MBlank, "", comboURL.Text, false);
                    }
                    else
                        NavToUrl(comboURL.Text);
                }
                else if (sender == tsBtnBack)
                {
                    if (_mCurWb.CanGoBack)
                        _mCurWb.GoBack();
                }
                else if (sender == tsBtnForward)
                {
                    if (_mCurWb.CanGoForward)
                        _mCurWb.GoForward();
                }
                else if (sender == tsBtnRefresh)
                    _mCurWb.Refresh();
                else if (sender == tsBtnStop)
                    _mCurWb.Stop();
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("GoSearchToolStripButtonClickHandler\r\n" + ee);
            }
        }

        private void ToolStripHelpMenuClickHandler(object sender, EventArgs e)
        {
            try
            {
                if (sender == tsHelpMnuHelpAbout)
                {
                    var about = new frmAbout();
                    about.ShowDialog(this);
                    about.Dispose();
                }
                else if (sender == tsHelpMnuHelpContents)
                {
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("ToolStripHelpMenuClickHandler\r\n" + ee);
            }
        }

        private void ToolStripToolsMenuClickHandler(object sender, EventArgs e)
        {
            if (!CheckWbPointer())
                return;
            if (sender == tsToolsMnuTravelLogEntries)
            {
                //m_CurWB.RemoveTravelLogEntry(0);
                //m_CurWB.AddTravelLogEntry("http://www.yahoo.com", "Yahoo");
                AllForms.m_frmLog.AppendToLog("Travel Log Entries (History for current browser) - Count = " + _mCurWb.GetTravelLogCount().ToString() + "\r\n");
                List<TravelLogStruct> items = _mCurWb.GetTraveLogEntries();
                foreach (TravelLogStruct item in items)
                {
                    //Current page will have it's url and title as zero length string
                    if ((!string.IsNullOrEmpty(item.Title)) &&
                        (!string.IsNullOrEmpty(item.URL)))
                        AllForms.m_frmLog.AppendToLog("Title==>" + item.Title + " <>Url==>" + item.URL);
                    else
                        AllForms.m_frmLog.AppendToLog("Current Title==>" + _mCurWb.GetTitle(true) + " <>Current Url==>" + _mCurWb.LocationUrl);
                }
                if (!AllForms.m_frmLog.Visible)
                    AllForms.m_frmLog.Show(this);
                return;
            }
            if (sender == tsToolsMnuDocumentDOM)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    //load and display DOM, passing Document object
                    _mFrmDom.LoadDOM(_mCurWb.WebbrowserObject.Document);
                    if (!_mFrmDom.Visible)
                        _mFrmDom.Show(this);
                    else
                        _mFrmDom.BringToFront();
                    Cursor = Cursors.Default;
                }
                catch (Exception eee)
                {
                    Cursor = Cursors.Default;
                    AllForms.m_frmLog.AppendToLog("tsToolsMnuDocumentDOM\r\n" + eee);
                }
                return;
            }
            if (sender == tsToolsMnuDocumentInfo)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    _mFrmDocInfo.LoadDocumentInfo(_mCurWb);
                    if (!_mFrmDocInfo.Visible)
                        _mFrmDocInfo.Show(this);
                    else
                        _mFrmDocInfo.BringToFront();
                    Cursor = Cursors.Default;
                }
                catch (Exception eeee)
                {
                    Cursor = Cursors.Default;
                    AllForms.m_frmLog.AppendToLog("tsToolsMnuDocumentInfo\r\n" + eeee);
                }
                return;
            }
            if (sender == tsToolsMnuSimpleHTMLEditor)
            {
                if (!_mFrmHtmlEditor.Visible)
                    _mFrmHtmlEditor.Show(this);
                return;
            }
            if (sender == tsToolsMnufileDownloads)
            {
                if (!_mFrmFileDownload.Visible)
                    _mFrmFileDownload.Show(this);
                return;
            }
            if (sender == tsToolsMnuHTMLDialogs)
            {
                MessageBox.Show(@"This workaround is for systems prior to WinXP sp2 IE6/7\r\nFor WinXP sp2 IE6/7 and up, use WBEvaluateNewWindow event.");
                if (tsToolsMnuHTMLDialogs.Checked) //Allow
                {
                    tsToolsMnuHTMLDialogs.Text = @"Allow HTML Dialogs";
                    cEXWB1.SetAllowHTMLDialogs(true);
                }
                else
                {
                    tsToolsMnuHTMLDialogs.Text = @"Disllow HTML Dialogs";
                    cEXWB1.SetAllowHTMLDialogs(false);
                }
                return;
            }
            if (sender == tsToolsMnudisplayHTMLPopup)
            {
                var doc2 = _mCurWb.WebbrowserObject.Document as IHTMLDocument2;

                if (doc2 == null)
                    return;

                var win4 = doc2.parentWindow as IHTMLWindow4;
                if (win4 == null)
                    return;
                var popup = win4.createPopup(null) as IHTMLPopup;
                if (popup != null)
                {
                    var popdoc = popup.document as IHTMLDocument2;
                    if ((popdoc != null) && (popdoc.body != null))
                    {
                        popdoc.body.style.backgroundColor = "lightyellow";
                        popdoc.body.style.border = "solid black 1px";
                        popdoc.body.innerHTML = "<p align=\"center\">Displaying some <B>HTML</B> as a tooltip! X = 10, Y = 10</p>";
                    }
                    popup.show(10, 10, 400, 25, doc2.body);
                }
                return;
            }
            if (sender == tsToolsMnuWindowExternal)
            {
                if (!_mFrmWindowExternal.Visible)
                    _mFrmWindowExternal.Show(this);
                return;
            }
            if (sender == tsToolsMnuAutomation)
            {
                _mFrmAutomation = new frmAutomation();
                _mFrmAutomation.Show(this);
                return;
            }

            #region Cache Cookie History
            
            bool bshowform = true;
            int iCount = 0; //Number of cookies or cache entries deleted
            try
            {
                if (sender == tsToolsMnuClearHistory)
                {
                    if (!AllForms.AskForConfirmation("Proceed to remove all History entries?", this))
                        return;
                    _mCurWb.ClearHistory();
                }
                else if (sender == tsToolsMnuCookieViewAll)
                {
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.LoadListViewItems(AllForms.COOKIE);
                    Cursor = Cursors.Default;
                }
                else if (sender == tsToolsMnuCookieViewCurrentSite)
                {
                    string url = _mCurWb.LocationUrl;
                    if (url.Length > 0)
                    {
                        Cursor = Cursors.WaitCursor;
                        iCount = _mFrmCacheCookie.LoadListViewItems(
                            AllForms.SetupCookieCachePattern(_mCurWb.LocationUrl, AllForms.COOKIE));
                        Cursor = Cursors.Default;
                    }
                }
                else if (sender == tsToolsMnuCacheViewAll)
                {
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.LoadListViewItems(AllForms.VISITED);
                    Cursor = Cursors.Default;
                }
                else if (sender == tsToolsMnuCacheViewCurrentSite)
                {
                    string url = _mCurWb.LocationUrl;
                    if (url.Length > 0)
                    {
                        //Visited:.*\.example\.com
                        Cursor = Cursors.WaitCursor;
                        iCount = _mFrmCacheCookie.LoadListViewItems(
                            AllForms.SetupCookieCachePattern(_mCurWb.LocationUrl, AllForms.VISITED));
                        Cursor = Cursors.Default;
                    }
                }
                else if (sender == tsToolsMnuCookieEmptyAll)
                {
                    if (!AllForms.AskForConfirmation("Proceed to remove all cookies?", this))
                        return;
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.ClearAllCookies(string.Empty);
                    bshowform = false;
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, @"Deleted " + iCount.ToString() + @" Cookies.",
                        @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (sender == tsToolsMnuCookieEmptyCurrentSite)
                {
                    if (!AllForms.AskForConfirmation("Proceed to remove cookies from "
                        + _mCurWb.LocationUrl + " ?", this))
                        return;
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.ClearAllCookies(_mCurWb.LocationUrl);
                    bshowform = false;
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, @"Deleted " + iCount.ToString() +
                        @" Cookies from\r\n" + _mCurWb.LocationUrl,
                        @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (sender == tsToolsMnuCacheEmptyAll)
                {
                    if (!AllForms.AskForConfirmation("Proceed to remove all cache entries?", this))
                        return;
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.ClearAllCache(string.Empty);
                    bshowform = false;
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, @"Deleted " + iCount.ToString() + @" Cache Entries.",
                        @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (sender == tsToolsMnuCacheEmptyCurrentSite)
                {
                    if (!AllForms.AskForConfirmation("Proceed to remove cache entries from "
                        + _mCurWb.LocationUrl + " ?", this))
                        return;
                    Cursor = Cursors.WaitCursor;
                    iCount = _mFrmCacheCookie.ClearAllCache(_mCurWb.LocationUrl);
                    bshowform = false;
                    Cursor = Cursors.Default;
                    MessageBox.Show(this, @"Deleted " + iCount.ToString() +
                        @" Cache Entries from\r\n" + _mCurWb.LocationUrl,
                        @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (bshowform)
                {
                    if (iCount > 0)
                    {
                        if (!_mFrmCacheCookie.Visible)
                            _mFrmCacheCookie.Show(this);
                    }
                    else
                        MessageBox.Show(this, @"No Items Found", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ee)
            {
                Cursor = Cursors.Default;
                AllForms.m_frmLog.AppendToLog("ToolStripToolsMenuClickHandler\r\n" + ee);
            }

            #endregion

        }

        /// <summary>
        /// File menu items click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripFileMenuClickHandler(object sender, EventArgs e)
        {
            try
            {
                if (sender == tsFileMnuBackgroundBlankPage)
                {
                    AddNewBrowser(MBlank, MAboutBlank, string.Empty, false);
                }
                else if (sender == tsFileMnuBackgroundFromAddress)
                {
                    AddNewBrowser(MBlank, "", comboURL.Text, false);
                }
                else if (sender == tsFileMnuForegroundBlankPage)
                {
                    AddNewBrowser(MBlank, MAboutBlank, string.Empty, true);
                }
                else if (sender == tsFileMnuForegroundFromAddress)
                {
                    AddNewBrowser(MBlank, "", comboURL.Text, true);
                }
                else if (sender == tsFileMnuPrint)
                {
                    _mCurWb.Print();
                }
                else if (sender == tsFileMnuPrintPreview)
                {
                    _mCurWb.PrintPreview();
                    //m_CurWB.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_PRINTPREVIEW);
                }
                else if (sender == tsFileMnuSaveDocument)
                {
                    _mCurWb.SaveAs();
                }
                else if (sender == tsFileMnuSaveDocumentImage)
                {
                    ////gif format produces some of the smallest sizes
                    if (AllForms.ShowStaticSaveDialogForImage(this) == DialogResult.OK)
                    {
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
                        string ext = ".bmp";
                        switch (AllForms.m_dlgSave.FilterIndex)
                        {
                            case 1:
                                break;
                            case 2:
                                format = System.Drawing.Imaging.ImageFormat.Gif;
                                ext = ".gif";
                                break;
                            case 3:
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                ext = ".jpeg";
                                break;
                            case 4:
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                ext = ".png";
                                break;
                            case 5:
                                format = System.Drawing.Imaging.ImageFormat.Wmf;
                                ext = ".wmf";
                                break;
                            case 6:
                                format = System.Drawing.Imaging.ImageFormat.Tiff;
                                ext = ".tiff";
                                break;
                            case 7:
                                format = System.Drawing.Imaging.ImageFormat.Emf;
                                ext = ".emf";
                                break;
                        }
                        if (string.IsNullOrEmpty(Path.GetExtension(AllForms.m_dlgSave.FileName)))
                            AllForms.m_dlgSave.FileName += ext;

                        _mCurWb.SaveBrowserImage(AllForms.m_dlgSave.FileName,
                            System.Drawing.Imaging.PixelFormat.Format24bppRgb, format);
                    }
                }
                else if (sender == tsFileMnuOpen)
                {
                    if (AllForms.ShowStaticOpenDialog(this, AllForms.DLG_HTMLS_FILTER, 
                        string.Empty, "C:",true) == DialogResult.OK)
                        _mCurWb.Navigate(AllForms.m_dlgOpen.FileName);
                }
                else if (sender == tsFileMnuExit)
                {
                    Application.Exit();
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("File Menu\r\n" + ee);
            }
        }

        /// <summary>
        /// Edit menu items click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripEditMenuClickHandler(object sender, EventArgs e)
        {
            try
            {
                if (!CheckWbPointer())
                    return;
                if (sender == tsEditMnuSelectAll)
                    _mCurWb.SelectAll();
                else if (sender == tsEditMnuCopy)
                    _mCurWb.Copy();
                else if (sender == tsEditMnuCut)
                    _mCurWb.Cut();
                else if (sender == tsEditMnuPaste)
                    _mCurWb.Paste();
                else if (sender == tsEditMnuFindInPage)
                {
                    _mFrmFind.Show(this);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("ToolStripEditMenuClickHandler\r\n" + ee);
            }
        }

        /// <summary>
        /// URL combo click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboUrlKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.Return)
                {
                    comboURL.Text = "www." + comboURL.Text + ".com";
                    NavToUrl(comboURL.Text);
                }
                else if (e.KeyCode == Keys.Return)
                {
                    NavToUrl(comboURL.Text);
                }
                else if (e.Control && e.KeyCode == Keys.C)
                {
                    Clipboard.SetData(DataFormats.StringFormat, comboURL.SelectedText);
                }
                else if (e.Control && e.KeyCode == Keys.V)
                {
                    if (comboURL.SelectionLength > 0)
                        comboURL.SelectedText = Clipboard.GetData(DataFormats.StringFormat).ToString();
                    else comboURL.Text = Clipboard.GetData(DataFormats.StringFormat).ToString();
                }
                else if (e.Control && e.KeyCode == Keys.A)
                {
                    comboURL.SelectAll();
                }
            }
            catch (Exception eex)
            {
                MessageBox.Show(eex.ToString(), @"comboUrl_KeyUp");
            }
        }

        /// <summary>
        /// Handles click event of the drop down menu items of the
        /// toolstrip button responsible to display number of open browsers
        /// and to offer a quick menu to select a browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsBtnOpenWBsDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                SwitchTabs(e.ClickedItem.Name, FindTab(e.ClickedItem.Name));
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsBtnOpenWBs_DropDownItemClicked\r\n" + ee);
            }
        }

        /// <summary>
        /// Handles toolstripbutton (tabs) click events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsWBTabsItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Name == tsBtnOpenWBs.Name)
                    return;

                var btn = (ToolStripButton)e.ClickedItem;
                if (e.ClickedItem.Name == tsBtnAddWB.Name)
                {
                    AddNewBrowser(MBlank, MAboutBlank, MAboutBlank, !tsChkBtnAddWB.Checked);
                }
                else if (e.ClickedItem.Name == tsBtnRemoveWB.Name)
                {
                    RemoveBrowser(tsWBTabs.Items[_mICurTab].Name);
                }
                else if (e.ClickedItem.Name == tsBtnRemoveAllWBs.Name)
                {
                    TsMnuCloasAllWBsClick(this, EventArgs.Empty);
                }
                else
                    SwitchTabs(e.ClickedItem.Name, btn);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsWBTabs_ItemClicked\r\n" + ee);
            }
        }

        /// <summary>
        /// Handles close menu click event to remove a browser
        /// May not be the current browser in front
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsMnuCloseWBClick(object sender, EventArgs e)
        {
            try
            {
                if (_mTsBtnctxMnu == null)
                    return;
                //Is this the current one
                if (_mTsBtnctxMnu.Name == _mCurWb.Name)
                {
                    RemoveBrowser(_mTsBtnctxMnu.Name);
                }
                else
                {
                    RemoveBrowser2(_mTsBtnctxMnu.Name, true);
                }
                _mTsBtnctxMnu = null;
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsMnuCloseWB_Click\r\n" + ee);
            }
        }

        /// <summary>
        /// Close all browsers except the first one from design time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsMnuCloasAllWBsClick(object sender, EventArgs e)
        {
            if (_mICountWb == 1)
                return;
            try
            {
                foreach (ToolStripMenuItem item in ctxMnuOpenWBs.Items)
                {
                    RemoveBrowser2(item.Name, false);
                }
                ctxMnuOpenWBs.Items.Clear();

                _mCurWb = cEXWB1;
                _mCurWb.BringToFront();
                _mCurWb.SetFocus();

                _mTsBtnFirstTab.Checked = true;
                _mICurTab = tsWBTabs.Items.IndexOf(_mTsBtnFirstTab);

                string text = _mCurWb.GetTitle(true);
                if (text.Length == 0)
                    text = MBlank;
                var menu = new ToolStripMenuItem(text, _mImgUnLock) {Name = _mTsBtnFirstTab.Name, Checked = true};
                ctxMnuOpenWBs.Items.Add(menu);
                _mICurMenu = ctxMnuOpenWBs.Items.Count - 1;

                tsBtnOpenWBs.Text = _mICountWb.ToString() + @" open tab(s)";
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsMnuCloasAllWBs_Click\r\n" + ee);
            }
        }

        /// <summary>
        /// Update enable state of Edit menu items before displaying them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsMnuEditDropDownOpening(object sender, EventArgs e)
        {
            try
            {
                tsEditMnuSelectAll.Enabled = _mCurWb.IsCommandEnabled("SelectAll");
                tsEditMnuCopy.Enabled = _mCurWb.IsCommandEnabled("Copy");
                tsEditMnuCut.Enabled = _mCurWb.IsCommandEnabled("Cut");
                tsEditMnuPaste.Enabled = _mCurWb.IsCommandEnabled("Paste");
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsMnuEdit_DropDownOpening\r\n" + ee);
            }
        }

        /// <summary>
        /// Call back to intercept find requests from frmFind
        /// </summary>
        /// <param name="findPhrase"></param>
        /// <param name="matchWholeWord"></param>
        /// <param name="matchCase"></param>
        /// <param name="downward"></param>
        /// <param name="highlightAll"></param>
        /// <param name="sColor"></param>
        void MFrmFindFindInPageEvent(string findPhrase, bool matchWholeWord, bool matchCase, bool downward, bool highlightAll, string sColor)
        {
            try
            {
                if (highlightAll)
                {
                    int found = _mCurWb.FindAndHightAllInPage(findPhrase, matchWholeWord, matchCase, sColor, "black");
                    MessageBox.Show(this, @"Found " + found.ToString() + @" matches.", @"Find in Page", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (_mCurWb.FindInPage(findPhrase, downward, matchWholeWord, matchCase, true) == false)
                        MessageBox.Show(this, @"No more matches found for " + findPhrase, @"Find in Page", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("frmMain_m_frmFind_FindInPageEvent\r\n" + ee);
            }
        }

        private ToolStripMenuItem _mLastopticalzoomvalue;
        /// <summary>
        /// Optical Zoom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsOpticalZoomDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (_mLastopticalzoomvalue == null)
                tsOpticalZoom100.Checked = false;
            else
                _mLastopticalzoomvalue.Checked = false;
            _mLastopticalzoomvalue = e.ClickedItem as ToolStripMenuItem;
            if (_mLastopticalzoomvalue != null)
                _mCurWb.SetOpticalZoomValue(int.Parse(_mLastopticalzoomvalue.Text));
        }

        #endregion

        #region Favorites Handling

        //Use a FileSystemWatcher to determine whether to reload favorites upon
        //dropping down or not.
        //To be more effeicent, I would have, in case of
        //Create, insert a new menu item in the appropriate index
        //delete, remove the menu item
        //renamed, modify the text
        //changed, modify text and/or url
        private bool _mFavNeedReload;

        private void FswFavoritesCreated(object sender, FileSystemEventArgs e)
        {
            _mFavNeedReload = true;
            //e.ChangeType.ToString();
            //e.FullPath;
            //e.Name;
        }

        private void FswFavoritesDeleted(object sender, FileSystemEventArgs e)
        {
            _mFavNeedReload = true;
            //try
            //{
            //    //If a link then we remove it
            //    ToolStripItem itema = null;
            //    foreach (ToolStripItem item in tsLinks.Items)
            //    {
            //        if (item.Name == e.Name)
            //        {
            //            itema = item;
            //            break;
            //        }
            //    }
            //    if (itema != null)
            //        tsLinks.Items.Remove(itema);
            //}
            //catch (Exception ee)
            //{
            //    AllForms.m_frmLog.AppendToLog("fswFavorites_Deleted\r\n" + ee.ToString());
            //}
        }

        private void FswFavoritesRenamed(object sender, RenamedEventArgs e)
        {
            _mFavNeedReload = true;
        }

        private void FswFavoritesChanged(object sender, FileSystemEventArgs e)
        {
            _mFavNeedReload = true;
        }

        private void LoadFavoriteMenuItems()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var objDir = new DirectoryInfo(fswFavorites.Path);
                //Recurse, starting from main dir
                LoadFavoriteMenuItems(objDir, tsFavoritesMnu);
                Cursor = Cursors.Default;
            }
            catch (Exception ee)
            {
                Cursor = Cursors.Default;
                AllForms.m_frmLog.AppendToLog("LoadFavoriteMenuItems\r\n" + ee);
            }
        }

        /// <summary>
        /// Recursive function
        /// </summary>
        /// <param name="objDir"></param>
        /// <param name="menuitem"></param>
        private void LoadFavoriteMenuItems(DirectoryInfo objDir, ToolStripMenuItem menuitem)
        {
            try
            {
                DirectoryInfo[] dirs = objDir.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    
                    var diritem = new ToolStripMenuItem(dir.Name, tsFileMnuOpen.Image);
                    menuitem.DropDownItems.Add(diritem);
                    LoadFavoriteMenuItems(dir, diritem);
                }

                bool addlinks = (objDir.Name.Equals("links", StringComparison.CurrentCultureIgnoreCase));
                FileInfo[] urls = objDir.GetFiles("*.url");
                foreach (FileInfo url in urls)
                {
                    string strName = Path.GetFileNameWithoutExtension(url.Name);
                    string strUrl = _mCurWb.ResolveInternetShortCut(url.FullName);
                    //load up quick links
                    if (addlinks)
                    {
                        var btn = new ToolStripButton(strName, _mImgUnLock) {Tag = strUrl};
                        btn.Click += ToolStripLinksButtonClickHandler;
                        tsLinks.Items.Add(btn);
                    }
                    var item = new ToolStripMenuItem(strName, _mImgUnLock) {Tag = strUrl};
                    item.Click += ToolStripFavoritesMenuClickHandler;
                    menuitem.DropDownItems.Add(item);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("LoadFavoriteMenuItems\r\n" + ee);
            }
        }

        void ToolStripLinksButtonClickHandler(object sender, EventArgs e)
        {
            try
            {
                var item = (ToolStripItem)sender;
                if (item.Tag != null)
                    NavToUrl(item.Tag.ToString());
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("ToolStripLinksButtonClickHandler\r\n" + ee);
            }
        }

        private void TsFavoritesMnuDropDownOpening(object sender, EventArgs e)
        {
            if (!_mFavNeedReload)
                return;
            _mFavNeedReload = false;
            try
            {
                //Reload favorites
                if (tsFavoritesMnu.DropDownItems.Count > 3)
                {
                    //Remove from back to front except the original items
                    for (int i = tsFavoritesMnu.DropDownItems.Count - 1; i > 2; i--)
                    {
                        if ((tsFavoritesMnu.DropDownItems[i] != tsFavoritesMnuAddToFavorites) &&
                            (tsFavoritesMnu.DropDownItems[i] != tsFavoritesMnuOrganizeFavorites) &&
                            (tsFavoritesMnu.DropDownItems[i] != tsFavoritesMnuSeparator))
                        {
                            tsFavoritesMnu.DropDownItems.Remove(tsFavoritesMnu.DropDownItems[i]);
                        }
                    }
                    for (int i = tsLinks.Items.Count - 1; i > 0; i--)
                    {
                        tsLinks.Items.Remove(tsLinks.Items[i]);
                    }
                }
                //Load favorites
                LoadFavoriteMenuItems();
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsFavoritesMnu_DropDownOpening\r\n" + ee);
            }
        }

        private void ToolStripFavoritesMenuClickHandler(object sender, EventArgs e)
        {
            try
            {
                if (sender == tsFavoritesMnuAddToFavorites)
                {
                    _mCurWb.AddToFavorites();
                }
                else if (sender == tsFavoritesMnuOrganizeFavorites)
                {
                    _mCurWb.OrganizeFavorites();
                }
                var item = (ToolStripItem)sender;
                if (item.Tag != null)
                    NavToUrl(item.Tag.ToString());
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("ToolStripFavoritesMenuClickHandler\r\n" + ee);
            }
        }

        #endregion

        #region TextSize
        
        /// <summary>
        /// Sets browser text size based on given parameter 0-4
        /// Adjust the chack state of textsize menu items
        /// </summary>
        /// <param name="iLevel">Text size level 0-4</param>
        private void SetZoomLevel(int iLevel)
        {
            tsViewMnuTextSizeLargest.Checked = false;
            tsViewMnuTextSizeLarger.Checked = false;
            tsViewMnuTextSizeMedium.Checked = false;
            tsViewMnuTextSizeSmaller.Checked = false;
            tsViewMnuTextSizeSmallest.Checked = false;

            switch (iLevel)
            {
                case 0:
                    tsViewMnuTextSizeLargest.Checked = true;
                    if (_mCurWb != null)
                        _mCurWb.TextSize = TextSizeWB.Largest;
                    break;
                case 1:
                    tsViewMnuTextSizeLarger.Checked = true;
                    if (_mCurWb != null)
                        _mCurWb.TextSize = TextSizeWB.Larger;
                    break;
                case 2:
                    tsViewMnuTextSizeMedium.Checked = true;
                    if (_mCurWb != null)
                        _mCurWb.TextSize = TextSizeWB.Medium;
                    break;
                case 3:
                    tsViewMnuTextSizeSmaller.Checked = true;
                    if (_mCurWb != null)
                        _mCurWb.TextSize = TextSizeWB.Smaller;
                    break;
                case 4:
                    tsViewMnuTextSizeSmallest.Checked = true;
                    if (_mCurWb != null)
                        _mCurWb.TextSize = TextSizeWB.Smallest;
                    break;
            }
        }

        /// <summary>
        /// Hanldes textsize menu item clicks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsMnuTextSizeClickHandler(object sender, EventArgs e)
        {
            if (sender == tsViewMnuTextSizeLargest)
                SetZoomLevel(0);
            else if (sender == tsViewMnuTextSizeLarger)
                SetZoomLevel(1);
            else if (sender == tsViewMnuTextSizeMedium)
                SetZoomLevel(2);
            else if (sender == tsViewMnuTextSizeSmaller)
                SetZoomLevel(3);
            else if (sender == tsViewMnuTextSizeSmallest)
                SetZoomLevel(4);
        }

        /// <summary>
        /// Updates the check state of the text size menu items before displaying them
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TsMnuTextSizeDropDownOpening(object sender, EventArgs e)
        {
            tsViewMnuTextSizeLargest.Checked = false;
            tsViewMnuTextSizeLarger.Checked = false;
            tsViewMnuTextSizeMedium.Checked = false;
            tsViewMnuTextSizeSmaller.Checked = false;
            tsViewMnuTextSizeSmallest.Checked = false;
            if (cEXWB1.TextSize == TextSizeWB.Largest)
                tsViewMnuTextSizeLargest.Checked = true;
            else if (cEXWB1.TextSize == TextSizeWB.Larger)
                tsViewMnuTextSizeLarger.Checked = true;
            else if (cEXWB1.TextSize == TextSizeWB.Medium)
                tsViewMnuTextSizeMedium.Checked = true;
            else if (cEXWB1.TextSize == TextSizeWB.Smaller)
                tsViewMnuTextSizeSmaller.Checked = true;
            else if (cEXWB1.TextSize == TextSizeWB.Smallest)
                tsViewMnuTextSizeSmallest.Checked = true;
        } 

        #endregion

        #region WebBrowser Events

        private void CExwb1TitleChange(object sender, TitleChangeEventArgs e)
        {
            if (sender != _mCurWb)
                return;
            Text = "Test Recorder v3.0 - "+e.title;
            _scriptManager.UpdateBrowserWindow(_mCurWb.Name, e.title);
        }

        private void CExwb1StatusTextChange(object sender, StatusTextChangeEventArgs e)
        {
            if (sender != _mCurWb)
                return;
            //if (e.text.Length > 0) m_Status = e.text;
            tsStatus.Text = e.text;
        }

        private void CExwb1BeforeNavigate2(object sender, BeforeNavigate2EventArgs e)
        {

            //if (e.istoplevel)
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_BeforeNavigate2_TOPLEVEL== " + e.url.ToString());
            //else
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_BeforeNavigate2== " + e.url.ToString());
            //http://www.codeproject.com/atl/vbmhwb.asp
            //try
            //{
            //    if ((m_CurWB != null) && (m_CurWB == sender) && (e.istoplevel))
            //    {
            //    }
            //}
            //catch (Exception ee)
            //{
            //    if (m_CurWB != null)
            //        AllForms.m_frmLog.AppendToLog(m_CurWB.Name + "_BeforeNavigate2\r\n" + ee.ToString());
            //    else
            //        AllForms.m_frmLog.AppendToLog("cEXWB1_BeforeNavigate2\r\n" + ee.ToString());
            //}
            //finally
            //{

            //}
            System.Diagnostics.Debug.WriteLine("Before Nav");
        }

        private void CExwb1CommandStateChange(object sender, CommandStateChangeEventArgs e)
        {
            if (sender != _mCurWb)
                return;
            try
            {
                if (e.command == CommandStateChangeConstants.CSC_NAVIGATEBACK)
                    tsBtnBack.Enabled = e.enable;
                else if (e.command == CommandStateChangeConstants.CSC_NAVIGATEFORWARD)
                    tsBtnForward.Enabled = e.enable;
            }
            catch (Exception ee)
            {
                if(_mCurWb != null)
                    AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_CommandStateChange\r\n" + ee);
                else
                    AllForms.m_frmLog.AppendToLog("cEXWB1_CommandStateChange\r\n" + ee);
            }
        }

/*
        private bool _mRegisterEvents = true;
*/
        private void CExwb1DocumentComplete(object sender, DocumentCompleteEventArgs e)
        {
            try
            {
                ////To determine if an internal link has occured
                ////http://www.site.com/page.htm#internal
                //Uri url = new Uri(e.url);
                //if (!string.IsNullOrEmpty(url.Fragment))
                //    AllForms.m_frmLog.AppendToLog("Internal_Link" + url.Fragment);
                //if (e.istoplevel)
                //    AllForms.m_frmLog.AppendToLog("cEXWB1_DocumentComplete_TOPLEVEL== " + e.url.ToString());
                //else
                //    AllForms.m_frmLog.AppendToLog("cEXWB1_DocumentComplete== " + e.url.ToString());
                
                var pWb = (cEXWB)sender;


               HookUpItems(pWb);

                
                if (e.istoplevel)
                {
                    //csExWB.XmlHttpReadyStateFactory.wireXMLHttpReadyState(pWB.WebbrowserObject.Document, null);
                    
                    //AllForms.m_frmLog.AppendToLog("ISTOPLEVEL");
                    
                    ToolStripButton btn = FindTab(pWb.Name);
                    ToolStripMenuItem menu = FindWbMenu(pWb.Name);

                    btn.Text = pWb.GetTitle(true);
                    if (btn.Text.Length == 0)
                    {
                        btn.Text = MBlank;
                    }
                    else if (btn.Text.Length > MMaxTextLen)
                        btn.Text = btn.Text.Substring(0, MMaxTextLen) + @"...";
                    menu.Text = btn.Text;
                    btn.ToolTipText = HttpUtility.UrlDecode(e.url);

                    try
                    {
                        if ((WindowState != FormWindowState.Minimized) &&
                            (e.url != MAboutBlank))
                        {
                            btn.Image = pWb.DrawThumb(80, 80,
                                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                    }
                    catch (Exception ee1)
                    {
                        AllForms.m_frmLog.AppendToLog(pWb.Name + "_DocumentComplete_UpdateThumb\r\n" + ee1);
                    }

                    if (sender == _mCurWb)
                    {
                        comboURL.Text = btn.ToolTipText;
                        pWb.SetFocus();
                    }
                    

                }
                else if (pWb.MainDocumentFullyLoaded) // a frame naviagtion within a frameset
                {
                    try
                    {
                        ToolStripButton btn = FindTab(pWb.Name);
                        if( (WindowState != FormWindowState.Minimized) &&
                            (btn != null) )
                        {
                            btn.Image = pWb.DrawThumb(80, 80,
                                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        }
                    }
                    catch (Exception ee2)
                    {
                        AllForms.m_frmLog.AppendToLog(pWb.Name + "_DocumentComplete_UpdateThumb\r\n" + ee2);
                    }
                }
            }
            catch (Exception ee)
            {
                if (_mCurWb != null)
                    AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_DocumentComplete\r\n" + ee);
                else
                    AllForms.m_frmLog.AppendToLog("cEXWBxx_DocumentComplete\r\n" + ee);
            }
        }

        private void cEXWB1_DocumentCompleteEX(object sender, DocumentCompleteExEventArgs e)
        {
            //Activate this event, if you need to process the source HTML before
            //any scripts have been executed.
        }

        private void HookUpItems(cEXWB sender)
        {
            IHTMLElementCollection inputCollection = sender.GetElementsByTagName(true, "input");
            foreach (IHTMLElement input in inputCollection)
            {
                //regester the html event handlers
                if (((IHTMLInputElement)input).type == "text")
                {
                    if (!_inputList.Contains(input))
                    {
                        HtmlEventProxy.Create("onblur", input, TextChangeHandler);
                        _inputList.Add(input);
                    }
                }
            }

            inputCollection = sender.GetElementsByTagName(true, "select");
            foreach (IHTMLSelectElement input in inputCollection)
            {
                if (!_inputList.Contains((IHTMLElement)input))
                {
                    _selectLister.Add(input.GetHashCode(), input.selectedIndex);
                    HtmlEventProxy.Create("onclick", input, SelectChangeHandler);
                    _inputList.Add((IHTMLElement)input);
                }
            }
        }

        private void TextChangeHandler(object sender, EventArgs e)
        {
            if (!Recording) return;
            _scriptManager.AddText(CurrentBrowserControl.Name, (IHTMLInputElement)((HtmlEventProxy)sender).HtmlElement, CurrentBrowserControl.LocationUrl);
        }

        private void SelectChangeHandler(object sender, EventArgs e)
        {
            if (!Recording) return;
            int hash = ((HtmlEventProxy) sender).HtmlElement.GetHashCode();
            if (((IHTMLSelectElement)((HtmlEventProxy)sender).HtmlElement).selectedIndex != _selectLister[hash])
                _scriptManager.AddSelect(CurrentBrowserControl.Name, (IHTMLSelectElement)((HtmlEventProxy)sender).HtmlElement, CurrentBrowserControl.LocationUrl);
        }

        private void CExwb1NavigateComplete2(object sender, NavigateComplete2EventArgs e)
        {
            if (e.url != "about:blank") comboURL.Text = e.url;
        }

        //default, cancel = false;
        private void cEXWB1_NavigateError(object sender, NavigateErrorEventArgs e)
        {
            //If using internal filedownlod mechanism,
            //we get nav errors for file download with status code 200(OK)????
            if ((e.statuscode == WinInetErrors.HTTP_STATUS_OK) ||
                (e.statuscode == WinInetErrors.HTTP_STATUS_CONTINUE) ||
                (e.statuscode == WinInetErrors.HTTP_STATUS_REDIRECT) ||
                //(e.statuscode == WinInetErrors.HTTP_STATUS_REQUEST_TIMEOUT) ||
                (e.statuscode == WinInetErrors.HTTP_STATUS_ACCEPTED))
            {
                return; //default handling
            }

            var pWB = (cEXWB)sender;
            //if (pWB != null)
            //{
            //    AllForms.m_frmLog.AppendToLog(pWB.Name +
            //        "_NavigateError\r\nURL\r\n" + HttpUtility.UrlDecode(e.url) +
            //        "\r\nStatus Code\r\n" + e.statuscode.ToString());
            //}
            //else
            //{
            //    AllForms.m_frmLog.AppendToLog("cEXWBxx_NavigateError\r\nURL\r\n" + HttpUtility.UrlDecode(e.url) +
            //        "\r\nStatus Code\r\n" + e.statuscode.ToString());
            //}

            try
            {
                object nobj = null;
                //<HTML> tag is appended
                //Each line 200 chars
                string data = "about:<HEAD><title>Page Not Found</title></Head><Body><H3>Unable to load</H3><p>" + e.url + "<br>Reason:<br>" + e.statuscode.ToString() + "</p></Body>";

                //No frames, main document
                //Do not set e.Cancel to true
                //as it stops the navigation of our main document
                //resulting in a blank page. Instead, we stop the navigation
                //and call our own navigate
                if (pWB.FramesCount() == 0)
                {
                    //default
                    //e.Cancel = false;
                    pWB.Stop();
                    pWB.Navigate(data);
                    return;
                }

                //If this is a Frame or IFrame then
                //we cancel the navigation to this page
                //and navigate to our error page.
                e.Cancel = true;

                //Variaty of ways to display an error page

                //Attempt to pass HTML directly to Navigate
                ((IWebBrowser2)e.browser).Navigate(data, ref nobj, ref nobj, ref nobj, ref nobj);

                //Attempt to nav to a page on a server
                //http://www.google.com
                //((IWebBrowser2)e.browser).Navigate("http://www.google.com", ref nobj, ref nobj, ref nobj, ref nobj);
                
                //Attempt to load a res file
                //res://exePath/#220
                //Open up the compiled exe using VS and add resources!
                //Refer to this article for information
                //http://www.codeproject.com/csharp/embedwin32resources.asp
                
                //Display a blank page
                //about:blank
                //((IWebBrowser2)e.browser).Navigate("about:blank", ref nobj, ref nobj, ref nobj, ref nobj);

                //Attempt to load a local file
                //file:///E|//Test.htm - using this protocol in IE7 causes a
                //StackOverFlowException which means we don't have local file access
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("cEXWB1_NavigateError\r\n" + ee);
            }
        }

        private void AssignPopup(ref object obj)
        {
            if (_mCurWb != null)
            {
                if (!_mCurWb.RegisterAsBrowser)
                    _mCurWb.RegisterAsBrowser = true;
                obj = _mCurWb.WebbrowserObject;
            }
        }
        private void CExwb1NewWindow2(object sender, NewWindow2EventArgs e)
        { 
            try
            {
                string result = AllForms.m_frmDynamicConfirm.DisplayConfirm(this,
                    "A new window2 has been requested by " + _mCurWb.Name,
                    "Popup Window", 
                    "Cancel popup", "Open in popup", "Open in new tab", string.Empty);

                if (result == "Cancel popup")
                    e.Cancel = true;
                else if (result == "Open in popup")
                {
                    if (!_mFrmPopup.Visible)
                        _mFrmPopup.Show(this);
                    _mFrmPopup.AssignBrowserObject(ref e.browser);
                }
                else if (result == "Open in new tab")
                {
                    AddNewBrowser(MBlank, MAboutBlank, MAboutBlank, true);
                    AssignPopup(ref e.browser);
                }
                //else open in current webbrowser
            }
            catch (Exception nEx)
            {
                if (_mCurWb != null)
                    AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_NewWindow2\r\n" + nEx);
                else
                    AllForms.m_frmLog.AppendToLog("cEXWBxx_NewWindow2\r\n" + nEx);
            }
        }

        private void CExwb1NewWindow3(object sender, NewWindow3EventArgs e)
        {
            try
            {
                string str;
                switch (e.flags)
                {
                    case NWMF.HTMLDIALOG:
                        str = "HTML Dialog";
                        break;
                    case NWMF.SHOWHELP:
                        str = "Show Help";
                        break;
                    default:
                        str = e.flags.ToString();
                        break;
                }

                string result = AllForms.m_frmDynamicConfirm.DisplayConfirm(this,
                    "A new window3 has been requested by:\r\n" + e.url + "\r\nType:" + str,
                    "Popup Window",
                    "Cancel popup", "Open in popup", "Open in new tab", string.Empty);

                if (result == "Cancel popup")
                    e.Cancel = true;
                else if (result == "Open in popup")
                {
                    if (!_mFrmPopup.Visible)
                        _mFrmPopup.Show(this);
                    _mFrmPopup.AssignBrowserObject(ref e.browser);
                }
                else if (result == "Open in new tab")
                {
                    AddNewBrowser(MBlank, MAboutBlank, MAboutBlank, true);
                    AssignPopup(ref e.browser);
                }
            }
            catch (Exception nEx)
            {
                if (_mCurWb != null)
                    AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_NewWindow3\r\n" + nEx);
                else
                    AllForms.m_frmLog.AppendToLog("cEXWBxx_NewWindow3\r\n" + nEx);
            }
        }

        private void CExwb1ProgressChange(object sender, ProgressChangeEventArgs e)
        {
            if (sender != _mCurWb)
                return;
            try
            {
                if ((e.progress == -1) || (e.progressmax == e.progress))
                {
                    tsProgress.Value = 0; // 100;
                    tsProgress.Maximum = 0;
                    return;
                }
                if ((e.progressmax > 0) && (e.progress > 0) && (e.progress < e.progressmax))
                {
                    tsProgress.Maximum = e.progressmax;
                    tsProgress.Value = e.progress; //* 100) / tsProgress.Maximum;
                }
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        private void CExwb1ScriptError(object sender, ScriptErrorEventArgs e)
        {
            string wbname = (_mCurWb != null) ? _mCurWb.Name : "cEXWBxx";
            e.continueScripts = true;
            AllForms.m_frmLog.AppendToLog(wbname + "_ScriptError - Continuing to run scripts");
            AllForms.m_frmLog.AppendToLog("Error Message" + e.errorMessage + "\r\nLine Number" + e.lineNumber.ToString());
        }

        private void CExwb1SetSecureLockIcon(object sender, SetSecureLockIconEventArgs e)
        {
            if (sender != _mCurWb)
                return;
            try
            {
                UpdateSecureLockIcon(e.securelockicon);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_SetSecureLockIcon" + ee);
            }
        }

        private void CExwb1WbContextMenu(object sender, ContextMenuEventArgs e)
        {
            try
            {
                if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_ANCHOR)
                {
                    e.displaydefault = false;
                    _mOHtmlCtxMenu = e.dispctxmenuobj;
                    ctxMnuWB_A.Show(e.pt);
                }
                else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_IMAGE)
                {
                    //If image has a HREF then enable CopyURLText image menu
                    e.displaydefault = false;
                    _mOHtmlCtxMenu = e.dispctxmenuobj;

                    ctxMnuImgCopyUrlText.Enabled = false;
                    var pelem = (IHTMLElement)_mOHtmlCtxMenu;
                    if (pelem != null)
                    {
                        IHTMLElement pParent = pelem.parentElement;
                        ctxMnuImgCopyUrlText.Enabled = (pParent.tagName.ToUpper() == "A");
                    }
                    ctxMnuImgOpenInBack.Enabled = ctxMnuImgCopyUrlText.Enabled;
                    ctxMnuImgOpenInFront.Enabled = ctxMnuImgCopyUrlText.Enabled;

                    ctxMnuWB_Img.Show(e.pt);
                }
                else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_CONTROL)
                {
                    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_CONTROL");
                }
                else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_TEXTSELECT)
                {
                    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_TEXTSELECT");
                }
                else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_TABLE)
                {
                    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_TABLE");
                }
                
                AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_TAGNAME = " + e.ctxmenuelem.tagName);
                
                //else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_DEFAULT)
                //{
                //    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_DEFAULT");
                //}
                //else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_IMGART)
                //{
                //    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_IMGART");
                //}
                //else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_IMGDYNSRC)
                //{
                //    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_IMGDYNSRC");
                //}
                //else if (e.contextmenutype == WB_CONTEXTMENU_TYPES.CONTEXT_MENU_UNKNOWN)
                //{
                //    AllForms.m_frmLog.AppendToLog("CONTEXT_MENU_UNKNOWN");
                //}
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBContextMenu\r\n" + ee);
            }
        }

        private void CExwb1WbDocHostShowUiShowMessage(object sender, DocHostShowUIShowMessageEventArgs e)
        {
            string wbname = (_mCurWb != null) ? _mCurWb.Name : "cEXWBxx";
            AllForms.m_frmLog.AppendToLog(wbname + "_WBDocHostShowUIShowMessage - handled - Text\r\n" + e.text);
 
            //To stop messageboxs
            e.handled = true;
            //Default
            e.result = (int)DialogResult.Cancel;

            // simple alert dialog
            if (e.type == 48)
            {
                e.result = (int)MessageBox.Show(e.text, @"TestRecorder");
            }
            // confirm dialog
            else if (e.type == 33)
            {
                e.result = (int)MessageBox.Show(e.text, @"TestRecorder", MessageBoxButtons.OKCancel);
            }

       }

        private void CExwb1WbEvaluteNewWindow(object sender, EvaluateNewWindowEventArgs e)
        {
            try
            {
                string str;
                if (e.flags == NWMF.HTMLDIALOG)
                    str = "HTML Dialog";
                else if (e.flags == NWMF.SHOWHELP)
                    str = "Show Help";
                else
                    str = e.flags.ToString();

                /*
                if (MessageBox.Show(@"A new EvaluteNewWindow has been requested by:
" + e.url + @"\r\nType:" + str + @"\r\nCancel it?", @"Popup Window", MessageBoxButtons.YesNo).Equals(DialogResult.Yes))
                {
                    e.Cancel = true;
                }
                */
            }
            catch (Exception nEx)
            {
                AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_WBEvaluteNewWindow\r\n" + nEx);
            }
        }

        private void CExwb1WbSecurityProblem(object sender, SecurityProblemEventArgs e)
        {
            //if (e.problem == WinInetErrors.ERROR_INTERNET_INVALID_CA)
            //{
            //    e.handled = true;
            //    e.retvalue = Hresults.S_OK;
            //    return;
            //}

            if( (e.problem == WinInetErrors.HTTP_REDIRECT_NEEDS_CONFIRMATION) ||
                (e.problem == WinInetErrors.ERROR_INTERNET_HTTP_TO_HTTPS_ON_REDIR) ||
                (e.problem == WinInetErrors.ERROR_INTERNET_HTTPS_HTTP_SUBMIT_REDIR) ||
                (e.problem == WinInetErrors.ERROR_INTERNET_HTTPS_TO_HTTP_ON_REDIR) ||
                (e.problem == WinInetErrors.ERROR_INTERNET_MIXED_SECURITY) )
            {
                e.handled = true;
                e.retvalue = Hresults.S_FALSE;
            }
                
            string wbname = (_mCurWb != null) ? _mCurWb.Name : "cEXWBxx";
            AllForms.m_frmLog.AppendToLog(wbname + "_WBSecurityProblem - Wininet Problem=" + e.problem.ToString());
        }

        private void CExwb1WindowClosing(object sender, WindowClosingEventArgs e)
        {
            //Set cancel to true, as we want to remove the browser ourselves
            e.Cancel = true;
            if (MessageBox.Show(this,
                @"A script has requested to close this window. Proceed?",
                @"Browser Closing Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.No)
                return;
            var pWb = (cEXWB)sender;
            if (pWb != null)
            {
                //RemoveBrowser methods do not remove the first browser
                //that was placed on the form in design time.
                if (pWb.Name == _mCurWb.Name) //current browser, active
                {
                    //Removes and activates the browser before this one
                    RemoveBrowser(pWb.Name);
                }
                else
                {
                    //Removes without deactivating the active one
                    RemoveBrowser2(pWb.Name, true);
                }
            }
            //AllForms.m_frmLog.AppendToLog("frmMain_cEXWB1_WindowClosing");
        }

        private void cEXWB1_WBKeyUp(object sender, WBKeyUpEventArgs e)
        {

        }

        private void CExwb1WbKeyDown(object sender, WBKeyDownEventArgs e)
        {
            //Consume keys here, if needed
            try
            {
                if (e.virtualkey == Keys.ControlKey)
                {
                    switch (e.keycode)
                    {
                        case Keys.F:
                            _mFrmFind.Show(this);
                            e.handled = true;
                            break;
                        case Keys.N:
                            AddNewBrowser(MBlank, MAboutBlank, MAboutBlank, true);
                            e.handled = true;
                            break;
                        case Keys.O:
                            AddNewBrowser(MBlank, MAboutBlank, MAboutBlank, true);
                            e.handled = true;
                            break;
                    }
                }
            }
            catch (Exception eex)
            {
                if (_mCurWb != null)
                    AllForms.m_frmLog.AppendToLog(_mCurWb.Name + "_WBKeyDown\r\n" + eex);
                else
                    AllForms.m_frmLog.AppendToLog("cEXWBxx_WBKeyDown\r\n" + eex);            
            }
        }

        private void CExwb1DownloadBegin(object sender)
        {
            if (sender != _mCurWb)
                return;
            tsProgress.Visible = true;
        }

        private void CExwb1DownloadComplete(object sender)
        {
            if (sender != _mCurWb)
                return;
            tsProgress.Value = 0;
            tsProgress.Maximum = 0;
            tsProgress.Visible = false;
        }

        private void cEXWB1_RefreshBegin(object sender)
        {
            //AllForms.m_frmLog.AppendToLog("cEXWB1_RefreshBegin");
        }

        private void cEXWB1_RefreshEnd(object sender)
        {
            //AllForms.m_frmLog.AppendToLog("cEXWB1_RefreshEnd");
        }

        private void cEXWB1_FileDownload(object sender, FileDownloadEventArgs e)
        {
            //Here is the easiest way to find out the download file name
            //m_Status is set in StatusTextChange event handler
            //After the user has clicked a downloadable link, we get a 
            //BeforeNavigate2 and then at least two calls to StatusTextChange
            //One containing a text such as below
            //Start downloading from site: http://www.codeproject.com/cs/media/cameraviewer/cv_demo.zip
            //and one that sends an empty string to clear the status text.
            //After the status calls, we should get this event fired.
            //AllForms.m_frmLog.AppendToLog("cEXWBxx_FileDownload\r\n" + m_Status);

            //Here you can cancel the download and take over.
            //e.Cancel = true;
        }
        
        private void CExwb1WbAuthenticate(object sender, AuthenticateEventArgs e)
        {
            if (_mFrmAuth.ShowDialogInternal(this) == DialogResult.OK)
            {
                //Default value of handled is false
                e.handled = true;
                //To pass a doamin as in a NTLM authentication scheme,
                //use this format: Username = Domain\username
                e.username = _mFrmAuth.m_Username;
                e.password = _mFrmAuth.m_Password;
                e.retvalue = 0;
                //Default value of retValue is 0 or S_OK
            }
        }
     
        private void CExwb1WbDragDrop(object sender, WBDropEventArgs e)
        {
            if (e.dataobject == null)
                return;
            if (e.dataobject.ContainsText())
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop\r\n" + e.dataobject.GetText());
            else if (e.dataobject.ContainsFileDropList())
            {
                System.Collections.Specialized.StringCollection files = e.dataobject.GetFileDropList();
                if (files.Count > 1)
                    MessageBox.Show(@"Can not do multi file drop!");
                else
                {
                    if(_mCurWb != null)
                        _mCurWb.Navigate(files[0]);
                }
            }
            else
            {
                //Example of how to catch a dragdrop of a ListViewItem from frmCacheCookie form
                object obj = e.dataobject.GetData("WindowsForms10PersistentObject");
                if (obj != null)
                {
                    if (obj is ListViewItem)
                    {
                        var ctl = (ListViewItem)obj;
                        AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop\r\n" + ctl.Text);
                    }
                }
            }
            //To get the available formats
            //string[] formats = obja.GetFormats();
            //foreach (string str in formats)
            //{
            //    Debug.Print("\r\n" + str);
            //}
        }

        void cEXWB1_WBStop(object sender)
        {
            AllForms.m_frmLog.AppendToLog("STOPPED");
        }

        private int _mMposX;
        private int _mMposY;
        void CExwb1WblButtonUp(object sender, HTMLMouseEventArgs e)
        {
            if (e.SrcElement != null)
            {
                //user is scrolling using scrollbars
                //if (e.SrcElement.tagName == "HTML")
                //    return;
                //If DIV then we can look for an HTML child element
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBLButtonUp==>" + e.SrcElement.tagName);
                if (Recording)
                    _scriptManager.AddClick(cEXWB1.Name, e.SrcElement, CurrentBrowserControl.LocationUrl);
            }
            else
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBLButtonUp");

            var rt = new Rectangle(_mMposX - 1, _mMposY - 1, 2, 2);
            if (rt.Contains(e.ClientX, e.ClientY))
            {
                AllForms.m_frmLog.AppendToLog("MOUSE CLICKED");
            }
        }

        void CExwb1WblButtonDown(object sender, HTMLMouseEventArgs e)
        {
            _mMposX = e.ClientX;
            _mMposY = e.ClientY;
            if (e.SrcElement != null)
            {
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBLButtonDown==>" + e.SrcElement.tagName);                
            }
            else
                AllForms.m_frmLog.AppendToLog("cEXWB1_WBLButtonDown");
        }



        //void cEXWB1_WBMouseMove(object sender, csExWB.HTMLMouseEventArgs e)
        //{
        //    if (e.SrcElement != null)
        //    {
        //        AllForms.m_frmLog.AppendToLog("cEXWB1_WBMouseMove==>" + e.SrcElement.tagName);
        //    }
        //    else
        //        AllForms.m_frmLog.AppendToLog("cEXWB1_WBMouseMove");
        //}

        ////To stop security dialog in regard to viewing mix content
        ////if (e.urlAction == URLACTION.HTML_MIXED_CONTENT)
        ////{
        ////e.handled = true;
        ////e.urlPolicy = URLPOLICY.ALLOW
        ////}
        //////ShockwaveFlash.ShockwaveFlash.9
        ////Guid flash = new Guid("D27CDB6E-AE6D-11cf-96B8-444553540000");
        //void cEXWB1_ProcessUrlAction(object sender, csExWB.ProcessUrlActionEventArgs e)
        //{
        //    //if (e.urlAction == URLACTION.SCRIPT_RUN)
        //    //{
        //    //    AllForms.m_frmLog.AppendToLog("cEXWB1_ProcessUrlAction==>SCRIPT_RUN");
        //    //}
        //    //CLIENT_CERT_PROMPT
        //    //if (e.urlAction == URLACTION.ACTIVEX_RUN)
        //    //{
        //    //    if (e.context == flash)
        //    //    {
        //    //        AllForms.m_frmLog.AppendToLog("cEXWB1_ProcessUrlAction\r\n" + e.context.ToString());
        //    //        e.handled = true;
        //    //        e.urlPolicy = URLPOLICY.DISALLOW;
        //    //    }
        //    //}
        //}

        #endregion

        #region File Download Events

        //private const string RESPONSE_CONTENT_LENGTH = "content-length:";
        //private const string SPACE_CHAR = " ";
        //private const string NEW_LINE = "\r\n";
        //private int m_Index = 0;
        //private int m_Begin = 0;
        //private int m_End = 0;
        //private string m_FileSize = string.Empty;
        private int _mLFileSize;

        /// <summary>
        /// Called from frmFileDownload to stop a file download
        /// </summary>
        /// <param name="browsername"></param>
        /// <param name="dlUid"></param>
        public void StopFileDownload(string browsername, int dlUid)
        {
            cEXWB pWb = FindBrowser(browsername);
            if (pWb != null)
                pWb.StopFileDownload(dlUid);
        }

        void CExwb1FileDownloadExAuthenticate(object sender, FileDownloadExAuthenticateEventArgs e)
        {
            var pWb = (cEXWB)sender;
            try
            {
                if (_mFrmAuth.ShowDialogInternal(this) == DialogResult.OK)
                {
                    e.Cancel = false; //default value true
                    //To pass a doamin as in a NTLM authentication scheme,
                    //use this format: Username = Domain\username
                    e.username = _mFrmAuth.m_Username;
                    e.password = _mFrmAuth.m_Password;
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(pWb.Name + "_FileDownloadExAuthenticate" + ee);
            }
        }

        void CExwb1FileDownloadExError(object sender, FileDownloadExErrorEventArgs e)
        {
            var pWb = (cEXWB)sender;
            try
            {
                _mFrmFileDownload.DeleteDownloadItem(pWb.Name, e.m_dlUID, e.m_URL,  "Error");
                AllForms.m_frmLog.AppendToLog("An error occured during downloading of " + e.m_URL + "\r\n" + e.m_ErrorMsg);

            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(pWb.Name + "_FileDownloadExError" + ee);
            }
        }

        void CExwb1FileDownloadExProgress(object sender, FileDownloadExProgressEventArgs e)
        {
            var pWb = (cEXWB)sender;
            try
            {
                _mFrmFileDownload.UpdateDownloadItem(pWb.Name, e.m_dlUID, e.m_URL, e.m_Progress, e.m_ProgressMax);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(pWb.Name + "_FileDownloadExProgress" + ee);
            }
        }

        void CExwb1FileDownloadExEnd(object sender, FileDownloadExEndEventArgs e)
        {
            var pWB = (cEXWB)sender;
            try
            {
                _mFrmFileDownload.DeleteDownloadItem(pWB.Name, e.m_dlUID, e.m_URL,  "Completed");
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(pWB.Name + "_FileDownloadExEnd" + ee);
            }
        }

        void cEXWB1_FileDownloadExDownloadFileFullyWritten(object sender, FileDownloadExDownloadFileFullyWrittenEventArgs e)
        {
            AllForms.m_frmLog.AppendToLog("cEXWB1_FileDownloadExDownloadFileFullyWritten");
        }

        //A typical file attachment header from hotmail
        //HTTP/1.1 200 OK
        //Date: Mon, 20 Aug 2007 12:02:06 GMT
        //Server: Microsoft-IIS/6.0
        //P3P: CP="BUS CUR CONo FIN IVDo ONL OUR PHY SAMo TELo"
        //xxn:18
        //X-Powered-By: ASP.NET
        //Content-Length: 24955
        //Content-Disposition: attachment; filename="TabStrip.zip"
        //MSNSERVER: H: BAY139-W18 V: 11.10.0.115 D: 2007-07-09T20:51:32
        //Set-Cookie: KVC=11.10.0000.0115; domain=.mail.live.com; path=/
        //Cache-Control: private
        //Content-Type: application/x-zip-compressed

        //http://data.unaids.org/pub/GlobalReport/2006/Annex2_Data_en.xls
        void CExwb1FileDownloadExStart(object sender, FileDownloadExEventArgs e)
        {
            var pWB = (cEXWB)sender;
            try
            {
                AllForms.m_frmLog.AppendToLog("cEXWB1_FileDownloadExStart\r\n" + e.m_URL +
                    "\r\n" + e.m_Filename + "\r\n" + e.m_Ext + "\r\n" + e.m_FileSize + 
                    "\r\n" + e.m_ExtraHeaders);
                _mLFileSize = !string.IsNullOrEmpty(e.m_FileSize) ? int.Parse(e.m_FileSize) : 0;
                //Send progress events. default false
                e.m_SendProgressEvents = true;
                //The initial value of FileDownloadDirectory defaults to MyDocuments dir
                e.m_PathToSave = pWB.FileDownloadDirectory + e.m_Filename;
                _mFrmFileDownload.AddDownloadItem(pWB.Name, e.m_Filename, e.m_URL,  e.m_dlUID, e.m_URL, e.m_PathToSave, _mLFileSize);
                if (!_mFrmFileDownload.Visible)
                    _mFrmFileDownload.Show(this);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(pWB.Name + "_FileDownloadExStart" + ee);
            }
        }
        #endregion

        private void TsbExpandActionPanelClick(object sender, EventArgs e)
        {
            pnlActions.Visible = !pnlActions.Visible;
        }

        private void TsLoadScriptClick(object sender, EventArgs e)
        {
            if (dlgLoadScript.ShowDialog()!=DialogResult.OK) return;
            _scriptManager.ClearActions();
            _scriptManager.LoadActionsFromFile(dlgLoadScript.FileName);
        }

        private void TsSaveScriptClick(object sender, EventArgs e)
        {
            var saveDlg = new SaveScriptDialogControl();
            saveDlg.FileDlgFilter = Settings.GetConfigSettingString("CodeSaveFileDialogFilter");
            if (saveDlg.ShowDialog(this) != DialogResult.OK) return;
            if (saveDlg.GetSelectedFilter() == "*.xml")
                _scriptManager.SaveActionsToFile(saveDlg.FileDlgFileName);
            else _scriptManager.SaveCodeToFile((CodeTemplate)saveDlg.ddlTemplate.SelectedItem, saveDlg.FileDlgFileName);
        }

        private void TsRemoveActionClick(object sender, EventArgs e)
        {
            if (lvActions.SelectedItems.Count != 1) return;
            _scriptManager.RemoveAction((ActionBase)lvActions.SelectedItems[0].Tag);
        }

        private void TsClearActionsClick(object sender, EventArgs e)
        {
            _scriptManager.ClearActions();
        }

        private void TsMoveActionUpClick(object sender, EventArgs e)
        {
            if (lvActions.SelectedItems.Count != 1 || lvActions.SelectedIndices[0]==0) return;
            int index = _scriptManager.MoveUp((ActionBase)lvActions.SelectedItems[0].Tag);
            lvActions.SelectedIndices.Add(index);
        }

        private void TsMoveActionDownClick(object sender, EventArgs e)
        {
            if (lvActions.SelectedItems.Count != 1 || lvActions.SelectedIndices[0] == lvActions.Items.Count-1) return;
            int index = _scriptManager.MoveDown((ActionBase)lvActions.SelectedItems[0].Tag);
            lvActions.SelectedIndices.Add(index);
        }

        private void ActionAdded(ActionBase action, int index)
        {
            string actionType = action.GetType().IsSubclassOf(typeof (ActionElementBase)) ? ((ActionElementBase)action).ActionFinder.TagName:action.ActionName;
            int imageIdx = 6; // TODO: determine the image to show, based on tag type
            switch (actionType.ToLower())
            {
                case "checkbox":
                    imageIdx = 0;
                    break;
                case "combo":
                    imageIdx = 1;
                    break;
                case "edit":
                    imageIdx = 2;
                    break;
                case "button":
                    imageIdx = 3;
                    break;
                case "radio":
                    imageIdx = 4;
                    break;
                case "typing":
                    imageIdx = 5;
                    break;
                case "generic click":
                    imageIdx = 6;
                    break;
                case "back":
                    imageIdx = 7;
                    break;
                case "forward":
                    imageIdx = 8;
                    break;
                case "refresh":
                    imageIdx = 9;
                    break;
                case "navigate":
                    imageIdx = 10;
                    break;
                case "sleep":
                    imageIdx = 11;
                    break;
                case "alert":
                    imageIdx = 12;
                    break;
                case "close":
                    imageIdx = 13;
                    break;
            }

            ListViewItem item = lvActions.Items.Insert(index, actionType, imageIdx);
            item.Tag = action;
            item.SubItems.Add(action.Description);
        }

        private void ActionRemoved(ActionBase action, int index)
        {
            lvActions.Items.RemoveAt(index);
        }

        private void ActionModified(ActionBase action)
        {
            foreach (ListViewItem item in lvActions.Items)
            {
                if (item.Tag == action)
                {
                    item.SubItems[1].Text = action.Description;
                    return;
                }
            }
        }

        private void TsRecordStartCheckedChanged(object sender, EventArgs e)
        {
            Recording = tsRecordStart.Checked;
            tsRecordStart.BackColor = Recording ? Color.Red : SystemColors.Control;
        }

        private void lvActions_DoubleClick(object sender, EventArgs e)
        {
            if (lvActions.SelectedIndices.Count == 0) return;
            var frm = new frmModifyFinder();
            var action = (ActionBase) lvActions.SelectedItems[0].Tag;
            if (!action.GetType().IsSubclassOf(typeof(ActionElementBase))) return;

            var actionelement = (ActionElementBase) action;
            frm.SetCheckList(actionelement);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                 _scriptManager.ChangeActionFinder(actionelement, frm.GetChecked());
            }
        }

    }



}
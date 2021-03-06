using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using IfacesEnumsStructsClasses;
using System.Runtime.InteropServices;

namespace TestRecorder
{
    public partial class frmHTMLeditor : Form 
    {

        public frmHTMLeditor()
        {
            InitializeComponent();
            cEXWB1.TitleChange += new csExWB.TitleChangeEventHandler(cEXWB1_TitleChange);
            editorfontCombo.InitComboBox(ToolStripCustomComboBox.ToolStripComboBoxType.Type_Font);

            selectionbackcolorSelector.Selector.SelectionChanged += new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            selectionforecolorSelector1.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            docbackcolorselector.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            docforecolorselector.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            doclinkcolorselector.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            docalinkcolorselector.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);
            docvlinkcolorselector.Selector.SelectionChanged +=new HtmlColorSelectorEventHandler(Selector_SelectionChanged);

            //fontsizes
            //Hopefully the fonts and the sizes should
            //update themselves
            SetupComboFontSize();
        }

        #region Private variables and methods

        //This class is used to capture and relay HTMLElementEvents2 of Document object
        private csExWB.HTMLElementEvents m_docelemevents = new csExWB.HTMLElementEvents();

        //Html editing helper class
        private HTMLEditHelper htmledit = new HTMLEditHelper();

        private frmTable m_frmTable = new frmTable();
        private frmAnchor m_frmAnchor = new frmAnchor();
        private frmTableCellProp m_frmTableCellProp = new frmTableCellProp();
        private frmFind m_frmFind = new frmFind();

        //Is used in browser context menu event to hold a ref
        //to the HTML element under the mouse for further processing
        private IHTMLElement m_oHTMLCtxMenu = null;
        //Keep track of the element last clicked
        private IHTMLElement m_pClickedElem = null;

        private void SynchEditButtons()
        {
            tsBtnSelectAll.Enabled = this.cEXWB1.IsCommandEnabled("SelectAll");
            //tsBtnCopy.Enabled = this.cEXWB1.IsCommandEnabled("Copy");
            tsBtnCut.Enabled = this.cEXWB1.IsCommandEnabled("Cut");
            tsBtnCopy.Enabled = tsBtnCut.Enabled;
            tsBtnRedo.Enabled = this.cEXWB1.IsCommandEnabled("Redo");
            tsBtnUndo.Enabled = this.cEXWB1.IsCommandEnabled("Undo");
            if ((Clipboard.ContainsText(TextDataFormat.Text)) ||
                (Clipboard.ContainsText(TextDataFormat.Html)))
                tsBtnPaste.Enabled = true;

            cutToolStripMenuItem.Enabled = tsBtnCut.Enabled;
            copyToolStripMenuItem.Enabled = tsBtnCopy.Enabled;
            pasteToolStripMenuItem.Enabled = tsBtnPaste.Enabled;

            tsBtnLeftAlign.Checked = this.cEXWB1.QueryCommandState(true, "JustifyLeft");
            tsBtnRightAlign.Checked = this.cEXWB1.QueryCommandState(true, "JustifyRight");
            tsBtnCenterAlign.Checked = this.cEXWB1.QueryCommandState(true, "JustifyCenter");

            tsBtnBold.Checked = this.cEXWB1.QueryCommandState(true, "Bold");
            tsBtnItalic.Checked = this.cEXWB1.QueryCommandState(true, "Italic");
            tsBtnUnderline.Checked = this.cEXWB1.QueryCommandState(true, "Underline");

            tsBtnBulletList.Checked = this.cEXWB1.QueryCommandState(true, "InsertUnorderedList");
            tsBtnNumberList.Checked = this.cEXWB1.QueryCommandState(true, "InsertOrderedList");
        }

        private void AdjustForHeading(string sTag)
        {
            if (string.IsNullOrEmpty(sTag))
                return;
            int index = 0;
            if (sTag == "H1")
                index = 5; //24pt
            else if (sTag == "H2")
                index = 4; //18pt
            else if (sTag == "H3")
                index = 3; //14pt
            else if (sTag == "H4")
                index = 2; //12pt
            else if (sTag == "H5")
                index = 1; //10pt
            else if (sTag == "H6")
                index = 0; //8pt
            else
                return; //do nothing
            m_InternalCall = true;
            tsComboFontSize.SelectedIndex = index;
        }

        private void SynchFont(string sTagName)
        {
            //Times Roman New
            string fontname = string.Empty;

            object obj = cEXWB1.QueryCommandValue("FontName");
            if (obj == null)
                return;
            fontname = obj.ToString();
            obj = cEXWB1.QueryCommandValue("FontSize");
            if (obj == null)
                return;

            //Could indicate a headingxxx, P, or BODY
            m_InternalCall = true;
            if (obj.ToString().Length > 0)
                tsComboFontSize.SelectedIndex = (int)Convert.ToInt32(obj) - 1; //x (x - 1)
            else
                AdjustForHeading(sTagName);

            editorfontCombo.SelectedFontNameItem = fontname;
        }

        #endregion

        #region Form Events
 
        private void frmHTMLeditor_Load(object sender, EventArgs e)
        {
            try
            {
                //Setup icons...
                tsBtnBold.Image = AllForms.m_imgListMain.Images[51];
                tsBtnItalic.Image = AllForms.m_imgListMain.Images[52];
                tsBtnUnderline.Image = AllForms.m_imgListMain.Images[53];
                tsBtnLeftAlign.Image = AllForms.m_imgListMain.Images[56];
                tsBtnCenterAlign.Image = AllForms.m_imgListMain.Images[54];
                tsBtnRightAlign.Image = AllForms.m_imgListMain.Images[57];
                tsBtnFullAlign.Image = AllForms.m_imgListMain.Images[55];
                tsBtnNumberList.Image = AllForms.m_imgListMain.Images[49];
                tsBtnBulletList.Image = AllForms.m_imgListMain.Images[50];
                tsBtnNew.Image = AllForms.m_imgListMain.Images[19];
                tsBtnOpenEdit.Image = AllForms.m_imgListMain.Images[43];
                tsBtnSave.Image = AllForms.m_imgListMain.Images[21];
                tsBtnPrint.Image = AllForms.m_imgListMain.Images[8];
                tsBtnCut.Image = AllForms.m_imgListMain.Images[23];
                tsBtnCopy.Image = AllForms.m_imgListMain.Images[24];
                tsBtnPaste.Image = AllForms.m_imgListMain.Images[25];
                tsBtnSelectAll.Image = AllForms.m_imgListMain.Images[28];
                tsBtnUndo.Image = AllForms.m_imgListMain.Images[26];
                tsBtnRedo.Image = AllForms.m_imgListMain.Images[27];
                tsBtnTable.Image = AllForms.m_imgListMain.Images[47];
                tsBtnPicture.Image = AllForms.m_imgListMain.Images[46];
                tsBtnLink.Image = AllForms.m_imgListMain.Images[48];
                tsBtnBR.Image = AllForms.m_imgListMain.Images[61];
                tsBtnHorizontalLine.Image = AllForms.m_imgListMain.Images[63];
                tsBtnIndent.Image = AllForms.m_imgListMain.Images[64];
                tsBtnOutdent.Image = AllForms.m_imgListMain.Images[65];
                tsBtnRemoveFormat.Image = AllForms.m_imgListMain.Images[15];

                this.Icon = AllForms.BitmapToIcon(11);

                //Generates an error, complaining that System.XmlSerializers.dll
                //does not exists, a known bug????
                //Need to save and load internally
                ////Setup HTML tags
                foreach (string obj in m_tags)
                {
                    treeHTMLTags.Nodes.Add(obj);
                }
                
                //Setup richtextbox highlighting, a bit slow
                htmlRichTextBox1.SetupHighLighting(m_tags);
                htmlRichTextBox1.SuppressHightlighting = false;

                //To handle external dragdrop - files,...
                this.cEXWB1.WBDragDrop += new csExWB.WBDropEventHandler(cEXWB1_WBDragDrop);

                //Setup html events
                //To be notified when and how an object is being resized
                cEXWB1.HTMLEditHostSnapRect += new csExWB.HTMLEditHostSnapRectEventHandler(cEXWB1_HTMLEditHostSnapRect);
                //Register
                cEXWB1.RegisterAsBrowser = true; //using default webbrowser dragdrop
                
                //Setup document events
                m_docelemevents.elemonclick += new csExWB.HTMLElementEventHandler(m_docelemevents_elemonclick);
                m_docelemevents.elemonmousemove += new csExWB.HTMLElementEventHandler(m_docelemevents_elemonmousemove);
                m_docelemevents.elemonkeyup += new csExWB.HTMLElementEventHandler(m_docelemevents_elemonkeyup);
                m_docelemevents.elemoncontextmenu += new csExWB.HTMLElementEventHandler(m_docelemevents_elemoncontextmenu);
                m_docelemevents.elemondrag += new csExWB.HTMLElementEventHandler(m_docelemevents_elemondrag);
                m_docelemevents.elemondragstart += new csExWB.HTMLElementEventHandler(m_docelemevents_elemondragstart);
                m_docelemevents.elemondragend += new csExWB.HTMLElementEventHandler(m_docelemevents_elemondragend);
                
                //Initialize webbrowser events
                cEXWB1.NavToBlank();
                //Enter design mode
                this.cEXWB1.SetDesignMode("on");
                
                //Using our Find dlg
                m_frmFind.FindInPageEvent += new FindInPage(m_frmFind_FindInPageEvent);

                ////To handle drops, does not have eny effect on browsers
                ////dragdrop functionality.
                //this.AllowDrop = true;
                //this.DragEnter += new DragEventHandler(frmHTMLeditor_DragEnter);
                ////this.DragDrop += new DragEventHandler(frmHTMLeditor_DragDrop);
                //this.DragLeave += new EventHandler(frmHTMLeditor_DragLeave);
                ////this.DragOver += new DragEventHandler(frmHTMLeditor_DragOver);
                this.Left = 0;
                this.Top = 0;
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("frmHTMLeditor_Load\r\n" + ee.ToString());
            }
        }

        ////This is called, but not needed
        //void frmHTMLeditor_DragOver(object sender, DragEventArgs e)
        //{
        //    AllForms.m_frmLog.AppendToLog("frmHTMLeditor_DragOver");
        //}

        ////This is called instead of DragDrop???
        //void frmHTMLeditor_DragLeave(object sender, EventArgs e)
        //{
        //    AllForms.m_frmLog.AppendToLog("frmHTMLeditor_DragLeave");
        //}

        ////Never gets called
        //void frmHTMLeditor_DragDrop(object sender, DragEventArgs e)
        //{
        //    AllForms.m_frmLog.AppendToLog("frmHTMLeditor_DragDrop");
        //}

        //void frmHTMLeditor_DragEnter(object sender, DragEventArgs e)
        //{
        //    IDataObject dataobj = e.Data;
        //    //string[] sformats = dataobj.GetFormats();
        //    //foreach (string ite in sformats)
        //    //{
        //    //    AllForms.m_frmLog.AppendToLog("frmHTMLeditor_DragEnter ==>" + ite);
        //    //}
        //    //or "FileName" or "FileNameW"
        //    string[] fnames = (string[])dataobj.GetData("FileDrop");
        //    foreach(string item in fnames)
        //    {
        //        AllForms.m_frmLog.AppendToLog("frmHTMLeditor_DragEnter ==>" + item);
        //        //only the first filename is used
        //        break;
        //    }
        //}

        void m_frmFind_FindInPageEvent(string FindPhrase, bool MatchWholeWord, bool MatchCase, bool Downward, bool HighlightAll, string sColor)
        {
            try
            {
                if (HighlightAll)
                {
                    int found = cEXWB1.FindAndHightAllInPage(FindPhrase, MatchWholeWord, MatchCase, sColor, "black");
                    MessageBox.Show(this, "Found " + found.ToString() + " matches.", "Finf in Page", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (cEXWB1.FindInPage(FindPhrase, Downward, MatchWholeWord, MatchCase, true) == false)
                        MessageBox.Show(this, "No more matches found for " + FindPhrase, "Find in Page", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("HTMLEditor_m_frmFind_FindInPageEvent\r\n" + ee.ToString());
            }
        }

        private void frmHTMLeditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void frmHTMLeditor_Activated(object sender, EventArgs e)
        {
            try
            {
                if(tabControl1.SelectedIndex == 0)
                    this.cEXWB1.SetFocus();
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("frmHTMLeditor_Activated\r\n" + ee.ToString());
            }
        }

        private void frmHTMLeditor_Shown(object sender, EventArgs e)
        {
            try
            {
                SynchEditButtons();
                SynchFont(string.Empty);
                ////This should initialize the document to have some sort of basic structure
                //string html = "<HTML><HEAD><meta http-equiv=\"Content-Type\" content=\"text/html; charset=windows-1252\"></Head><title>New Page</title><Body><P>&nbsp;</P><Body></HTML>";
                //this.cEXWB1.LoadHtmlIntoBrowser(html);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("frmHTMLeditor_Shown\r\n" + ee.ToString());
            }
        }

        private void tsMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Name == tsBtnBulletList.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_UNORDERLIST);
                }
                else if (e.ClickedItem.Name == tsBtnNumberList.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_ORDERLIST);
                }
                else if (e.ClickedItem.Name == tsBtnLeftAlign.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_JUSTIFYLEFT);
                }
                else if (e.ClickedItem.Name == tsBtnCenterAlign.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_JUSTIFYCENTER);
                }
                else if (e.ClickedItem.Name == tsBtnRightAlign.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_JUSTIFYRIGHT);
                }
                else if (e.ClickedItem.Name == tsBtnFullAlign.Name)
                {
                    //MSDN, not curently supported
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_JUSTIFYFULL);
                }
                else if (e.ClickedItem.Name == tsBtnBold.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_BOLD);
                }
                else if (e.ClickedItem.Name == tsBtnItalic.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_ITALIC);
                }
                else if (e.ClickedItem.Name == tsBtnUnderline.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_UNDERLINE);
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsMain_ItemClicked\r\n" + ee.ToString());
            }
        }

        private void tsFile_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Name == tsBtnNew.Name)
                {
                    cEXWB1.Clear();
                }
                else if (e.ClickedItem.Name == tsBtnSave.Name)
                {
                    cEXWB1.SaveAs();
                }
                else if (e.ClickedItem.Name == tsBtnPrint.Name)
                {
                    //cEXWB1.DocumentTitle = "My Title";
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_PRINTPREVIEW);
                    //cEXWB1.PrintPreview();
                }
                else if (e.ClickedItem.Name == tsBtnCut.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_CUT);
                    SynchEditButtons();
                    SynchFont(string.Empty);
                }
                else if (e.ClickedItem.Name == tsBtnCopy.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_COPY);
                }
                else if (e.ClickedItem.Name == tsBtnPaste.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_PASTE);
                    SynchEditButtons();
                    SynchFont(string.Empty);
                }
                else if (e.ClickedItem.Name == tsBtnSelectAll.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_SELECTALL);
                    SynchEditButtons();
                }
                else if (e.ClickedItem.Name == tsBtnTable.Name)
                {
                    m_frmTable.ShowDialog(this);
                    if (m_frmTable.m_Result == DialogResult.OK)
                    {
                        htmledit.AppendTable(m_frmTable.m_Cols, m_frmTable.m_Rows,
                            m_frmTable.m_BorderSize, m_frmTable.m_Alignment,
                            m_frmTable.m_CellPadding, m_frmTable.m_CellSpacing,
                            m_frmTable.m_WidthPercent, m_frmTable.m_WidthPixels,
                            m_frmTable.m_BackColor, m_frmTable.m_BorderColor,
                            m_frmTable.m_LightBorderColor, m_frmTable.m_DarkBorderColor);
                    }
                }
                else if (e.ClickedItem.Name == tsBtnLink.Name)
                {
                    string anchor = cEXWB1.GetSelectedText(true, false);
                    if (m_frmAnchor.ShowDialogInternal(this, anchor) == DialogResult.OK)
                    {
                        anchor = "<A href=\"" + m_frmAnchor.Address + "\" ";
                        if (!string.IsNullOrEmpty(m_frmAnchor.ScreenTip))
                            anchor += "title=\"" + m_frmAnchor.ScreenTip + "\" ";
                        if (m_frmAnchor.TargetFrameChecked)
                            anchor += "target=\"" + m_frmAnchor.TargetFrame + "\"";
                        anchor += ">" + m_frmAnchor.AnchorText + "</A>";
                        htmledit.PasteIntoSelection(anchor);
                    }
                    //cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_HYPERLINK);
                }
                else if (e.ClickedItem.Name == tsBtnPicture.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_IMAGE);
                }
                else if (e.ClickedItem.Name == tsBtnBR.Name)
                {
                    htmledit.EmbedBr();
                }
                else if (e.ClickedItem.Name == tsBtnIndent.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_INDENT);
                }
                else if (e.ClickedItem.Name == tsBtnOutdent.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_OUTDENT);
                }
                else if (e.ClickedItem.Name == tsBtnHorizontalLine.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_HORIZONTALLINE);
                }
                else if (e.ClickedItem.Name == tsBtnRedo.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_REDO);
                    SynchEditButtons();
                    //cEXWB1.ExecCommand(true, "Redo", false, null);
                }
                else if (e.ClickedItem.Name == tsBtnUndo.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_UNDO);
                    SynchEditButtons();
                    //cEXWB1.ExecCommand(true, "Undo", false, null);
                }
                else if (e.ClickedItem.Name == tsBtnRemoveFormat.Name)
                {
                    cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_REMOVEFORMAT);
                    SynchEditButtons();
                }
                
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsFile_ItemClicked\r\n" + ee.ToString());
            }
        }

        private void tsBtnOpenEdit_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == fileToolStripMenuItem.Name)
            {
                //To avoid MSHTML save dialog from displaying, either reset content using
                //LoadHtmlIntoBrowser with an empty page or load the file manually and then
                //call LoadHtmlIntoBrowser with the target source.
                cEXWB1.LoadHtmlIntoBrowser("<HTML><HEAD></Head><title>New Page</title><Body><P>&nbsp;</P></Body></HTML>");
                if (AllForms.ShowStaticOpenDialog(this, AllForms.DLG_HTMLS_FILTER,
                    string.Empty, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    true) == DialogResult.OK)
                {
                    //string src = string.Empty;
                    //using(System.IO.StreamReader reader = new System.IO.StreamReader(AllForms.m_dlgOpen.FileName))
                    //{
                    //    src = reader.ReadToEnd();
                    //}
                    //if(!string.IsNullOrEmpty(src))
                    //    this.cEXWB1.LoadHtmlIntoBrowser(src); //AllForms.m_dlgOpen.FileName
                    cEXWB1.Navigate(AllForms.m_dlgOpen.FileName);
                }
            }
            else if (e.ClickedItem.Name == currentBrowserToolStripMenuItem.Name)
            {

                Control ctl = this.Owner;
                if ((ctl != null) && (ctl is frmMain))
                {
                    csExWB.cEXWB wb = ((frmMain)ctl).CurrentBrowserControl;
                    if (wb != null)
                    {
                        string source = wb.GetSource(true);
                        if (!string.IsNullOrEmpty(source))
                        {
                            //Experimental
                            this.cEXWB1.LoadHtmlIntoBrowser(source);
                        }
                    }
                }
            }
            else if (e.ClickedItem.Name == clipboardToolStripMenuItem.Name)
            {
                if (Clipboard.ContainsText())
                {
                    string ssource = Clipboard.GetText();
                    if ( (!string.IsNullOrEmpty(ssource)) &&
                        (ssource.StartsWith("<HTML>", StringComparison.CurrentCultureIgnoreCase)) &&
                        (ssource.EndsWith("</HTML>", StringComparison.CurrentCultureIgnoreCase)))
                            this.cEXWB1.LoadHtmlIntoBrowser(ssource);
                }
            }
        }

        private void BrowserCtxMenuClickHandler(object sender, EventArgs e)
        {
            if (m_oHTMLCtxMenu == null)
                return;
            if (sender == cutToolStripMenuItem)
            {
                cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_CUT);
                SynchEditButtons();
                SynchFont(string.Empty);
            }
            else if (sender == copyToolStripMenuItem)
            {
                cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_COPY);
            }
            else if (sender == pasteToolStripMenuItem)
            {
                cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_PASTE);
                SynchEditButtons();
                SynchFont(string.Empty);
            }
            else if (sender == editLinkToolStripMenuItem)
            {
                IHTMLAnchorElement phref = (IHTMLAnchorElement)m_oHTMLCtxMenu;
                if (AllForms.m_frmInput.ShowDialogInternal(phref.href, this) == DialogResult.OK)
                {
                    //Normally, we check to see if this is a valid URL
                    if (AllForms.m_frmInput.GetInputBoxText().Length > 0)
                    {
                        phref.href = AllForms.m_frmInput.GetInputBoxText();
                    }
                }
            }
            else if (sender == undoLinkToolStripMenuItem)
            {
                cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_UNLINK);
            }
            else if( (sender == deleteLinkToolStripMenuItem) ||
                (sender == deleteImageToolStripMenuItem) )
            {
                if(AllForms.AskForConfirmation("Proceed to remove element?", this))
                    htmledit.RemoveNode(m_oHTMLCtxMenu, true);
            }
            else if (sender == editImageToolStripMenuItem)
            {
                IHTMLImgElement pimg = (IHTMLImgElement)m_oHTMLCtxMenu;
                if (AllForms.m_frmInput.ShowDialogInternal(pimg.src, this) == DialogResult.OK)
                {
                    //Normally, we check to see if this is a valid URL/File
                    if (AllForms.m_frmInput.GetInputBoxText().Length > 0)
                    {
                        pimg.src = AllForms.m_frmInput.GetInputBoxText();
                    }
                }
            }
            else if (sender == editTablePropertiesToolStripMenuItem)
            {
                //Fill in the m_frmTable fields, display and reset
            }
            else if (sender == editCellPropertiesToolStripMenuItem)
            {
                IHTMLTableCell cell = m_oHTMLCtxMenu as IHTMLTableCell;
                if (cell == null)
                    return;

                //Need to examine all cell properties
                //before passing them to frmTableCellProp

                try
                {
                    string align = string.Empty;
                    if (!string.IsNullOrEmpty(cell.align))
                        align = cell.align;

                    string valign = string.Empty;
                    if (!string.IsNullOrEmpty(cell.vAlign))
                        valign = cell.vAlign;

                    Color bgcolor = Color.Empty;
                    if (cell.bgColor != null)
                        bgcolor = ColorTranslator.FromHtml(cell.bgColor.ToString()); // "#003399"

                    Color bordercolor = Color.Empty;
                    if (cell.borderColor != null)
                        bordercolor = ColorTranslator.FromHtml(cell.borderColor.ToString());

                    Color bordercolorlight = Color.Empty;
                    if (cell.borderColorLight != null)
                        bordercolorlight = ColorTranslator.FromHtml(cell.borderColorLight.ToString());

                    Color bordercolordark = Color.Empty;
                    if (cell.borderColorDark != null)
                        bordercolordark = ColorTranslator.FromHtml(cell.borderColorDark.ToString());

                    m_frmTableCellProp.SetupValues(align, valign, bgcolor,
                        bordercolor, bordercolorlight,
                        bordercolordark, cell.noWrap);

                    m_frmTableCellProp.ShowDialog(this);
                    if (m_frmTableCellProp.m_Result == DialogResult.OK)
                    {
                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_VAlignment))
                            cell.vAlign = m_frmTableCellProp.m_VAlignment;

                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_HAlignment))
                            cell.align = m_frmTableCellProp.m_HAlignment;

                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_BackColor))
                            cell.bgColor = m_frmTableCellProp.m_BackColor;
                        else
                            cell.bgColor = null;

                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_BorderColor))
                            cell.borderColor = m_frmTableCellProp.m_BorderColor;
                        else
                            cell.borderColor = null;

                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_LightBorderColor))
                            cell.borderColorLight = m_frmTableCellProp.m_LightBorderColor;
                        else
                            cell.borderColorLight = null;

                        if (!string.IsNullOrEmpty(m_frmTableCellProp.m_DarkBorderColor))
                            cell.borderColorDark = m_frmTableCellProp.m_DarkBorderColor;
                        else
                            cell.borderColorDark = null;

                        cell.noWrap = m_frmTableCellProp.m_NoWrap;
                    }
                }
                catch (Exception eTabelCellProp)
                {
                    AllForms.m_frmLog.AppendToLog("frmHTMLEditor.Cellproperties\r\n" + eTabelCellProp.ToString());
                }
            }
            else if (sender == insertColToolStripMenuItem)
            {
                IHTMLTableCell cell = m_oHTMLCtxMenu as IHTMLTableCell;
                int index = 0;
                if (cell != null)
                    index = cell.cellIndex;
                IHTMLTable table = htmledit.GetParentTable(m_oHTMLCtxMenu);
                if (table == null)
                    return;
                htmledit.InsertCol(table, index); //Need to account for width factor
            }
            else if (sender == insertRowToolStripMenuItem)
            {
                IHTMLTableCell cell = m_oHTMLCtxMenu as IHTMLTableCell;
                IHTMLTableRow row = htmledit.GetParentRow(m_oHTMLCtxMenu);
                int index = 0;
                if (row != null)
                    index = row.rowIndex;
                IHTMLTable table = htmledit.GetParentTable(m_oHTMLCtxMenu);
                if (table == null)
                    return;
                htmledit.InsertRow(table, index, htmledit.Row_GetCellCount(row));
            }
            else if (sender == deleteColToolStripMenuItem)
            {
                IHTMLTableCell cell = m_oHTMLCtxMenu as IHTMLTableCell;
                if (cell == null)
                    return;
                int index = cell.cellIndex;
                if (index < 0)
                    index = 0;
                IHTMLTable table = htmledit.GetParentTable(m_oHTMLCtxMenu);
                if (table == null)
                    return;
                htmledit.DeleteCol(table, index);
            }
            else if (sender == deleteRowToolStripMenuItem)
            {
                IHTMLTableRow row = htmledit.GetParentRow(m_oHTMLCtxMenu);
                int index = 0;
                if (row != null)
                    index = row.rowIndex;
                IHTMLTable table = htmledit.GetParentTable(m_oHTMLCtxMenu);
                if (table == null)
                    return;
                htmledit.DeleteRow(table, index);
            }
            else if (sender == viewSourceToolStripMenuItem)
            {
                tabControl1.SelectedTab = tabPageSource;
                //cEXWB1.OleCommandExec(true, MSHTML_COMMAND_IDS.IDM_VIEWSOURCE);
            }
        }
        
        private void treeHTMLTags_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if(e.Node != null)
                {
                    string source = "<" + e.Node.Text + ">";
                    //Do we have a selection?
                    if (htmlRichTextBox1.SelectionLength > 0)
                        source = source + htmlRichTextBox1.SelectedText;
                    source = source + "</" + e.Node.Text + ">";
                    htmlRichTextBox1.Focus();
                    htmlRichTextBox1.SelectedText = source;
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("treeHTMLTags_NodeMouseClick\r\n" + ee.ToString());
            }
        }

        private void treeHTMLTags_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            if (e.Node == null)
                return;
            e.Node.ToolTipText = "Click to insert or surround selection with\r\n<" + e.Node.Text + "> tag.";
        }

        #endregion

        #region Webbrowser Events

        private void cEXWB1_DocumentComplete(object sender, csExWB.DocumentCompleteEventArgs e)
        {
            try
            {
                ////A liitle trick to get the IHTMLElement of the document object
                ////so we can sink events, using IHTMLDocument2 does not work, neither QIing
                ////IHTMLDocument2 for IHTMLElement
                //IHTMLDocument3 pBody = ((IWebBrowser2)e.browser).Document as IHTMLDocument3;
                //m_elemEvents.ConnectToHtmlEvents(pBody.documentElement);

                //IHTMLDocument2 pDoc2 = ((IWebBrowser2)e.browser).Document as IHTMLDocument2;
                ////Set the htmleditor document object
                //htmledit.Document2 = pDoc2;
                //if (!this.Visible)
                //    return;
                
                IWebBrowser2 wb = e.browser as IWebBrowser2;
                if( (wb == null) || (wb.Document == null) )
                    return;
                //this.cEXWB1.WebbrowserObject.Document
                htmledit.DOM = wb.Document;
                if ((htmledit.Document3 != null) && (htmledit.Document3.documentElement != null))
                    m_docelemevents.ConnectToHtmlElementEvents(htmledit.Document3.documentElement);
                    //m_elemEvents.ConnectToHtmlEvents(htmledit.Document3.documentElement);

                if (e.url == "about:blank")
                    return;
                //In response to openning a file
                //See if the document has any bgcolor, fgcolors, ... get them
                if (htmledit.Document2 != null)
                    SynchDocumentColorCombos(htmledit.Document2);
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog(this.Name + "_cEXWB1_DocumentComplete\r\n" + ee.ToString());
            }
        }

        private void cEXWB1_StatusTextChange(object sender, csExWB.StatusTextChangeEventArgs e)
        {
            tsStatus.Text = e.text;
        }

        private void cEXWB1_BeforeNavigate2(object sender, csExWB.BeforeNavigate2EventArgs e)
        {
            //m_elemEvents.DisconnectHtmlEvents();
            if(e.istoplevel)
                m_docelemevents.DisconnectHtmlElementEvents();
        }

        private void cEXWB1_WBKeyDown(object sender, csExWB.WBKeyDownEventArgs e)
        {
            //Consume keys
            try
            {
                if (e.virtualkey == Keys.ControlKey)
                {
                    switch (e.keycode)
                    {
                        case Keys.F:
                            m_frmFind.Show(this);
                            e.handled = true;
                            break;
                        case Keys.N:
                            e.handled = true;
                            break;
                        case Keys.O:
                            e.handled = true;
                            break;
                        //case Keys.V:
                        //    AllForms.m_frmLog.AppendToLog("KEYDOWN");
                        //    e.handled = true;
                        //    break;
                    }
                }
            }
            catch (Exception eex)
            {
                AllForms.m_frmLog.AppendToLog("cEXWBxx_WBKeyDown\r\n" + eex.ToString());
            }
        }

        /// <summary>
        /// This is to intercept external dragdrops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cEXWB1_WBDragDrop(object sender, csExWB.WBDropEventArgs e)
        {

            //string text = string.Empty;
            //((csExWB.cEXWB)sender).Focus();
            //IHTMLDocument2 doc = (IHTMLDocument2)
            //((csExWB.cEXWB)sender).WebbrowserObject.Document;
            //IHTMLBodyElement body = (IHTMLBodyElement)doc.body;
            //IHTMLTxtRange range = body.createTextRange();
            //Point mousepos = ((csExWB.cEXWB)sender).PointToClient(e.pt);
            //range.moveToPoint(mousepos.X, mousepos.Y);
            //range.select();

            //if (e.dataobject.GetDataPresent(DataFormats.Html, true))
            //{
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop==>HTML");
            //    text = e.dataobject.GetData(DataFormats.Html, true).ToString();
            //    Clipboard.Clear();
            //    Clipboard.SetText(text, TextDataFormat.Html);
            //    ((csExWB.cEXWB)sender).Paste();
            //}
            //else if (e.dataobject.GetDataPresent(DataFormats.Rtf, true))
            //{
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop==>RTF");
            //    text = e.dataobject.GetData(DataFormats.Rtf).ToString();
            //    Clipboard.Clear();
            //    Clipboard.SetData(DataFormats.Rtf, text);
            //    ((csExWB.cEXWB)sender).Paste();
            //} 


            //if (e.dataobject == null)
            //    return;
            //if (e.dataobject.ContainsText())
            //{
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop\r\n" + e.dataobject.GetText());
            //    this.cEXWB1.Paste();
            //}
            //else if (e.dataobject.ContainsFileDropList())
            //{
            //    System.Collections.Specialized.StringCollection files = e.dataobject.GetFileDropList();
            //    AllForms.m_frmLog.AppendToLog("cEXWB1_WBDragDrop\r\nCount=>" + files.Count.ToString() + "\r\nFirst file\r\n" + files[0]);
            //}
        }

        void cEXWB1_TitleChange(object sender, csExWB.TitleChangeEventArgs e)
        {
            this.Text = e.title + " - HTMLEditor";
        }

        void cEXWB1_HTMLEditHostSnapRect(object sender, csExWB.HTMLEditHostSnapRectEventArgs e)
        {
            AllForms.m_frmLog.AppendToLog("Element tagName=" + e.Element.tagName
                + "\r\nElement Coordinates=> Left=" + e.ElemRect.Left.ToString() + 
                " Right=" + e.ElemRect.Right.ToString() + " Top=" + e.ElemRect.Top.ToString() +
                " Bottom=" + e.ElemRect.Bottom.ToString());
        }

        #endregion

        #region Tab Control

        private TabPage m_LastTabPage = null;

        //For now, I am disabling toolbars if not in edit tab
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
                return;
            
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string source = string.Empty;
                if (page.Name == tabPageEdit.Name)
                {
                    tsMain.Enabled = true;
                    tsFile.Enabled = true;
                    tsColors.Enabled = true;

                    if (htmlRichTextBox1.Modified)
                    {
                        cEXWB1.SetFocus();
                        source = htmlRichTextBox1.Text;
                        if (!string.IsNullOrEmpty(source))
                            cEXWB1.LoadHtmlIntoBrowser(source);
                    }
                }
                else if (page.Name == tabPagePreview.Name)
                {
                    tsMain.Enabled = false;
                    tsFile.Enabled = false;
                    tsColors.Enabled = false;

                    if ((m_LastTabPage != null) &&
                        (m_LastTabPage.Name == tabPageSource.Name))
                    {
                        source = htmlRichTextBox1.Text;
                        if ((htmlRichTextBox1.Modified) &&
                            (!string.IsNullOrEmpty(source)))
                            cEXWB1.LoadHtmlIntoBrowser(source);
                    }
                    else
                        source = cEXWB1.GetSource(cEXWB1.WebbrowserObject);  //cEXWB1.GetSource(true);
                    if (!string.IsNullOrEmpty(source))
                    {
                        //To have the relative links work properly, need to
                        //set the baseUrl
                        //Save google locally, open saved file in editor
                        //, "http://www.google.com");
                        cEXWB2.LoadHtmlIntoBrowser(source);
                    }
                }
                else if (page.Name == tabPageSource.Name)
                {
                    tsMain.Enabled = false;
                    tsFile.Enabled = false;
                    tsColors.Enabled = false;
                    htmlRichTextBox1.Text = cEXWB1.GetSource(cEXWB1.WebbrowserObject); //cEXWB1.GetSource(true);
                    htmlRichTextBox1.Modified = false;
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("frmHTMLEditor.tabControl1_SelectedIndexChanged\r\n" + ee.ToString());
            }
            this.Cursor = Cursors.Default;
            m_LastTabPage = page;
        }

        #endregion

        #region RichTextBox ContextMenu

        private void ctxMnuRichTextBox_Opening(object sender, CancelEventArgs e)
        {
            cutToolStripMenuItem.Enabled = (htmlRichTextBox1.SelectionLength > 0);
            copyToolStripMenuItem.Enabled = cutToolStripMenuItem.Enabled;
            pasteToolStripMenuItem.Enabled = Clipboard.ContainsText();
            undoLinkToolStripMenuItem.Enabled = htmlRichTextBox1.CanUndo;
            redoToolStripMenuItem.Enabled = htmlRichTextBox1.CanRedo;
            selectAllToolStripMenuItem.Enabled = htmlRichTextBox1.CanSelect;
            saveToolStripMenuItem.Enabled = (htmlRichTextBox1.TextLength > 0);
        }

        private void ctxMnuRichTextBoxMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;
            if (item == cutRtToolStripMenuItem)
                htmlRichTextBox1.Cut();
            else if (item == copyRtToolStripMenuItem)
                htmlRichTextBox1.Copy();
            else if(item == pasteRtToolStripMenuItem)
                htmlRichTextBox1.Paste();
            else if(item == selectAllToolStripMenuItem)
                htmlRichTextBox1.SelectAll();
            else if(item == undoToolStripMenuItem)
                htmlRichTextBox1.Undo();
            else if(item == redoToolStripMenuItem)
                htmlRichTextBox1.Redo();
            else if (item == leftarrowtoolStripMenuItem)
            {
                htmlRichTextBox1.Focus();
                htmlRichTextBox1.SelectedText = htmledit.HtmlTagOpen;
            }
            else if (item == rightarrowtoolStripMenuItem)
            {
                htmlRichTextBox1.Focus();
                htmlRichTextBox1.SelectedText = htmledit.HtmlTagClose;
            }
            else if (item == spaceToolStripMenuItem)
            {
                htmlRichTextBox1.Focus();
                htmlRichTextBox1.SelectedText = htmledit.HtmlSpace;
            }
            else if (item == amptoolStripMenuItem)
            {
                htmlRichTextBox1.Focus();
                htmlRichTextBox1.SelectedText = htmledit.HtmlAmp;
            }
            else if (item == saveToolStripMenuItem)
            {
                if (AllForms.ShowStaticSaveDialogForHTML(this) == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AllForms.m_dlgSave.FileName))
                    {
                        sw.Write(htmlRichTextBox1.Text);
                    }
                }
            }
        }

        #endregion

        #region Fonts and Color Handling

        //To handle the fact that setting SelectedIndex property calls SelectedIndexChanged event
        //We set this value to true whenever SelectedIndex is set. 
        private bool m_InternalCall = false;
        private void SetupComboFontSize()
        {
            tsComboFontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            tsComboFontSize.Items.Add("1 (8 pt)");
            tsComboFontSize.Items.Add("2 (10 pt)");
            tsComboFontSize.Items.Add("3 (12 pt)");
            tsComboFontSize.Items.Add("4 (14 pt)");
            tsComboFontSize.Items.Add("5 (18 pt)");
            tsComboFontSize.Items.Add("6 (24 pt)");
            tsComboFontSize.Items.Add("7 (36 pt)");
            tsComboFontSize.Click += new EventHandler(tsComboFontSize_Click);
            tsComboFontSize.SelectedIndexChanged += new EventHandler(tsComboFontSize_SelectedIndexChanged);
        }

        void tsComboFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //Fontsize changed 1 to 7
                if ((tsComboFontSize.SelectedIndex > -1) && (!m_InternalCall))
                {
                    object obj = tsComboFontSize.SelectedIndex + 1;
                    cEXWB1.ExecCommand(true, "FontSize", false, obj);
                    cEXWB1.SetFocus();
                }
                m_InternalCall = false;
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("tsComboFontSize_SelectedIndexChanged\r\n" + ee.ToString());
            }
        }

        void tsComboFontSize_Click(object sender, EventArgs e)
        {
            m_InternalCall = false;
        }

        void Selector_SelectionChanged(object sender, HtmlColorSelectorEventArgs e)
        {
            HtmlColorSelector item = sender as HtmlColorSelector;
            if( (this.cEXWB1.WebbrowserObject == null) || (item == null) )
                return;
            if (item == selectionbackcolorSelector.Selector)
            {
                //Remove colors?
                if(e.SelectedColor != Color.Empty)
                    cEXWB1.ExecCommand(true, "BackColor", false, ColorTranslator.ToHtml(e.SelectedColor));
                else
                    cEXWB1.ExecCommand(true, "BackColor", false, "");
                return;
            }
            else if (item == selectionforecolorSelector1.Selector)
            {
                if(e.SelectedColor != Color.Empty)
                    cEXWB1.ExecCommand(true, "ForeColor", false, ColorTranslator.ToHtml(e.SelectedColor));
                else
                    cEXWB1.ExecCommand(true, "ForeColor", false, "");
                return;
            }
            IHTMLDocument2 pDoc2 = this.cEXWB1.WebbrowserObject.Document as IHTMLDocument2;
            if (pDoc2 == null)
                return;
            if (item == docbackcolorselector.Selector)
            {
                //Reset backcolor to nothing
                if (e.SelectedColor == Color.Empty)
                {
                    pDoc2.bgColor = "";
                    docbackcolor.BackColor = Control.DefaultBackColor;
                }
                else
                {
                    pDoc2.bgColor = e.SelectedColor.Name;
                    docbackcolor.BackColor = e.SelectedColor;
                }
            }
            else if (item == docforecolorselector.Selector)
            {
                if (e.SelectedColor == Color.Empty)
                {
                    pDoc2.fgColor = "";
                    docforecolor.BackColor = Control.DefaultBackColor;
                }
                else
                {
                    pDoc2.fgColor = e.SelectedColor.Name;
                    docforecolor.BackColor = e.SelectedColor;
                }
            }
            else if (item == doclinkcolorselector.Selector)
            {
                if (e.SelectedColor == Color.Empty)
                {
                    pDoc2.linkColor = "";
                    doclinkcolor.BackColor = Control.DefaultBackColor;
                }
                else
                {
                    pDoc2.linkColor = e.SelectedColor.Name;
                    doclinkcolor.BackColor = e.SelectedColor;
                }
            }
            else if (item == docalinkcolorselector.Selector)
            {
                if (e.SelectedColor == Color.Empty)
                {
                    pDoc2.alinkColor = "";
                    docalinkcolor.BackColor = Control.DefaultBackColor;
                }
                else
                {
                    pDoc2.alinkColor = e.SelectedColor.Name;
                    docalinkcolor.BackColor = e.SelectedColor;
                }
            }
            else if (item == docvlinkcolorselector.Selector)
            {
                if (e.SelectedColor == Color.Empty)
                {
                    pDoc2.vlinkColor = "";
                    docvlinkcolor.BackColor = Control.DefaultBackColor;
                }
                else
                {
                    pDoc2.vlinkColor = e.SelectedColor.Name;
                    docvlinkcolor.BackColor = e.SelectedColor;
                }
            }
        }

        private void editorfontCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if ((editorfontCombo.SelectedIndex > -1) && 
                    (!editorfontCombo.InternalCall))
                {
                    Font f = editorfontCombo.SelectedFontItem;
                    editorfontCombo.Text = f.Name;
                    cEXWB1.ExecCommand(true, "FontName", false, f.Name);
                    cEXWB1.SetFocus();
                }
            }
            catch (Exception ee)
            {
                AllForms.m_frmLog.AppendToLog("editorfontCombo_SelectedIndexChanged\r\n" + ee.ToString());
            }
        }

        private void SynchDocumentColorCombos(IHTMLDocument2 pDoc2)
        {
            if (pDoc2 == null)
                return;
            if (pDoc2.bgColor != null)
            {
                docbackcolorselector.Selector.SelectedColor = ColorTranslator.FromHtml(pDoc2.bgColor.ToString());
                docbackcolor.BackColor = docbackcolorselector.Selector.SelectedColor;
            }
            else
            {
                docbackcolorselector.Selector.SelectedColor = Color.Empty;
                docbackcolor.BackColor = Control.DefaultBackColor;
            }

            if (pDoc2.fgColor != null)
            {
                docforecolorselector.Selector.SelectedColor = ColorTranslator.FromHtml(pDoc2.fgColor.ToString());
                docforecolor.BackColor = docforecolorselector.Selector.SelectedColor;
            }
            else
            {
                docforecolorselector.Selector.SelectedColor = Color.Empty;
                docforecolor.BackColor = Control.DefaultBackColor;
            }

            if (pDoc2.linkColor != null)
            {
                doclinkcolorselector.Selector.SelectedColor = ColorTranslator.FromHtml(pDoc2.linkColor.ToString());
                doclinkcolor.BackColor = doclinkcolorselector.Selector.SelectedColor;
            }
            else
            {
                doclinkcolorselector.Selector.SelectedColor = Color.Empty;
                doclinkcolor.BackColor = Control.DefaultBackColor;
            }

            if (pDoc2.alinkColor != null)
            {
                docalinkcolorselector.Selector.SelectedColor = ColorTranslator.FromHtml(pDoc2.alinkColor.ToString());
                docalinkcolor.BackColor = docalinkcolorselector.Selector.SelectedColor;
            }
            else
            {
                docalinkcolorselector.Selector.SelectedColor = Color.Empty;
                docalinkcolor.BackColor = Control.DefaultBackColor;
            }


            if (pDoc2.vlinkColor != null)
            {
                docvlinkcolorselector.Selector.SelectedColor = ColorTranslator.FromHtml(pDoc2.vlinkColor.ToString());
                docvlinkcolor.BackColor = docvlinkcolorselector.Selector.SelectedColor;
            }
            else
            {
                docvlinkcolorselector.Selector.SelectedColor = Color.Empty;
                docvlinkcolor.BackColor = Control.DefaultBackColor;
            }

        }

        #endregion

        #region DOCUMENT EVENTS
        void m_docelemevents_elemondragend(object sender, csExWB.HTMLElementEventArgs e)
        {
            /*
             * IHTMLEventObj2::dataTransfer
             * The IHTMLDataTransfer interface retrieved by this method also provides 
             * access to IDataObject. Call QueryInterface on the IHTMLDataTransfer
             * interface pointer to obtain an IServiceProvider interface pointer.
             * Then call IServiceProvider::QueryService, using IID_IDataObject
             * for the service and interface identifiers, to obtain an IDataObject
             * interface pointer.
             * 
             * IHTMLEventObj2::reason
             */
            //this is the element which data was dropped on
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
            {
                IHTMLEventObj2 eveobj2 = e.EventObj as IHTMLEventObj2;
                if ((eveobj2 != null) && (eveobj2.dataTransfer != null))
                {
                    IfacesEnumsStructsClasses.IServiceProvider pSP = eveobj2.dataTransfer as IfacesEnumsStructsClasses.IServiceProvider;
                    if (pSP != null)
                    {
                        IntPtr pdataobj = IntPtr.Zero;
                        int iret = pSP.QueryService(ref Iid_Clsids.IID_IDataObject, ref Iid_Clsids.IID_IDataObject, out pdataobj);
                        object obj = Marshal.GetObjectForIUnknown(pdataobj);
                        System.Runtime.InteropServices.ComTypes.IDataObject idataobj = obj as System.Runtime.InteropServices.ComTypes.IDataObject;
                        DataObject obja = new DataObject(idataobj);
                        //string[] formats = obja.GetFormats(false);
                        //foreach (string str in formats)
                        //{
                        //    AllForms.m_frmLog.AppendToLog("format ==> " + str);
                        //}
                        AllForms.m_frmLog.AppendToLog("HTMLEvent_ONDRAGEND==> " + obja.GetText(TextDataFormat.Html));
                    }
                }
            }
            else
                AllForms.m_frmLog.AppendToLog("ID_ONDRAGEND");
        }

        void m_docelemevents_elemondragstart(object sender, csExWB.HTMLElementEventArgs e)
        {
            //this is the element that started the drag
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
                AllForms.m_frmLog.AppendToLog("HTMLEvent_ONDRAGSTART==>pEvtObj.SrcElement.tagName\r\n" + e.EventObj.SrcElement.tagName);
            else
                AllForms.m_frmLog.AppendToLog("ID_ONDRAGSTART");
        }

        void m_docelemevents_elemondrag(object sender, csExWB.HTMLElementEventArgs e)
        {
            AllForms.m_frmLog.AppendToLog("ID_ONDRAG");
        }

        void m_docelemevents_elemoncontextmenu(object sender, csExWB.HTMLElementEventArgs e)
        {
            e.AllowDefault = false;
            editLinkToolStripMenuItem.Visible = false;
            undoLinkToolStripMenuItem.Visible = false;
            deleteLinkToolStripMenuItem.Visible = false;
            editImageToolStripMenuItem.Visible = false;
            deleteImageToolStripMenuItem.Visible = false;
            editTablePropertiesToolStripMenuItem.Visible = false;
            insertColToolStripMenuItem.Visible = false;
            insertRowToolStripMenuItem.Visible = false;
            deleteRowToolStripMenuItem.Visible = false;
            deleteColToolStripMenuItem.Visible = false;
            editCellPropertiesToolStripMenuItem.Visible = false;

            m_oHTMLCtxMenu = null;

            if (e.EventObj != null)
            {
                if (e.EventObj.SrcElement != null)
                {
                    m_oHTMLCtxMenu = e.EventObj.SrcElement;
                    //AllForms.m_frmLog.AppendToLog("HTMLEvent==>e.EventObj.SrcElement.tagName\r\n" + e.EventObj.SrcElement.tagName);
                    if (e.EventObj.SrcElement.tagName == "A")
                    {
                        editLinkToolStripMenuItem.Visible = true;
                        undoLinkToolStripMenuItem.Visible = true;
                        deleteLinkToolStripMenuItem.Visible = true;
                    }
                    else if (e.EventObj.SrcElement.tagName == "IMG")
                    {
                        editImageToolStripMenuItem.Visible = true;
                        deleteImageToolStripMenuItem.Visible = true;
                    }
                    else if ((e.EventObj.SrcElement.tagName == "TD"))
                    //|| (e.EventObj.SrcElement.tagName == "TABLE"))
                    {
                        editTablePropertiesToolStripMenuItem.Visible = true;
                        insertColToolStripMenuItem.Visible = true;
                        insertRowToolStripMenuItem.Visible = true;
                        deleteRowToolStripMenuItem.Visible = true;
                        deleteColToolStripMenuItem.Visible = true;
                        editCellPropertiesToolStripMenuItem.Visible = true;
                    }
                    //else if ((e.EventObj.SrcElement.tagName == "P") ||
                    //        (e.EventObj.SrcElement.tagName == "BODY"))
                    //{
                    //}
                }
                ctxMnuHTMLEditor.Show(e.EventObj.ScreenX, e.EventObj.ScreenY);
            }
        }

        void m_docelemevents_elemonkeyup(object sender, csExWB.HTMLElementEventArgs e)
        {
            m_pClickedElem = null;
            SynchEditButtons();
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
            {
                m_pClickedElem = e.EventObj.SrcElement;
                //AllForms.m_frmLog.AppendToLog("HTMLEvent==>e.EventObj.SrcElement.tagName\r\n" + m_pClickedElem.tagName);
                SynchFont(e.EventObj.SrcElement.tagName);
                //if (e.EventObj.keyCode == (int)Keys.Space)
                //{
                //    UnderlineWord5();
                //}
            }
            else
                SynchFont(string.Empty);
        }

        private string m_wordunder = string.Empty;
        void m_docelemevents_elemonmousemove(object sender, csExWB.HTMLElementEventArgs e)
        {
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null) &&
                (!string.IsNullOrEmpty(e.EventObj.SrcElement.tagName)))
            {
                if (e.EventObj.SrcElement.tagName == "P")
                {
                    m_wordunder = htmledit.GetWordUnderMouse(e.EventObj.ClientX, e.EventObj.ClientY);
                    if( !string.IsNullOrEmpty(m_wordunder))
                        AllForms.m_frmLog.AppendToLog(e.EventObj.ClientX.ToString() + "=" + e.EventObj.ClientY.ToString() +
                            "=>" + m_wordunder + "<=");
                }
                else if (e.EventObj.SrcElement.tagName == "A")
                {
                    //This does not work in desigmode
                    //<element style="cursor: pointer">
                    System.Windows.Forms.Cursor.Current = Cursors.Hand;
                }
                //<P><SPAN>AAAAA</SPAN></P>
                //<SPAN>AAAAA</SPAN>
                else if (e.EventObj.SrcElement.tagName == "SPAN")
                {
                    this.Text = "SPAN";
                }
            }
        }

        void m_docelemevents_elemonclick(object sender, csExWB.HTMLElementEventArgs e)
        {
            m_pClickedElem = null;
            SynchEditButtons();
            if ((e.EventObj != null) && (e.EventObj.SrcElement != null))
            {
                m_pClickedElem = e.EventObj.SrcElement;
                //AllForms.m_frmLog.AppendToLog("HTMLEvent==>e.EventObj.SrcElement.tagName\r\n" + m_pClickedElem.tagName);
                SynchFont(e.EventObj.SrcElement.tagName);

            }
            else
                SynchFont(string.Empty);
        }

        /// <summary>
        /// Example of how to underline a word after a space has been pressed
        /// Uncomment if (e.EventObj.keyCode == (int)Keys.Space) part
        /// in m_docelemevents_elemonkeyup event and test
        /// </summary>
        private void UnderlineWord5()
        {
            try
            {
                IHTMLDocument2 doc2 = null;
                IHTMLSelectionObject selobj = null;
                IHTMLTxtRange range = null;

                doc2 = (IHTMLDocument2)cEXWB1.WebbrowserObject.Document;

                selobj = (IHTMLSelectionObject)doc2.selection;
                if ((selobj.EventType == "none") | (selobj.EventType == "control"))
                    return;

                range = (IHTMLTxtRange)selobj.createRange();

                //Here the range must be empty as the user just typed space. Right?
                //So the logical check would be to test if range != null and string.IsNullOrEmpty(range.text)
                //else, just exit the method
                if ((range == null) || (!string.IsNullOrEmpty(range.text)))
                    return;

                //Now to get the last word, I mentioned before that you can use range object to acheive this without getting the source
                //Here is how, move the start backward by one character and one word, if successful, then move end backward by one character (space). Now the range will contain the word in question

                //First move one character (space)
                int icount = range.moveStart("character", -1);
                if (icount == 0) //nothing
                    return;
                //Now move backward by one word
                icount = range.moveStart("word", -1);
                if (icount == 0) //nothing
                    return;
                //Now move end backward by one character
                range.moveEnd("character", -1);
                //nothing
                if ((icount == 0) || (string.IsNullOrEmpty(range.text)))
                    return;
                //Now the range contains the word
                htmledit.UnderLineWord2(range);
                range.collapse(false);
            }
            catch (Exception)
            {
            }

        }

#endregion

        #region Misc tests
        //string text = string.Empty;
        //string html = string.Empty;
        ////Get the text in plain and html formats
        //if (Clipboard.ContainsText(TextDataFormat.Html))
        //    html = Clipboard.GetText(TextDataFormat.Html);
        //if (Clipboard.ContainsText(TextDataFormat.Text))
        //    text = Clipboard.GetText();

        //Clipboard.Clear();
        //Clipboard.SetText(text);
        //this.cEXWB1.Paste();
        //Clipboard.Clear();
        //Clipboard.SetText(html, TextDataFormat.Html);

        //htmledit.SetSelectionText(Clipboard.GetText());
        //string text = string.Empty;
        //AllForms.m_frmLog.AppendToLog(TextDataFormat.Html.ToString());
        //bool haveit = Clipboard.ContainsText(TextDataFormat.Html);
        //if (haveit)
        //{
        //    text = Clipboard.GetText(TextDataFormat.Html);
        //    AllForms.m_frmLog.AppendToLog("Case1\r\n" + text);
        //}
        //AllForms.m_frmLog.AppendToLog("Case2\r\n" + Clipboard.GetText());
        //Clipboard.Clear();
        //if (!string.IsNullOrEmpty(text))
        //{
        //    Clipboard.SetData("HTML Format", text);
        //    Clipboard.setSetText(text);
        //    //Clipboard.SetDataObject(
        //}
        //else
        //    AllForms.m_frmLog.AppendToLog("Unable to set");

        //Clipboard.SetText(text, TextDataFormat.Html);

        //tagPOINT point = htmledit.GetPosFromCharIndex(66);
        //this.Text = "=" + point.X.ToString() + "=" + point.Y.ToString();

        //this.Text = htmledit.GetCharIndexFromPos(125, 62).ToString();
        //if ((m_pClickedElem != null) &&
        //    (!string.IsNullOrEmpty(m_pClickedElem.tagName)) &&
        //    (m_pClickedElem.tagName == "P"))
        //    htmledit.SetCursorAtElementStart(m_pClickedElem);

        //firstword secondword<-highlighted 5 chars
        //next line,....
        //htmledit.UnderLineWord(2, 5);

        //if (m_oHTMLCtxMenu != null)
        //    htmledit.Underline(m_oHTMLCtxMenu);

        //IHTMLDocument4 doc4;
        //IDisplayServices ids;
        //doc4 = (IHTMLDocument4)cEXWB1.WebbrowserObject.Document;
        //ids = (IDisplayServices)doc4;
        //IHTMLCaret ihtmlc = null;
        //ids.GetCaret(out ihtmlc);

        //int caretdir = 0;
        //ihtmlc.InsertText("=INSERTED TEXT IN CARET POSITION=", -1);
        //ihtmlc.GetCaretDirection(out caretdir);
        //AllForms.m_frmLog.AppendToLog("direction=" + caretdir.ToString()); 
        #endregion

string[] m_tags = {"A" , "ABBR" , "ACRONYM" ,
"ADDRESS" ,
"APPLE" ,
"AREA" ,
"B" ,
"BASE" ,
"BASEFONT" ,
"BDO" ,
"BGSOUND" ,
"BIG" ,
"BLINK" ,
"BLOCKQUOTE" ,
"BODY" ,
"BR" ,
"BUTTON" ,
"CAPTION" ,
"CENTER" ,
"CITE" ,
"CODE" ,
"COL" ,
"COLGROUP" ,
"DD" ,
"DEL" ,
"DFN" ,
"DIR" ,
"DIV" ,
"DL" ,
"DT" ,
"EM" ,
"EMBED" ,
"FIELDSET" ,
"FONT" ,
"FORM" ,
"FRAME" ,
"FRAMESET" ,
"H1" ,
"H2" ,
"H3" ,
"H4" ,
"H5" ,
"H6" ,
"HEAD" ,
"HR" ,
"HTML" ,
"I" ,
"IFRAME" ,
"ILAYER" ,
"IMG" ,
"INPUT" ,
"INS" ,
"ISINDEX" ,
"KBD" ,
"KEYGEN" ,
"LABEL" ,
"LAYER" ,
"LEGEND" ,
"LI" ,
"LINK" ,
"LISTING" ,
"MAP" ,
"MENU" ,
"META" ,
"MULTICOL" ,
"NOBR" ,
"NOEMBED" ,
"NOFRAMES" ,
"NOLAYER" ,
"NOSCRIPT" ,
"OBJECT" ,
"OL" ,
"OPTGROUP" ,
"OPTION" ,
"P" ,
"PARAM" ,
"PLAINTEXT" ,
"PRE" ,
"Q" ,
"S" ,
"SAMP" ,
"SCRIPT" ,
"SELECT" ,
"SERVER" ,
"SMALL" ,
"SOUND" ,
"SPACER" ,
"SPAN" ,
"STRIKE" ,
"STRONG" ,
"STYLE" ,
"SUB" ,
"SUP" ,
"TABLE" ,
"TD" ,
"TR" ,
"TBODY" ,
"TEXTAREA" ,
"TITLE" ,
"TT" ,
"U" ,
"UL" ,
"VAR" ,
"WBR" ,
"XMP" };

    }

}
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
    public partial class frmFileDownload : Form
    {
        public frmFileDownload()
        {
            InitializeComponent();
        }

        public bool NotifyEndDownload
        {
            get
            {
                return tsNotifyEndDownload.Checked;
            }
        }

        private struct DLIDS
        {
            public string BrowserName;
            public string FileName;
            public string URL;
            public int DlUid;
            public int FileSize;
            public bool DlDone;

            public DLIDS(string browsername_,string filename_,string url_, int dluid_, int filesize_)
            {
                BrowserName = browsername_;
                FileName = filename_;
                URL = url_;
                DlUid = dluid_;
                FileSize = filesize_;
                DlDone = false;
            }
        }
        private int m_TotalDownloads = 0;
        private void UpdateThisText(bool add)
        {
            if (add)
                m_TotalDownloads++;
            else
                m_TotalDownloads--;
            this.Text = "<< " + m_TotalDownloads.ToString() + " >> File Download(s) in progress....";
        }

        private int m_namecounter = 0;
        public void AddDownloadItem(string browsername, string filename, string url, int uDlId, string FromUrl, string ToPath, int filesize)
        {
            try
            {
                FileDownloadStatusCtl m_stat = new FileDownloadStatusCtl();
                m_namecounter++;
                m_stat.Name = "DownloadStatus" + m_namecounter.ToString();
                m_stat.lblFrom.Text = FromUrl;
                m_stat.lblTo.Text = ToPath;
                m_stat.lblStatus.Text = "Downloading";
                m_stat.lblBytesReceived.Text = "0";
                m_stat.lblBytesTotal.Text = filesize.ToString();
                DLIDS id = new DLIDS(browsername, filename, url, uDlId, filesize);
                m_stat.Tag = id;
                m_stat.btnCancel.Tag = id;
                m_stat.btnCancel.Click += new EventHandler(btnCancel_Click);
                flowLayoutPanel1.Controls.Add(m_stat);

                UpdateThisText(true);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null)
                    return;

                DLIDS id = (DLIDS)btn.Tag;
                if (this.Owner != null)
                {
                    ((frmMain)this.Owner).StopFileDownload(id.BrowserName, id.DlUid);
                    btn.Enabled = false;
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void UpdateDownloadItem(string browsername, int uDlId, string url, int progress, int progressmax)
        {
            try
            {
                DLIDS id = new DLIDS();
                foreach (Control item in flowLayoutPanel1.Controls)
                {
                    if (item.Tag == null) //first one
                        continue;
                    id = (DLIDS)item.Tag;
                    if ((id.DlUid == uDlId) &&
                        (id.URL == url) &&
                        (id.BrowserName == browsername))
                    {
                        FileDownloadStatusCtl ctl = (FileDownloadStatusCtl)item;

                        if ((ctl != null) && (progress > 0))
                        {
                            if ((id.FileSize == 0) && (progressmax > 0))
                                id.FileSize = progressmax;
                            if ((id.FileSize > 0) && (id.FileSize > progress))
                            {
                                ctl.pbProgress.Maximum = id.FileSize;
                                ctl.pbProgress.Value = progress;
                                //item.SubItems[4].Text = ((progress * 100) / id.FileSize).ToString() + "%";
                                ctl.lblBytesReceived.Text = progress.ToString();
                            }
                            else
                            {
                                //last progress will contain actual file size
                                ctl.lblBytesReceived.Text = progress.ToString();
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public void DeleteDownloadItem(string browsername, int uDlId, string url, string Msg)
        {
            try
            {
                DLIDS id = new DLIDS();
                FileDownloadStatusCtl ctl = null;
                foreach (Control item in flowLayoutPanel1.Controls)
                {
                    if (item.Tag == null) //first one
                        continue;
                    id = (DLIDS)item.Tag;
                    if ((id.DlUid == uDlId) &&
                        (id.URL == url) &&
                        (id.BrowserName == browsername))
                    {
                        id.DlDone = true;
                        ctl = (FileDownloadStatusCtl)item;
                        if (ctl == null)
                            return;

                        if (id.FileSize > 0)
                            ctl.lblBytesReceived.Text = id.FileSize.ToString();
                        ctl.btnCancel.Enabled = false;
                        ctl.lblStatus.Text = Msg;
                        UpdateThisText(false);
                        if (NotifyEndDownload)
                            MessageBox.Show(this, "Finished downloading\r\n" +
                                ctl.lblFrom.ToString() + "\r\nTO:\r\n" + ctl.lblTo.ToString());
                        break;
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void frmFileDownload_Load(object sender, EventArgs e)
        {
            this.Icon = AllForms.BitmapToIcon(45);
            try
            {
                FileDownloadStatusCtlHeader header = new FileDownloadStatusCtlHeader();
                header.Name = "DownloadStatusHeader";
                flowLayoutPanel1.Controls.Add(header);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void frmFileDownload_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //Clear completed - Save download list
            if (e.ClickedItem.Name == tsCloseDownload.Name)
            {
                this.Hide();
            }
            else if (e.ClickedItem.Name == this.tsSave.Name)
            {
                if (AllForms.ShowStaticSaveDialogForText(this) == DialogResult.OK)
                {
                    string tmp = "====================\r\nDownload Date: ";
                    tmp += DateTime.Today.ToLongDateString();
                    tmp += "\r\nDownload From: {0}\r\nDownload To: {0}\r\nFile Size: {0} Bytes\r\n====================\r\n";
                    using (StreamWriter sw = new StreamWriter(AllForms.m_dlgSave.FileName,true))
                    {
                        try
                        {
                            DLIDS id = new DLIDS();
                            FileDownloadStatusCtl ctl = null;
                            foreach (Control item in flowLayoutPanel1.Controls)
                            {
                                if (item.Tag == null) //first one
                                    continue;
                                id = (DLIDS)item.Tag;
                                if (id.DlDone)
                                {
                                    ctl = (FileDownloadStatusCtl)item;
                                    if (ctl == null)
                                        return;
                                    //Date - From - To - filesize
                                    sw.Write(
                                        string.Format(tmp, 
                                        ctl.lblFrom.Text, 
                                        ctl.lblTo.Text, 
                                        id.FileSize.ToString())
                                        );
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            else if (e.ClickedItem.Name == this.tsClearFinished.Name)
            {
                try
                {
                    DLIDS id = new DLIDS();
                    Control.ControlCollection col = flowLayoutPanel1.Controls;
                    foreach (Control item in col)
                    {
                        if (item.Tag == null) //first one
                            continue;
                        id = (DLIDS)item.Tag;
                        if (id.DlDone)
                        {
                            flowLayoutPanel1.Controls.Remove(item);
                            break;
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }

    #region FileDownloadStatusCtlHeader class
    public class FileDownloadStatusCtlHeader : TableLayoutPanel
    {
        public System.Windows.Forms.Label lblFrom = new Label();
        public System.Windows.Forms.Label lblTo = new Label();
        public System.Windows.Forms.Label lblStatus = new Label();
        public System.Windows.Forms.Label lblBytesReceived = new Label();
        public System.Windows.Forms.Label lblBytesTotal = new Label();
        public System.Windows.Forms.Label lblProgress = new Label();
        public System.Windows.Forms.Label lblCancel = new Label();
        public int RowIndex = 0; // which row

        private void InitControls()
        {
            // 
            // lblFrom
            //
            this.lblFrom.AutoEllipsis = true;
            this.lblFrom.AutoSize = true;
            this.lblFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "Downloading From";
            this.lblFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTo
            // 
            this.lblTo.AutoEllipsis = true;
            this.lblTo.AutoSize = true;
            this.lblTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTo.Name = "lblTo";
            this.lblTo.TabIndex = 1;
            this.lblTo.Text = "Downloading To";
            this.lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Status";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBytesReceived
            // 
            this.lblBytesReceived.AutoEllipsis = true;
            this.lblBytesReceived.AutoSize = true;
            this.lblBytesReceived.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBytesReceived.Name = "lblBytesReceived";
            this.lblBytesReceived.Text = "Received";
            this.lblBytesReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblBytesTotal
            //
            this.lblBytesTotal.AutoEllipsis = true;
            this.lblBytesTotal.AutoSize = true;
            this.lblBytesTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBytesTotal.Name = "lblBytesTotal";
            this.lblBytesTotal.Text = "Total";
            this.lblBytesTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblProgress
            //
            this.lblProgress.AutoEllipsis = true;
            this.lblProgress.AutoSize = true;
            this.lblProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Text = "Progress";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // lblCancel
            //
            this.lblCancel.AutoEllipsis = true;
            this.lblCancel.AutoSize = true;
            this.lblCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCancel.Name = "lblCancel";
            this.lblCancel.Text = "Cancel";
            this.lblCancel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        public FileDownloadStatusCtlHeader()
        {
            this.InitControls();
            this.CellBorderStyle = TableLayoutPanelCellBorderStyle.OutsetDouble;
            this.BackColor = Color.Khaki;
            this.ColumnCount = 7;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.Controls.Add(this.lblFrom, 0, 0);
            this.Controls.Add(this.lblTo, 1, 0);
            this.Controls.Add(this.lblStatus, 2, 0);
            this.Controls.Add(this.lblBytesReceived, 3, 0);
            this.Controls.Add(this.lblBytesTotal, 4, 0);
            this.Controls.Add(this.lblProgress, 5, 0);
            this.Controls.Add(this.lblCancel, 6, 0);
            this.Name = "tableLayoutPanel1";
            this.Size = new System.Drawing.Size(700, 28);
            this.RowCount = 1;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        }
    } 
    #endregion

    #region FileDownloadStatusCtl class
    public class FileDownloadStatusCtl : TableLayoutPanel
    {
        public System.Windows.Forms.Label lblFrom = new Label();
        public System.Windows.Forms.Label lblTo = new Label();
        public System.Windows.Forms.Label lblStatus = new Label();
        public System.Windows.Forms.Label lblBytesReceived = new Label();
        public System.Windows.Forms.Label lblBytesTotal = new Label();
        public System.Windows.Forms.ProgressBar pbProgress = new ProgressBar();
        public System.Windows.Forms.Button btnCancel = new Button();
        public int RowIndex = 0; // which row

        private void InitControls()
        {
            // 
            // lblFrom
            // 
            this.lblFrom.AutoEllipsis = true;
            this.lblFrom.AutoSize = true;
            this.lblFrom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "Downloading From";
            this.lblFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTo
            // 
            this.lblTo.AutoEllipsis = true;
            this.lblTo.AutoSize = true;
            this.lblTo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTo.Name = "lblTo";
            this.lblTo.TabIndex = 1;
            this.lblTo.Text = "Downloading To";
            this.lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Status";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBytesReceived
            // 
            this.lblBytesReceived.AutoEllipsis = true;
            this.lblBytesReceived.AutoSize = true;
            this.lblBytesReceived.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBytesReceived.Name = "lblBytesReceived";
            this.lblBytesReceived.Text = "Received";
            this.lblBytesReceived.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblBytesTotal
            //
            this.lblBytesTotal.AutoEllipsis = true;
            this.lblBytesTotal.AutoSize = true;
            this.lblBytesTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBytesTotal.Name = "lblBytesTotal";
            this.lblBytesTotal.Text = "Total";
            this.lblBytesTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbProgress
            //
            this.pbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbProgress.Name = "pbProgress";
            //
            // btnCancel
            //
            this.btnCancel.AutoSize = true;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Text = "Cancel";
        }

        public FileDownloadStatusCtl()
        {
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.ResizeRedraw, true);
            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.InitControls();
            this.ColumnCount = 7;
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.Controls.Add(this.lblFrom, 0, 0);
            this.Controls.Add(this.lblTo, 1, 0);
            this.Controls.Add(this.lblStatus, 2, 0);
            this.Controls.Add(this.lblBytesReceived, 3, 0);
            this.Controls.Add(this.lblBytesTotal, 4, 0);
            this.Controls.Add(this.pbProgress, 5, 0);
            this.Controls.Add(this.btnCancel, 6, 0);
            this.Name = "tableLayoutPanel1";
            this.Size = new System.Drawing.Size(700, 28);
            this.RowCount = 1;
            this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        }

    } 
    #endregion

}
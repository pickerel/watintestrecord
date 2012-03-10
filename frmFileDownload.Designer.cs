namespace TestRecorder
{
    partial class frmFileDownload
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileDownload));
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tsNotifyEndDownload = new System.Windows.Forms.ToolStripButton();
            this.tsCloseDownload = new System.Windows.Forms.ToolStripButton();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsClearFinished = new System.Windows.Forms.ToolStripButton();
            this.tsSave = new System.Windows.Forms.ToolStripButton();
            this.tsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsNotifyEndDownload,
            this.tsCloseDownload,
            this.toolStripSeparator1,
            this.tsClearFinished,
            this.tsSave});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Size = new System.Drawing.Size(808, 25);
            this.tsMain.TabIndex = 0;
            this.tsMain.Text = "toolStrip1";
            this.tsMain.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // tsNotifyEndDownload
            // 
            this.tsNotifyEndDownload.CheckOnClick = true;
            this.tsNotifyEndDownload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsNotifyEndDownload.Image = ((System.Drawing.Image)(resources.GetObject("tsNotifyEndDownload.Image")));
            this.tsNotifyEndDownload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsNotifyEndDownload.Name = "tsNotifyEndDownload";
            this.tsNotifyEndDownload.Size = new System.Drawing.Size(172, 22);
            this.tsNotifyEndDownload.Text = "Notify When Download Ends";
            // 
            // tsCloseDownload
            // 
            this.tsCloseDownload.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tsCloseDownload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsCloseDownload.Image = ((System.Drawing.Image)(resources.GetObject("tsCloseDownload.Image")));
            this.tsCloseDownload.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsCloseDownload.Name = "tsCloseDownload";
            this.tsCloseDownload.Size = new System.Drawing.Size(43, 22);
            this.tsCloseDownload.Text = "Close";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 25);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(808, 363);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsClearFinished
            // 
            this.tsClearFinished.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsClearFinished.Image = ((System.Drawing.Image)(resources.GetObject("tsClearFinished.Image")));
            this.tsClearFinished.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsClearFinished.Name = "tsClearFinished";
            this.tsClearFinished.Size = new System.Drawing.Size(93, 22);
            this.tsClearFinished.Text = "Clear Finished";
            // 
            // tsSave
            // 
            this.tsSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsSave.Image = ((System.Drawing.Image)(resources.GetObject("tsSave.Image")));
            this.tsSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsSave.Name = "tsSave";
            this.tsSave.Size = new System.Drawing.Size(79, 22);
            this.tsSave.Text = "Save to Log";
            // 
            // frmFileDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 388);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.tsMain);
            this.Name = "frmFileDownload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "< 0 > File Downloads";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFileDownload_FormClosing);
            this.Load += new System.EventHandler(this.frmFileDownload_Load);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ToolStripButton tsNotifyEndDownload;
        private System.Windows.Forms.ToolStripButton tsCloseDownload;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsClearFinished;
        private System.Windows.Forms.ToolStripButton tsSave;
    }
}
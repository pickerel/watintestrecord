namespace TestRecorder
{
    partial class SaveScriptDialogControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.ddlTemplate = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Template";
            // 
            // ddlTemplate
            // 
            this.ddlTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlTemplate.FormattingEnabled = true;
            this.ddlTemplate.Location = new System.Drawing.Point(196, 16);
            this.ddlTemplate.Name = "ddlTemplate";
            this.ddlTemplate.Size = new System.Drawing.Size(243, 21);
            this.ddlTemplate.TabIndex = 1;
            // 
            // SaveScriptDialogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ddlTemplate);
            this.Controls.Add(this.label1);
            this.FileDlgCheckFileExists = false;
            this.FileDlgDefaultExt = "";
            this.FileDlgOkCaption = "&Save";
            this.FileDlgStartLocation = FileDialogExtenders.AddonWindowLocation.Bottom;
            this.FileDlgType = Win32Types.FileDialogType.SaveFileDlg;
            this.Name = "SaveScriptDialogControl";
            this.Size = new System.Drawing.Size(628, 50);
            this.EventFilterChanged += new FileDialogExtenders.FileDialogControlBase.FilterChangedEventHandler(this.SaveScriptDialogControlEventFilterChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox ddlTemplate;
    }
}

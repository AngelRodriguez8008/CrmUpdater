namespace CrmWebResourcesUpdater.OptionsForms
{
    partial class WebResourcesUpdaterForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebResourcesUpdaterForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbManageConnections = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbMergeConnectionsFiles = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbClearLogs = new System.Windows.Forms.ToolStripButton();
            this.sbtnWhoAmI = new System.Windows.Forms.ToolStripButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tbLogs = new System.Windows.Forms.TextBox();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.bCreateMapping = new System.Windows.Forms.Button();
            this.cbExtendedLog = new System.Windows.Forms.CheckBox();
            this.cbIgnoreExtensions = new System.Windows.Forms.CheckBox();
            this.cbAutoPublish = new System.Windows.Forms.CheckBox();
            this.bSave = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bGetSolutions = new System.Windows.Forms.Button();
            this.comboBoxSolutions = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbManageConnections,
            this.toolStripSeparator1,
            this.tsbMergeConnectionsFiles,
            this.toolStripSeparator2,
            this.tsbClearLogs,
            this.sbtnWhoAmI});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(582, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "tsMain";
            // 
            // tsbManageConnections
            // 
            this.tsbManageConnections.Image = ((System.Drawing.Image)(resources.GetObject("tsbManageConnections.Image")));
            this.tsbManageConnections.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbManageConnections.Name = "tsbManageConnections";
            this.tsbManageConnections.Size = new System.Drawing.Size(138, 22);
            this.tsbManageConnections.Text = "Manage connections";
            this.tsbManageConnections.Click += new System.EventHandler(this.tsbManageConnections_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbMergeConnectionsFiles
            // 
            this.tsbMergeConnectionsFiles.CheckOnClick = true;
            this.tsbMergeConnectionsFiles.Image = ((System.Drawing.Image)(resources.GetObject("tsbMergeConnectionsFiles.Image")));
            this.tsbMergeConnectionsFiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbMergeConnectionsFiles.Name = "tsbMergeConnectionsFiles";
            this.tsbMergeConnectionsFiles.Size = new System.Drawing.Size(153, 22);
            this.tsbMergeConnectionsFiles.Text = "Merge connections files";
            this.tsbMergeConnectionsFiles.Click += new System.EventHandler(this.tsbMergeConnectionsFiles_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbClearLogs
            // 
            this.tsbClearLogs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClearLogs.Image = ((System.Drawing.Image)(resources.GetObject("tsbClearLogs.Image")));
            this.tsbClearLogs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbClearLogs.Name = "tsbClearLogs";
            this.tsbClearLogs.Size = new System.Drawing.Size(63, 22);
            this.tsbClearLogs.Text = "Clear logs";
            this.tsbClearLogs.Click += new System.EventHandler(this.tsbClearLogs_Click);
            // 
            // sbtnWhoAmI
            // 
            this.sbtnWhoAmI.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sbtnWhoAmI.Image = ((System.Drawing.Image)(resources.GetObject("sbtnWhoAmI.Image")));
            this.sbtnWhoAmI.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sbtnWhoAmI.Name = "sbtnWhoAmI";
            this.sbtnWhoAmI.Size = new System.Drawing.Size(67, 22);
            this.sbtnWhoAmI.Text = "Who am I?";
            this.sbtnWhoAmI.Click += new System.EventHandler(this.btnWhoAmI_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tbLogs);
            this.panel2.Controls.Add(this.groupBoxConnection);
            this.panel2.Controls.Add(this.bCreateMapping);
            this.panel2.Controls.Add(this.cbExtendedLog);
            this.panel2.Controls.Add(this.cbIgnoreExtensions);
            this.panel2.Controls.Add(this.cbAutoPublish);
            this.panel2.Controls.Add(this.bSave);
            this.panel2.Controls.Add(this.bCancel);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 25);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5);
            this.panel2.Size = new System.Drawing.Size(582, 313);
            this.panel2.TabIndex = 1;
            // 
            // tbLogs
            // 
            this.tbLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbLogs.Location = new System.Drawing.Point(8, 182);
            this.tbLogs.Multiline = true;
            this.tbLogs.Name = "tbLogs";
            this.tbLogs.Size = new System.Drawing.Size(566, 123);
            this.tbLogs.TabIndex = 10;
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConnection.Location = new System.Drawing.Point(12, 0);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(561, 48);
            this.groupBoxConnection.TabIndex = 1;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Connection";
            // 
            // bCreateMapping
            // 
            this.bCreateMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bCreateMapping.Location = new System.Drawing.Point(418, 124);
            this.bCreateMapping.Name = "bCreateMapping";
            this.bCreateMapping.Size = new System.Drawing.Size(155, 23);
            this.bCreateMapping.TabIndex = 9;
            this.bCreateMapping.Text = "Create Mapping File";
            this.bCreateMapping.UseVisualStyleBackColor = true;
            this.bCreateMapping.Click += new System.EventHandler(this.bCreateMapping_Click);
            // 
            // cbExtendedLog
            // 
            this.cbExtendedLog.AutoSize = true;
            this.cbExtendedLog.Location = new System.Drawing.Point(13, 157);
            this.cbExtendedLog.Name = "cbExtendedLog";
            this.cbExtendedLog.Size = new System.Drawing.Size(88, 17);
            this.cbExtendedLog.TabIndex = 5;
            this.cbExtendedLog.Text = "Extended log";
            this.cbExtendedLog.UseVisualStyleBackColor = true;
            // 
            // cbIgnoreExtensions
            // 
            this.cbIgnoreExtensions.AutoSize = true;
            this.cbIgnoreExtensions.Location = new System.Drawing.Point(13, 134);
            this.cbIgnoreExtensions.Name = "cbIgnoreExtensions";
            this.cbIgnoreExtensions.Size = new System.Drawing.Size(193, 17);
            this.cbIgnoreExtensions.TabIndex = 4;
            this.cbIgnoreExtensions.Text = "Search with and without extensions";
            this.cbIgnoreExtensions.UseVisualStyleBackColor = true;
            // 
            // cbAutoPublish
            // 
            this.cbAutoPublish.AutoSize = true;
            this.cbAutoPublish.Checked = true;
            this.cbAutoPublish.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAutoPublish.Location = new System.Drawing.Point(13, 111);
            this.cbAutoPublish.Name = "cbAutoPublish";
            this.cbAutoPublish.Size = new System.Drawing.Size(119, 17);
            this.cbAutoPublish.TabIndex = 3;
            this.cbAutoPublish.Text = "Publish after upload";
            this.cbAutoPublish.UseVisualStyleBackColor = true;
            // 
            // bSave
            // 
            this.bSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bSave.Location = new System.Drawing.Point(418, 153);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(75, 23);
            this.bSave.TabIndex = 7;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(498, 153);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 8;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.bGetSolutions);
            this.groupBox1.Controls.Add(this.comboBoxSolutions);
            this.groupBox1.Location = new System.Drawing.Point(11, 51);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(561, 54);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Solution";
            // 
            // bGetSolutions
            // 
            this.bGetSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bGetSolutions.Location = new System.Drawing.Point(444, 19);
            this.bGetSolutions.Name = "bGetSolutions";
            this.bGetSolutions.Size = new System.Drawing.Size(108, 23);
            this.bGetSolutions.TabIndex = 1;
            this.bGetSolutions.Text = "Get Solutions";
            this.bGetSolutions.UseVisualStyleBackColor = true;
            this.bGetSolutions.Click += new System.EventHandler(this.bGetSolutions_Click);
            // 
            // comboBoxSolutions
            // 
            this.comboBoxSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxSolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSolutions.FormattingEnabled = true;
            this.comboBoxSolutions.Location = new System.Drawing.Point(6, 19);
            this.comboBoxSolutions.Name = "comboBoxSolutions";
            this.comboBoxSolutions.Size = new System.Drawing.Size(432, 21);
            this.comboBoxSolutions.Sorted = true;
            this.comboBoxSolutions.TabIndex = 0;
            // 
            // WebResourcesUpdaterForm
            // 
            this.AcceptButton = this.bSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(582, 338);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "WebResourcesUpdaterForm";
            this.Text = "Microsoft Dynamics 365 Web Resources Updater Options";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbManageConnections;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbMergeConnectionsFiles;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbExtendedLog;
        private System.Windows.Forms.CheckBox cbIgnoreExtensions;
        private System.Windows.Forms.CheckBox cbAutoPublish;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bGetSolutions;
        private System.Windows.Forms.ComboBox comboBoxSolutions;
        private System.Windows.Forms.ToolStripButton tsbClearLogs;
        private System.Windows.Forms.ToolStripButton sbtnWhoAmI;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.Button bCreateMapping;
        private System.Windows.Forms.TextBox tbLogs;
    }
}


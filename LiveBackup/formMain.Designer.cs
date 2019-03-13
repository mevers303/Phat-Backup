namespace LiveBackup
{
    partial class formMain
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
            this.components = new System.ComponentModel.Container();
            this.queueWorker = new System.ComponentModel.BackgroundWorker();
            this.listView1 = new System.Windows.Forms.ListView();
            this.name = new System.Windows.Forms.ColumnHeader();
            this.localPath = new System.Windows.Forms.ColumnHeader();
            this.remotePath = new System.Windows.Forms.ColumnHeader();
            this.status = new System.Windows.Forms.ColumnHeader();
            this.backupContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rescanFilesForChangesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDeletedFilesFromBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initialWorker = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupContext.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // queueWorker
            // 
            this.queueWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.queueWorker_DoWork);
            this.queueWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.queueWorker_RunWorkerCompleted);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.localPath,
            this.remotePath,
            this.status});
            this.listView1.ContextMenuStrip = this.backupContext;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(0, 25);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(485, 290);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // name
            // 
            this.name.Text = "Name";
            this.name.Width = 75;
            // 
            // localPath
            // 
            this.localPath.Text = "Local Path";
            this.localPath.Width = 150;
            // 
            // remotePath
            // 
            this.remotePath.Text = "Remote Path";
            this.remotePath.Width = 150;
            // 
            // status
            // 
            this.status.Text = "Status";
            this.status.Width = 150;
            // 
            // backupContext
            // 
            this.backupContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rescanFilesForChangesToolStripMenuItem,
            this.removeDeletedFilesFromBackupToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.backupContext.Name = "backupContext";
            this.backupContext.Size = new System.Drawing.Size(255, 70);
            // 
            // rescanFilesForChangesToolStripMenuItem
            // 
            this.rescanFilesForChangesToolStripMenuItem.Name = "rescanFilesForChangesToolStripMenuItem";
            this.rescanFilesForChangesToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.rescanFilesForChangesToolStripMenuItem.Text = "Rescan files for changes";
            this.rescanFilesForChangesToolStripMenuItem.Click += new System.EventHandler(this.rescanFilesForChangesToolStripMenuItem_Click);
            // 
            // removeDeletedFilesFromBackupToolStripMenuItem
            // 
            this.removeDeletedFilesFromBackupToolStripMenuItem.Name = "removeDeletedFilesFromBackupToolStripMenuItem";
            this.removeDeletedFilesFromBackupToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.removeDeletedFilesFromBackupToolStripMenuItem.Text = "Remove deleted files from backup";
            this.removeDeletedFilesFromBackupToolStripMenuItem.Click += new System.EventHandler(this.removeDeletedFilesFromBackupToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // initialWorker
            // 
            this.initialWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.initialWorker_DoWork);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.fileToolStripMenuItem,
            this.addBackupToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(486, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(12, 20);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // addBackupToolStripMenuItem
            // 
            this.addBackupToolStripMenuItem.Name = "addBackupToolStripMenuItem";
            this.addBackupToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.addBackupToolStripMenuItem.Text = "Add Backup";
            this.addBackupToolStripMenuItem.Click += new System.EventHandler(this.addBackupToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 289);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "formMain";
            this.Text = "formMain";
            this.Load += new System.EventHandler(this.formMain_Load);
            this.SizeChanged += new System.EventHandler(this.formMain_SizeChanged);
            this.backupContext.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader localPath;
        private System.Windows.Forms.ColumnHeader remotePath;
        private System.ComponentModel.BackgroundWorker initialWorker;
        private System.Windows.Forms.ColumnHeader status;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBackupToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip backupContext;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        public System.ComponentModel.BackgroundWorker queueWorker;
        private System.Windows.Forms.ToolStripMenuItem rescanFilesForChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeDeletedFilesFromBackupToolStripMenuItem;
    }
}
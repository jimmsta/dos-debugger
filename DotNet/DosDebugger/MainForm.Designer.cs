namespace DosDebugger
{
    partial class MainForm
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
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuListingGoToXRef = new System.Windows.Forms.ToolStripMenuItem();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.lvProcedures = new Util.Forms.DoubleBufferedListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvErrors = new Util.Forms.DoubleBufferedListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnGoTo = new System.Windows.Forms.Button();
            this.cbBookmarks = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnForward = new System.Windows.Forms.Button();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.lvSegments = new Util.Forms.DoubleBufferedListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvListing
            // 
            this.lvListing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvListing.ContextMenuStrip = this.contextMenuListing;
            this.lvListing.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvListing.FullRowSelect = true;
            this.lvListing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvListing.HideSelection = false;
            this.lvListing.Location = new System.Drawing.Point(158, 74);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.Size = new System.Drawing.Size(670, 235);
            this.lvListing.TabIndex = 0;
            this.lvListing.UseCompatibleStateImageBehavior = false;
            this.lvListing.View = System.Windows.Forms.View.Details;
            this.lvListing.VirtualMode = true;
            this.lvListing.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvListing_RetrieveVirtualItem);
            this.lvListing.SelectedIndexChanged += new System.EventHandler(this.lvListing_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Address";
            this.columnHeader1.Width = 90;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Opcode";
            this.columnHeader2.Width = 160;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Disassembly";
            this.columnHeader3.Width = 380;
            // 
            // contextMenuListing
            // 
            this.contextMenuListing.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuListingGoToXRef});
            this.contextMenuListing.Name = "contextMenuListing";
            this.contextMenuListing.Size = new System.Drawing.Size(177, 28);
            this.contextMenuListing.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuListing_Opening);
            // 
            // mnuListingGoToXRef
            // 
            this.mnuListingGoToXRef.Name = "mnuListingGoToXRef";
            this.mnuListingGoToXRef.Size = new System.Drawing.Size(176, 24);
            this.mnuListingGoToXRef.Text = "Go to Reference";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(728, 38);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(100, 30);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Location = new System.Drawing.Point(12, 38);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(76, 30);
            this.btnAnalyze.TabIndex = 3;
            this.btnAnalyze.Text = "Analyze";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // lvProcedures
            // 
            this.lvProcedures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.lvProcedures.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvProcedures.FullRowSelect = true;
            this.lvProcedures.HideSelection = false;
            this.lvProcedures.Location = new System.Drawing.Point(12, 74);
            this.lvProcedures.MultiSelect = false;
            this.lvProcedures.Name = "lvProcedures";
            this.lvProcedures.Size = new System.Drawing.Size(140, 235);
            this.lvProcedures.TabIndex = 4;
            this.lvProcedures.UseCompatibleStateImageBehavior = false;
            this.lvProcedures.View = System.Windows.Forms.View.Details;
            this.lvProcedures.SelectedIndexChanged += new System.EventHandler(this.lvProcedures_SelectedIndexChanged);
            this.lvProcedures.DoubleClick += new System.EventHandler(this.lvProcedures_DoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Procedure";
            this.columnHeader4.Width = 100;
            // 
            // lvErrors
            // 
            this.lvErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
            this.lvErrors.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvErrors.FullRowSelect = true;
            this.lvErrors.HideSelection = false;
            this.lvErrors.Location = new System.Drawing.Point(158, 315);
            this.lvErrors.MultiSelect = false;
            this.lvErrors.Name = "lvErrors";
            this.lvErrors.Size = new System.Drawing.Size(670, 136);
            this.lvErrors.TabIndex = 6;
            this.lvErrors.UseCompatibleStateImageBehavior = false;
            this.lvErrors.View = System.Windows.Forms.View.Details;
            this.lvErrors.DoubleClick += new System.EventHandler(this.lvErrors_DoubleClick);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Location";
            this.columnHeader5.Width = 100;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Message";
            this.columnHeader6.Width = 480;
            // 
            // btnGoTo
            // 
            this.btnGoTo.Location = new System.Drawing.Point(416, 38);
            this.btnGoTo.Name = "btnGoTo";
            this.btnGoTo.Size = new System.Drawing.Size(65, 30);
            this.btnGoTo.TabIndex = 7;
            this.btnGoTo.Text = "Go";
            this.btnGoTo.UseVisualStyleBackColor = true;
            this.btnGoTo.Click += new System.EventHandler(this.btnGoTo_Click);
            // 
            // cbBookmarks
            // 
            this.cbBookmarks.FormattingEnabled = true;
            this.cbBookmarks.Items.AddRange(new object[] {
            "2920:17FC useful routine",
            "3FE6:C830 single-entry jump table",
            "16C6:C830 single-entry jump table?",
            "2920:264A es?",
            "2920:377D jump table 1",
            "2920:8B53 jump table 2",
            "2920:6184 jump table 3",
            "2920:44B4 jump table 4",
            "2920:3FCC rep prefix",
            "2920:7430 program entry"});
            this.cbBookmarks.Location = new System.Drawing.Point(203, 41);
            this.cbBookmarks.Name = "cbBookmarks";
            this.cbBookmarks.Size = new System.Drawing.Size(207, 27);
            this.cbBookmarks.TabIndex = 8;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(840, 27);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpen,
            this.toolStripMenuItem1,
            this.mnuFileInfo,
            this.toolStripMenuItem2,
            this.mnuFileExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(41, 23);
            this.mnuFile.Text = "&File";
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.Size = new System.Drawing.Size(179, 24);
            this.mnuFileOpen.Text = "&Open...";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(176, 6);
            // 
            // mnuFileInfo
            // 
            this.mnuFileInfo.Name = "mnuFileInfo";
            this.mnuFileInfo.Size = new System.Drawing.Size(179, 24);
            this.mnuFileInfo.Text = "Executable &Info...";
            this.mnuFileInfo.Click += new System.EventHandler(this.mnuFileInfo_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(176, 6);
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.Size = new System.Drawing.Size(179, 24);
            this.mnuFileExit.Text = "E&xit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Executable file|*.exe";
            this.openFileDialog1.Title = "Select DOS Executable File";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 452);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(840, 24);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // txtStatus
            // 
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(63, 19);
            this.txtStatus.Text = "Message";
            // 
            // btnBack
            // 
            this.btnBack.Location = new System.Drawing.Point(113, 38);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(39, 30);
            this.btnBack.TabIndex = 11;
            this.btnBack.Text = "<";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnForward
            // 
            this.btnForward.Location = new System.Drawing.Point(158, 38);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(39, 30);
            this.btnForward.TabIndex = 12;
            this.btnForward.Text = ">";
            this.btnForward.UseVisualStyleBackColor = true;
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // txtFind
            // 
            this.txtFind.Location = new System.Drawing.Point(487, 41);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(130, 26);
            this.txtFind.TabIndex = 13;
            this.txtFind.Text = "jmpn word ptr cs:[";
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(623, 38);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(65, 30);
            this.btnFind.TabIndex = 14;
            this.btnFind.Text = "Find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // lvSegments
            // 
            this.lvSegments.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7});
            this.lvSegments.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvSegments.FullRowSelect = true;
            this.lvSegments.HideSelection = false;
            this.lvSegments.Location = new System.Drawing.Point(12, 315);
            this.lvSegments.MultiSelect = false;
            this.lvSegments.Name = "lvSegments";
            this.lvSegments.Size = new System.Drawing.Size(140, 134);
            this.lvSegments.TabIndex = 15;
            this.lvSegments.UseCompatibleStateImageBehavior = false;
            this.lvSegments.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Procedure";
            this.columnHeader7.Width = 100;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 476);
            this.Controls.Add(this.lvSegments);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cbBookmarks);
            this.Controls.Add(this.btnGoTo);
            this.Controls.Add(this.lvErrors);
            this.Controls.Add(this.lvProcedures);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lvListing);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "DOS Disassembler";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.contextMenuListing.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvListing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnAnalyze;
        private Util.Forms.DoubleBufferedListView lvProcedures;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private Util.Forms.DoubleBufferedListView lvErrors;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button btnGoTo;
        private System.Windows.Forms.ComboBox cbBookmarks;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnForward;
        private System.Windows.Forms.ContextMenuStrip contextMenuListing;
        private System.Windows.Forms.ToolStripMenuItem mnuListingGoToXRef;
        private System.Windows.Forms.ToolStripMenuItem mnuFileInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.Button btnFind;
        private Util.Forms.DoubleBufferedListView lvSegments;
        private System.Windows.Forms.ColumnHeader columnHeader7;
    }
}


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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuListingGoToXRef = new System.Windows.Forms.ToolStripMenuItem();
            this.lvProcedures = new Util.Forms.DoubleBufferedListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvErrors = new Util.Forms.DoubleBufferedListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDisassemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyOpcodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAddressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditFind = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditFindNext = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditFindPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditBookmarks = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewNavigateBackward = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewNavigateForward = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAnalyze = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAnalyzeExecutable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lvSegments = new Util.Forms.DoubleBufferedListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNavigateBackward = new System.Windows.Forms.ToolStripButton();
            this.btnNavigateForward = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cbBookmarks = new System.Windows.Forms.ToolStripComboBox();
            this.btnGoToBookmark = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cbFind = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTest = new System.Windows.Forms.ToolStripButton();
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
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
            this.lvListing.Location = new System.Drawing.Point(243, 61);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.Size = new System.Drawing.Size(670, 265);
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
            // lvProcedures
            // 
            this.lvProcedures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader8,
            this.columnHeader9});
            this.lvProcedures.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvProcedures.FullRowSelect = true;
            this.lvProcedures.HideSelection = false;
            this.lvProcedures.Location = new System.Drawing.Point(12, 61);
            this.lvProcedures.MultiSelect = false;
            this.lvProcedures.Name = "lvProcedures";
            this.lvProcedures.Size = new System.Drawing.Size(225, 265);
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
            this.lvErrors.Location = new System.Drawing.Point(243, 330);
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
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuView,
            this.mnuAnalyze,
            this.mnuHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(925, 27);
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
            this.mnuFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
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
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyDisassemblyToolStripMenuItem,
            this.copyOpcodeToolStripMenuItem,
            this.copyAddressToolStripMenuItem,
            this.toolStripMenuItem3,
            this.mnuEditFind,
            this.mnuEditFindNext,
            this.mnuEditFindPrevious,
            this.toolStripMenuItem5,
            this.mnuEditGoTo,
            this.toolStripMenuItem4,
            this.mnuEditBookmarks});
            this.mnuEdit.Name = "mnuEdit";
            this.mnuEdit.Size = new System.Drawing.Size(44, 23);
            this.mnuEdit.Text = "&Edit";
            // 
            // copyDisassemblyToolStripMenuItem
            // 
            this.copyDisassemblyToolStripMenuItem.Enabled = false;
            this.copyDisassemblyToolStripMenuItem.Name = "copyDisassemblyToolStripMenuItem";
            this.copyDisassemblyToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.copyDisassemblyToolStripMenuItem.Text = "Copy Disassembly";
            // 
            // copyOpcodeToolStripMenuItem
            // 
            this.copyOpcodeToolStripMenuItem.Enabled = false;
            this.copyOpcodeToolStripMenuItem.Name = "copyOpcodeToolStripMenuItem";
            this.copyOpcodeToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.copyOpcodeToolStripMenuItem.Text = "Copy Opcode";
            // 
            // copyAddressToolStripMenuItem
            // 
            this.copyAddressToolStripMenuItem.Enabled = false;
            this.copyAddressToolStripMenuItem.Name = "copyAddressToolStripMenuItem";
            this.copyAddressToolStripMenuItem.Size = new System.Drawing.Size(221, 24);
            this.copyAddressToolStripMenuItem.Text = "Copy Address";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(218, 6);
            // 
            // mnuEditFind
            // 
            this.mnuEditFind.Name = "mnuEditFind";
            this.mnuEditFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.mnuEditFind.Size = new System.Drawing.Size(221, 24);
            this.mnuEditFind.Text = "&Find...";
            // 
            // mnuEditFindNext
            // 
            this.mnuEditFindNext.Name = "mnuEditFindNext";
            this.mnuEditFindNext.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.mnuEditFindNext.Size = new System.Drawing.Size(221, 24);
            this.mnuEditFindNext.Text = "Find &Next";
            // 
            // mnuEditFindPrevious
            // 
            this.mnuEditFindPrevious.Name = "mnuEditFindPrevious";
            this.mnuEditFindPrevious.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F3)));
            this.mnuEditFindPrevious.Size = new System.Drawing.Size(221, 24);
            this.mnuEditFindPrevious.Text = "Find &Previous";
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(218, 6);
            // 
            // mnuEditGoTo
            // 
            this.mnuEditGoTo.Name = "mnuEditGoTo";
            this.mnuEditGoTo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.mnuEditGoTo.Size = new System.Drawing.Size(221, 24);
            this.mnuEditGoTo.Text = "&Go To...";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(218, 6);
            // 
            // mnuEditBookmarks
            // 
            this.mnuEditBookmarks.Name = "mnuEditBookmarks";
            this.mnuEditBookmarks.Size = new System.Drawing.Size(221, 24);
            this.mnuEditBookmarks.Text = "Boo&kmarks";
            // 
            // mnuView
            // 
            this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuViewNavigateBackward,
            this.mnuViewNavigateForward});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(50, 23);
            this.mnuView.Text = "&View";
            // 
            // mnuViewNavigateBackward
            // 
            this.mnuViewNavigateBackward.Name = "mnuViewNavigateBackward";
            this.mnuViewNavigateBackward.Size = new System.Drawing.Size(194, 24);
            this.mnuViewNavigateBackward.Text = "Navigate &Backward";
            // 
            // mnuViewNavigateForward
            // 
            this.mnuViewNavigateForward.Name = "mnuViewNavigateForward";
            this.mnuViewNavigateForward.Size = new System.Drawing.Size(194, 24);
            this.mnuViewNavigateForward.Text = "Navigate &Forward";
            // 
            // mnuAnalyze
            // 
            this.mnuAnalyze.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAnalyzeExecutable});
            this.mnuAnalyze.Name = "mnuAnalyze";
            this.mnuAnalyze.Size = new System.Drawing.Size(68, 23);
            this.mnuAnalyze.Text = "&Analyze";
            // 
            // mnuAnalyzeExecutable
            // 
            this.mnuAnalyzeExecutable.Name = "mnuAnalyzeExecutable";
            this.mnuAnalyzeExecutable.Size = new System.Drawing.Size(193, 24);
            this.mnuAnalyzeExecutable.Text = "Analyze &Executable";
            this.mnuAnalyzeExecutable.Click += new System.EventHandler(this.mnuAnalyzeExecutable_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuHelpAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(49, 23);
            this.mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Name = "mnuHelpAbout";
            this.mnuHelpAbout.Size = new System.Drawing.Size(116, 24);
            this.mnuHelpAbout.Text = "&About";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Executable file|*.exe";
            this.openFileDialog1.Title = "Select DOS Executable File";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 475);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(925, 24);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // txtStatus
            // 
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(63, 19);
            this.txtStatus.Text = "Message";
            // 
            // lvSegments
            // 
            this.lvSegments.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7});
            this.lvSegments.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvSegments.FullRowSelect = true;
            this.lvSegments.HideSelection = false;
            this.lvSegments.Location = new System.Drawing.Point(12, 332);
            this.lvSegments.MultiSelect = false;
            this.lvSegments.Name = "lvSegments";
            this.lvSegments.Size = new System.Drawing.Size(225, 134);
            this.lvSegments.TabIndex = 15;
            this.lvSegments.UseCompatibleStateImageBehavior = false;
            this.lvSegments.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Segment";
            this.columnHeader7.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNavigateBackward,
            this.btnNavigateForward,
            this.toolStripSeparator1,
            this.cbBookmarks,
            this.btnGoToBookmark,
            this.toolStripSeparator2,
            this.cbFind,
            this.toolStripButton1,
            this.toolStripSeparator3,
            this.btnTest});
            this.toolStrip1.Location = new System.Drawing.Point(0, 27);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 2, 2);
            this.toolStrip1.Size = new System.Drawing.Size(925, 31);
            this.toolStrip1.TabIndex = 16;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnNavigateBackward
            // 
            this.btnNavigateBackward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnNavigateBackward.Image = ((System.Drawing.Image)(resources.GetObject("btnNavigateBackward.Image")));
            this.btnNavigateBackward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavigateBackward.Name = "btnNavigateBackward";
            this.btnNavigateBackward.Size = new System.Drawing.Size(23, 24);
            this.btnNavigateBackward.Text = "<";
            this.btnNavigateBackward.Click += new System.EventHandler(this.btnNavigateBackward_Click);
            // 
            // btnNavigateForward
            // 
            this.btnNavigateForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnNavigateForward.Image = ((System.Drawing.Image)(resources.GetObject("btnNavigateForward.Image")));
            this.btnNavigateForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavigateForward.Name = "btnNavigateForward";
            this.btnNavigateForward.Size = new System.Drawing.Size(23, 24);
            this.btnNavigateForward.Text = ">";
            this.btnNavigateForward.Click += new System.EventHandler(this.btnNavigateForward_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // cbBookmarks
            // 
            this.cbBookmarks.Items.AddRange(new object[] {
            "2920:17FC useful routine",
            "0000:6134 LOOP instruction",
            "0000:36DC proc with 5 parts",
            "3FE6:C830 single-entry jump table",
            "16C6:C830 single-entry jump table?",
            "2920:264A es?",
            "2920:377D jump table 1",
            "2920:8B53 jump table 2",
            "2920:6184 jump table 3",
            "2920:44B4 jump table 4",
            "2920:3FCC rep prefix",
            "2920:7430 program entry"});
            this.cbBookmarks.Name = "cbBookmarks";
            this.cbBookmarks.Size = new System.Drawing.Size(200, 27);
            // 
            // btnGoToBookmark
            // 
            this.btnGoToBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnGoToBookmark.Image = ((System.Drawing.Image)(resources.GetObject("btnGoToBookmark.Image")));
            this.btnGoToBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnGoToBookmark.Name = "btnGoToBookmark";
            this.btnGoToBookmark.Size = new System.Drawing.Size(31, 24);
            this.btnGoToBookmark.Text = "Go";
            this.btnGoToBookmark.Click += new System.EventHandler(this.btnGoToBookmark_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // cbFind
            // 
            this.cbFind.Items.AddRange(new object[] {
            "jmpn word ptr cs:[",
            "xlat"});
            this.cbFind.Name = "cbFind";
            this.cbFind.Size = new System.Drawing.Size(150, 27);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(39, 24);
            this.toolStripButton1.Text = "Find";
            this.toolStripButton1.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnTest
            // 
            this.btnTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnTest.Image = ((System.Drawing.Image)(resources.GetObject("btnTest.Image")));
            this.btnTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(38, 24);
            this.btnTest.Text = "Test";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Intervals";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Size";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(925, 499);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.lvSegments);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lvErrors);
            this.Controls.Add(this.lvProcedures);
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
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvListing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private Util.Forms.DoubleBufferedListView lvProcedures;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private Util.Forms.DoubleBufferedListView lvErrors;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuListing;
        private System.Windows.Forms.ToolStripMenuItem mnuListingGoToXRef;
        private System.Windows.Forms.ToolStripMenuItem mnuFileInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private Util.Forms.DoubleBufferedListView lvSegments;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem copyDisassemblyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyOpcodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyAddressToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFind;
        private System.Windows.Forms.ToolStripMenuItem mnuEditGoTo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuEditBookmarks;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFindNext;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFindPrevious;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private System.Windows.Forms.ToolStripMenuItem mnuViewNavigateBackward;
        private System.Windows.Forms.ToolStripMenuItem mnuViewNavigateForward;
        private System.Windows.Forms.ToolStripMenuItem mnuAnalyze;
        private System.Windows.Forms.ToolStripMenuItem mnuAnalyzeExecutable;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpAbout;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnNavigateBackward;
        private System.Windows.Forms.ToolStripButton btnNavigateForward;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox cbBookmarks;
        private System.Windows.Forms.ToolStripButton btnGoToBookmark;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripComboBox cbFind;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnTest;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
    }
}


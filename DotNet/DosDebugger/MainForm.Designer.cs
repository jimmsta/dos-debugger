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
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient1 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewDisassembly = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewHex = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuViewSegments = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewProcedures = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewErrors = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAnalyze = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAnalyzeExecutable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNavigateBackward = new System.Windows.Forms.ToolStripSplitButton();
            this.btnNavigateForward = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cbBookmarks = new System.Windows.Forms.ToolStripComboBox();
            this.btnGoToBookmark = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cbFind = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTest = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockPanel
            // 
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.Location = new System.Drawing.Point(0, 58);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(925, 417);
            dockPanelGradient1.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient1.StartColor = System.Drawing.SystemColors.ControlLight;
            autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
            tabGradient1.EndColor = System.Drawing.SystemColors.Control;
            tabGradient1.StartColor = System.Drawing.SystemColors.Control;
            tabGradient1.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            autoHideStripSkin1.TabGradient = tabGradient1;
            autoHideStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            dockPanelSkin1.AutoHideStripSkin = autoHideStripSkin1;
            tabGradient2.EndColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.StartColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient2.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
            dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
            tabGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            tabGradient3.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
            dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
            dockPaneStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            tabGradient4.EndColor = System.Drawing.SystemColors.ActiveCaption;
            tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient4.StartColor = System.Drawing.SystemColors.GradientActiveCaption;
            tabGradient4.TextColor = System.Drawing.SystemColors.ActiveCaptionText;
            dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
            tabGradient5.EndColor = System.Drawing.SystemColors.Control;
            tabGradient5.StartColor = System.Drawing.SystemColors.Control;
            tabGradient5.TextColor = System.Drawing.SystemColors.ControlText;
            dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
            dockPanelGradient3.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient3.StartColor = System.Drawing.SystemColors.ControlLight;
            dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
            tabGradient6.EndColor = System.Drawing.SystemColors.InactiveCaption;
            tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient6.StartColor = System.Drawing.SystemColors.GradientInactiveCaption;
            tabGradient6.TextColor = System.Drawing.SystemColors.InactiveCaptionText;
            dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
            tabGradient7.EndColor = System.Drawing.Color.Transparent;
            tabGradient7.StartColor = System.Drawing.Color.Transparent;
            tabGradient7.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
            dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
            dockPanelSkin1.DockPaneStripSkin = dockPaneStripSkin1;
            this.dockPanel.Skin = dockPanelSkin1;
            this.dockPanel.TabIndex = 1;
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
            this.mnuFileExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(41, 23);
            this.mnuFile.Text = "&File";
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuFileOpen.Size = new System.Drawing.Size(173, 24);
            this.mnuFileOpen.Text = "&Open...";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(170, 6);
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Name = "mnuFileExit";
            this.mnuFileExit.Size = new System.Drawing.Size(173, 24);
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
            this.mnuViewNavigateForward,
            this.toolStripMenuItem6,
            this.mnuViewDisassembly,
            this.mnuViewHex,
            this.toolStripMenuItem2,
            this.mnuViewSegments,
            this.mnuViewProcedures,
            this.mnuViewErrors,
            this.mnuViewProperties});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(50, 23);
            this.mnuView.Text = "&View";
            // 
            // mnuViewNavigateBackward
            // 
            this.mnuViewNavigateBackward.Enabled = false;
            this.mnuViewNavigateBackward.Image = global::DosDebugger.Properties.Resources.BackwardIcon;
            this.mnuViewNavigateBackward.Name = "mnuViewNavigateBackward";
            this.mnuViewNavigateBackward.Size = new System.Drawing.Size(194, 24);
            this.mnuViewNavigateBackward.Text = "Navigate &Backward";
            // 
            // mnuViewNavigateForward
            // 
            this.mnuViewNavigateForward.Enabled = false;
            this.mnuViewNavigateForward.Image = global::DosDebugger.Properties.Resources.ForwardIcon;
            this.mnuViewNavigateForward.Name = "mnuViewNavigateForward";
            this.mnuViewNavigateForward.Size = new System.Drawing.Size(194, 24);
            this.mnuViewNavigateForward.Text = "Navigate &Forward";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(191, 6);
            // 
            // mnuViewDisassembly
            // 
            this.mnuViewDisassembly.Name = "mnuViewDisassembly";
            this.mnuViewDisassembly.Size = new System.Drawing.Size(194, 24);
            this.mnuViewDisassembly.Text = "Disassembly";
            this.mnuViewDisassembly.Click += new System.EventHandler(this.mnuViewDisassembly_Click);
            // 
            // mnuViewHex
            // 
            this.mnuViewHex.Name = "mnuViewHex";
            this.mnuViewHex.Size = new System.Drawing.Size(194, 24);
            this.mnuViewHex.Text = "Hex View";
            this.mnuViewHex.Click += new System.EventHandler(this.mnuViewHex_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(191, 6);
            // 
            // mnuViewSegments
            // 
            this.mnuViewSegments.Image = ((System.Drawing.Image)(resources.GetObject("mnuViewSegments.Image")));
            this.mnuViewSegments.Name = "mnuViewSegments";
            this.mnuViewSegments.Size = new System.Drawing.Size(194, 24);
            this.mnuViewSegments.Text = "Segment List";
            this.mnuViewSegments.Click += new System.EventHandler(this.mnuViewSegments_Click);
            // 
            // mnuViewProcedures
            // 
            this.mnuViewProcedures.Image = global::DosDebugger.Properties.Resources.ProcedureIcon;
            this.mnuViewProcedures.Name = "mnuViewProcedures";
            this.mnuViewProcedures.Size = new System.Drawing.Size(194, 24);
            this.mnuViewProcedures.Text = "Procedure List";
            this.mnuViewProcedures.Click += new System.EventHandler(this.mnuViewProcedures_Click);
            // 
            // mnuViewErrors
            // 
            this.mnuViewErrors.Image = global::DosDebugger.Properties.Resources.WarningIcon;
            this.mnuViewErrors.Name = "mnuViewErrors";
            this.mnuViewErrors.Size = new System.Drawing.Size(194, 24);
            this.mnuViewErrors.Text = "Error List";
            this.mnuViewErrors.Click += new System.EventHandler(this.mnuViewErrors_Click);
            // 
            // mnuViewProperties
            // 
            this.mnuViewProperties.Image = global::DosDebugger.Properties.Resources.PropertyIcon;
            this.mnuViewProperties.Name = "mnuViewProperties";
            this.mnuViewProperties.Size = new System.Drawing.Size(194, 24);
            this.mnuViewProperties.Text = "Properties";
            this.mnuViewProperties.Click += new System.EventHandler(this.mnuViewProperties_Click);
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
            this.btnNavigateBackward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavigateBackward.Enabled = false;
            this.btnNavigateBackward.Image = global::DosDebugger.Properties.Resources.BackwardIcon;
            this.btnNavigateBackward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavigateBackward.Name = "btnNavigateBackward";
            this.btnNavigateBackward.Size = new System.Drawing.Size(32, 24);
            this.btnNavigateBackward.Text = "Navigate Backward";
            this.btnNavigateBackward.ButtonClick += new System.EventHandler(this.btnNavigateBackward_Click);
            this.btnNavigateBackward.DropDownClosed += new System.EventHandler(this.btnNavigateBackward_DropDownClosed);
            this.btnNavigateBackward.DropDownOpening += new System.EventHandler(this.btnNavigateBackward_DropDownOpening);
            // 
            // btnNavigateForward
            // 
            this.btnNavigateForward.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavigateForward.Enabled = false;
            this.btnNavigateForward.Image = global::DosDebugger.Properties.Resources.ForwardIcon;
            this.btnNavigateForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavigateForward.Name = "btnNavigateForward";
            this.btnNavigateForward.Size = new System.Drawing.Size(32, 24);
            this.btnNavigateForward.Text = "Navigate Forward";
            this.btnNavigateForward.ButtonClick += new System.EventHandler(this.btnNavigateForward_Click);
            this.btnNavigateForward.DropDownClosed += new System.EventHandler(this.btnNavigateForward_DropDownClosed);
            this.btnNavigateForward.DropDownOpening += new System.EventHandler(this.btnNavigateForward_DropDownOpening);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // cbBookmarks
            // 
            this.cbBookmarks.Items.AddRange(new object[] {
            "AD3A:76FA check xref context menu",
            "0000:1C38 simple routine",
            "0000:17FC complex routine",
            "2CF3:0AB4 large jump table",
            "0000:6134 LOOP instruction",
            "0000:36DC proc with 5 parts",
            "3FE6:C830 single-entry jump table",
            "16C6:C830 single-entry jump table?",
            "0000:264A es?",
            "0000:377D jump table 1",
            "0000:8B53 jump table 2",
            "0000:6184 jump table 3",
            "0000:44B4 jump table 4",
            "0000:3FCC rep prefix",
            "0000:7430 program entry"});
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(925, 499);
            this.Controls.Add(this.dockPanel);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "DOS Disassembler";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
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

        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFileExit;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox cbBookmarks;
        private System.Windows.Forms.ToolStripButton btnGoToBookmark;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripComboBox cbFind;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnTest;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem mnuViewDisassembly;
        private System.Windows.Forms.ToolStripMenuItem mnuViewSegments;
        private System.Windows.Forms.ToolStripMenuItem mnuViewProcedures;
        private System.Windows.Forms.ToolStripMenuItem mnuViewErrors;
        private System.Windows.Forms.ToolStripSplitButton btnNavigateBackward;
        private System.Windows.Forms.ToolStripMenuItem mnuViewProperties;
        private System.Windows.Forms.ToolStripMenuItem mnuViewHex;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripSplitButton btnNavigateForward;
    }
}


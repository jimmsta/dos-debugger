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
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnMzInfo = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.lvProcedures = new Util.Forms.DoubleBufferedListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvErrors = new Util.Forms.DoubleBufferedListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnGoTo = new System.Windows.Forms.Button();
            this.cbBookmarks = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lvListing
            // 
            this.lvListing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvListing.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvListing.FullRowSelect = true;
            this.lvListing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvListing.HideSelection = false;
            this.lvListing.Location = new System.Drawing.Point(183, 48);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.Size = new System.Drawing.Size(645, 248);
            this.lvListing.TabIndex = 0;
            this.lvListing.UseCompatibleStateImageBehavior = false;
            this.lvListing.View = System.Windows.Forms.View.Details;
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
            this.columnHeader3.Width = 340;
            // 
            // btnMzInfo
            // 
            this.btnMzInfo.Location = new System.Drawing.Point(728, 12);
            this.btnMzInfo.Name = "btnMzInfo";
            this.btnMzInfo.Size = new System.Drawing.Size(100, 30);
            this.btnMzInfo.TabIndex = 1;
            this.btnMzInfo.Text = "MZ Info...";
            this.btnMzInfo.UseVisualStyleBackColor = true;
            this.btnMzInfo.Click += new System.EventHandler(this.btnMzInfo_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(622, 12);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(100, 30);
            this.btnTest.TabIndex = 2;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Location = new System.Drawing.Point(12, 12);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(100, 30);
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
            this.lvProcedures.Location = new System.Drawing.Point(12, 48);
            this.lvProcedures.MultiSelect = false;
            this.lvProcedures.Name = "lvProcedures";
            this.lvProcedures.Size = new System.Drawing.Size(165, 416);
            this.lvProcedures.TabIndex = 4;
            this.lvProcedures.UseCompatibleStateImageBehavior = false;
            this.lvProcedures.View = System.Windows.Forms.View.Details;
            this.lvProcedures.SelectedIndexChanged += new System.EventHandler(this.lvProcedures_SelectedIndexChanged);
            this.lvProcedures.DoubleClick += new System.EventHandler(this.lvProcedures_DoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Procedure";
            this.columnHeader4.Width = 125;
            // 
            // lvErrors
            // 
            this.lvErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
            this.lvErrors.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvErrors.FullRowSelect = true;
            this.lvErrors.HideSelection = false;
            this.lvErrors.Location = new System.Drawing.Point(183, 302);
            this.lvErrors.MultiSelect = false;
            this.lvErrors.Name = "lvErrors";
            this.lvErrors.Size = new System.Drawing.Size(645, 162);
            this.lvErrors.TabIndex = 6;
            this.lvErrors.UseCompatibleStateImageBehavior = false;
            this.lvErrors.View = System.Windows.Forms.View.Details;
            this.lvErrors.DoubleClick += new System.EventHandler(this.lvErrors_DoubleClick);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Location";
            this.columnHeader5.Width = 125;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Message";
            this.columnHeader6.Width = 480;
            // 
            // btnGoTo
            // 
            this.btnGoTo.Location = new System.Drawing.Point(426, 12);
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
            "2920:377D jump table 1",
            "2920:8B53 jump table 2",
            "2920:6184 jump table 3",
            "2920:44B4 jump table 4",
            "2920:7430 program entry"});
            this.cbBookmarks.Location = new System.Drawing.Point(183, 15);
            this.cbBookmarks.Name = "cbBookmarks";
            this.cbBookmarks.Size = new System.Drawing.Size(237, 27);
            this.cbBookmarks.TabIndex = 8;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 476);
            this.Controls.Add(this.cbBookmarks);
            this.Controls.Add(this.btnGoTo);
            this.Controls.Add(this.lvErrors);
            this.Controls.Add(this.lvProcedures);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnMzInfo);
            this.Controls.Add(this.lvListing);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "DOS Disassembler";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvListing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btnMzInfo;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnAnalyze;
        private Util.Forms.DoubleBufferedListView lvProcedures;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private Util.Forms.DoubleBufferedListView lvErrors;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button btnGoTo;
        private System.Windows.Forms.ComboBox cbBookmarks;
    }
}


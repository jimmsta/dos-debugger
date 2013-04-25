namespace DosDebugger
{
    partial class ListingWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListingWindow));
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuListingGoToXRef = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cbSegments = new System.Windows.Forms.ComboBox();
            this.cbProcedures = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.txtStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuListing.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvListing
            // 
            this.lvListing.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvListing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.tableLayoutPanel1.SetColumnSpan(this.lvListing, 2);
            this.lvListing.ContextMenuStrip = this.contextMenuListing;
            this.lvListing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvListing.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvListing.FullRowSelect = true;
            this.lvListing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvListing.HideSelection = false;
            this.lvListing.Location = new System.Drawing.Point(1, 51);
            this.lvListing.Margin = new System.Windows.Forms.Padding(1);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.Size = new System.Drawing.Size(717, 263);
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
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.cbSegments, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lvListing, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cbProcedures, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(719, 315);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // cbSegments
            // 
            this.cbSegments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSegments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSegments.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbSegments.FormattingEnabled = true;
            this.cbSegments.Location = new System.Drawing.Point(1, 1);
            this.cbSegments.Margin = new System.Windows.Forms.Padding(1);
            this.cbSegments.Name = "cbSegments";
            this.cbSegments.Size = new System.Drawing.Size(357, 27);
            this.cbSegments.TabIndex = 1;
            // 
            // cbProcedures
            // 
            this.cbProcedures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbProcedures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProcedures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbProcedures.FormattingEnabled = true;
            this.cbProcedures.Location = new System.Drawing.Point(360, 1);
            this.cbProcedures.Margin = new System.Windows.Forms.Padding(1);
            this.cbProcedures.Name = "cbProcedures";
            this.cbProcedures.Size = new System.Drawing.Size(358, 27);
            this.cbProcedures.TabIndex = 2;
            this.cbProcedures.SelectedIndexChanged += new System.EventHandler(this.cbProcedures_SelectedIndexChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.txtStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 315);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(719, 25);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(97, 23);
            this.toolStripSplitButton1.Text = "Split Button";
            // 
            // txtStatus
            // 
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(71, 20);
            this.txtStatus.Text = "(Message)";
            // 
            // ListingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 340);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ListingWindow";
            this.Text = "Disassembly";
            this.Load += new System.EventHandler(this.ListingWindow_Load);
            this.contextMenuListing.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
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
        private System.Windows.Forms.ContextMenuStrip contextMenuListing;
        private System.Windows.Forms.ToolStripMenuItem mnuListingGoToXRef;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox cbSegments;
        private System.Windows.Forms.ComboBox cbProcedures;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripStatusLabel txtStatus;
    }
}
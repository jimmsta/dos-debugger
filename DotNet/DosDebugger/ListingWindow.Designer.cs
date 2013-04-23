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
            this.lvListing = new Util.Forms.DoubleBufferedListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuListing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuListingGoToXRef = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuListing.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvListing
            // 
            this.lvListing.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvListing.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lvListing.ContextMenuStrip = this.contextMenuListing;
            this.lvListing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvListing.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvListing.FullRowSelect = true;
            this.lvListing.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvListing.HideSelection = false;
            this.lvListing.Location = new System.Drawing.Point(0, 0);
            this.lvListing.MultiSelect = false;
            this.lvListing.Name = "lvListing";
            this.lvListing.Size = new System.Drawing.Size(495, 270);
            this.lvListing.TabIndex = 1;
            this.lvListing.UseCompatibleStateImageBehavior = false;
            this.lvListing.View = System.Windows.Forms.View.Details;
            this.lvListing.VirtualMode = true;
            this.lvListing.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lvListing_RetrieveVirtualItem);
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
            // ListingWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 270);
            this.Controls.Add(this.lvListing);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.071428F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ListingWindow";
            this.Text = "Disassembly";
            this.contextMenuListing.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvListing;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ContextMenuStrip contextMenuListing;
        private System.Windows.Forms.ToolStripMenuItem mnuListingGoToXRef;
    }
}
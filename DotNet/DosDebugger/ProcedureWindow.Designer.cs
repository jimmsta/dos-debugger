namespace DosDebugger
{
    partial class ProcedureWindow
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
            this.lvProcedures = new Util.Forms.DoubleBufferedListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mnuContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuContextCallees = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuContextCallers = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvProcedures
            // 
            this.lvProcedures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader8,
            this.columnHeader9});
            this.lvProcedures.ContextMenuStrip = this.mnuContext;
            this.lvProcedures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProcedures.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvProcedures.FullRowSelect = true;
            this.lvProcedures.HideSelection = false;
            this.lvProcedures.Location = new System.Drawing.Point(0, 0);
            this.lvProcedures.MultiSelect = false;
            this.lvProcedures.Name = "lvProcedures";
            this.lvProcedures.Size = new System.Drawing.Size(288, 262);
            this.lvProcedures.TabIndex = 5;
            this.lvProcedures.UseCompatibleStateImageBehavior = false;
            this.lvProcedures.View = System.Windows.Forms.View.Details;
            this.lvProcedures.DoubleClick += new System.EventHandler(this.lvProcedures_DoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Entry Point";
            this.columnHeader4.Width = 100;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Intervals";
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Size";
            // 
            // mnuContext
            // 
            this.mnuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuContextCallees,
            this.toolStripMenuItem1,
            this.mnuContextCallers});
            this.mnuContext.Name = "mnuContext";
            this.mnuContext.Size = new System.Drawing.Size(217, 80);
            this.mnuContext.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.mnuContext_Closed);
            this.mnuContext.Opening += new System.ComponentModel.CancelEventHandler(this.mnuContext_Opening);
            // 
            // mnuContextCallees
            // 
            this.mnuContextCallees.Enabled = false;
            this.mnuContextCallees.Image = global::DosDebugger.Properties.Resources.OutgoingLink;
            this.mnuContextCallees.Name = "mnuContextCallees";
            this.mnuContextCallees.Size = new System.Drawing.Size(216, 24);
            this.mnuContextCallees.Text = "Calling ? Procedures";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(213, 6);
            // 
            // mnuContextCallers
            // 
            this.mnuContextCallers.Enabled = false;
            this.mnuContextCallers.Image = global::DosDebugger.Properties.Resources.IncomingLink;
            this.mnuContextCallers.Name = "mnuContextCallers";
            this.mnuContextCallers.Size = new System.Drawing.Size(216, 24);
            this.mnuContextCallers.Text = "Called By ? Procedures";
            // 
            // ProcedureWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 262);
            this.Controls.Add(this.lvProcedures);
            this.Name = "ProcedureWindow";
            this.TabText = "Procedures";
            this.Text = "ProcedureWindow";
            this.Load += new System.EventHandler(this.ProcedureWindow_Load);
            this.mnuContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvProcedures;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ContextMenuStrip mnuContext;
        private System.Windows.Forms.ToolStripMenuItem mnuContextCallees;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuContextCallers;
    }
}
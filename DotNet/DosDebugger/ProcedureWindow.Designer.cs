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
            this.lvProcedures = new Util.Forms.DoubleBufferedListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvProcedures
            // 
            this.lvProcedures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader8,
            this.columnHeader9});
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
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvProcedures;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
    }
}
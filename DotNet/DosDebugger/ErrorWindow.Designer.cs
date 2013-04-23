namespace DosDebugger
{
    partial class ErrorWindow
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
            this.lvErrors = new Util.Forms.DoubleBufferedListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvErrors
            // 
            this.lvErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
            this.lvErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvErrors.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvErrors.FullRowSelect = true;
            this.lvErrors.HideSelection = false;
            this.lvErrors.Location = new System.Drawing.Point(0, 0);
            this.lvErrors.MultiSelect = false;
            this.lvErrors.Name = "lvErrors";
            this.lvErrors.Size = new System.Drawing.Size(288, 262);
            this.lvErrors.TabIndex = 7;
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
            // ErrorWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 262);
            this.Controls.Add(this.lvErrors);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.071428F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "ErrorWindow";
            this.TabText = "Error List";
            this.Text = "ErrorWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvErrors;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
    }
}
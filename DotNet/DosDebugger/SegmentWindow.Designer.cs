namespace DosDebugger
{
    partial class SegmentWindow
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
            this.lvSegments = new Util.Forms.DoubleBufferedListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lvSegments
            // 
            this.lvSegments.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader1});
            this.lvSegments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSegments.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvSegments.FullRowSelect = true;
            this.lvSegments.HideSelection = false;
            this.lvSegments.Location = new System.Drawing.Point(0, 0);
            this.lvSegments.MultiSelect = false;
            this.lvSegments.Name = "lvSegments";
            this.lvSegments.Size = new System.Drawing.Size(288, 262);
            this.lvSegments.TabIndex = 16;
            this.lvSegments.UseCompatibleStateImageBehavior = false;
            this.lvSegments.View = System.Windows.Forms.View.Details;
            this.lvSegments.DoubleClick += new System.EventHandler(this.lvSegments_DoubleClick);
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Start";
            this.columnHeader7.Width = 150;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "End";
            this.columnHeader1.Width = 150;
            // 
            // SegmentWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 262);
            this.Controls.Add(this.lvSegments);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.071428F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "SegmentWindow";
            this.TabText = "Segments";
            this.Text = "SegmentWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedListView lvSegments;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}
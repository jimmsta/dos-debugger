namespace DosDebugger
{
    partial class CallGraphWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CallGraphWindow));
            this.panelScroller = new System.Windows.Forms.Panel();
            this.panelCanvas = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOutputDot = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnArrangeLayout = new System.Windows.Forms.ToolStripButton();
            this.panelScroller.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelScroller
            // 
            this.panelScroller.AutoScroll = true;
            this.panelScroller.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelScroller.Controls.Add(this.panelCanvas);
            this.panelScroller.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelScroller.Location = new System.Drawing.Point(0, 26);
            this.panelScroller.Name = "panelScroller";
            this.panelScroller.Size = new System.Drawing.Size(543, 331);
            this.panelScroller.TabIndex = 0;
            // 
            // panelCanvas
            // 
            this.panelCanvas.Location = new System.Drawing.Point(80, 92);
            this.panelCanvas.Name = "panelCanvas";
            this.panelCanvas.Size = new System.Drawing.Size(200, 100);
            this.panelCanvas.TabIndex = 0;
            this.panelCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCanvas_Paint);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOutputDot,
            this.toolStripSeparator1,
            this.btnArrangeLayout});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(543, 26);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnOutputDot
            // 
            this.btnOutputDot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnOutputDot.Image = ((System.Drawing.Image)(resources.GetObject("btnOutputDot.Image")));
            this.btnOutputDot.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOutputDot.Name = "btnOutputDot";
            this.btnOutputDot.Size = new System.Drawing.Size(109, 23);
            this.btnOutputDot.Text = "Output Dot File";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 26);
            // 
            // btnArrangeLayout
            // 
            this.btnArrangeLayout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnArrangeLayout.Image = ((System.Drawing.Image)(resources.GetObject("btnArrangeLayout.Image")));
            this.btnArrangeLayout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnArrangeLayout.Name = "btnArrangeLayout";
            this.btnArrangeLayout.Size = new System.Drawing.Size(108, 23);
            this.btnArrangeLayout.Text = "Arrange Layout";
            // 
            // CallGraphWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(543, 357);
            this.Controls.Add(this.panelScroller);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CallGraphWindow";
            this.Text = "CallGraphWindow";
            this.Load += new System.EventHandler(this.CallGraphWindow_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CallGraphWindow_Paint);
            this.panelScroller.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelScroller;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnOutputDot;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnArrangeLayout;
        private System.Windows.Forms.Panel panelCanvas;
    }
}
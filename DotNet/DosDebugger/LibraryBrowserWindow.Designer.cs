namespace DosDebugger
{
    partial class LibraryBrowserWindow
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Function 1");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Function 2");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Module 1", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Module 2");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("LibName", new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode4});
            this.tvLibrary = new Util.Forms.DoubleBufferedTreeView();
            this.SuspendLayout();
            // 
            // tvLibrary
            // 
            this.tvLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvLibrary.FullRowSelect = true;
            this.tvLibrary.HideSelection = false;
            this.tvLibrary.Location = new System.Drawing.Point(0, 0);
            this.tvLibrary.Name = "tvLibrary";
            treeNode1.Name = "Node3";
            treeNode1.Text = "Function 1";
            treeNode2.Name = "Node4";
            treeNode2.Text = "Function 2";
            treeNode3.Name = "Node1";
            treeNode3.Text = "Module 1";
            treeNode4.Name = "Node2";
            treeNode4.Text = "Module 2";
            treeNode5.Name = "Node0";
            treeNode5.Text = "LibName";
            this.tvLibrary.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode5});
            this.tvLibrary.ShowLines = false;
            this.tvLibrary.Size = new System.Drawing.Size(286, 331);
            this.tvLibrary.TabIndex = 0;
            this.tvLibrary.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvLibrary_AfterSelect);
            // 
            // LibraryBrowserWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 331);
            this.Controls.Add(this.tvLibrary);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "LibraryBrowserWindow";
            this.TabText = "Library Browser";
            this.Text = "Library Browser";
            this.Load += new System.EventHandler(this.LibraryBrowserWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Util.Forms.DoubleBufferedTreeView tvLibrary;
    }
}
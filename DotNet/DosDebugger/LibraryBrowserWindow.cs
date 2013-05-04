using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using Util.Forms;

namespace DosDebugger
{
    public partial class LibraryBrowserWindow : ToolWindow
    {
        ObjectLibrary library;

        public LibraryBrowserWindow()
        {
            InitializeComponent();
        }

        public ObjectLibrary Library
        {
            get { return this.library; }
            set
            {
                this.library = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            tvLibrary.Nodes.Clear();
            TreeNode root = tvLibrary.Nodes.Add("Library");
            foreach (var module in library.Modules)
            {
                TreeNode nodeModule = root.Nodes.Add(module.Name);
                foreach (var sym in module.PublicNames)
                {
                    nodeModule.Nodes.Add(sym.ToString());
                }
            }
        }

        private void LibraryBrowserWindow_Load(object sender, EventArgs e)
        {
            tvLibrary.SetWindowTheme("explorer");
        }

        private void tvLibrary_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Disassembler;
using X86Codec;

namespace DosDebugger
{
    public partial class ToolWindow : DockContent
    {
        public ToolWindow()
        {
            InitializeComponent();
        }
    }

    public class NavigationRequestedEventArgs : EventArgs
    {
        Pointer location;

        public NavigationRequestedEventArgs(Pointer location)
        {
            this.location = location;
        }

        public Pointer Location
        {
            get { return this.location; }
        }
    }
}

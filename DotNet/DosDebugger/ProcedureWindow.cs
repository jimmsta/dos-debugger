using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using System.Drawing;
using X86Codec;

namespace DosDebugger
{
    public partial class ProcedureWindow : ToolWindow
    {
        private Document document;

        // TODO: dispose monoFont when no longer used
        private Font monoFont;

        public ProcedureWindow()
        {
            InitializeComponent();
            this.monoFont = new Font(FontFamily.GenericMonospace, mnuContext.Font.Size);
        }

        internal Document Document
        {
            get { return this.document; }
            set
            {
                this.document = value;
                UpdateUI();
            }
        }

        private void ProcedureWindow_Load(object sender, EventArgs e)
        {
            // UpdateUI();
        }

        private void UpdateUI()
        {
            lvProcedures.Items.Clear();

            if (document == null)
                return;

            Disassembler16 dasm = document.Disassembler;
            Dictionary<UInt16, int> segStat = new Dictionary<UInt16, int>();
            foreach (Procedure proc in dasm.Procedures)
            {
                ListViewItem item = new ListViewItem();
                item.Text = proc.EntryPoint.ToString();
                //item.SubItems.Add(proc.ByteRange.Intervals.Count.ToString());
                //item.SubItems.Add(proc.ByteRange.Length.ToString());
                item.SubItems.Add("?");
                item.SubItems.Add("?"); // proc.Bounds.Length.ToString());
                item.Tag = proc;
                lvProcedures.Items.Add(item);
                segStat[proc.EntryPoint.Segment] = 1;
            }
        }

        private void lvProcedures_DoubleClick(object sender, EventArgs e)
        {
            if (lvProcedures.SelectedIndices.Count == 1)
            {
                Procedure proc = (Procedure)lvProcedures.SelectedItems[0].Tag;
                document.Navigator.SetLocation(proc.EntryPoint, this);
            }
        }

        private void mnuContext_Opening(object sender, CancelEventArgs e)
        {
            if (lvProcedures.SelectedIndices.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            Procedure proc = (Procedure)lvProcedures.SelectedItems[0].Tag;

            // Display procedures calling this procedure.
            int i = mnuContext.Items.IndexOf(mnuContextCallers);
            int n = 0;
            foreach (Procedure caller in proc.GetCallers())
            {
                mnuContext.Items.Insert(++i, CreateContextMenuItem(caller));
                n++;
            }
            mnuContextCallers.Text =
                string.Format("Called By {0} Procedures", n);

            // Display procedures called by this procedure.
            i = mnuContext.Items.IndexOf(mnuContextCallees);
            n = 0;
            foreach (Procedure callee in proc.GetCallees())
            {
                mnuContext.Items.Insert(++i, CreateContextMenuItem(callee));
                n++;
            }
            mnuContextCallees.Text =
                string.Format("Calling {0} Procedures", n);
        }

        private ToolStripItem CreateContextMenuItem(Procedure proc)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = string.Format("{0}  sub_{1}",
                proc.EntryPoint, proc.EntryPoint.LinearAddress);
            item.Font = monoFont;
            item.Tag = proc;
            item.Click += mnuContextItem_Click;
            return item;
        }
        
        void mnuContextItem_Click(object sender, EventArgs e)
        {
            Procedure proc = (Procedure)((ToolStripMenuItem)sender).Tag;
            document.Navigator.SetLocation(proc.EntryPoint, this);
        }

        private void mnuContext_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            mnuContext.Items.ClearAndDispose(
                mnuContext.Items.IndexOf(mnuContextCallees) + 1,
                mnuContext.Items.IndexOf(mnuContextCallers) - 1);
            mnuContext.Items.ClearAndDispose(
                mnuContext.Items.IndexOf(mnuContextCallers) + 1,
                mnuContext.Items.Count);
        }
    }

#if false
    public class ProcedureActivatedEventArgs : EventArgs
    {
        Procedure proc;

        public ProcedureActivatedEventArgs(Procedure procedure)
        {
            this.proc = procedure;
        }

        public Procedure Procedure
        {
            get { return proc; }
        }
    }
#endif
}

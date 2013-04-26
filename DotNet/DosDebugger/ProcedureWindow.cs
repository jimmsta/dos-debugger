using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;

namespace DosDebugger
{
    public partial class ProcedureWindow : ToolWindow
    {
        private Document document;

        public ProcedureWindow()
        {
            InitializeComponent();
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
                item.SubItems.Add(proc.ByteRange.Intervals.Count.ToString());
                item.SubItems.Add(proc.ByteRange.Length.ToString());
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

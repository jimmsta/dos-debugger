using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using X86Codec;

namespace DosDebugger
{
    public partial class SegmentWindow : ToolWindow
    {
        public SegmentWindow()
        {
            InitializeComponent();
        }

        private Document document;

        internal Document Document
        {
            get { return this.document; }
            set
            {
                this.document = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            lvSegments.Items.Clear();

            if (document == null)
                return;

            Disassembler16 dasm = document.Disassembler;
            Dictionary<UInt16, int> segStat = new Dictionary<UInt16, int>();
            Procedure[] procs = dasm.Procedures;
            foreach (Procedure proc in procs)
            {
                segStat[proc.EntryPoint.Segment] = 1;
            }

            // Display segments.
            ushort[] segs = new ushort[segStat.Count];
            segStat.Keys.CopyTo(segs, 0);
            Array.Sort(segs);
            foreach (ushort s in segs)
            {
                lvSegments.Items.Add(s.ToString("X4"));
            }
        }
    }
}

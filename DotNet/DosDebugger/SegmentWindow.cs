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

            foreach (Segment segment in document.Disassembler.Segments)
            {
                ListViewItem item = new ListViewItem();
                item.Text = segment.StartAddress.ToString();
                item.Tag = segment;
                lvSegments.Items.Add(item);
            }
        }
    }
}

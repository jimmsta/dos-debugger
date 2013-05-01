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

            foreach (Segment segment in document.Disassembler.Image.Segments)
            {
                ListViewItem item = new ListViewItem();
                item.Text = FormatAddress(segment.Start);
                item.SubItems.Add(FormatAddress(segment.End));
                item.Tag = segment;
                lvSegments.Items.Add(item);
            }
        }

        private static string FormatAddress(Pointer address)
        {
            return string.Format("{0} ({1:X5})", address, address.LinearAddress);
        }

        //public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        private void lvSegments_DoubleClick(object sender, EventArgs e)
        {
            if (lvSegments.SelectedIndices.Count == 0)
                return;

            Segment segment = (Segment)lvSegments.SelectedItems[0].Tag;
            document.Navigator.SetLocation(segment.Start, this);
        }
    }
}

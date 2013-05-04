using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            foreach (Segment segment in document.Image.Segments)
            {
                ListViewItem item = new ListViewItem();
                item.Text = FormatAddress(segment.StartAddress, segment.SegmentAddress);
                item.SubItems.Add(FormatAddress(segment.EndAddress - 1, segment.SegmentAddress));
                item.Tag = segment;
                lvSegments.Items.Add(item);
            }
        }

        private static string FormatAddress(LinearPointer address, UInt16 segment)
        {
            return string.Format(
                "{0} ({1:X5})", new Pointer(segment, address), address);
        }

        //public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        private void lvSegments_DoubleClick(object sender, EventArgs e)
        {
            if (lvSegments.SelectedIndices.Count == 0)
                return;

            Segment segment = (Segment)lvSegments.SelectedItems[0].Tag;
            document.Navigator.SetLocation(
                segment.StartAddress.ToFarPointer(segment.SegmentAddress), this);
        }
    }
}

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
    public partial class ListingWindow : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public ListingWindow()
        {
            InitializeComponent();
        }

        private Document document;
        private ListingViewModel listingView;

        internal Document Document
        {
            get { return this.document; }
            set
            {
                this.document = value;
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            lvListing.VirtualListSize = 0;
            if (document == null)
                return;

            listingView = new ListingViewModel(document.Disassembler);
            lvListing.VirtualListSize = listingView.Rows.Count;

            // Fill the procedure window.
            cbProcedures.Items.Clear();
            //foreach (Procedure proc in document.Disassembler.Procedures)
            //{
                //cbProcedures.Items.Add(new ProcedureItem(proc));
            //}
            cbProcedures.Items.AddRange(listingView.ProcedureItems.ToArray());

            // Fill the segment window.
            cbSegments.Items.Clear();
            foreach (Pointer segStart in document.Disassembler.Segments)
            {
                cbSegments.Items.Add(new SegmentListItem(segStart));
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (lvListing.SelectedIndices.Count > 0)
                    return lvListing.SelectedIndices[0];
                else
                    return -1;
            }
        }

        private void lvListing_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = listingView.CreateViewItem(viewportBeginIndex + e.ItemIndex);
        }

        private void contextMenuListing_Opening(object sender, CancelEventArgs e)
        {
            mnuListingGoToXRef.DropDownItems.Clear();
            mnuListingGoToXRef.Enabled = false;

            int index = SelectedIndex;
            if (index == -1)
                return;

            Pointer location = listingView.Rows[index].Location;
            if (location == Pointer.Invalid)
                return;

            foreach (XRef xref in document.Disassembler.GetReferencesTo(location))
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = xref.Source.ToString();
                item.Click += mnuListingGoToXRefItem_Click;
                item.Tag = xref.Source;
                mnuListingGoToXRef.DropDownItems.Add(item);
            }
            mnuListingGoToXRef.Enabled = mnuListingGoToXRef.HasDropDownItems;
        }

        private void mnuListingGoToXRefItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Pointer source = (Pointer)item.Tag;
            if (NavigationRequested != null)
            {
                NavigationRequestedEventArgs args =
                    new NavigationRequestedEventArgs(source);
                NavigationRequested(this, args);
            }
        }

        public bool Navigate(Pointer target)
        {
            // Find the smallest entry that is greater than target.
            for (int i = 0; i < listingView.Rows.Count; i++)
            {
                Pointer current = listingView.Rows[i].Location;
                if (current != Pointer.Invalid &&
                    current.EffectiveAddress > target.EffectiveAddress)
                {
                    if (i == 0)
                        GoToRow(0, true);
                    else
                        GoToRow(i - 1, true);
                    return true;
                }
            }
            return false;
        }

        private void GoToRow(int index, bool bringToTop = false)
        {
            ListViewItem item = lvListing.Items[index];
            lvListing.Focus();
            if (bringToTop)
                lvListing.TopItem = item;
            else
                item.EnsureVisible();
            item.Focused = true;
            item.Selected = true;
        }

        public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        private void ListingWindow_Load(object sender, EventArgs e)
        {
            tableLayoutPanel1.RowStyles[0].Height =
                cbSegments.Height + cbSegments.Margin.Vertical;
        }

        private void lvListing_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = this.SelectedIndex;
            if (i == -1)
                return;

            Pointer address = listingView.Rows[i].Location;
            ByteProperties b = document.Disassembler.GetByteProperties(address);
            if (b == null)
                return;

            this.ActiveSegment = address.Segment;
        }

        // Keeps track of the segment selected. If this value is different
        // from what is displayed in the UI, then either the UI must be
        // updated or an ActiveSegmentChanged event must be raised.
        private ushort activeSegment;

        /// <summary>
        /// Gets or sets the active segment selected in the window. A value
        /// of 0xFFFF indicates no segment is selected.
        /// </summary>
        public UInt16 ActiveSegment
        {
            get
            {
                if (cbSegments.SelectedIndex < 0)
                    return 0xFFFF;
                else
                    return UInt16.Parse(cbSegments.SelectedText.Substring(0, 4));
            }
            set
            {
                int k = Array.BinarySearch(
                    document.Disassembler.Segments,
                    new Pointer(value, 0));
                if (k < 0)
                    k = ~k;
                if (k < cbSegments.Items.Count &&
                    UInt16.Parse(cbSegments.Items[k].ToString().Substring(0, 4)) == value)
                {
                    cbSegments.SelectedIndex = k;
                }
            }
        }

        private void cbProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProcedures.SelectedIndex < 0)
                return;

            ProcedureItem item = (ProcedureItem)cbProcedures.SelectedItem;
            Navigate(item.Procedure.EntryPoint);

            // Filter out only those rows we're interested in.
            viewportBeginIndex = item.BeginIndex;
            viewportEndIndex = item.EndIndex;
            lvListing.VirtualListSize = viewportEndIndex - viewportBeginIndex;
        }

        int viewportBeginIndex;
        int viewportEndIndex;
    }
}

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
            e.Item = listingView.CreateViewItem(e.ItemIndex);
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
            NavigateTo(source);
        }

        private LinkedList<int> navHistory = new LinkedList<int>();
        private LinkedListNode<int> navCurrent = null;

        public bool NavigateBackward()
        {
             if (navCurrent != null && navCurrent.Previous != null)
            {
                navCurrent = navCurrent.Previous;
                GoToRow(navCurrent.Value);
                 return true;
            }
            return false;
        }

        public bool NavigateForward()
        {
            if (navCurrent != null && navCurrent.Next != null)
            {
                navCurrent = navCurrent.Next;
                GoToRow(navCurrent.Value);
                return true;
            }
            return false;
        }

        public bool NavigateTo(Pointer target)
        {
            // Find the first entry that is greater than or equal to target.
            for (int i = 0; i < listingView.Rows.Count; i++)
            {
                Pointer current = listingView.Rows[i].Location;
                if (current != Pointer.Invalid &&
                    current.EffectiveAddress >= target.EffectiveAddress)
                {
                    GoToRow(i, true);
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

            // Update navigation history.
                if (navCurrent == null)
                {
                    navCurrent = navHistory.AddFirst(index);
                    return;
                }

            if (index == navCurrent.Value)
                return;
            if (navCurrent.Next != null && index == navCurrent.Next.Value)
            {
                navCurrent = navCurrent.Next;
                return;
            }

            while (navCurrent.Next != null)
            {
                navHistory.RemoveLast();
            }
            navCurrent = navHistory.AddAfter(navCurrent, index);
        }

        
    }
}

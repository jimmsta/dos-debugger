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
        private ListingViewModel viewModel;

        // viewport control
        private int viewportBeginIndex;
        private int viewportEndIndex;
        private ListingScope scope;
        private int activeRowIndex;

        /// <summary>
        /// Gets or sets the Document object being displayed. This value
        /// may be null.
        /// </summary>
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
            lvListing.VirtualListSize = 0;
            viewModel = null;
            if (document == null)
                return;

            // Create the view model.
            viewModel = new ListingViewModel(document.Disassembler);

            // Fill the procedure window.
            cbProcedures.Items.Clear();
            cbProcedures.Items.AddRange(viewModel.ProcedureItems.ToArray());

            // Fill the segment window.
            cbSegments.Items.Clear();
            cbSegments.Items.AddRange(viewModel.SegmentItems.ToArray());

            // Display the listing rows.
            scope = ListingScope.Procedure;
            UpdateScope();
        }

        private void UpdateScope()
        {
            if (viewModel.Rows.Count == 0)
                return;

            if (scope == ListingScope.Executable)
            {
                DisplayViewport(0, viewModel.Rows.Count);
                return;
            }
            
            if (scope == ListingScope.Segment)
            {
                UInt16 seg = viewModel.Rows[activeRowIndex].Location.Segment;
                Segment s = document.Disassembler.FindSegment(seg);
                int k1 = viewModel.FindRowIndex(s.StartAddress);
                int k2 = viewModel.FindRowIndex(s.EndAddress);
                DisplayViewport(k1, k2);
                return;
            }

            if (scope == ListingScope.Procedure)
            {
                Pointer address = viewModel.Rows[activeRowIndex].Location;
                Procedure proc = document.Disassembler.Image[address].Procedure;
                if (proc == null)
                {
                    DisplayViewport(0, viewModel.Rows.Count);
                }
                else
                {
                    Range r = proc.ByteRange.BoundingRange;
                    int k1 = viewModel.FindRowIndex(r.Begin);
                    int k2 = viewModel.FindRowIndex(r.End);
                    DisplayViewport(k1, k2);
                }
                return;
            }
        }

        private void DisplayViewport(int beginIndex, int endIndex)
        {
            if (beginIndex < 0 || beginIndex > viewModel.Rows.Count)
                throw new ArgumentOutOfRangeException("beginIndex");
            if (endIndex < beginIndex || endIndex > viewModel.Rows.Count)
                throw new ArgumentOutOfRangeException("endIndex");

            // We should keep the selected item still selected.
            // This is a bit complicated with our current implementation,
            // but should be more straightforward if we use ObjectListView's
            // filter functionality.
            // activeRowIndex = this.viewportBeginIndex + activeRowIndex - beginIndex;

            this.viewportBeginIndex = beginIndex;
            this.viewportEndIndex = endIndex;
            lvListing.VirtualListSize = endIndex - beginIndex;
        }

        internal ListingScope ListingScope
        {
            get { return this.scope; }
            set
            {
                if (scope == value)
                    return;
                scope = value;
                UpdateScope();
            }
        }

        private void lvListing_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = viewModel.CreateViewItem(viewportBeginIndex + e.ItemIndex);
        }

        private void contextMenuListing_Opening(object sender, CancelEventArgs e)
        {
            // TODO: need to dispose() unused items.
#if true
            mnuListingGoToXRef.DropDownItems.Clear();
            mnuListingGoToXRef.Enabled = false;

            if (lvListing.SelectedIndices.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            int index = lvListing.SelectedIndices[0];

            Pointer location = viewModel.Rows[viewportBeginIndex + index].Location;
            if (location == Pointer.Invalid)
                return;

            foreach (XRef xref in document.Disassembler.GetReferencesTo(location))
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = string.Format(
                    "{0} {1}", xref.Source, xref.Type);
                item.Click += mnuListingGoToXRefItem_Click;
                item.Tag = xref.Source;
                mnuListingGoToXRef.DropDownItems.Add(item);
            }
            mnuListingGoToXRef.Enabled = mnuListingGoToXRef.HasDropDownItems;
#endif
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

        // TODO: what should we do if the navigation target is out of the current sub?
        public bool Navigate(Pointer target)
        {
            return Navigate(target, true, true);
        }

        // TODO: what should we do if the navigation target is out of the current sub?
        /// <summary>
        /// Navigates to the given address. This also updates the current
        /// segment and procedure displayed at the top of the window.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="scrollToTop"></param>
        /// <param name="setFocus"></param>
        /// <returns></returns>
        private bool Navigate(Pointer target, bool scrollToTop, bool setFocus)
        {
            if (viewModel.Rows.Count == 0)
                return false;

            int rowIndex = viewModel.FindRowIndex(target);
            activeRowIndex = rowIndex;
            UpdateScope();

            if (rowIndex < viewportBeginIndex || rowIndex >= viewportEndIndex)
            {
                throw new NotImplementedException();
            }

            ListViewItem item = lvListing.Items[rowIndex - viewportBeginIndex];

            item.Selected = true;
            item.Focused = true;

            if (scrollToTop)
                item.ScrollToTop();
            else
                item.EnsureVisible();

            if (setFocus)
                item.ListView.Focus();

            return true;
        }

        public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        // can we hard code it?
        private void ListingWindow_Load(object sender, EventArgs e)
        {
            tableLayoutPanel1.RowStyles[0].Height =
                cbSegments.Height + cbSegments.Margin.Vertical;
        }

        private void lvListing_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtStatus.Text = "";
            if (lvListing.SelectedIndices.Count == 0)
                return;

            int i = lvListing.SelectedIndices[0];
            activeRowIndex = i;

            // Display brief description of instruction.
            ListingRow row = viewModel.Rows[viewportBeginIndex + i];
            if (row is CodeListingRow)
            {
                Operation op = ((CodeListingRow)row).Instruction.Operation;
                string desc = GetEnumMemberDescription(op);
                if (desc != null)
                {
                    txtStatus.Text = string.Format("{0} - {1}",
                        op.ToString().ToUpperInvariant(), desc);
                }
            }

            Pointer address = viewModel.Rows[viewportBeginIndex + i].Location;
            ByteProperties b = document.Disassembler.Image[address];
            if (b == null) // TBD: we should also do something for an unanalyzed byte.
                return;

            // this.ActiveSegment = address.Segment;
        }

        private string GetEnumMemberDescription<T>(T value)
        {
            var type = typeof(T);
            var memInfo = type.GetMember(value.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                var attributes = memInfo[0].GetCustomAttributes(
                    typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    return ((DescriptionAttribute)attributes[0]).Description;
            }
            return null;
        }

        // Keeps track of the segment selected. If this value is different
        // from what is displayed in the UI, then either the UI must be
        // updated or an ActiveSegmentChanged event must be raised.
        // private ushort activeSegment;

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
                throw new NotImplementedException();
#if false
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
#endif
            }
        }

        private void cbProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProcedures.SelectedIndex < 0)
                return;

            ProcedureItem item = (ProcedureItem)cbProcedures.SelectedItem;

            // Display only the rows that belong to this procedure.
            DisplayViewport(item.BeginRowIndex, item.EndRowIndex);

            // Navigate to the entry point of the procedure. Note that
            // this may not be the first instruction in the procedure's
            // range, though it usually is.
            Navigate(item.Procedure.EntryPoint, false, true);
        }

        private void btnViewScope_DropDownOpening(object sender, EventArgs e)
        {
            mnuScopeExecutable.Checked = (scope == ListingScope.Executable);
            mnuScopeSegment.Checked = (scope == ListingScope.Segment);
            mnuScopeProcedure.Checked = (scope == ListingScope.Procedure);
        }

        private void mnuScopeProcedure_Click(object sender, EventArgs e)
        {
            this.ListingScope = ListingScope.Procedure;
        }

        private void mnuScopeSegment_Click(object sender, EventArgs e)
        {
            this.ListingScope = ListingScope.Segment;
        }

        private void mnuScopeExecutable_Click(object sender, EventArgs e)
        {
            this.ListingScope = ListingScope.Executable;
        }
    }

    static class ListViewItemExtensions
    {
        public static void ScrollToTop(this ListViewItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.ListView == null)
                throw new InvalidOperationException("The ListViewItem is not part of a ListView.");

            item.ListView.TopItem = item;
        }

#if false
        public static void Activate(this ListViewItem item)
        {   
            //item.ListView.Focus();
            //if (bringToTop)
            //    lvListing.TopItem = item;
            item.ListView.TopItem = item;
            //else
            //    item.EnsureVisible();
            item.Focused = true;
            item.Selected = true;
        }
#endif
    }

#if false
    [Flags]
    enum ListViewItemActivationOptions
    {
        None = 0,
        Default = Select | Focus | EnsureVisible ,

        Select = 1,
        Focus = 2,
        FocusOwner = 4,

        EnsureVisible = 8,
        ScrollToTop = 16,
    }
#endif
}

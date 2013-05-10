using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using X86Codec;
using Util.Forms;
using Util;

namespace DosDebugger
{
    public partial class ListingWindow : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public ListingWindow()
        {
            InitializeComponent();
            this.monoFont = new Font(FontFamily.GenericMonospace, mnuListing.Font.Size);
            this.linkHoverFont = new Font(lvListing.Font, FontStyle.Underline);
            // TODO: dispose monoFont when no longer used
        }

        private Font monoFont;
        private Font linkHoverFont;

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
            dataGridView1.DataSource = null;

            lvListing.SetWindowTheme("explorer");
            lvListing.VirtualListSize = 0;
            viewModel = null;
            if (document == null)
                return;

            // Listen to navigation events.
            document.Navigator.LocationChanged += navigator_LocationChanged;

            // Create the view model.
            viewModel = new ListingViewModel(document.Image);
            //dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Font = new System.Drawing.Font(FontFamily.GenericMonospace, this.Font.Size);
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = viewModel.Rows;
            dataGridView1.Refresh();

            // Fill the procedure window.
            cbProcedures.Items.Clear();
            cbProcedures.Items.AddRange(viewModel.ProcedureItems.ToArray());

            // Fill the segment window.
            cbSegments.Items.Clear();
            cbSegments.Items.AddRange(viewModel.SegmentItems.ToArray());

            // Display the listing rows.
            //scope = ListingScope.Procedure;
            scope = ListingScope.Executable; // should rename to Image
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
                Segment s = document.Image.FindSegment(seg);
                int k1 = viewModel.FindRowIndex(s.StartAddress);
                int k2 = viewModel.FindRowIndex(s.EndAddress);
                DisplayViewport(k1, k2);
                return;
            }

            if (scope == ListingScope.Procedure)
            {
                Pointer address = viewModel.Rows[activeRowIndex].Location;
                Procedure proc = document.Image[address].Procedure;
                if (proc == null)
                {
                    // TBD: if the current row is not part of a procedure,
                    // what should be display?
                    DisplayViewport(0, viewModel.Rows.Count);
                }
                else
                {
                    int k1 = viewModel.FindRowIndex(proc.StartAddress);
                    int k2 = viewModel.FindRowIndex(proc.EndAddress);
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
            //System.Diagnostics.Debug.WriteLine("RetrieveVirtualItem");
        }

        /// <summary>
        /// Updates the location of the navigation object to the selected
        /// item.
        /// </summary>
        private void UpdateLocation()
        {
            if (document != null)
            {
                //document.Navigator.SetLocation(
            }
        }

        private void contextMenuListing_Opening(object sender, CancelEventArgs e)
        {
            if (lvListing.SelectedIndices.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            int index = lvListing.SelectedIndices[0];

            Pointer location = viewModel.Rows[viewportBeginIndex + index].Location;
            if (location == Pointer.Invalid)
                return;

            var image = document.Image;

            // Fill xrefs to this location.
            int n = 0;
            foreach (XRef xref in image.CrossReferences.GetReferencesTo(location.LinearAddress))
            {
                mnuListing.Items.Add(CreateXRefMenuItem(xref.Source));
                n++;
            }
            mnuListingIncomingXRefs.Text = ReplaceFirstWord(
                mnuListingIncomingXRefs.Text,
                n == 0 ? "No" : n.ToString());

            // Fill xrefs from this location.
            int i = mnuListing.Items.IndexOf(mnuListingOutgoingXRefs);
            n = 0;
            foreach (XRef xref in image.CrossReferences.GetReferencesFrom(location.LinearAddress))
            {
                mnuListing.Items.Insert(++i, CreateXRefMenuItem(xref.Target));
                n++;
            }
            mnuListingOutgoingXRefs.Text = ReplaceFirstWord(
                mnuListingOutgoingXRefs.Text,
                n == 0 ? "No" : n.ToString());
        }

        private ToolStripMenuItem CreateXRefMenuItem(Pointer location)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            if (location == Pointer.Invalid)
            {
                item.Text = string.Format("{0}  (Dynamic)", location);
                item.Enabled = false;
            }
            else
            {
                Instruction instruction = viewModel.Image.DecodeInstruction(location);
                item.Text = string.Format("{0}  {1}", location, instruction);
                item.Enabled = true;
            }
            item.Font = monoFont;
            item.Tag = location;
            item.Click += mnuListingGoToXRefItem_Click;
            return item;
        }

        private static string ReplaceFirstWord(string s, string newWord)
        {
            int k = s.IndexOf(' ');
            return newWord + s.Substring(k);
        }

        private void contextMenuListing_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            mnuListing.Items.ClearAndDispose(
                mnuListing.Items.IndexOf(mnuListingOutgoingXRefs) + 1,
                mnuListing.Items.IndexOf(mnuListingIncomingXRefs) - 1);
            mnuListing.Items.ClearAndDispose(
                mnuListing.Items.IndexOf(mnuListingIncomingXRefs) + 1,
                mnuListing.Items.Count);
        }

        private void mnuListingGoToXRefItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Pointer source = (Pointer)item.Tag;
            Navigate(source);
#if false
            if (NavigationRequested != null)
            {
                NavigationRequestedEventArgs args =
                    new NavigationRequestedEventArgs(source);
                NavigationRequested(this, args);
            }
#endif
        }

        private void navigator_LocationChanged(object sender, LocationChangedEventArgs<Pointer> e)
        {
            if (e.Source == this)
                return;

            Navigate(e.NewLocation);
        }

        // TODO: what should we do if the navigation target is out of the current sub?

        /// <summary>
        /// Navigates to the given address. This function does not raise the
        /// LocationChanged event.
        /// </summary>
        /// <param name="target">The address to navigate to.</param>
        /// <returns></returns>
        public void Navigate(Pointer target)
        {
            // Navigate(target, true, true);
            if (viewModel.Rows.Count == 0)
                return;

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

            //if (scrollToTop)
            //    item.ScrollToTop();
            //else
                item.EnsureVisible();

            //if (setFocus)
                item.ListView.Focus();

            // Notify the navigation controller.
            //document.Navigator.SetLocation(target, this);
            // TBD: navigation doesn't work in listing window yet.
        }

#if false
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

            // Notify the navigation controller.
            document.Navigator.SetLocation(target, this);
            // TBD: navigation doesn't work in listing window yet.

            return true;
        }
#endif

        //public event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

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
                string desc = op.GetDescription();
                if (desc != null)
                {
                    txtStatus.Text = string.Format("{0} - {1}",
                        op.ToString().ToUpperInvariant(), desc);
                }
            }

            Pointer address = row.Location;
            ByteProperties b = document.Image[address];
            if (b == null) // TBD: we should also do something for an unanalyzed byte.
                return;

            // Update the current location.
            document.Navigator.SetLocation(row.Location, this, LocationChangeType.Minor);

            // this.ActiveSegment = address.Segment;
        }

        // Keeps track of the segment selected. If this value is different
        // from what is displayed in the UI, then either the UI must be
        // updated or an ActiveSegmentChanged event must be raised.
        // private ushort activeSegment;

#if false
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
#endif

        private void cbProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbProcedures.SelectedIndex < 0)
                return;

#if false
            ProcedureItem item = (ProcedureItem)cbProcedures.SelectedItem;

            // Display only the rows that belong to this procedure.
            DisplayViewport(item.BeginRowIndex, item.EndRowIndex);

            // Navigate to the entry point of the procedure. Note that
            // this may not be the first instruction in the procedure's
            // range, though it usually is.
            Navigate(item.Procedure.EntryPoint, false, true);
#endif
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

        private void lvListing_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void lvListing_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //e.DrawBackground();
            //e.Graphics.DrawString(e.Item.Text, e.Item.Font, Brushes.Black, e.Bounds);
            //e.Graphics.DrawString("AAAA", e.Item.Font, Brushes.Black, e.Bounds);
            
            //e.DrawDefault = true;
            //e.DrawFocusRectangle();
            //e.DrawBackground();
        }

        private void lvListing_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            //e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);

            // Use default drawing routine if this cell doesn't contain html.
            if (!e.SubItem.Text.Contains("<"))
            {
                //e.DrawBackground();
                //e.DrawFocusRectangle(e.SubItem.Bounds);
                e.DrawDefault = true;
                return;
            }

#if false
            Point pt=e.Bounds.Location;
            pt =lvListing.PointToScreen(pt);
            pt=this.PointToClient(pt);
            linkLabel1.Location = pt;
            linkLabel1.Size = e.Bounds.Size;
            return;
#endif

            //e.DrawFocusRectangle(e.Bounds);
            //e.DrawBackground();
            if (e.Item.Selected)
            {
                //e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                //e.Graphics.DrawRectangle(Pens.Gray, e.Bounds);
            }
            Rectangle rect = e.Item.GetSubItemBounds(e.ColumnIndex);
            HtmlRenderer renderer = new HtmlRenderer(
                e.SubItem.Text, e.SubItem.Font, linkHoverFont);
            renderer.Measure(e.Graphics, rect);
            renderer.Draw(e.Graphics, lvListing.PointToClient(Cursor.Position));
        }

        private void lvListing_MouseMove(object sender, MouseEventArgs e)
        {
            var ht = lvListing.HitTest(e.Location);
            if (ht.Item != lastItem)
            {
                if (lastItem != null && lastItem.Index < lvListing.Items.Count)
                    lvListing.RedrawItems(lastItem.Index, lastItem.Index, true);
                if (ht.Item != null)
                    lvListing.RedrawItems(ht.Item.Index, ht.Item.Index, true);
                lastItem = ht.Item;
            }

            // Update cursor shape.
            if (ht.Item == null || ht.SubItem == null)
            {
                lvListing.Cursor = Cursors.Default;
            }
            else
            {
                HtmlRenderer renderer = new HtmlRenderer(
                    ht.SubItem.Text, ht.SubItem.Font, linkHoverFont);
                using (Graphics g = lvListing.CreateGraphics())
                {
                    renderer.Measure(g, ht.SubItem.Bounds); // TBD: if ColumnIndex == 0...
                    if (renderer.HitTest(e.Location))
                    {
                        lvListing.Cursor = Cursors.Hand;
                        return;
                    }
                }
                lvListing.Cursor = Cursors.Default;
            }

#if false
            // Display a tool-tip for an instruction.
            if (ht.SubItem != null && ht.SubItem.Text.Contains(" _"))
            {
                using (Graphics g = lvListing.CreateGraphics())
                {
                    TextWithInfo[] texts = MeasureSubItemTexts(g, ht.SubItem);
                    //System.Diagnostics.Debug.WriteLine("Checking...");
                    if (texts.Length > 0 && texts[0].Bounds.Contains(e.Location))
                    {
                        if (opcodeToolTip == null)
                            opcodeToolTip = new ToolTip();

                        Rectangle r = ht.SubItem.Bounds;
                        Point ptToolTip = new Point(r.Left, r.Bottom);
                        opcodeToolTip.Show(texts[0].Text, lvListing, ptToolTip, 1000);
                        //ht.SubItem.
                    }
                }
            }
#endif
        }

        ToolTip opcodeToolTip;
        ListViewItem lastItem;
    }
}

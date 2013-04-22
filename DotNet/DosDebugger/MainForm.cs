using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using X86Codec;
using Util.Forms;

namespace DosDebugger
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        MZFile mzFile;
        UInt16 baseSegment = 0x2920;

        Disassembler.Disassembler16 dasm;
        ListingViewModel listingView;

        private void MainForm_Load(object sender, EventArgs e)
        {
            //lvListing.SetWindowTheme("explorer");
            cbBookmarks.SelectedIndex = 1;
            string fileName = @"E:\Dev\Projects\DosDebugger\Reference\Q.EXE";
            DoLoadFile(fileName);
        }

        private void DoLoadFile(string fileName)
        {
            mzFile = new MZFile(fileName);
            mzFile.Relocate(baseSegment);
            dasm = new Disassembler.Disassembler16(mzFile.Image, mzFile.BaseAddress);
            lvErrors.Items.Clear();
            lvListing.Items.Clear();
            lvProcedures.Items.Clear();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            //TestDecode(mzFile.Image, mzFile.EntryPoint, mzFile.BaseAddress);
            //TestDecode(mzFile.Image, new FarPointer16(baseSegment, 0x17fc), mzFile.BaseAddress);
        }

#if false
        private void TestDecode(
            byte[] image,
            Pointer startAddress, 
            Pointer baseAddress)
        {
            DecoderContext options = new DecoderContext();
            options.AddressSize = CpuSize.Use16Bit;
            options.OperandSize = CpuSize.Use16Bit;

            X86Codec.Decoder decoder = new X86Codec.Decoder();

            Pointer ip = startAddress;
            for (int index = startAddress - baseAddress; index < image.Length; )
            {
                Instruction instruction = null;
                try
                {
                    instruction = decoder.Decode(image, index, ip, options);
                }
                catch (InvalidInstructionException ex)
                {
                    if (MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OKCancel)
                        == DialogResult.Cancel)
                    {
                        throw;
                    }
                    break;
                }
#if false
                // Output address.
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("0000:{0:X4}  ", index - startAddress);

                // Output binary code. */
                for (int i = 0; i < 8; i++)
                {
                    if (i < instruction.EncodedLength)
                        sb.AppendFormat("{0:x2} ", image[index + i]);
                    else
                        sb.Append("   ");
                }

                // Output the instruction.
                string s = instruction.ToString();
                if (s.StartsWith("*"))
                    throw new InvalidOperationException("Cannot format instruction.");
                sb.Append(s);

                System.Diagnostics.Debug.WriteLine(sb.ToString());
#else
                DisplayInstruction(instruction);
#endif
                index += instruction.EncodedLength;
                ip += instruction.EncodedLength;
            }
        }
#endif

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            dasm.Analyze(mzFile.EntryPoint, true);
            X86Codec.Decoder decoder = new X86Codec.Decoder();

            // Display analysis errors.
            Error[] errors = dasm.Errors;
            Array.Sort(errors, new ErrorLocationComparer());
            foreach (Error error in errors)
            {
                ListViewItem item = new ListViewItem();
                item.Text = error.Location.ToString();
                item.SubItems.Add(error.Message);
                lvErrors.Items.Add(item);
            }

            // Display analyzed code and data.
            listingView = new ListingViewModel(dasm);
            lvListing.VirtualListSize = listingView.Rows.Count;

            // Display subroutines.
            Dictionary<UInt16, int> segStat = new Dictionary<UInt16, int>();
            Pointer[] procEntries = dasm.Procedures;
            foreach (Pointer ptr in procEntries)
            {
                lvProcedures.Items.Add(ptr.ToString());
                segStat[ptr.Segment] = 1;
            }

            // Display status.
            txtStatus.Text = string.Format(
                "{3} segments, {0} procedures, {1} instructions, {2} errors",
                lvProcedures.Items.Count,
                lvListing.Items.Count,
                lvErrors.Items.Count,
                segStat.Count);
        }

        private class ErrorLocationComparer : IComparer<Error>
        {
            public int Compare(Error x, Error y)
            {
                return x.Location.ToString().CompareTo(y.Location.ToString());
            }
        }

        private void btnGoTo_Click(object sender, EventArgs e)
        {
            // Find the address.
            Pointer target;
            string addr = cbBookmarks.Text;
            if (addr.Length < 9 || !Pointer.TryParse(addr.Substring(0, 9), out target))
            {
                MessageBox.Show(this, "The address '" + addr + "' is invalid.");
                return;
            }

            // Go to that location.
            if (!GoToLocation(target))
            {
                MessageBox.Show(this, "Cannot find that address.");
            }
        }

        private bool GoToLocation(Pointer target)
        {
            // Find the first entry that is greater than or equal to target.
            for (int i = 0; i < listingView.Rows.Count; i++)
            {
                Pointer current = listingView.Rows[i].Location;
                if (current != Pointer.Invalid &&
                    current.EffectiveAddress >= target.EffectiveAddress)
                {
                    ListViewItem item = lvListing.Items[i];
                    lvListing.Focus();
                    lvListing.TopItem = item;
                    item.Selected = true;
                    return true;
                }
            }
            return false;
        }

        private void lvProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lvProcedures_DoubleClick(object sender, EventArgs e)
        {
            if (lvProcedures.SelectedIndices.Count == 1)
            {
                GoToLocation(Pointer.Parse(lvProcedures.SelectedItems[0].Text));
            }
        }

        private void lvErrors_DoubleClick(object sender, EventArgs e)
        {
            if (lvErrors.SelectedIndices.Count == 1)
            {
                GoToLocation(Pointer.Parse(lvErrors.SelectedItems[0].Text));
            }
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            string fileName = openFileDialog1.FileName;
            DoLoadFile(fileName);
        }

        private void lvListing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvListing.SelectedIndices.Count == 1)
            {
                int row = lvListing.SelectedIndices[0];
                if (navCurrent == null)
                {
                    navCurrent = navHistory.AddFirst(row);
                    return;
                }

                if (row == navCurrent.Value)
                    return;
                if (navCurrent.Next != null && row == navCurrent.Next.Value)
                {
                    navCurrent = navCurrent.Next;
                    return;
                }

                while (navCurrent.Next != null)
                {
                    navHistory.RemoveLast();
                }
                navCurrent = navHistory.AddAfter(navCurrent, row);
            }
        }

        private LinkedList<int> navHistory = new LinkedList<int>();
        private LinkedListNode<int> navCurrent = null;

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (navCurrent != null && navCurrent.Previous != null)
            {
                navCurrent = navCurrent.Previous;
                lvListing.Items[navCurrent.Value].Selected = true;
                lvListing.Items[navCurrent.Value].EnsureVisible();
                lvListing.Focus();
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (navCurrent != null && navCurrent.Next != null)
            {
                navCurrent = navCurrent.Next;
                lvListing.Items[navCurrent.Value].Selected = true;
                lvListing.Items[navCurrent.Value].EnsureVisible();
                lvListing.Focus();
            }
        }

        private int CurrentListingIndex
        {
            get
            {
                if (lvListing.SelectedIndices.Count > 0)
                    return lvListing.SelectedIndices[0];
                else
                    return -1;
            }
        }

        private void contextMenuListing_Opening(object sender, CancelEventArgs e)
        {
            mnuListingGoToXRef.DropDownItems.Clear();
            mnuListingGoToXRef.Enabled = false;

            int index = CurrentListingIndex;
            if (index == -1)
                return;

            Pointer location = listingView.Rows[index].Location;
            if (location == Pointer.Invalid)
                return;

            foreach (XRef xref in dasm.GetReferencesTo(location))
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
            GoToLocation(source);
        }

        private void mnuFileInfo_Click(object sender, EventArgs e)
        {
            ExecutableInfoForm f = new ExecutableInfoForm();
            f.MzFile = mzFile;
            f.Show(this);
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            string s = txtFind.Text.ToUpperInvariant();
            if (s.Length == 0)
                return;

            int selection = CurrentListingIndex;

            int n = lvListing.Items.Count;
            for (int i = 0; i < n; i++)
            {
                ListViewItem item = lvListing.Items[(selection + 1 + i) % n];
                for (int j = 0; j < item.SubItems.Count; j++)
                {
                    if (item.SubItems[j].Text.ToUpperInvariant().Contains(s))
                    {
                        item.Selected = true;
                        lvListing.TopItem = item;
                        lvListing.Focus();
                        return;
                    }
                }
            }
            MessageBox.Show(this, "Cannot find " + txtFind.Text);
        }

        private void lvListing_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = listingView.CreateViewItem(e.ItemIndex);
        }
    }
}

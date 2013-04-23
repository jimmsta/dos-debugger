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
        UInt16 baseSegment = 0; // 0x2920;

        Disassembler.Disassembler16 dasm;
        ListingViewModel listingView;

        private void MainForm_Load(object sender, EventArgs e)
        {
            //lvListing.SetWindowTheme("explorer");
            cbBookmarks.SelectedIndex = 1;
            cbFind.SelectedIndex = 0;
            string fileName = @"E:\Dev\Projects\DosDebugger\Reference\H.EXE";
            DoLoadFile(fileName);
        }

        private void DoLoadFile(string fileName)
        {
            mzFile = new MZFile(fileName);
            mzFile.Relocate(baseSegment);
            dasm = new Disassembler.Disassembler16(mzFile.Image, mzFile.BaseAddress);

            lvErrors.Items.Clear();
            lvProcedures.Items.Clear();
            lvSegments.Items.Clear();

            listingView = null;
            lvListing.VirtualListSize = 0;

            this.Text = "DOS Disassembler - " + System.IO.Path.GetFileName(fileName);

            DoAnalyze();
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

        private void DoAnalyze()
        {
            dasm.Analyze(mzFile.EntryPoint);
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
            Procedure[] procs = dasm.Procedures;
            foreach (Procedure proc in procs)
            {
                ListViewItem item = new ListViewItem();
                item.Text = proc.EntryPoint.ToString();
                item.SubItems.Add(proc.ByteRange.IntervalCount.ToString());
                item.SubItems.Add(proc.ByteRange.Length.ToString());
                item.Tag = proc;
                lvProcedures.Items.Add(item);
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

        private void btnGoToBookmark_Click(object sender, EventArgs e)
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
        }

        private void lvProcedures_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lvProcedures_DoubleClick(object sender, EventArgs e)
        {
            if (lvProcedures.SelectedIndices.Count == 1)
            {
                Procedure proc = (Procedure)lvProcedures.SelectedItems[0].Tag;
                GoToLocation(proc.EntryPoint);
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

        private void btnNavigateBackward_Click(object sender, EventArgs e)
        {
            if (navCurrent != null && navCurrent.Previous != null)
            {
                navCurrent = navCurrent.Previous;
                GoToRow(navCurrent.Value);
            }
        }

        private void btnNavigateForward_Click(object sender, EventArgs e)
        {
            if (navCurrent != null && navCurrent.Next != null)
            {
                navCurrent = navCurrent.Next;
                GoToRow(navCurrent.Value);
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
            string s = cbFind.Text.ToUpperInvariant();
            if (s.Length == 0)
                return;

            int selection = CurrentListingIndex;

            int n = lvListing.Items.Count;
            for (int i = 0; i < n; i++)
            {
                int k = (selection + 1 + i) % n;
                ListViewItem item = lvListing.Items[k];
                for (int j = 0; j < item.SubItems.Count; j++)
                {
                    if (item.SubItems[j].Text.ToUpperInvariant().Contains(s))
                    {
                        GoToRow(k, true);
                        return;
                    }
                }
            }
            MessageBox.Show(this, "Cannot find " + cbFind.Text);
        }

        private void lvListing_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = listingView.CreateViewItem(e.ItemIndex);
        }

        private void mnuAnalyzeExecutable_Click(object sender, EventArgs e)
        {
            DoAnalyze();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "DOS Disassembler\r\nCopyright fanci 2012-2013\r\n",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            // Find all instructions that change segment registers.
            ByteAttribute[] attr = dasm.ByteAttributes;
            for (int i = 0; i < attr.Length; )
            {
                if (attr[i].IsBoundary && attr[i].Type == ByteType.Code)
                {
                    Pointer location = dasm.OffsetToPointer(i);

                    Instruction insn = X86Codec.Decoder.Decode(dasm.Image,
                        i, location, CpuMode.RealAddressMode);

                    if (insn.Operands.Length >= 1 && insn.Operands[0] is RegisterOperand)
                    {
                        RegisterOperand opr = (RegisterOperand)insn.Operands[0];
                        if (opr.Type == RegisterType.Segment &&
                            opr.Register != Register.ES &&
                            insn.Operation != Operation.PUSH)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format(
                                "{0} {1}", location, insn));
                        }
                    }
                    i += insn.EncodedLength;
                }
                else
                {
                    i++;
                }
            }
        }
    }
}

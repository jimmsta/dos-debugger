using Disassembler;
using System;
using System.Windows.Forms;
using X86Codec;
using WeifenLuo.WinFormsUI.Docking;

namespace DosDebugger
{
    public partial class MainForm : Form
    {
        ProcedureWindow procWindow;
        ErrorWindow errorWindow;
        SegmentWindow segmentWindow;
        ListingWindow listingWindow;

        public MainForm()
        {
            InitializeComponent();

            procWindow = new ProcedureWindow();
            procWindow.NavigationRequested += OnNavigationRequested;

            errorWindow = new ErrorWindow();
            errorWindow.NavigationRequested += OnNavigationRequested;

            this.segmentWindow = new SegmentWindow();

            this.listingWindow = new ListingWindow();
            listingWindow.NavigationRequested += OnNavigationRequested;

            this.navHistory.Changed += navHistory_Changed;
        }

        Document document;
        MZFile mzFile;
        UInt16 baseSegment = 0; // 0x2920;

        Disassembler.Disassembler16 dasm;
        NavigationHistory<Pointer> navHistory = new NavigationHistory<Pointer>();

        private void MainForm_Load(object sender, EventArgs e)
        {
            //lvListing.SetWindowTheme("explorer");
            cbBookmarks.SelectedIndex = 1;
            cbFind.SelectedIndex = 0;
            string fileName = @"E:\Dev\Projects\DosDebugger\Reference\H.EXE";
            DoLoadFile(fileName);

            procWindow.Show(dockPanel);
            procWindow.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
            errorWindow.Show(dockPanel);
            errorWindow.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
            segmentWindow.Show(dockPanel);
            segmentWindow.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
            listingWindow.Show(dockPanel);
        }

        private void navHistory_Changed(object sender, EventArgs e)
        {
            btnNavigateBackward.Enabled = navHistory.CanGoBackward;
            mnuViewNavigateBackward.Enabled = navHistory.CanGoBackward;

            btnNavigateForward.Enabled = navHistory.CanGoForward;
            mnuViewNavigateForward.Enabled = navHistory.CanGoForward;
        }

        private void DoLoadFile(string fileName)
        {
            mzFile = new MZFile(fileName);
            mzFile.Relocate(baseSegment);
            dasm = new Disassembler.Disassembler16(mzFile.Image, mzFile.BaseAddress);

            document = new Document();
            document.Disassembler = dasm;
            navHistory.Clear();

            DoAnalyze();

            procWindow.Document = document;
            errorWindow.Document = document;
            segmentWindow.Document = document;
            listingWindow.Document = document;

            this.Text = "DOS Disassembler - " + System.IO.Path.GetFileName(fileName);

            GoToLocation(new Pointer(0, 0));
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

            // Display status.
            txtStatus.Text = string.Format(
                "{3} segments, {0} procedures, {1} instructions, {2} errors",
                dasm.Procedures.Length,
                "?", // lvListing.Items.Count,
                dasm.Errors.Length,
                0 /* segStat.Count */);
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
            navHistory.GoTo(target);
            return listingWindow.Navigate(target);
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

        private void btnNavigateBackward_Click(object sender, EventArgs e)
        {
            if (navHistory.CanGoBackward)
            {
                listingWindow.Navigate(navHistory.GoBackward());
            }
        }

        private void btnNavigateForward_Click(object sender, EventArgs e)
        {
            if (navHistory.CanGoForward)
            {
                listingWindow.Navigate(navHistory.GoForward());
            }
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

#if false
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
#endif
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

        public void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
        {
            GoToLocation(e.Location);
        }

        private void mnuView_DropDownOpening(object sender, EventArgs e)
        {
            mnuViewSegments.Checked = segmentWindow.Visible;
            mnuViewProcedures.Checked = procWindow.Visible;
            mnuViewErrors.Checked = errorWindow.Visible;
        }

        private void mnuViewDisassembly_Click(object sender, EventArgs e)
        {

        }

        private void mnuViewSegments_Click(object sender, EventArgs e)
        {
            //DockState ds = segmentWindow.DockState;
#if false
            segmentWindow.Activate();
            if (!segmentWindow.Visible)
            {
                if (segmentWindow.DockState == WeifenLuo.WinFormsUI.Docking.DockState.Hidden)
                {
                    MessageBox.Show("Hidden");
                }
            }
            segmentWindow.Focus();
#endif
            segmentWindow.Activate();
        }

        private void mnuViewProcedures_Click(object sender, EventArgs e)
        {
            procWindow.Activate();
        }

        private void mnuViewErrors_Click(object sender, EventArgs e)
        {
            errorWindow.Activate();
        }
    }
}

using Disassembler;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using X86Codec;

namespace DosDebugger
{
    public partial class MainForm : Form
    {
        ProcedureWindow procWindow;
        ErrorWindow errorWindow;
        SegmentWindow segmentWindow;
        ListingWindow listingWindow;
        PropertiesWindow propertiesWindow;
        HexWindow hexWindow;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolWindows();
            InitializeDockPanel();

            this.navHistory.Changed += navHistory_Changed;
        }

        private void InitializeToolWindows()
        {
            procWindow = new ProcedureWindow();
            procWindow.NavigationRequested += OnNavigationRequested;

            errorWindow = new ErrorWindow();
            errorWindow.NavigationRequested += OnNavigationRequested;

            segmentWindow = new SegmentWindow();

            listingWindow = new ListingWindow();
            listingWindow.NavigationRequested += OnNavigationRequested;

            propertiesWindow = new PropertiesWindow();

            hexWindow = new HexWindow();
        }

        private void InitializeDockPanel()
        {
            try
            {
                LoadDockPanelLayout();
            }
            catch (Exception)
            {
                DetachToolWindowsFromDockPanel();

                // Create dock panel with default layout.
                procWindow.Show(dockPanel);
                segmentWindow.Show(dockPanel);
                errorWindow.Show(dockPanel);
                listingWindow.Show(dockPanel);
                propertiesWindow.Show(dockPanel);
                hexWindow.Show(dockPanel);
            }
            
            // ActivateDockWindow(listingWindow);
        }

        private void SaveDockPanelLayout()
        {
            string fileName = "WorkspaceLayout.xml";
            using (Stream stream = new FileStream(fileName,
                FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                dockPanel.SaveAsXml(stream, Encoding.UTF8);
            }
        }

        private void LoadDockPanelLayout()
        {
            DeserializeDockContent context =
                new DeserializeDockContent(GetContentFromPersistString);

            string fileName = "WorkspaceLayout.xml";
            using (Stream stream = File.OpenRead(fileName))
            {
                DetachToolWindowsFromDockPanel();
                dockPanel.LoadFromXml(stream, context);
            }
        }

        /// <summary>
        /// Detaches tool windows from the dock panel. This is needed if
        /// we want to reconstruct the dock panel's layout after the
        /// tool windows have been added to the dock panel.
        /// </summary>
        private void DetachToolWindowsFromDockPanel()
        {
            this.segmentWindow.DockPanel = null;
            this.listingWindow.DockPanel = null;
            this.errorWindow.DockPanel = null;
            this.procWindow.DockPanel = null;
            this.propertiesWindow.DockPanel = null;
            this.hexWindow.DockPanel = null;
        }

        Document document;
        MZFile mzFile;
        UInt16 baseSegment = 0; // 0x2920;

        Disassembler.Disassembler16 dasm;

        // TODO: when we close the disassembly window, what do we do with
        // the navigation history?
        NavigationHistory<Pointer> navHistory = new NavigationHistory<Pointer>();

        private void MainForm_Load(object sender, EventArgs e)
        {
            //lvListing.SetWindowTheme("explorer");
            cbBookmarks.SelectedIndex = 1;
            cbFind.SelectedIndex = 0;
            string fileName = @"E:\Dev\Projects\DosDebugger\Reference\H.EXE";
            DoLoadFile(fileName);
            this.WindowState = FormWindowState.Maximized;
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
            hexWindow.Document = document;
            propertiesWindow.SelectedObject = mzFile;

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
            // SaveDockPanelLayout();
        }

        /// <summary>
        /// Gets the tool window object instance corresponding to each tool
        /// window type. This is necessary so that we don't need to create
        /// the instances repeatedly.
        /// </summary>
        /// <param name="persistString"></param>
        /// <returns>The tool window instance, or null if not found.</returns>
        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(SegmentWindow).ToString())
                return segmentWindow;
            else if (persistString == typeof(ProcedureWindow).ToString())
                return procWindow;
            else if (persistString == typeof(ErrorWindow).ToString())
                return errorWindow;
            else if (persistString == typeof(ListingWindow).ToString())
                return listingWindow;
            else if (persistString == typeof(PropertiesWindow).ToString())
                return propertiesWindow;
            else if (persistString == typeof(HexWindow).ToString())
                return hexWindow;
            else
                return null;

#if false
            else
            {
                // DummyDoc overrides GetPersistString to add extra information into persistString.
                // Any DockContent may override this value to add any needed information for deserialization.

                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length != 3)
                    return null;

                if (parsedStrings[0] != typeof(DummyDoc).ToString())
                    return null;

                DummyDoc dummyDoc = new DummyDoc();
                if (parsedStrings[1] != string.Empty)
                    dummyDoc.FileName = parsedStrings[1];
                if (parsedStrings[2] != string.Empty)
                    dummyDoc.Text = parsedStrings[2];

                return dummyDoc;
            }
#endif
        }

        private void ListInstructionsThatChangeSegmentRegisters()
        {
            // Find all instructions that change segment registers.
            ByteProperties[] attr = dasm.ByteProperties;
            for (int i = 0; i < attr.Length; )
            {
                if (attr[i] != null && attr[i].IsLeadByte && attr[i].Type == ByteType.Code)
                {
                    Pointer location = attr[i].Address;

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
            ActivateDockWindow(segmentWindow);
        }

        private void mnuViewProcedures_Click(object sender, EventArgs e)
        {
            ActivateDockWindow(procWindow);
        }

        private void mnuViewErrors_Click(object sender, EventArgs e)
        {
            ActivateDockWindow(errorWindow);
        }

        private void mnuViewProperties_Click(object sender, EventArgs e)
        {
            ActivateDockWindow(propertiesWindow);
        }

        private void mnuViewHex_Click(object sender, EventArgs e)
        {
            ActivateDockWindow(hexWindow);
        }

        private void ActivateDockWindow(DockContent window)
        {
            if (window.DockPanel == null)
                window.Show(dockPanel);
            window.Activate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                SaveDockPanelLayout();
            }
            catch (Exception)
            {
            }
        }
    }
}

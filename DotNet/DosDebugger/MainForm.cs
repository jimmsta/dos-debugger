using Disassembler;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using X86Codec;
using Disassembler.Omf;

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
        LibraryBrowserWindow libraryWindow;

        public MainForm()
        {
            InitializeComponent();
            InitializeToolWindows();
            InitializeDockPanel();
        }

        private void InitializeToolWindows()
        {
            procWindow = new ProcedureWindow();
            errorWindow = new ErrorWindow();
            segmentWindow = new SegmentWindow();
            listingWindow = new ListingWindow();
            propertiesWindow = new PropertiesWindow();
            hexWindow = new HexWindow();
            libraryWindow = new LibraryBrowserWindow();
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
                libraryWindow.Show(dockPanel);
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
            this.libraryWindow.DockPanel = null;
        }

        Document document;
        MZFile mzFile;
        UInt16 baseSegment = 0; // 0xAAAA; // 0x2920;

        Disassembler.Disassembler16 dasm;

        // TODO: when we close the disassembly window, what do we do with
        // the navigation history?
        NavigationHistory<Pointer> navHistory = new NavigationHistory<Pointer>();

        private void MainForm_Load(object sender, EventArgs e)
        {
            //lvListing.SetWindowTheme("explorer");
            cbBookmarks.SelectedIndex = 0;
            cbFind.SelectedIndex = 0;
            string fileName = @"E:\Dev\Projects\DosDebugger\Test\H.EXE";
            DoLoadFile(fileName);
            this.WindowState = FormWindowState.Maximized;

#if false
            CallGraphWindow f = new CallGraphWindow();
            LinearPointer procEntry = dasm.Image.BaseAddress.LinearAddress + 0x17FC;
            f.SourceProcedure = dasm.Image.Procedures.Find(procEntry);
            f.WindowState = FormWindowState.Maximized;
            f.ShowDialog(this);
            //Application.Exit();
#else
            btnTest_Click(null, null);
#endif
        }

        private void DoLoadFile(string fileName)
        {
            mzFile = new MZFile(fileName);
            mzFile.Relocate(baseSegment);
            dasm = new Disassembler.Disassembler16(mzFile.Image, mzFile.BaseAddress);

            document = new Document();
            document.Disassembler = dasm;
            document.Navigator.LocationChanged += navigator_LocationChanged;
            navHistory.Clear();

            DoAnalyze();

            procWindow.Document = document;
            errorWindow.Document = document;
            segmentWindow.Document = document;
            listingWindow.Document = document;
            hexWindow.Document = document;
            propertiesWindow.SelectedObject = mzFile;

            this.Text = "DOS Disassembler - " + System.IO.Path.GetFileName(fileName);

            GoToLocation(dasm.Image.BaseAddress);
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
                "{3} segments, {0} procedures, {4} xrefs, {1} instructions, {2} errors",
                dasm.Image.Procedures.Count,
                "?", // lvListing.Items.Count,
                dasm.Image.Errors.Count,
                0 /* segStat.Count */,
                dasm.Image.CrossReferences.Count);
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
            GoToLocation(target);
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

        #region Navigation Related Members

        private void GoToLocation(Pointer target)
        {
            navHistory.Add(target);
            document.Navigator.SetLocation(target, this);
        }

        private void navigator_LocationChanged(object sender, LocationChangedEventArgs<Pointer> e)
        {
            if (e.Source != this)
            {
                if (e.Type == LocationChangeType.Major || navHistory.IsEmpty)
                {
                    navHistory.Add(e.NewLocation);
                }
                else
                {
                    navHistory.Current = e.NewLocation;
                }
            }

            btnNavigateBackward.Enabled = navHistory.CanMove(-1);
            mnuViewNavigateBackward.Enabled = navHistory.CanMove(-1);

            btnNavigateForward.Enabled = navHistory.CanMove(1);
            mnuViewNavigateForward.Enabled = navHistory.CanMove(1);
        }

        private void NavigateThroughHistory(int offset)
        {
            if (navHistory.CanMove(offset))
            {
                navHistory.Move(offset);
                document.Navigator.SetLocation(navHistory.Current, this);
            }
        }

        private void btnNavigateBackward_Click(object sender, EventArgs e)
        {
            NavigateThroughHistory(-1);
        }

        private void btnNavigateForward_Click(object sender, EventArgs e)
        {
            NavigateThroughHistory(1);
        }

        private void btnNavigateBackward_DropDownOpening(object sender, EventArgs e)
        {
            for (int offset = -1; navHistory.CanMove(offset); offset--)
            {
                Pointer location = navHistory.Peek(offset);
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = location.ToString();
                item.Click += btnNavigateEntry_Click;
                item.Tag = offset;
                btnNavigateBackward.DropDownItems.Add(item);
            }
        }

        private void btnNavigateForward_DropDownOpening(object sender, EventArgs e)
        {
            for (int offset = 1; navHistory.CanMove(offset); offset++)
            {
                Pointer location = navHistory.Peek(offset);
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = location.ToString();
                item.Click += btnNavigateEntry_Click;
                item.Tag = offset;
                btnNavigateForward.DropDownItems.Add(item);
            }
        }

        private void btnNavigateBackward_DropDownClosed(object sender, EventArgs e)
        {
            btnNavigateBackward.DropDownItems.ClearAndDispose();
        }

        private void btnNavigateForward_DropDownClosed(object sender, EventArgs e)
        {
            btnNavigateForward.DropDownItems.ClearAndDispose();
        }

        private void btnNavigateEntry_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int offset = (int)item.Tag;
            NavigateThroughHistory(offset);
        }

        #endregion

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
#if false
            CallGraphWindow f = new CallGraphWindow();
            LinearPointer procEntry = dasm.Image.BaseAddress.LinearAddress + 0x14FE;
            f.SourceProcedure = dasm.Image.Procedures.Find(procEntry);
            f.WindowState = FormWindowState.Maximized;
            f.Show(this);
#else
            string fileName = @"..\..\..\..\Test\SLIBC7.LIB";
            ObjectLibrary library = OmfLoader.LoadLibrary(fileName);
            propertiesWindow.SelectedObject = library;

            library.ResolveAllSymbols();
            libraryWindow.Library = library;
            libraryWindow.PropertiesWindow = propertiesWindow;
            libraryWindow.ListingWindow = listingWindow;
#endif
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
            else if (persistString == typeof(LibraryBrowserWindow).ToString())
                return libraryWindow;
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
#if false
            // Find all instructions that change segment registers.
            var attr = dasm.Image;
            for (int i = 0; i < attr.Length; )
            {
                if (attr[i].IsLeadByte && attr[i].Type == ByteType.Code)
                {
                    Pointer location = attr[i].Address;

                    Instruction insn = dasm.Image.DecodeInstruction(location);

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
#endif
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

        private void mnuViewLibraryBrowser_Click(object sender, EventArgs e)
        {
            ActivateDockWindow(libraryWindow);
        }

        private void ActivateDockWindow(DockContent window)
        {
            if (window.DockPanel == null)
                window.Show(dockPanel);
            window.Activate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Save dock panel layout.
            try
            {
                SaveDockPanelLayout();
            }
            catch (Exception)
            {
            }

            // Dispose tool windows.
            using (procWindow) { }
            using (errorWindow) { }
            using (segmentWindow) { }
            using (listingWindow) { }
            using (propertiesWindow) { }
            using (hexWindow) { }
            using (libraryWindow) { }
        }
    }
}

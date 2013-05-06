using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using Util.Forms;
using Disassembler.Omf;

namespace DosDebugger
{
    public partial class LibraryBrowserWindow : ToolWindow
    {
        ObjectLibrary library;

        public LibraryBrowserWindow()
        {
            InitializeComponent();
        }

        public ObjectLibrary Library
        {
            get { return this.library; }
            set
            {
                this.library = value;
                UpdateUI();
            }
        }

        // TODO: we need a better architecture, but for the moment let's just
        // to this quick and dirty.
        public PropertiesWindow PropertiesWindow { get; set; }

        // TODO: another quick and dirty hack to be fixed.
        public ListingWindow ListingWindow { get; set; }

        private void UpdateUI()
        {
            tvLibrary.Nodes.Clear();
            TreeNode root = tvLibrary.Nodes.Add("Library");
            root.Tag = library;
            foreach (var module in library.Modules)
            {
                TreeNode nodeModule = root.Nodes.Add(module.ObjectName);
                nodeModule.Tag = module;
                foreach (var sym in module.PublicNames)
                {
                    // Try demangle the symbol's name.
                    string s = sym.Name;
                    if (sym.BaseSegment != null && sym.BaseSegment.ClassName == "CODE")
                    {
                        var sig = NameMangler.Demangle(s);
                        if (sig != null)
                            s = sig.Name;
                    }

                    if (sym.BaseSegment == null)
                    {
                        s = string.Format("{0} : {1:X4}:{2:X4}",
                            s, sym.BaseFrame, sym.Offset);
                    }
                    else
                    {
                        s = string.Format("{0} : {1}+{2:X}h",
                            s, sym.BaseSegment.SegmentName, sym.Offset);
                    }

                    TreeNode node = nodeModule.Nodes.Add(s);
                    node.Tag = sym;
                }
            }
        }

        private void LibraryBrowserWindow_Load(object sender, EventArgs e)
        {
            tvLibrary.SetWindowTheme("explorer");
        }

        private void tvLibrary_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.PropertiesWindow != null && e.Node != null)
            {
                this.PropertiesWindow.SelectedObject = e.Node.Tag;
                if (e.Node.Tag is ObjectModule)
                    UpdateImage((ObjectModule)e.Node.Tag);
            }
        }

        private void UpdateImage(ObjectModule module)
        {
            // For each segment, construct a list of LEDATA/LIDATA records.
            // These records fill data into the segment.
            // It is required that the data do not overlap, and do not
            // exceed segment boundary (here we only support 16-bit segments,
            // whose maximum size is 64KB).

            // Find the first CODE segment.
            LogicalSegment codeSegment = null;
            foreach (var seg in module.Segments)
            {
                if (seg.ClassName == "CODE")
                {
                    codeSegment = seg;
                    break;
                }
            }
            if (codeSegment == null)
                return;

            // Create a BinaryImage with the code.
            BinaryImage image = new BinaryImage(codeSegment.Data, new X86Codec.Pointer(0, 0));

            // Disassemble the instructions literally. Note that this should
            // be improved, but we don't do that yet.
            var addr = image.BaseAddress;
            for (var i = image.StartAddress; i < image.EndAddress; )
            {
                var instruction = image.DecodeInstruction(addr);
                image.CreatePiece(addr, addr + instruction.EncodedLength, ByteType.Code);
                addr = addr.Increment(instruction.EncodedLength);
                i += instruction.EncodedLength;
            }
            // ...

            // Display the code in our disassmbly window.
            if (this.ListingWindow != null)
            {
                Document doc = new Document();
                doc.Image = image;
                this.ListingWindow.Document = doc;
            }
        }
    }
}

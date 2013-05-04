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
                TreeNode nodeModule = root.Nodes.Add(module.Name);
                nodeModule.Tag = module;
                foreach (var sym in module.PublicNames)
                {
                    TreeNode node = nodeModule.Nodes.Add(sym.ToString());
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
            var map = new Dictionary<SegmentDefinition, byte[]>();
            foreach (var seg in module.SegmentDefinitions)
            {
                map.Add(seg, new byte[seg.Length]);
            }

            // Fill the segment with data from LEDATA/LIDATA records.
            foreach (var record in module.Records)
            {
                if (record.RecordNumber == RecordNumber.LEDATA ||
                    record.RecordNumber == RecordNumber.LEDATA32)
                {
                    var r = (LogicalEnumeratedDataRecord)record;
                    if (r.Segment == null)
                        continue;
                    
                    var seg = r.Segment;
                    if (r.DataOffset + r.Data.Length > seg.Length)
                        throw new InvalidOperationException();

                    byte[] segData = map[seg];
                    Array.Copy(r.Data, 0, segData, r.DataOffset, r.Data.Length);
                }
            }

            // Find the first CODE segment.
            byte[] code = null;
            foreach (var seg in module.SegmentDefinitions)
            {
                if (seg.Class == "CODE")
                {
                    code = map[seg];
                    break;
                }
            }
            if (code == null)
                return;

            // Create a BinaryImage with the code.
            BinaryImage image = new BinaryImage(code, new X86Codec.Pointer(0, 0));

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

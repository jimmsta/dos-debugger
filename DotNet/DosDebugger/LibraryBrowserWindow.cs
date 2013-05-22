using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Disassembler;
using Util.Forms;
using Disassembler2;
using Disassembler2.Omf;
using X86Codec;

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
            foreach (ObjectModule module in library.Modules)
            {
                TreeNode nodeModule = root.Nodes.Add(module.ObjectName);
                nodeModule.Tag = module;
                foreach (DefinedSymbol symbol in module.DefinedNames)
                {
                    string s = symbol.Name;
#if false
                    // Try demangle the symbol's name.
                    if (sym.BaseSegment != null && sym.BaseSegment.Class == "CODE")
                    {
                        //var sig = NameMangler.Demangle(s);
                        //if (sig != null)
                        //    s = sig.Name;
                    }
#endif
                    if (symbol.BaseSegment == null)
                    {
                        s = string.Format("{0} : {1:X4}:{2:X4}",
                            s, symbol.BaseFrame, symbol.Offset);
                    }
                    else
                    {
                        s = string.Format("{0} : {1}+{2:X}h",
                            s, symbol.BaseSegment.Name, symbol.Offset);
                    }

                    TreeNode node = nodeModule.Nodes.Add(s);
                    node.Tag = symbol;
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
#if false
            // For each segment, construct a list of LEDATA/LIDATA records.
            // These records fill data into the segment.
            // It is required that the data do not overlap, and do not
            // exceed segment boundary (here we only support 16-bit segments,
            // whose maximum size is 64KB).

            // Find the first CODE segment.
            LogicalSegment codeSegment = null;
            foreach (var seg in module.Segments)
            {
                if (seg.Class == "CODE")
                {
                    codeSegment = seg;
                    break;
                }
            }
            if (codeSegment == null)
                return;

            // Create a BinaryImage with the code.
            BinaryImage image = new BinaryImage(codeSegment.Image.Data, new Pointer(0, 0));

            // Disassemble the instructions literally. Note that this should
            // be improved, but we don't do that yet.
            var addr = image.BaseAddress;
            for (var i = image.StartAddress; i < image.EndAddress; )
            {
                var instruction = image.DecodeInstruction(addr);

                // An operand may have zero or one component that may be
                // fixed up. Check this.
#if false
                for (int k = 0; k < instruction.Operands.Length; k++)
                {
                    var operand = instruction.Operands[k];
                    if (operand is RelativeOperand)
                    {
                        var opr = (RelativeOperand)operand;
                        var loc = opr.Offset.Location;
                        int j = i - image.StartAddress + loc.StartOffset;
                        int fixupIndex = codeSegment.DataFixups[j];
                        if (fixupIndex != 0)
                        {
                            FixupDefinition fixup = codeSegment.Fixups[fixupIndex - 1];
                            if (fixup.DataOffset != j)
                                continue;

                            var target = new SymbolicTarget(fixup, module);
                            instruction.Operands[k] = new SymbolicRelativeOperand(target);
                            System.Diagnostics.Debug.WriteLine(instruction.ToString());
                        }
                    }
                }
#endif

                image.CreatePiece(addr, addr + instruction.EncodedLength, ByteType.Code);
                image[addr].Instruction = instruction;
                addr = addr.Increment(instruction.EncodedLength);

                // TODO: we need to check more accurately.

#if false
                // Check if any bytes covered by this instruction has a fixup
                // record associated with it. Note that an instruction might
                // have multiple fixup records associated with it, such as 
                // in a far call.
                for (int j = 0; j < instruction.EncodedLength; j++)
                {
                    int fixupIndex = codeSegment.DataFixups[i - image.StartAddress + j];
                    if (fixupIndex != 0)
                    {
                        FixupDefinition fixup = codeSegment.Fixups[fixupIndex - 1];
                        if (fixup.DataOffset != i - image.StartAddress + j)
                            continue;

                        if (fixup.Target.Method == FixupTargetSpecFormat.ExternalPlusDisplacement ||
                            fixup.Target.Method == FixupTargetSpecFormat.ExternalWithoutDisplacement)
                        {
                            var extIndex = fixup.Target.IndexOrFrame;
                            var extName = module.ExternalNames[extIndex - 1];
                            var disp = fixup.Target.Displacement;

                            System.Diagnostics.Debug.WriteLine(string.Format(
                                "{0} refers to {1}+{2} : {3}",
                                instruction, extName, disp, fixup.Location));
                        }
                    }
                }
#endif

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
#endif
        }

#if false
        private static string FormatSymbolicOperand(
            X86Codec.Instruction instruction,
            X86Codec.Operand operand,
            FixupDefinition fixup,
            ObjectModule module)
        {

            if (fixup.Target.Method == FixupTargetMethod.ExternalPlusDisplacement ||
                fixup.Target.Method == FixupTargetMethod.ExternalWithoutDisplacement)
            {
                var extIndex = fixup.Target.IndexOrFrame;
                var extName = module.ExternalNames[extIndex - 1];
                var disp = fixup.Target.Displacement;

                //System.Diagnostics.Debug.WriteLine(string.Format(
                //    "{0} : operand {4} refers to {1}+{2} : {3}",
                //    instruction, extName, disp, fixup.Location, operand));
                return extName.Name;
            }
            return null;
        }
#endif
    }
}

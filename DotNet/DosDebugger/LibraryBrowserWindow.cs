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
                        //var sig = NameMangler.Demangle(s);
                        //if (sig != null)
                        //    s = sig.Name;
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

                // An operand may have zero or one component that may be
                // fixed up. Check this.
                SymbolicInstruction si = new SymbolicInstruction(instruction);
                for (int k = 0; k < instruction.Operands.Length; k++)
                {
                    var opr = instruction.Operands[k];
                    if (opr.EncodedLength != 0)
                    {
                        int j = i - image.StartAddress + opr.EncodedPosition;
                        int fixupIndex = codeSegment.DataFixups[j];
                        if (fixupIndex != 0)
                        {
                            FixupDefinition fixup = codeSegment.Fixups[fixupIndex - 1];
                            if (fixup.DataOffset != j)
                                continue;

                            si.operandText[k] = FormatSymbolicOperand(instruction, opr, fixup, module);
                            System.Diagnostics.Debug.WriteLine(si.ToString());
                        }
                    }
                }

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
        }

        private static string FormatSymbolicOperand(
            X86Codec.Instruction instruction,
            X86Codec.Operand operand,
            FixupDefinition fixup,
            ObjectModule module)
        {

            if (fixup.Target.Method == FixupTargetSpecFormat.ExternalPlusDisplacement ||
                fixup.Target.Method == FixupTargetSpecFormat.ExternalWithoutDisplacement)
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
    }

    /// <summary>
    /// Represents an instruction with some fields replaced with a symbol,
    /// e.g. 
    /// CALL _strcpy
    /// MOV DX, seg DGROUP
    /// JMP [BX + off Table]
    /// </summary>
    public class SymbolicInstruction
    {
        public Instruction instruction { get; private set; }
        public string[] operandText;

        public SymbolicInstruction(Instruction instruction)
        {
            this.instruction = instruction;
            this.operandText = new string[instruction.Operands.Length];
        }

        /// <summary>
        /// Converts the instruction to a string in Intel syntax.
        /// </summary>
        /// <returns>The formatted instruction.</returns>
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            // Write address.
            s.Append(instruction.Location.ToString());
            s.Append("  ");

            // Format group 1 (LOCK/REPZ/REPNZ) prefix.
            if ((instruction.Prefix & Prefixes.Group1) != 0)
            {
                s.Append((instruction.Prefix & Prefixes.Group1).ToString());
                s.Append(' ');
            }

            // Format mnemonic.
            s.Append(instruction.Operation.ToString());

            // Format operands.
            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (i > 0)
                {
                    s.Append(',');
                }
                s.Append(' ');
                s.Append(FormatOperand(i));
            }
            return s.ToString().ToLowerInvariant();
        }

        private string FormatOperand(int i)
        {
            var operand = instruction.Operands[i];
            if (operandText[i] != null)
                return operandText[i];

            if (operand is RelativeOperand)
            {
                RelativeOperand opr = (RelativeOperand)operand;
                return ((ushort)(instruction.Location.Offset + instruction.EncodedLength + opr.Offset)).ToString("X4");
            }
            else
            {
                return operand.ToString();
            }
        }
    }
}

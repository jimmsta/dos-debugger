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

        private void MainForm_Load(object sender, EventArgs e)
        {
            lvListing.SetWindowTheme("explorer");
            string fileName = @"E:\Dev\Projects\RevEng\data\H.EXE";
            mzFile = new MZFile(fileName);
            mzFile.Relocate(baseSegment);
            dasm = new Disassembler.Disassembler(mzFile.Image, mzFile.BaseAddress);
            cbBookmarks.SelectedIndex = 1;
        }

        private void btnMzInfo_Click(object sender, EventArgs e)
        {
            ExecutableInfoForm f = new ExecutableInfoForm();
            f.MzFile = mzFile;
            f.Show(this);
        }

        Disassembler.Disassembler dasm;

        private void btnDisassemble_Click(object sender, EventArgs e)
        {
            //TestDecode(mzFile.Image, mzFile.EntryPoint, mzFile.BaseAddress);
            TestDecode(mzFile.Image, new FarPointer16(baseSegment, 0x17fc), mzFile.BaseAddress);
        }

        private void TestDecode(
            byte[] image,
            FarPointer16 startAddress, 
            FarPointer16 baseAddress)
        {
            DecoderContext options = new DecoderContext();
            options.AddressSize = CpuSize.Use16Bit;
            options.OperandSize = CpuSize.Use16Bit;

            X86Codec.Decoder decoder = new X86Codec.Decoder();

            FarPointer16 ip = startAddress;
            for (int index = startAddress - baseAddress; index < image.Length; )
            {
                Instruction instruction = null;
                try
                {
                    instruction = decoder.Decode(image, index, options);
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
                DisplayInstruction(ip, instruction);
#endif
                index += instruction.EncodedLength;
                ip += instruction.EncodedLength;
            }
        }

        private static string FormatBinary(byte[] data, int startIndex, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    sb.Append(' ');
                sb.AppendFormat("{0:x2}", data[startIndex + i]);
            }
            return sb.ToString();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            //dasm.Analyze(new FarPointer16(baseSegment, 0x17fc), true);
            dasm.Analyze(mzFile.EntryPoint, true);
            X86Codec.Decoder decoder = new X86Codec.Decoder();

            // Display analyzed code.
            ByteAttributes[] attr = dasm.ByteAttributes;
            bool inCodeBlock = false;
            lvListing.Visible = false;
            for (int i = 0; i < attr.Length; )
            {
                if (attr[i].IsBoundary && attr[i].Type == ByteType.Code)
                {
                    DecoderContext context = new DecoderContext();
                    context.AddressSize = CpuSize.Use16Bit;
                    context.OperandSize = CpuSize.Use16Bit;

                    Instruction insn = decoder.Decode(dasm.Image, i, context);
                    DisplayInstruction(mzFile.BaseAddress + i, insn);
                    i += insn.EncodedLength;
                    inCodeBlock = true;
                }
                else
                {
                    if (inCodeBlock)
                    {
                        lvListing.Items.Add("");
                    }
                    inCodeBlock = false;
                    i++;
                }
            }
            lvListing.Visible = true;

            // Display subroutines.
            FarPointer16[] procEntries = dasm.Procedures;
            foreach (FarPointer16 ptr in procEntries)
            {
                lvProcedures.Items.Add(ptr.ToString());
            }

            // Display analysis errors.
            foreach (Error error in dasm.Errors)
            {
                ListViewItem item = new ListViewItem();
                item.Text = error.Location.ToString();
                item.SubItems.Add(error.Message);
                lvErrors.Items.Add(item);
            }
        }

        private void DisplayInstruction(FarPointer16 start, Instruction instruction)
        {
            ListViewItem item = new ListViewItem();
            item.Text = string.Format(start.ToString());
            item.SubItems.Add(FormatBinary(mzFile.Image, start - mzFile.BaseAddress, instruction.EncodedLength));
            item.SubItems.Add(instruction.ToString());

            //item.UseItemStyleForSubItems = false;
            //item.SubItems[1].BackColor = Color.LightGray;
            lvListing.Items.Add(item);
        }

        private void btnGoTo_Click(object sender, EventArgs e)
        {
            // Find the address.
            FarPointer16 target;
            string addr = cbBookmarks.Text;
            if (addr.Length < 9 || !FarPointer16.TryParse(addr.Substring(0, 9), out target))
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

        private bool GoToLocation(FarPointer16 target)
        {
            // Find the first entry that is greater than or equal to target.
            foreach (ListViewItem item in lvListing.Items)
            {
                FarPointer16 current;
                if (!FarPointer16.TryParse(item.Text, out current))
                    continue;
                if (current.EffectiveAddress >= target.EffectiveAddress)
                {
                    //item.EnsureVisible();
                    lvListing.TopItem = item;
                    lvListing.Focus();
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
                GoToLocation(FarPointer16.Parse(lvProcedures.SelectedItems[0].Text));
            }
        }
    }
}

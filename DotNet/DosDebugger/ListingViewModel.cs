using System;
using System.Collections.Generic;
using System.Text;
using Disassembler;
using X86Codec;
using System.Windows.Forms;
using System.Drawing;

namespace DosDebugger
{
    /// <summary>
    /// Represents the view model of ASM listing.
    /// </summary>
    class ListingViewModel
    {
        private List<ListingRow> rows;

        public ListingViewModel(Disassembler16 dasm)
        {
            rows = new List<ListingRow>();

            // Make a dictionary from CS:IP to the error at that location.
            Dictionary<int, Error> errorMap = new Dictionary<int, Error>();
            foreach (Error error in dasm.Errors)
            {
                errorMap[error.Location - dasm.BaseAddress] = error;
            }

            // Display analyzed code and data.
            ByteAttribute[] attr = dasm.ByteAttributes;
            bool inCodeBlock = false;
            for (int i = 0; i < attr.Length; )
            {
                if (attr[i].IsBoundary && attr[i].Type == ByteType.Code)
                {
                    int baseSegment = dasm.BaseAddress.Segment;
                    UInt16 seg = dasm.ByteSegments[i];
                    Pointer location = new Pointer(seg, (UInt16)(i - (seg - baseSegment) * 16));

                    // TBD: the location parameter is incorrect.
                    Instruction insn = X86Codec.Decoder.Decode(dasm.Image, i, location, CpuMode.RealAddressMode);
                    rows.Add(new CodeListingRow(insn, ArraySlice(dasm.Image, i, insn.EncodedLength)));
                    i += insn.EncodedLength;
                    inCodeBlock = true;
                }
                else if (attr[i].IsBoundary && attr[i].Type == ByteType.Data)
                {
                    int j = i + 1;
                    while (attr[j].Type == ByteType.Data && !attr[j].IsBoundary)
                        j++;

                    int baseSegment = dasm.BaseAddress.Segment;
                    UInt16 seg = dasm.ByteSegments[i];
                    Pointer location = new Pointer(seg, (UInt16)(i - (seg - baseSegment) * 16));

                    rows.Add(new DataListingRow(location, ArraySlice(dasm.Image, i, j - i)));
                    i = j;
                    inCodeBlock = true;
                }
                else
                {
                    if (errorMap.ContainsKey(i))
                    {
                        rows.Add(new ErrorListingRow(errorMap[i]));
                    }

                    if (inCodeBlock)
                    {
                        rows.Add(new BlankListingRow());
                    }
                    inCodeBlock = false;
                    i++;
                }
            }
        }

        public List<ListingRow> Rows
        {
            get { return rows; }
        }

        public ListViewItem CreateViewItem(int index)
        {
            return rows[index].CreateViewItem();
        }

        private static byte[] ArraySlice(byte[] array, int offset, int count)
        {
            byte[] result = new byte[count];
            Array.Copy(array, offset, result, 0, count);
            return result;
        }
    }

    /// <summary>
    /// Represents a row in ASM listing.
    /// </summary>
    abstract class ListingRow
    {
        //public ListingRowType Type { get; set; }
        public abstract Pointer Location { get; }
        public abstract ListViewItem CreateViewItem();

        public static string FormatBinary(byte[] data, int startIndex, int count)
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
    }

    class BlankListingRow : ListingRow
    {
        public override Pointer Location
        {
            get { return Pointer.Invalid; }
        }

        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = new ListViewItem();
            item.SubItems.Add("");
            item.SubItems.Add("");
            return item;
        }
    }

    class CodeListingRow : ListingRow
    {
        private Instruction instruction;
        private byte[] code;

        public CodeListingRow(Instruction instruction, byte[] code)
        {
            this.instruction = instruction;
            this.code = code;
        }

        public override Pointer Location
        {
            get { return instruction.Location; }
        }

        public override ListViewItem CreateViewItem()
        {
            Pointer location = instruction.Location;
            ListViewItem item = new ListViewItem();
            item.Text = location.ToString();
            item.SubItems.Add(FormatBinary(code, 0, code.Length));
            item.SubItems.Add(instruction.ToString());

            //item.UseItemStyleForSubItems = false;
            //item.SubItems[1].BackColor = Color.LightGray;
            return item;
        }
    }

    class DataListingRow : ListingRow
    {
        private Pointer location;
        private byte[] data;

        public DataListingRow(Pointer location, byte[] data)
        {
            this.location = location;
            this.data = data;
        }

        public override Pointer Location
        {
            get { return location; }
        }

        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = new ListViewItem();
            item.Text = location.ToString();
            item.SubItems.Add(FormatBinary(data, 0, data.Length));

            string s;
            switch (data.Length)
            {
                case 1:
                    s = string.Format("db {0:x2}", data[0]);
                    break;
                case 2:
                    s = string.Format("dw {0:x4}", BitConverter.ToUInt16(data, 0));
                    break;
                default:
                    s = "** data **";
                    break;
            }
            item.SubItems.Add(s);

            //item.UseItemStyleForSubItems = false;
            //item.SubItems[1].BackColor = Color.LightGray;
            return item;
        }
    }

    class ErrorListingRow : ListingRow
    {
        private Error error;

        public ErrorListingRow(Error error)
        {
            this.error = error;
        }

        public override Pointer Location
        {
            get { return error.Location; }
        }

        public override ListViewItem CreateViewItem()
        {
            Pointer start = error.Location;
            ListViewItem item = new ListViewItem();
            item.Text = start.ToString();
            item.SubItems.Add("");
            item.SubItems.Add(error.Message);

            //item.UseItemStyleForSubItems = false;
            //item.SubItems[1].BackColor = Color.LightGray;
            item.ForeColor = Color.Red;
            return item;
        }
    }

#if false
    enum ListingRowType
    {
        /// <summary>
        /// The row is a blank row used to improve readability.
        /// </summary>
        Blank,

        /// <summary>
        /// The row displays an instruction.
        /// </summary>
        Code,

        /// <summary>
        /// The row displays a data item.
        /// </summary>
        Data,

        /// <summary>
        /// The row displays an error.
        /// </summary>
        Error,
    }
#endif
}

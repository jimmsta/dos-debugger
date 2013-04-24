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
        private List<ListingRow> rows = new List<ListingRow>();
        private List<ProcedureItem> procItems = new List<ProcedureItem>();

        public ListingViewModel(Disassembler16 dasm)
        {
            // Make a dictionary from CS:IP to the error at that location.
            Dictionary<int, Error> errorMap = new Dictionary<int, Error>();
            foreach (Error error in dasm.Errors)
            {
                errorMap[error.Location - dasm.BaseAddress] = error;
            }

            // Make a dictionary from byte offset to listing row index.
            Dictionary<int, int> rowLookup = new Dictionary<int, int>();
            //Dictionary<Procedure, ProcedureItem> procMap 
            //    = new Dictionary<Procedure, ProcedureItem>();

            // Display analyzed code and data.
            ByteProperties[] attr = dasm.ByteProperties;
            Pointer address = dasm.BaseAddress;
            for (int i = 0; i < attr.Length; )
            {
                ByteProperties b = attr[i];

                if (IsLeadByteOfCode(b))
                {
                    Instruction insn = X86Codec.Decoder.Decode(dasm.Image, i, b.Address, CpuMode.RealAddressMode);
                    rowLookup[i] = rows.Count;
                    rows.Add(new CodeListingRow(insn, ArraySlice(dasm.Image, i, insn.EncodedLength)));
                    address = b.Address + insn.EncodedLength;
                    i += insn.EncodedLength;
                }
                else if (IsLeadByteOfData(b))
                {
                    int j = i + 1;
                    while (j < attr.Length &&
                           attr[j] != null &&
                           attr[j].Type == ByteType.Data &&
                           !attr[j].IsLeadByte)
                        j++;

                    rowLookup[i] = rows.Count;
                    rows.Add(new DataListingRow(b.Address, ArraySlice(dasm.Image, i, j - i)));
                    address = b.Address + (j - i);
                    i = j;
                }
                else
                {
                    if (errorMap.ContainsKey(i))
                    {
                    //    rows.Add(new ErrorListingRow(errorMap[i]));
                    }
                    int j = i + 1;
                    while (j < attr.Length &&
                           !IsLeadByteOfCode(attr[j]) &&
                           !IsLeadByteOfData(attr[j]))
                        j++;

                    rowLookup[i] = rows.Count;
                    rows.Add(new BlankListingRow(address, ArraySlice(dasm.Image, i, j - i)));
                    address += (j - i);
                    i = j;
                }
            }

            // Fill the BeginIndex and EndIndex of the procedures.
            foreach (Procedure proc in dasm.Procedures)
            {
                if (!proc.ByteRange.IsEmpty)
                {
                    ProcedureItem item = new ProcedureItem(proc);
                    item.BeginIndex = rowLookup[proc.ByteRange.LowerBound];
                    item.EndIndex = rowLookup[proc.ByteRange.UpperBound];
                    procItems.Add(item);
                }
            }
        }

        private static bool IsLeadByteOfCode(ByteProperties b)
        {
            return (b != null && b.Type == ByteType.Code && b.IsLeadByte);
        }

        private static bool IsLeadByteOfData(ByteProperties b)
        {
            return (b != null && b.Type == ByteType.Data && b.IsLeadByte);
        }

        public List<ListingRow> Rows
        {
            get { return rows; }
        }

        public List<ProcedureItem> ProcedureItems
        {
            get { return procItems; }
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
        /// <summary>
        /// Gets the address of the listing row.
        /// </summary>
        public abstract Pointer Location { get; }

        /// <summary>
        /// Gets the opcode bytes of this listing row. Must not be null.
        /// </summary>
        public abstract byte[] Opcode { get; }

        /// <summary>
        /// Gets the main text to display for this listing row.
        /// </summary>
        public abstract string Text { get; }

        public virtual ListViewItem CreateViewItem()
        {
            ListViewItem item = new ListViewItem();
            item.Text = this.Location.ToString();
            byte[] data = this.Opcode;
            if (data.Length > 6)
            {
                item.SubItems.Add(FormatBinary(data, 0, 6) + "...");
            }
            else
            {
                item.SubItems.Add(FormatBinary(data, 0, data.Length));
            }
            item.SubItems.Add(this.Text);
            return item;
        }

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

    /// <summary>
    /// Represents a continuous range of unanalyzed bytes.
    /// </summary>
    class BlankListingRow : ListingRow
    {
        private Pointer location;
        private byte[] data;

        public BlankListingRow(Pointer location, byte[] data)
        {
            this.location = location;
            this.data = data;
        }

        public override Pointer Location
        {
            get { return location; }
        }

        public override byte[] Opcode
        {
            get { return data; }
        }

        public override string Text
        {
            get { return string.Format("{0} unanalyzed bytes.", data.Length); }
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

        public override byte[] Opcode
        {
            get { return code; }
        }

        public override string Text
        {
            get { return instruction.ToString(); }
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

        public override byte[] Opcode
        {
            get { return data; }
        }

        public override string Text
        {
            get
            {
                switch (data.Length)
                {
                    case 1:
                        return string.Format("db {0:x2}", data[0]);
                    case 2:
                        return string.Format("dw {0:x4}", BitConverter.ToUInt16(data, 0));
                    case 4:
                        return string.Format("dd {0:x8}", BitConverter.ToUInt32(data, 0));
                    default:
                        return "** data **";
                }
            }
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

        public override byte[] Opcode
        {
            get { return new byte[0]; }
        }

        public override string Text
        {
            get { return error.Message; }
        }

        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = base.CreateViewItem();
            item.ForeColor = Color.Red;
            return item;
        }
    }

    class ProcedureItem
    {
        public ProcedureItem(Procedure procedure)
        {
            this.Procedure = procedure;
        }

        public Procedure Procedure { get; private set; }

        // [begin,end) index of the listing rows that belong to
        // this procedure.
        public int BeginIndex;
        public int EndIndex;

        public override string ToString()
        {
            return Procedure.EntryPoint.ToString();
        }
    }

    class SegmentListItem
    {
        public SegmentListItem(Pointer segmentStart)
        {
            this.SegmentStart = segmentStart;
        }

        public Pointer SegmentStart { get; private set; }

        public UInt16 Segment
        {
            get { return SegmentStart.Segment; }
        }

        public override string ToString()
        {
            return SegmentStart.ToString();
        }
    }
}

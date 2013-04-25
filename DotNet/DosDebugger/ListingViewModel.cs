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
        private List<SegmentItem> segmentItems = new List<SegmentItem>();
        private Disassembler16 dasm;

        /// <summary>
        /// Array of the offsets or each row. This array is used to speed up
        /// row lookup. While this information can be obtained from the rows
        /// collection itself, using a separate array has two benefits:
        /// 1, it utilizes BinarySearch() without the need to create a dummy
        ///    ListingRow object or a custom comparer;
        /// 2, it saves extra memory indirections and is thus faster.
        /// The cost is of course a little extra memory footprint.
        /// </summary>
        private int[] rowOffsets;

        public ListingViewModel(Disassembler16 dasm)
        {
            this.dasm = dasm;

            // Make a dictionary from CS:IP to the error at that location.
            Dictionary<int, Error> errorMap = new Dictionary<int, Error>();
            foreach (Error error in dasm.Errors)
            {
                errorMap[error.Location - dasm.BaseAddress] = error;
            }

            // Display analyzed code and data.
            BinaryImage image = dasm.Image;
            Pointer address = dasm.BaseAddress;
            for (int i = 0; i < image.Length; )
            {
                ByteProperties b = image[i];

                if (IsLeadByteOfCode(b))
                {
                    Instruction insn = dasm.Image.DecodeInstruction(b.Address);
                    rows.Add(new CodeListingRow(i, insn, dasm.Image.GetBytes(i, insn.EncodedLength)));
                    address = b.Address + insn.EncodedLength;
                    i += insn.EncodedLength;
                }
                else if (IsLeadByteOfData(b))
                {
                    int j = i + 1;
                    while (j < image.Length && 
                           image[j].Type == ByteType.Data &&
                           !image[j].IsLeadByte)
                        j++;

                    rows.Add(new DataListingRow(i, b.Address, image.GetBytes(i, j - i)));
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
                    while (j < image.Length &&
                           !IsLeadByteOfCode(image[j]) &&
                           !IsLeadByteOfData(image[j]))
                        j++;

                    rows.Add(new BlankListingRow(i, address, image.GetBytes(i, j - i)));
                    address += (j - i);
                    i = j;
                }
            }

            // Create a sorted array of the offsets or each row.
            rowOffsets = new int[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                rowOffsets[i] = dasm.PointerToOffset(rows[i].Location);
            }

            // Fill the BeginIndex and EndIndex of the procedures.
            foreach (Procedure proc in dasm.Procedures)
            {
                if (!proc.ByteRange.IsEmpty)
                {
                    ProcedureItem item = new ProcedureItem(proc);
                    Range range = proc.ByteRange.BoundingRange;
                    item.BeginRowIndex = range.Begin;
                    item.EndRowIndex = range.End;
                    // TBD: need to check broken instruction conditions
                    // as well as leading/trailing unanalyzed bytes.
                    procItems.Add(item);
                }
            }

            // Create segment items.
            foreach (Segment segment in dasm.Segments)
            {
                segmentItems.Add(new SegmentItem(segment.StartAddress));
            }
        }

        private static bool IsLeadByteOfCode(ByteProperties b)
        {
            return (b.Type == ByteType.Code && b.IsLeadByte);
        }

        private static bool IsLeadByteOfData(ByteProperties b)
        {
            return (b.Type == ByteType.Data && b.IsLeadByte);
        }

        public List<ListingRow> Rows
        {
            get { return rows; }
        }

        /// <summary>
        /// Finds the row that occupies the given address. If no row occupies
        /// that address, finds the closest row.
        /// </summary>
        /// <param name="address">The address to find.</param>
        /// <returns>ListingRow, or null if the view is empty.</returns>
        public int FindRowIndex(Pointer address)
        {
            return FindRowIndex(dasm.PointerToOffset(address));
        }

        /// <summary>
        /// Finds the row that occupies the given offset. If no row occupies
        /// that offset, finds the closest row.
        /// </summary>
        /// <param name="offset">The offset to find.</param>
        /// <returns>ListingRow, or -1 if the view is empty.</returns>
        public int FindRowIndex(int offset)
        {
            if (rowOffsets.Length == 0)
                return -1;

            int k = Array.BinarySearch(rowOffsets, offset);
            if (k >= 0) // found
            {
                return k;
            }
            else // not found, but would be inserted at ~k
            {
                k = ~k;
                return k - 1;
            }
        }

        public List<ProcedureItem> ProcedureItems
        {
            get { return procItems; }
        }

        public List<SegmentItem> SegmentItems
        {
            get { return segmentItems; }
        }

        public ListViewItem CreateViewItem(int index)
        {
            return rows[index].CreateViewItem();
        }
    }

    /// <summary>
    /// Represents a row in ASM listing.
    /// </summary>
    abstract class ListingRow
    {
        public int Index { get; protected set; }

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

        protected ListingRow(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            this.Index = index;
        }

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

#if false
    class ListingRowLocationComparer : IComparer<ListingRow>
    {
        public int Compare(ListingRow x, ListingRow y)
        {
            return x.Location.EffectiveAddress.CompareTo(y.Location.EffectiveAddress);
        }
    }
#endif

    /// <summary>
    /// Represents a continuous range of unanalyzed bytes.
    /// </summary>
    class BlankListingRow : ListingRow
    {
        private Pointer location;
        private byte[] data;

        public BlankListingRow(int index, Pointer location, byte[] data)
            : base(index)
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

        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = base.CreateViewItem();
            item.BackColor = Color.LightGray;
            return item;
        }
    }

    class CodeListingRow : ListingRow
    {
        private Instruction instruction;
        private byte[] code;

        public CodeListingRow(int index, Instruction instruction, byte[] code)
            : base(index)
        {
            this.instruction = instruction;
            this.code = code;
        }

        public Instruction Instruction
        {
            get { return this.instruction; }
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

        public DataListingRow(int index, Pointer location, byte[] data)
            : base(index)
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

        public ErrorListingRow(int index, Error error)
            : base(index)
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
        public int BeginRowIndex;
        public int EndRowIndex;

        public override string ToString()
        {
            return Procedure.EntryPoint.ToString();
        }
    }

    class SegmentItem
    {
        public SegmentItem(Pointer segmentStart)
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

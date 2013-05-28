using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;
using Disassembler;
using Util.Data;
using X86Codec;

namespace WpfDebugger
{
    /// <summary>
    /// Represents the view model of ASM listing.
    /// </summary>
    class ListingViewModel
    {
        readonly List<ListingRow> rows = new List<ListingRow>();
        readonly List<ProcedureItem> procItems = new List<ProcedureItem>();
        //readonly List<SegmentItem> segmentItems = new List<SegmentItem>();
        //private Disassembler16 dasm;
        //private BinaryImage image;
        readonly ImageChunk image;

        /// <summary>
        /// Array of the address of each row. This array is used to speed up
        /// row lookup. While this information can be obtained from the rows
        /// collection itself, using a separate array has two benefits:
        /// 1, it utilizes BinarySearch() without the need to create a dummy
        ///    ListingRow object or a custom comparer;
        /// 2, it saves extra memory indirections and is thus faster.
        /// The cost is of course a little extra memory footprint.
        /// </summary>
        private int[] rowAddresses; // rename to rowOffsets

        public ListingViewModel(Assembly assembly, Segment segment)
        {
            ImageChunk image = segment.Image;
            this.image = image;

            // Make a list of the errors in this segment. Ideally we should
            // put this logic into ErrorCollection. But for convenience we
            // leave it here for the moment.
            List<Error> errors =
                (from error in assembly.Errors
                 where error.Location.Segment == segment.Id
                 orderby error.Location
                 select error).ToList();
            int iError = 0;

            // Display analyzed code and data.
            Address address = new Address(segment.Id, 0);
            for (int i = 0; i < image.Length; )
            {
                ImageByte b = image[i];

                while (iError < errors.Count && errors[iError].Location.Offset <= i)
                {
                    rows.Add(new ErrorListingRow(assembly, errors[iError++]));
                }

                if (IsLeadByteOfCode(b))
                {
#if false
                    if ( b.BasicBlock != null && b.BasicBlock.Location.Offset == i)
                    {
                        rows.Add(new LabelListingRow(0, b.BasicBlock));
                    }
#endif

                    Instruction insn = b.Instruction;
                    System.Diagnostics.Debug.Assert(insn != null);
                    rows.Add(new CodeListingRow(assembly, address, insn, image.Data.Slice(i, insn.EncodedLength)));

                    address += insn.EncodedLength; // TODO: handle wrapping
                    i += insn.EncodedLength;
                }
                else if (IsLeadByteOfData(b))
                {
                    var j = i + 1;
                    while (j < image.Length &&
                           image[j].Type == ByteType.Data &&
                           !image[j].IsLeadByte)
                        j++;

                    rows.Add(new DataListingRow(assembly, address, image.Data.Slice(i, j - i)));
                    address += (j - i); // TODO: handle wrapping
                    i = j;
                }
                else
                {
                    //if (errorMap.ContainsKey(i))
                    {
                        //    rows.Add(new ErrorListingRow(errorMap[i]));
                    }
                    var j = i + 1;
                    while (j < image.Length &&
                           !IsLeadByteOfCode(image[j]) &&
                           !IsLeadByteOfData(image[j]))
                        j++;

                    rows.Add(new BlankListingRow(assembly, address, image.Data.Slice(i, j - i)));
                    address += (j - i); // TODO: handle wrapping
#if false
                    try
                    {
                        address = address.Increment(j - i);
                    }
                    catch (AddressWrappedException)
                    {
                        address = Address.Invalid;
                    }
#endif
                    i = j;
                }
            }

            while (iError < errors.Count)
            {
                rows.Add(new ErrorListingRow(assembly, errors[iError++]));
            }

            // Create a sorted array containing the address of each row.
            rowAddresses = new int[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                rowAddresses[i] = rows[i].Location.Offset;
            }

#if false
            // Create a ProcedureItem view object for each non-empty
            // procedure.
            // TODO: display an error for empty procedures.
            foreach (Procedure proc in image.Procedures)
            {
                if (proc.IsEmpty)
                    continue;

                ProcedureItem item = new ProcedureItem(proc);
                //var range = proc.Bounds;
                //item.FirstRowIndex = FindRowIndex(range.Begin);
                //item.LastRowIndex = FindRowIndex(range.End - 1);
                
                // TBD: need to check broken instruction conditions
                // as well as leading/trailing unanalyzed bytes.
                procItems.Add(item);
            }

            // Create segment items.
            foreach (Segment segment in image.Segments)
            {
                segmentItems.Add(new SegmentItem(segment));
            }
#endif
        }

        private static bool IsLeadByteOfCode(ImageByte b)
        {
            return (b.Type == ByteType.Code && b.IsLeadByte);
        }

        private static bool IsLeadByteOfData(ImageByte b)
        {
            return (b.Type == ByteType.Data && b.IsLeadByte);
        }

        public ImageChunk Image
        {
            get { return image; }
        }

        public List<ListingRow> Rows
        {
            get { return rows; }
        }

#if false
        /// <summary>
        /// Finds the row that covers the given address. If no row occupies
        /// that address, finds the closest row.
        /// </summary>
        /// <param name="address">The address to find.</param>
        /// <returns>ListingRow, or null if the view is empty.</returns>
        public int FindRowIndex(Pointer address)
        {
            return FindRowIndex(address.LinearAddress);
        }
#endif

        /// <summary>
        /// Finds the first row that covers the given address.
        /// </summary>
        /// <param name="address">The address to find.</param>
        /// <returns>Index of the row found, which may be rows.Count if the
        /// address is past the end of the list.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The view model is
        /// empty, or address is smaller than the address of the first row.
        /// </exception>
        /// TODO: maybe we should split this to FindRowLowerBound and FindRowUpperBound
        public int FindRowIndex(int offset)
        {
#if false
            if (rowAddresses.Length == 0 ||
                address < rowAddresses[0])
                throw new ArgumentOutOfRangeException("address");
            if (address > image.EndAddress)
                throw new ArgumentOutOfRangeException("address");
            if (address == image.EndAddress)
                return rowAddresses.Length;
#endif

            int k = Array.BinarySearch(rowAddresses, offset);
            if (k >= 0) // found; find first one
            {
                while (k > 0 && rowAddresses[k - 1] == offset)
                    k--;
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

        //public List<SegmentItem> SegmentItems
        //{
        //    get { return segmentItems; }
        //}
    }

    /// <summary>
    /// Represents a row in ASM listing.
    /// </summary>
    abstract class ListingRow
    {
        readonly Assembly assembly;
        readonly Address location;
        
        protected ListingRow(Assembly assembly, Address location)
        {
            this.assembly = assembly;
            this.location = location;
        }

        public Assembly Assembly
        {
            get { return assembly; }
        }

        /// <summary>
        /// Gets the address of the listing row.
        /// </summary>
        public Address Location
        {
            get { return location; }
        }

        public string LocationString
        {
            get
            {
                Segment segment = assembly.GetSegment(location.Segment);
                if (segment is LogicalSegment)
                {
                    return ((LogicalSegment)segment).FullName + "+" + location.Offset.ToString("X4");
                }
                else
                {
                    return location.ToString();
                }
            }
        }

        public virtual Color ForeColor
        {
            get { return Colors.Black; }
        }

        /// <summary>
        /// Gets the opcode bytes of this listing row. Must not be null.
        /// </summary>
        public abstract byte[] Opcode { get; }

        public string OpcodeText
        {
            get
            {
                if (Opcode == null)
                    return null;
                else if (Opcode.Length <= 6)
                    return FormatBinary(Opcode, 0, Opcode.Length);
                else
                    return FormatBinary(Opcode, 0, 6) + "...";
            }
        }

        /// <summary>
        /// Gets the label to display for this row, or null if there is no
        /// label to display.
        /// </summary>
        public virtual string Label
        {
            get
            {
                // Check whether we have a procedure starting at this address.
                Procedure proc = assembly.Procedures.Find(this.Location);
                if (proc != null)
                    return proc.Name;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the main text to display for this listing row.
        /// </summary>
        public abstract string Text { get; }

        public virtual string RichText
        {
            get { return Text; }
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
        private byte[] data;

        public BlankListingRow(Assembly assembly, Address location, byte[] data)
            : base(assembly, location)
        {
            this.data = data;
        }

        public override byte[] Opcode
        {
            get { return data; }
        }

        public override string Text
        {
            get { return string.Format("{0} unanalyzed bytes.", data.Length); }
        }

#if false
        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = base.CreateViewItem();
            item.BackColor = Color.LightGray;
            return item;
        }
#endif
    }

    class CodeListingRow : ListingRow
    {
        private Instruction instruction;
        private byte[] code;
        private string strInstruction;

        public CodeListingRow(Assembly assembly, Address location, Instruction instruction, byte[] code)
            : base(assembly, location)
        {
            this.instruction = instruction;
            this.code = code;
            //this.strInstruction = instruction.ToString();
            this.strInstruction =
                new SymbolicInstructionFormatter().FormatInstruction(instruction);
        }

        public Instruction Instruction
        {
            get { return this.instruction; }
        }

        public override byte[] Opcode
        {
            get { return code; }
        }

        public override string Text
        {
            //get { return instruction.ToString(); }
            get { return strInstruction; }
        }

        public override string RichText
        {
            get
            {
#if false
                if (instruction.Operands.Length == 1 &&
                    instruction.Operands[0] is RelativeOperand)
                {
                    StringBuilder sb = new StringBuilder();
                    // TODO: add prefix
                    sb.Append(instruction.Operation.ToString());

                    sb.AppendFormat(" <a href=\"ddd://document1/#{0}\">{1}</a>",
                        "somewhere",
                        instruction.Operands[0]);
                    return sb.ToString();
                }
                else
#endif
                {
                    return Text;
                }
            }
        }
    }

    class DataListingRow : ListingRow
    {
        private byte[] data;

        public DataListingRow(Assembly assembly, Address location, byte[] data)
            : base(assembly, location)
        {
            this.data = data;
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

        public ErrorListingRow(Assembly assembly, Error error)
            : base(assembly, error.Location)
        {
            this.error = error;
        }

        public override Color ForeColor
        {
            get { return Colors.Red; }
        }

        public override byte[] Opcode
        {
            get { return new byte[0]; }
        }

        public override string Text
        {
            get { return error.Message; }
        }

#if false
        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = base.CreateViewItem();
            item.ForeColor = Color.Red;
            return item;
        }
#endif
    }

    class LabelListingRow : ListingRow
    {
        private BasicBlock block;

        public LabelListingRow(Assembly assembly, BasicBlock block)
            : base(assembly, Address.Invalid)
        {
            this.block = block;
        }

        public override byte[] Opcode
        {
            get { return null; }
        }

        public override string Text
        {
            get { return string.Format("loc_{0}", block.Location.Offset); }
        }

#if false
        public override ListViewItem CreateViewItem()
        {
            ListViewItem item = base.CreateViewItem();
            //item.UseItemStyleForSubItems = true;
            //item.SubItems[2].ForeColor = Color.Blue;
            item.ForeColor = Color.Blue;
            return item;
        }
#endif
    }

    class ProcedureItem
    {
        public ProcedureItem(Procedure procedure)
        {
            this.Procedure = procedure;
        }

        public Procedure Procedure { get; private set; }

        /// <summary>
        /// Gets or sets the index of the first row to display for this
        /// procedure. Note that this row may not belong to this procedure.
        /// </summary>
        //public int FirstRowIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the last row to display for this
        /// procedure. Note that this row may not belong to this procedure.
        /// </summary>
        //public int LastRowIndex { get; set; }

        public override string ToString()
        {
            return Procedure.EntryPoint.ToString();
        }
    }

#if false
    class SegmentItem
    {
        public SegmentItem(Segment segment)
        {
            this.SegmentStart = segment.StartAddress.ToFarPointer(segment.SegmentAddress);
        }

        public Pointer SegmentStart { get; private set; }

        public UInt16 SegmentAddress
        {
            get { return SegmentStart.Segment; }
        }

        public override string ToString()
        {
            return SegmentStart.ToString();
        }
    }
#endif

    enum ListingScope
    {
        /// <summary>
        /// Displays only the current procedure. If this procedure crosses
        /// multiple segments or is not contiguous, display a label to
        /// indicate the discontinuities.
        /// </summary>
        Procedure,

        /// <summary>
        /// Displays only the current segment. If a procedure on this segment
        /// jumps to another segment, that part is not displayed.
        /// </summary>
        Segment,

        /// <summary>
        /// Displays all segments in the current module, in the order of their
        /// segment ID.
        /// </summary>
        Module,
    }
}

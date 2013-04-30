using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Stores the analysis results of an executable image. This class only
    /// takes care of book-keeping; it does not analyze the image. The 
    /// analysis is done by Disassembler.
    /// </summary>
    public class BinaryImage
    {
        private byte[] image;
        private Pointer baseAddress;
        private ByteProperties[] attr;

        private List<BasicBlock> blocks = new List<BasicBlock>();

        /// <summary>
        /// Dictionary that maps the entry point offset of a procedure
        /// to a Procedure object.
        /// </summary>
        private SortedList<int, Procedure> procedures
            = new SortedList<int, Procedure>();

        /// <summary>
        /// Dictionary that maps a 16-bit segment address to a Segment
        /// object. 
        /// </summary>
        private SortedList<UInt16, Segment> segments
            = new SortedList<UInt16, Segment>();

        /// <summary>
        /// Creates a binary image with the given data and base address.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="baseAddress"></param>
        /// <exception cref="ArgumentNullException">If image is null.
        /// </exception>
        public BinaryImage(byte[] image, Pointer baseAddress)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            
            this.image = image;
            this.baseAddress = baseAddress;

            this.attr = new ByteProperties[image.Length];
            for (int i = 0; i < attr.Length; i++)
            {
                attr[i] = new ByteProperties();
            }
        }

        /// <summary>
        /// Gets the underlying executable image.
        /// </summary>
        public byte[] Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the number of bytes in the image.
        /// </summary>
        public int Length
        {
            get { return image.Length; }
        }

        /// <summary>
        /// Returns an object that wraps the byte at the given offset.
        /// </summary>
        /// <param name="offset">Offset of the byte to return.</param>
        /// <returns></returns>
        public ByteProperties this[int offset]
        {
            get
            {
                if (offset < 0 || offset >= attr.Length)
                    throw new ArgumentOutOfRangeException("index");
                return attr[offset];
            }
            set
            {
                if (offset < 0 || offset >= attr.Length)
                    throw new ArgumentOutOfRangeException("index");
                if (value == null)
                    throw new ArgumentNullException("value");

                attr[offset] = value;
            }
        }

        /// <summary>
        /// Returns an object that wraps the byte at the given address.
        /// </summary>
        /// <param name="address">Address of the byte to return.</param>
        /// <returns></returns>
        public ByteProperties this[Pointer address]
        {
            get { return this[PointerToOffset(address)]; }
            set { this[PointerToOffset(address)] = value; }
        }

        /// <summary>
        /// Gets the CS:IP address of the first byte in the image.
        /// </summary>
        public Pointer BaseAddress
        {
            get { return baseAddress; }
        }

        /// <summary>
        /// Converts a CS:IP pointer to its offset within the executable
        /// image. Note that different CS:IP pointers may correspond to the
        /// same offset.
        /// </summary>
        /// <param name="address">The CS:IP address to convert.</param>
        /// <returns>An offset that may be outside the image.</returns>
        public int PointerToOffset(Pointer address)
        {
            return address.LinearAddress - baseAddress.LinearAddress;
        }

        /// <summary>
        /// Decodes an instruction at the given address.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="address">The address to decode.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">If address refers
        /// outside the image.</exception>
        public Instruction DecodeInstruction(Pointer address)
        {
            int offset = PointerToOffset(address);
            if (offset < 0 || offset >= image.Length)
                throw new ArgumentOutOfRangeException("address");

            Instruction instruction = X86Codec.Decoder.Decode(
                image, offset, address, CpuMode.RealAddressMode);
            return instruction;
        }

        public byte[] GetBytes(int offset, int count)
        {
            if (offset < 0 || offset > image.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0 || offset + count > image.Length)
                throw new ArgumentOutOfRangeException("count");

            byte[] result = new byte[count];
            Array.Copy(image, offset, result, 0, count);
            return result;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the given offset.
        /// </summary>
        /// <param name="offset">Offset to read.</param>
        /// <returns>A 16-bit unsigned integer in little endian.
        /// </returns>
        public UInt16 GetUInt16(int offset)
        {
            if (offset < 0 || offset + 1 >= image.Length)
                throw new ArgumentOutOfRangeException("offset");

            return (UInt16)(image[offset] | (image[offset + 1] << 8));
        }

        public bool CheckByteType(Pointer start, Pointer end, ByteType type)
        {
            int pos1 = PointerToOffset(start);
            int pos2 = PointerToOffset(end);
            
            for (int i = pos1; i < pos2; i++)
            {
                if (attr[i].Type != type)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Marks a continuous range of bytes as an atomic item of the given
        /// type.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Piece CreatePiece(Pointer start, Pointer end, ByteType type)
        {
            if (type != ByteType.Code &&
                type != ByteType.Data &&
                type != ByteType.Padding)
                throw new ArgumentException("type is invalid.");

            if (start.Segment != end.Segment)
                throw new ArgumentException("start and end must be in the same segment.");

            // Verify that [start, end) is within the image.
            int pos1 = PointerToOffset(start);
            int pos2 = PointerToOffset(end);
            if (pos1 < 0 || pos1 >= image.Length)
                throw new ArgumentOutOfRangeException("start");
            if (pos2 <= pos1 || pos2 > image.Length)
                throw new ArgumentOutOfRangeException("end");

            // Verify that [start, end) is unoccupied.
            if (!CheckByteType(start, end, ByteType.Unknown))
                throw new ArgumentException("[start, end) overlaps with analyzed bytes.");

            // Create a Piece object for this range of bytes.
            Piece piece = new Piece(this, start, end, type);

            // Mark the byte range as 'type' and associate it with the Piece
            // object.
            for (int i = pos1; i < pos2; i++)
            {
                attr[i].Address = start + (i - pos1);
                attr[i].IsLeadByte = (i == pos1);
                attr[i].Type = type;
                //attr[i].Piece = piece;
            }

            // Update the segment bounds.
            Segment segment = FindSegment(start.Segment);
            if (segment == null)
            {
                segment = new Segment();
                segment.StartAddress = start;
                segment.EndAddress = end;
                segments.Add(start.Segment, segment);
            }
            else
            {
                if (start.LinearAddress < segment.StartAddress.LinearAddress)
                    segment.StartAddress = start;
                if (end.LinearAddress > segment.EndAddress.LinearAddress)
                    segment.EndAddress = end;
            }

            return piece;
        }

        /// <summary>
        /// Gets a collection of analyzed segments. The segments are returned
        /// in order of their 16-bit segment number.
        /// </summary>
        public ICollection<Segment> Segments
        {
            get { return segments.Values; }
        }

        public Segment FindSegment(UInt16 seg)
        {
            Segment segment;
            if (segments.TryGetValue(seg, out segment))
                return segment;
            else
                return null;
        }

        public UInt16 LargestSegmentThatStartsBefore(int offset)
        {
            UInt16 result = 0;
            foreach (Segment segment in Segments)
            {
                if (PointerToOffset(segment.StartAddress) <= offset)
                    result = segment.SegmentAddress;
            }
            return result;
        }

        private bool RangeCoversWholeInstructions(int pos1, int pos2)
        {
            if (!attr[pos1].IsLeadByte)
                return false;

            for (int i = pos1; i < pos2; i++)
            {
                if (attr[i].Type != ByteType.Code)
                    return false;
            }

            if (pos2 < attr.Length &&
                attr[pos2].Type == ByteType.Code &&
                !attr[pos2].IsLeadByte)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a basic block over the given byte range. The basic block
        /// must cover consecutive Piece objects.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public BasicBlock CreateBasicBlock(Pointer start, Pointer end)
        {
            if (start.Segment != end.Segment)
                throw new ArgumentException("A BasicBlock must be on a single segment.");

            int pos1 = PointerToOffset(start);
            int pos2 = PointerToOffset(end);
            if (pos1 < 0 || pos1 >= image.Length)
                throw new ArgumentOutOfRangeException("start");
            if (pos2 <= pos1 || pos2 > image.Length)
                throw new ArgumentOutOfRangeException("end");

            // Verify that the basic block covers continuous code bytes.
            if (!RangeCoversWholeInstructions(pos1, pos2))
            {
                throw new ArgumentException("A basic block must consist of whole instructions.");
            }

            // Verify that no existing basic block overlaps with this range.
            for (int i = pos1; i < pos2; i++)
            {
                if (attr[i].BasicBlock != null)
                    throw new ArgumentException("A existing basic block overlaps with this range.");
            }

            // Update the mapping from byte to basic block.
            BasicBlock block = new BasicBlock(this, start, end);
            for (int i = pos1; i < pos2; i++)
            {
                attr[i].BasicBlock = block;
            }

            // Add the block to our internal list of blocks.
            blocks.Add(block);
            return block;
        }

        public ICollection<Procedure> Procedures
        {
            get { return procedures.Values; }
        }

        public Procedure CreateProcedure(Pointer entryPoint)
        {
            if (FindProcedure(entryPoint) != null)
            {
                throw new InvalidOperationException(
                    "A procedure already exists at the given entry point.");
            }

            Procedure proc = new Procedure(this, entryPoint);
            this.procedures.Add(PointerToOffset(entryPoint), proc);
            return proc;
        }

        /// <summary>
        /// Finds a procedure at the given entry point. Only the absolute
        /// position of the entry point is used; its CS:IP combination 
        /// does not matter.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns></returns>
        public Procedure FindProcedure(Pointer entryPoint)
        {
            Procedure proc;
            if (procedures.TryGetValue(PointerToOffset(entryPoint), out proc))
                return proc;
            else
                return null;
        }

        //class ImageByte : ByteProperties
        //{
        //}
    }

    /// <summary>
    /// Contains information about a byte in a binary image.
    /// </summary>
    public class ByteProperties
    {
        /// <summary>
        /// Gets or sets the type of the byte.
        /// </summary>
        public ByteType Type { get; internal set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether this byte is the first
        /// byte of an instruction or data item.
        /// </summary>
        public bool IsLeadByte { get; internal set; }

        /// <summary>
        /// Gets or sets the CS:IP address that this byte is interpreted as.
        /// </summary>
        public Pointer Address { get; internal set; }

        public BasicBlock BasicBlock { get; internal set; }

        /// <summary>
        /// Gets or sets the procedure that owns this byte.
        /// </summary>
        public Procedure Procedure { get; internal set; }

        //public Piece Piece { get; internal set; }

        public ByteProperties()
        {
            this.Address = Pointer.Invalid;
        }
    }

    /// <summary>
    /// Represents a range of consecutive bytes that constitute a single
    /// instruction or data item.
    /// </summary>
    public class ByteRange
    {
        /// <summary>
        /// Gets or sets the type of the byte range.
        /// </summary>
        public ByteType Type { get; internal set; }

        /// <summary>
        /// Gets or sets the number of (consecutive) bytes in the range.
        /// </summary>
        public int Length { get; internal set; }
    }

#if false
    public struct ByteAttribute
    {
        byte x;

        const byte TypeMask = 3;
        const byte BoundaryBit = 4;
        const byte BlockStartBit = 8;

        /// <summary>
        /// Tests whether the byte has been analyzed.
        /// </summary>
        public bool IsProcessed
        {
            get { return Type == ByteType.Code || Type == ByteType.Data; }
        }

        /// <summary>
        /// Gets or sets the type of the byte.
        /// </summary>
        public ByteType Type
        {
            get { return (ByteType)(x & TypeMask); }
            set
            {
                x = (byte)(x & ~TypeMask | (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether the byte is the first
        /// byte of an instruction or a data item.
        /// </summary>
        public bool IsBoundary
        {
            get { return (x & BoundaryBit) != 0; }
            set
            {
                if (value)
                    x |= BoundaryBit;
                else
                    x = (byte)(x & ~BoundaryBit);
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether the byte is the first
        /// byte of an instruction that starts a basic block.
        /// </summary>
        public bool IsBlockStart
        {
            get { return (x & BlockStartBit) != 0; }
            set
            {
                if (value)
                    x |= BlockStartBit;
                else
                    x = (byte)(x & ~BlockStartBit);
            }
        }
    }
#endif

    /// <summary>
    /// Defines the type of a byte in an executable image.
    /// </summary>
    public enum ByteType
    {
        /// <summary>
        /// The byte is not analyzed and its type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The byte is a padding byte (usually 0x90, NOP) used to align the
        /// next instruction or data item on a word or dword boundary.
        /// </summary>
        Padding,

        /// <summary>
        /// The byte is part of an instruction.
        /// </summary>
        Code,

        /// <summary>
        /// The byte is part of a data item.
        /// </summary>
        Data,
    }

    public class Segment
    {
        public Pointer StartAddress { get; set; }
        public Pointer EndAddress { get; set; }
        public UInt16 SegmentAddress { get { return StartAddress.Segment; } }

#if false
        /// <summary>
        /// Gets the smallest range that covers this segment. The indices
        /// of the range are offsets within the binary image.
        /// </summary>
        // TBD: we need to subtract the BaseAddress!!
        public Range Bounds
        {
            get
            {
                return new Range(
                    StartAddress.EffectiveAddress,
                    EndAddress.EffectiveAddress);
            }
        }
#endif

        public override string ToString()
        {
            return string.Format("{0} - {1}", StartAddress, EndAddress);
        }
    }

    /// <summary>
    /// Represents an instruction or data item that takes up a continous range
    /// of bytes in a binary image.
    /// </summary>
    public class Piece
    {
        //private BinaryImage image;
        public Pointer Start { get; private set; }
        public Pointer End { get; private set; }
        public ByteType Type { get; private set; }

        internal Piece(BinaryImage image, Pointer start, Pointer end, ByteType type)
        {
            this.Start = start;
            this.End = end;
            this.Type = type;
        }
    }

    /// <summary>
    /// Represents a basic block of code.
    /// </summary>
    /// <remarks>
    /// A basic block is a continuous sequence of instructions such that in a
    /// well-behaved program, if any of these instructions is executed, then
    /// all the rest instructions must be executed.
    /// 
    /// For example, a basic block may begin with an instruction that is the
    /// target of a JMP instruction, continue execution for a few 
    /// instructions, and end with another JMP instruction.
    /// 
    /// In a control flow graph, each basic block can be represented by a
    /// node, and the control flow can be expressed as directed edges linking
    /// these nodes.
    /// 
    /// For the purpose in our application, we do NOT terminate a basic block
    /// when we encounter a CALL instruction. This has the benefit that the
    /// resulting control flow graph won't have too many nodes that merely
    /// call another function. 
    /// </remarks>
    public class BasicBlock
    {
        private BinaryImage image;
        private Pointer start;
        private Pointer end;

        /// <summary>
        /// Gets the start address of the basic block.
        /// </summary>
        public Pointer Start
        {
            get { return start; }
            private set { this.start = value; }
        }

        /// <summary>
        /// Gets the end address of the basic block.
        /// </summary>
        public Pointer End
        {
            get { return end; }
            private set { this.end = value; }
        }

        public int Length
        {
            get { return end.LinearAddress - start.LinearAddress; }
        }

        internal BasicBlock(BinaryImage image, Pointer start, Pointer end)
        {
            this.image = image;
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Splits the basic block into two at the given location.
        /// </summary>
        /// <param name="location"></param>
        internal BasicBlock Split(Pointer location)
        {
            if (location.Segment != start.Segment)
                throw new ArgumentException("location must be in the block's segment.", "location");
            if (location.LinearAddress <= start.LinearAddress ||
                location.LinearAddress >= end.LinearAddress)
                throw new ArgumentException("location must be within [start, end).");
            if (!image[location].IsLeadByte)
                throw new ArgumentException("location must be at piece boundary.");

            // Create a new block that covers [location, end).
            BasicBlock newBlock = new BasicBlock(image, location, end);

            // Update the BasicBlock property of bytes in the second block.
            int pos1 = image.PointerToOffset(location);
            int pos2 = image.PointerToOffset(end);
            for (int i = pos1; i < pos2; i++)
            {
                image[i].BasicBlock = newBlock;
            }

            // Update the end position of this block.
            this.end = location;

            return newBlock;
        }
    }

#if false
    /// <summary>
    /// Specifies a location in a binary image, which may be referenced either
    /// by its SEG:OFF address or by its relative position (byte offset) from
    /// the start of the image.
    /// </summary>
    public struct Location
    {
        private BinaryImage image;
        private Pointer address;

        public Pointer Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        public int Position
        {
            get { return address.EffectiveAddress - image.BaseAddress.EffectiveAddress; }
        }

        public Location(BinaryImage image, Pointer address)
        {
            this.image = image;
            this.address = address;
        }
    }
#endif
}

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
        /// Dictionary that maps the entry point (linear) address of a
        /// procedure to a Procedure object.
        /// </summary>
        private SortedList<LinearPointer, Procedure> procedures
            = new SortedList<LinearPointer, Procedure>();

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
            if (baseAddress.LinearAddress.Address + image.Length > 0x100000)
                throw new ArgumentOutOfRangeException("baseAddress");

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
        /// Gets the start address of the loaded image.
        /// </summary>
        public LinearPointer Start
        {
            get { return baseAddress.LinearAddress; }
        }

        /// <summary>
        /// Gets the end address of the loaded image.
        /// </summary>
        public LinearPointer End
        {
            get { return baseAddress.LinearAddress + image.Length; }
        }

        /// <summary>
        /// Gets the number of bytes in the image.
        /// </summary>
        public int Length
        {
            get { return image.Length; }
        }

        /// <summary>
        /// Returns an object that encapsulates the byte at the given address.
        /// </summary>
        /// <param name="address">Address of the byte to return.</param>
        /// <returns></returns>
        public ByteProperties this[LinearPointer address]
        {
            get
            {
                int offset = address - this.Start;
                if (offset < 0 || offset >= attr.Length)
                    throw new ArgumentOutOfRangeException("address");
                return attr[offset];
            }
        }

        /// <summary>
        /// Returns an object that encapsulates the byte at the given address.
        /// </summary>
        /// <param name="address">Address of the byte to return.</param>
        /// <returns></returns>
        public ByteProperties this[Pointer address]
        {
            get { return this[address.LinearAddress]; }
        }

        /// <summary>
        /// Gets the CS:IP address of the first byte in the image.
        /// </summary>
        public Pointer BaseAddress
        {
            get { return baseAddress; }
        }

#if true
        /// <summary>
        /// Converts a CS:IP pointer to its offset within the executable
        /// image. Note that different CS:IP pointers may correspond to the
        /// same offset.
        /// </summary>
        /// <param name="address">The CS:IP address to convert.</param>
        /// <returns>An offset that may be outside the image.</returns>
        private int PointerToOffset(Pointer address)
        {
            return address.LinearAddress - baseAddress.LinearAddress;
        }

        private int PointerToOffset(LinearPointer address)
        {
            return address - baseAddress.LinearAddress;
        }
#endif

        /// <summary>
        /// Decodes an instruction at the given address.
        /// </summary>
        /// <param name="address">The address to decode.</param>
        /// <returns>The decoded instruction.</returns>
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

        public bool IsAddressValid(LinearPointer address)
        {
            return (address >= Start) && (address < End);
        }

        public byte[] GetBytes(LinearPointer address, int count)
        {
            int offset = PointerToOffset(address);
            if (offset < 0 || offset > image.Length)
                throw new ArgumentOutOfRangeException("address");
            if (count < 0 || offset + count > image.Length)
                throw new ArgumentOutOfRangeException("count");

            byte[] result = new byte[count];
            Array.Copy(image, offset, result, 0, count);
            return result;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the given linear address.
        /// </summary>
        /// <param name="address">Linear address to read from.</param>
        /// <returns>A 16-bit unsigned integer in little endian.
        /// </returns>
        public UInt16 GetUInt16(LinearPointer address)
        {
            int offset = PointerToOffset(address);
            if (offset < 0 || offset + 1 >= image.Length)
                throw new ArgumentOutOfRangeException("address");

            return (UInt16)(image[offset] | (image[offset + 1] << 8));
        }

        /// <summary>
        /// Returns true if all the bytes within [start, end) are of type
        /// 'type'.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
            }

            // Update the segment bounds.
            Segment segment = FindSegment(start.Segment);
            if (segment == null)
            {
                segment = new Segment();
                segment.Start = start;
                segment.End = end;
                segments.Add(start.Segment, segment);
            }
            else
            {
                // TODO: modify this to use MultiRange.
                if (start.LinearAddress < segment.Start.LinearAddress)
                    segment.Start = start;
                if (end.LinearAddress > segment.End.LinearAddress)
                    segment.End = end;
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

        /// <summary>
        /// Finds a Segment object with the given segment address.
        /// </summary>
        /// <param name="segmentAddress">16-bit segment address.</param>
        /// <returns>A Segment object if found, null otherwise.</returns>
        public Segment FindSegment(UInt16 segmentAddress)
        {
            Segment segment;
            if (segments.TryGetValue(segmentAddress, out segment))
                return segment;
            else
                return null;
        }

        public UInt16 LargestSegmentThatStartsBefore(LinearPointer address)
        {
            UInt16 result = 0;
            foreach (Segment segment in Segments)
            {
                if (segment.Start.LinearAddress <= address)
                    result = segment.SegmentAddress;
            }
            return result;
        }

        private bool RangeCoversWholeInstructions(LinearPointer startAddress, LinearPointer endAddress)
        {
            int pos1 = PointerToOffset(startAddress);
            int pos2 = PointerToOffset(endAddress);

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
        public BasicBlock CreateBasicBlock(LinearPointer start, LinearPointer end)
        {
            if (start < this.Start || start > this.End)
                throw new ArgumentOutOfRangeException("start");
            if (end < start || end > this.End)
                throw new ArgumentOutOfRangeException("end");

            // Verify that the basic block covers continuous code bytes.
            if (!RangeCoversWholeInstructions(start, end))
            {
                throw new ArgumentException("A basic block must consist of whole instructions.");
            }

            // Verify that no existing basic block overlaps with this range.
            for (var i = start; i < end; i++)
            {
                if (this[i].BasicBlock != null)
                    throw new ArgumentException("A existing basic block overlaps with this range.");
            }

            // Update the mapping from byte to basic block.
            BasicBlock block = new BasicBlock(this, start, end);
            for (var i = start; i < end; i++)
            {
                this[i].BasicBlock = block;
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
            if (FindProcedure(entryPoint.LinearAddress) != null)
            {
                throw new InvalidOperationException(
                    "A procedure already exists at the given entry point.");
            }

            Procedure proc = new Procedure(this, entryPoint);
            this.procedures.Add(entryPoint.LinearAddress, proc);
            return proc;
        }

        /// <summary>
        /// Finds a procedure at the given entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns></returns>
        public Procedure FindProcedure(LinearPointer entryPoint)
        {
            Procedure proc;
            if (procedures.TryGetValue(entryPoint, out proc))
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
        public Pointer Start { get; set; }
        public Pointer End { get; set; }
        public UInt16 SegmentAddress { get { return Start.Segment; } }

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
            return string.Format("{0} - {1}", Start, End);
        }
    }

    /// <summary>
    /// Represents a range of consecutive bytes that constitute a single
    /// instruction or data item.
    /// </summary>
    public class Piece
    {
        //private BinaryImage image;
        public Pointer Start { get; private set; }
        public Pointer End { get; private set; }

        /// <summary>
        /// Gets the type of the piece.
        /// </summary>
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

        public BinaryImage Image { get { return image; } }

        /// <summary>
        /// Gets the start address of the basic block.
        /// </summary>
        public LinearPointer Start { get; private set; }

        /// <summary>
        /// Gets the end address of the basic block.
        /// </summary>
        public LinearPointer End { get; private set; }

        public int Length
        {
            get { return End - Start; }
        }

        internal BasicBlock(BinaryImage image, LinearPointer start, LinearPointer end)
        {
            this.image = image;
            this.Start = start;
            this.End = end;
        }

        /// <summary>
        /// Splits the basic block into two at the given location.
        /// </summary>
        /// <param name="location"></param>
        internal BasicBlock Split(LinearPointer location)
        {
            if (location <= Start || location >= End)
                throw new ArgumentException("location must be within [start, end).");
            if (!image[location].IsLeadByte)
                throw new ArgumentException("location must be at piece boundary.");

            // Create a new block that covers [location, end).
            BasicBlock newBlock = new BasicBlock(image, location, End);

            // Update the BasicBlock property of bytes in the second block.
            for (var i = location; i < End; i++)
            {
                image[i].BasicBlock = newBlock;
            }

            // Update the end position of this block.
            this.End = location;

            return newBlock;
        }
    }

#if false
    /// <summary>
    /// Specifies a location in a binary image, which may be referenced either
    /// by its SEG:OFF address or by its linear address.
    /// </summary>
    public struct Location
    {
        private Pointer address;

        public Pointer Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        public int Position
        {
            get { return address.LinearAddress; }
        }

        public Location(Pointer address)
        {
            this.address = address;
        }
    }
#endif
}

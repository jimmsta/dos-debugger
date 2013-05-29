using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Defines an abstract class that provides methods to access the bytes
    /// in a binary image and keep track of analysis results.
    /// </summary>
    /// <remarks>
    /// This class only takes care of book-keeping; it does not analyze the
    /// image. The analysis is done by DisassemblerBase.
    /// </remarks>
    public abstract class BinaryImage
    {
        public BinaryImage()
        {
        }
        
        /// <summary>
        /// Returns true if the supplied address refers to an accessible byte
        /// in the image.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>true if the address is valid, false otherwise.</returns>
        public abstract bool IsAddressValid(Address address);

        /// <summary>
        /// Returns information about a byte the given address.
        /// </summary>
        /// <param name="address">Address of the byte to return.</param>
        /// <returns></returns>
        public ByteAttribute this[Address address]
        {
            get { return GetByteAttribute(address); }
        }

        protected abstract ByteAttribute GetByteAttribute(Address address);

        protected abstract void SetByteAttribute(Address address, ByteAttribute attr);

        public abstract Instruction GetInstruction(Address address);

        public abstract void SetInstruction(Address address, Instruction instruction);

#if false
        /// <summary>
        /// Decodes an instruction at the given address. In addition to being
        /// a shortcut for X86Codec.Decoder.Decode(...), this method does the
        /// following:
        /// 1. Replace RelativeOperand with SourceAwareRelativeOperand
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

            // If there's already an instruction decoded here, return it.
            if (attr[offset].Instruction != null)
                return attr[offset].Instruction;

            Instruction instruction = X86Codec.Decoder.Decode(
                image, offset, /*address,*/ CpuMode.RealAddressMode);
            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (instruction.Operands[i] is RelativeOperand)
                {
                    instruction.Operands[i] = new SourceAwareRelativeOperand(
                        (RelativeOperand)instruction.Operands[i],
                        address + instruction.EncodedLength);
                }
            }
            return instruction;
        }
#endif

        /// <summary>
        /// Gets the underlying binary data at the given location.
        /// </summary>
        /// <param name="address">Address to return.</param>
        /// <param name="count">Number of bytes to return. Must be within the
        /// segment.</param>
        /// <returns></returns>
        public abstract ArraySegment<byte> GetBytes(Address address, int count);

        /// <summary>
        /// Gets all data within the segment starting at the given address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public abstract ArraySegment<byte> GetBytes(Address address);

        public UInt16 GetUInt16(Address address)
        {
            ArraySegment<byte> x = GetBytes(address, 2);
            return (UInt16)(x.Array[x.Offset] | (x.Array[x.Offset + 1] << 8));
        }

#if false
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
#endif

        /// <summary>
        /// Returns true if all bytes within the given address range are of
        /// the given type.
        /// </summary>
        public bool CheckByteType(Address startAddress, Address endAddress, ByteType type)
        {
            if (startAddress.Segment!=endAddress.Segment)
                throw new ArgumentException("startAddress and endAddress must be on the same segment.");

            for (Address p=startAddress;p!=endAddress;p=p+1)
            {
                if (GetByteAttribute(p).Type != type)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Marks a contiguous range of bytes as the given type, and marks
        /// the first byte in this range as a lead byte.
        /// </summary>
        public void UpdateByteType(Address startAddress, Address endAddress, ByteType type)
        {
            if (startAddress.Segment!=endAddress.Segment)
                throw new ArgumentException("startAddress and endAddress must be on the same segment.");

            if (!CheckByteType(startAddress,endAddress,ByteType.Unknown))
                throw new ArgumentException("[start, end) overlaps with analyzed bytes.");

            for (Address p=startAddress;p!=endAddress;p=p+1)
            {
                ByteAttribute attr=new ByteAttribute
                {
                    Type=type,
                    IsLeadByte=(p==startAddress),
                };
                SetByteAttribute(p,attr);
            }

#if false
            // Update the segment bounds.
            Segment segment = FindSegment(start.Segment);
            if (segment == null)
            {
                segment = new Segment(start.Segment, start.LinearAddress, end.LinearAddress);
                segments.Add(start.Segment, segment);
            }
            else
            {
                // TODO: modify this to use MultiRange.
                segment.Extend(start.LinearAddress, end.LinearAddress);
            }
            return piece;
#endif
        }

#if false
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
            Piece piece = new Piece(this, start.LinearAddress, end.LinearAddress, type);

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
                segment = new Segment(start.Segment, start.LinearAddress, end.LinearAddress);
                segments.Add(start.Segment, segment);
            }
            else
            {
                // TODO: modify this to use MultiRange.
                segment.Extend(start.LinearAddress, end.LinearAddress);
            }

            return piece;
        }

        /// <summary>
        /// Gets a collection of analyzed segments. The segments are returned
        /// in order of their 16-bit segment number.
        /// </summary>
        [Browsable(true)]
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
                if (segment.StartAddress <= address)
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

        public ProcedureCollection Procedures
        {
            get { return procedures; }
        }

        public XRefCollection CrossReferences
        {
            get { return xrefs; }
        }

        //class ImageByte : ByteProperties
        //{
        //}
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This is a bit-field, described as below:
    /// 
    ///   7   6   5   4   3   2   1   0
    /// +---+---+---+---+---+---+---+---+
    /// | - | - | FL| F | L | - |  TYPE |
    /// +---+---+---+---+---+---+---+---+
    /// 
    /// -   : reserved
    /// TYPE: 00 = unknown
    ///       01 = padding
    ///       10 = code
    ///       11 = data
    /// L (LeadByte): 0 = not a lead byte
    ///               1 = is lead byte of code or data
    /// F (Fix-up):   0 = no fix-up info
    ///               1 = has fix-up info
    /// </remarks>
    public struct ByteAttribute
    {
        byte attr;

        public ByteType Type
        {
            get { return (ByteType)(attr & 0x3); }
            set { attr = (byte)((attr & ~3) | ((int)value & 3)); }
        }

        public bool IsLeadByte
        {
            get { return (attr & 0x08) != 0; }
            set
            {
                if (value)
                    attr |= 0x08;
                else
                    attr &= 0xF7;
            }
        }

        public bool HasFixup
        {
            get { return (attr & 0x10) != 0; }
            set
            {
                if (value)
                    attr |= 0x10;
                else
                    attr &= 0xEF;
            }
        }
    }

#if false
    /// <summary>
    /// Provides methods to retrieve the properties of a byte in an image.
    /// This is a wrapper class that is generated on the fly.
    /// </summary>
    public struct ImageByte
    {
        ByteAttribute attr
        readonly byte[] bytes;
        readonly BinaryImage
        readonly int index;

        public ImageByte(ImageChunk image, int index)
        {
            this.image = image;
            this.index = index;
        }

        public byte Value
        {
            get { return image.Data.GetAt(index); }
        }

        public ByteType Type
        {
            get { return image.Attributes.GetAt(index).Type; }
        }

        public bool IsLeadByte
        {
            get { return image.Attributes.GetAt(index).IsLeadByte; }
        }

        //public BasicBlock BasicBlock
        //{
        //    get { return image.BasicBlockMapping.GetValueOrDefault(index); }
        //}

        public Instruction Instruction
        {
            get { return image.Instructions[index]; }
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
        Unknown = 0,

        /// <summary>
        /// The byte is a padding byte (usually 0x90, NOP) used to align the
        /// next instruction or data item on a word or dword boundary.
        /// </summary>
        Padding = 1,

        /// <summary>
        /// The byte is part of an instruction.
        /// </summary>
        Code = 2,

        /// <summary>
        /// The byte is part of a data item.
        /// </summary>
        Data = 3,
    }

#if false
    /// <summary>
    /// Represents a range of consecutive bytes that constitute a single
    /// instruction or data item.
    /// </summary>
    public class Piece : ByteBlock // may rename to Unit, with Location and Length
    {
        //private BinaryImage image;

        /// <summary>
        /// Gets the type of the piece.
        /// </summary>
        public ByteType Type { get; private set; }

        internal Piece(BinaryImage image, LinearPointer start, LinearPointer end, ByteType type)
            : base(start, end)
        {
            this.Type = type;
        }
    }
#endif
}

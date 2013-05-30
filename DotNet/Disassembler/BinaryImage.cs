using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Provides methods to access the bytes in a binary image and keeps
    /// track of related analysis results.
    /// </summary>
    /// <remarks>
    /// The base class keeps track of the following information:
    /// 
    /// - byte attributes (code/data/padding)
    /// - instructions
    /// - basic blocks
    /// - procedures
    /// - analysis errors
    /// 
    /// A derived class must provide the actual image data as well as
    /// segmentation information about the image.
    /// </remarks>
    public abstract class BinaryImage
    {
        readonly SegmentCollection segments = new SegmentCollection();
        readonly InstructionCollection instructions;
        readonly XRefCollection crossReferences = new XRefCollection();
        readonly BasicBlockCollection basicBlocks = new BasicBlockCollection();
        readonly ProcedureCollection procedures = new ProcedureCollection();
        readonly ErrorCollection errors = new ErrorCollection();

        protected BinaryImage()
        {
            this.instructions = new InstructionCollection(this);
        }

        #region Abstract Methods

        /// <summary>
        /// Gets the underlying image data starting from the given address.
        /// </summary>
        /// <param name="address">Address to start return.</param>
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

        #endregion

        #region Overridable Methods

        /// <summary>
        /// Gets a user-friendly display string for the address.
        /// </summary>
        /// <remarks>
        /// It is possible that the offset of the address is not accessible,
        /// e.g. when we format the location of an error. But the segment is
        /// required to be valid.
        /// </remarks>
        public virtual string FormatAddress(Address address)
        {
            return string.Format("seg{0:000}:{1:X4}", 
                address.Segment, address.Offset);
        }

        /// <summary>
        /// Called when the attributes of the given range of bytes have
        /// changed from Unknown to something.
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="endAddress"></param>
        protected virtual void OnBytesAnalyzed(Address startAddress, Address endAddress)
        {
        }

        #endregion

        /// <summary>
        /// Checks whether the supplied address refers to an accessible byte
        /// in the image.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>true if the address is valid, false otherwise.</returns>
        /// <remarks>
        /// This method returns true if the segment index is valid and the
        /// offset is within the OffsetBounds of that segment.
        /// </remarks>
        public bool IsAddressValid(Address address)
        {
            int segmentId = address.Segment;
            if (segmentId < 0 || segmentId >= segments.Count)
                return false;

            Segment segment = segments[segmentId];
            if (!segment.OffsetBounds.Contains(address.Offset))
                return false;

            return true;
        }

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

        #region Properties

        public SegmentCollection Segments
        {
            get { return segments; }
        }

        public InstructionCollection Instructions
        {
            get { return instructions; }
        }

        public XRefCollection CrossReferences
        {
            get { return crossReferences; }
        }

        public BasicBlockCollection BasicBlocks
        {
            get { return basicBlocks; }
        }

        public ProcedureCollection Procedures
        {
            get { return procedures; }
        }

        public ErrorCollection Errors
        {
            get { return errors; }
        }

        #endregion

        // may change to
        // GetSegmentCount();
        // GetSegment(int index);

        // TODO: move this out somewhere
        public UInt16 GetUInt16(Address address)
        {
            ArraySegment<byte> x = GetBytes(address, 2);
            return (UInt16)(x.Array[x.Offset] | (x.Array[x.Offset + 1] << 8));
        }

        /// <summary>
        /// Returns true if all bytes within the given address range are of
        /// the given type. The address range must be on the same segment.
        /// </summary>
        public bool CheckByteType(Address startAddress, Address endAddress, ByteType type)
        {
            if (startAddress.Segment != endAddress.Segment)
                throw new ArgumentException("startAddress and endAddress must be on the same segment.");

            for (Address p = startAddress; p != endAddress; p = p + 1)
            {
                if (GetByteAttribute(p).Type != type)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Marks a contiguous range of bytes as the given type, and marks
        /// the first byte in this range as a lead byte. The address range
        /// must be on the same segment.
        /// </summary>
        public void UpdateByteType(Address startAddress, Address endAddress, ByteType type)
        {
            if (startAddress.Segment != endAddress.Segment)
                throw new ArgumentException("startAddress and endAddress must be on the same segment.");

            if (!CheckByteType(startAddress, endAddress, ByteType.Unknown))
                throw new ArgumentException("[start, end) overlaps with analyzed bytes.");

            for (Address p = startAddress; p != endAddress; p = p + 1)
            {
                ByteAttribute attr = new ByteAttribute
                {
                    Type = type,
                    IsLeadByte = (p == startAddress),
                };
                SetByteAttribute(p, attr);
            }

            OnBytesAnalyzed(startAddress, endAddress);
        }

#if false
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
    /// | - | - | - | - | L | - |  TYPE |
    /// +---+---+---+---+---+---+---+---+
    /// 
    /// -   : reserved
    /// TYPE: 00 = unknown
    ///       01 = padding
    ///       10 = code
    ///       11 = data
    /// L (LeadByte): 0 = not a lead byte
    ///               1 = is lead byte of code or data
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
}

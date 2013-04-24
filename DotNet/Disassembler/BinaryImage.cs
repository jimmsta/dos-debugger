using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
#if false
    public class BinaryImage
    {
        private byte[] image;
        private ByteAttribute[] attr;
        private UInt16[] byteSegment;
        private Pointer baseAddress;

        public BinaryImage(byte[] image, Pointer baseAddress)
        {
            this.image = image;
            this.baseAddress = baseAddress;
            this.attr = new ByteAttribute[image.Length];
            this.byteSegment = new ushort[image.Length]; // TBD
        }

        /// <summary>
        /// Gets the executable image being disassembled.
        /// </summary>
        public byte[] Image
        {
            get { return image; }
        }

        public Pointer BaseAddress
        {
            get { return baseAddress; }
        }
    }
#endif

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

        /// <summary>
        /// Gets or sets the procedure that owns this byte.
        /// </summary>
        public Procedure OwnerProcedure { get; internal set; }
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
        /// The byte is part of an instruction.
        /// </summary>
        Code,

        /// <summary>
        /// The byte is part of a data item.
        /// </summary>
        Data,
    }
}

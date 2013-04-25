using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
#if true
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
            return address - baseAddress;
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

        public bool IsCode { get { return Type == ByteType.Code; } }
        public bool IsData { get { return Type == ByteType.Data; } }

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
        public Procedure Procedure { get; internal set; }
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
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Util;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Contains information about the bytes in the binary image of a segment.
    /// These bytes may contain code, data, and unknown bytes. In particular,
    /// any fix-up information is associated.
    /// </summary>
    public class ImageChunk
    {
        private ArraySegment<byte> image;
        private ArraySegment<ByteAttribute> attrs;

        readonly FixupCollection fixups;
        readonly Dictionary<int, Instruction> instructions;

        public ImageChunk(byte[] bytes, ByteAttribute[] attrs, int startIndex, string name)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            if (attrs == null)
                throw new ArgumentNullException("attrs");
            if (bytes.Length != attrs.Length)
                throw new ArgumentException("bytes and attrs must have the same length.");
            if (startIndex < 0 || startIndex > bytes.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            int n = bytes.Length;
            this.image = new ArraySegment<byte>(bytes, startIndex, n - startIndex);
            this.attrs = new ArraySegment<ByteAttribute>(attrs, startIndex, n - startIndex);
            this.fixups = new FixupCollection();
            this.fixups.Name = name;
            this.instructions = new Dictionary<int, Instruction>();
        }

        public ImageChunk(int length, string name)
            : this(new byte[length], name)
        {
        }

        public ImageChunk(int length)
            : this(new byte[length], "")
        {
        }

        /// <summary>
        /// Creates an image chunk with the supplied binary data.
        /// </summary>
        /// <param name="image"></param>
        public ImageChunk(byte[] image, string name)
            : this(image, new ByteAttribute[image.Length], 0, name)
        {
        }

        // get/set accessible address range

        /// <summary>
        /// Gets the binary image data.
        /// </summary>
        public ArraySegment<byte> Data
        {
            get { return this.image; }
        }

        public ArraySegment<ByteAttribute> Attributes
        {
            get { return this.attrs; }
        }

        public ImageByte this[int index]
        {
            get { return new ImageByte(this, index); }
        }

        public Range<int> Bounds
        {
            get { return new Range<int>(0, image.Count); }
        }

        public int Length
        {
            get { return image.Count; }
        }

        /// <summary>
        /// Gets the fixups defined on this chunk.
        /// </summary>
        public FixupCollection Fixups
        {
            get { return fixups; }
        }

        /// <summary>
        /// Returns true if all the bytes within the given range are of the
        /// given type.
        /// </summary>
        public bool CheckByteType(int offset, int length, ByteType type)
        {
            for (int i = offset; i < offset + length; i++)
            {
                if (attrs.GetAt(i).Type != type)
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
        public void UpdateByteType(int offset, int length, ByteType type)
        {
            if (type != ByteType.Code &&
                type != ByteType.Data &&
                type != ByteType.Padding)
                throw new ArgumentException("type is invalid.", "type");
            if (offset < 0 || offset > this.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0 || length > this.Length - offset)
                throw new ArgumentOutOfRangeException("length");
            if (length == 0)
                return;

#if false
            if (start.Segment != end.Segment)
                throw new ArgumentException("start and end must be in the same segment.");
#endif

            if (!CheckByteType(offset,length,ByteType.Unknown))
                throw new ArgumentException("[start, end) overlaps with analyzed bytes.");

            // Mark the byte range as 'type'.
            for (int i = offset; i < offset + length; i++)
            {
                attrs.SetAt(i, new ByteAttribute
                {
                    Type = type,
                    IsLeadByte = (i == offset),
                });
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

        public Dictionary<int, Instruction> Instructions
        {
            get { return instructions; }
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
    }

    /// <summary>
    /// Provides methods to retrieve the properties of a byte in an image.
    /// This is a wrapper class that is generated on the fly.
    /// </summary>
    public struct ImageByte
    {
        readonly ImageChunk image;
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

#if false
        public Procedure Procedure
        {
            get
            {
                return image.ProcedureMapping.GetValueOrDefault(
                    new Range<int>(index, index + 1));
            }
        }
#endif

        //public BasicBlock BasicBlock
        //{
        //    get { return image.BasicBlockMapping.GetValueOrDefault(index); }
        //}

        public Instruction Instruction
        {
            get { return image.Instructions[index]; }
        }
    }

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

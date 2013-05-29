using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Util;
using Util.Data;
using X86Codec;

namespace Disassembler
{
#if false
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

        /// <summary>
        /// Gets the fixups defined in this library.
        /// </summary>
        public FixupCollection Fixups
        {
            get { return fixups; }
        }

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

        public void AssociateInstruction(int offset, Instruction instruction)
        {
            if (!Bounds.Contains(offset) ||
                !Bounds.Contains(offset + instruction.EncodedLength - 1))
                throw new ArgumentException();

            UpdateByteType(offset, instruction.EncodedLength, ByteType.Code);
            Instructions.Add(offset, instruction);
        }

        public Dictionary<int, Instruction> Instructions
        {
            get { return instructions; }
        }
    }
#endif
}

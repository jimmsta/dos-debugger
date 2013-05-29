using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    public class LibraryImage : BinaryImage
    {
        readonly List<SegmentImage> segmentImages = new List<SegmentImage>();

        public LibraryImage()
        {
        }

        internal List<SegmentImage> SegmentImages
        {
            get { return segmentImages; }
        }

        public override bool IsAddressValid(Address address)
        {
            int segment = address.Segment;
            if (segment < 0 || segment >= segmentImages.Count)
                return false;

            int offset = address.Offset;
            if (offset < 0 || offset >= segmentImages[segment].Length)
                return false;

            return true;
        }

        protected override ByteAttribute GetByteAttribute(Address address)
        {
            return segmentImages[address.Segment].attrs[address.Offset];
        }

        protected override void SetByteAttribute(Address address, ByteAttribute attr)
        {
            segmentImages[address.Segment].attrs[address.Offset] = attr;
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return new ArraySegment<byte>(seg.bytes, address.Offset, count);
        }

        public override ArraySegment<byte> GetBytes(Address address)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return new ArraySegment<byte>(seg.bytes, address.Offset, seg.bytes.Length - address.Offset);
        }

        public override Instruction GetInstruction(Address address)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return seg.instructions[address.Offset];
        }

        public override void SetInstruction(Address address, Instruction instruction)
        {
            SegmentImage seg = segmentImages[address.Segment];
            seg.instructions[address.Offset] = instruction;
        }
    }

    class SegmentImage
    {
        internal readonly byte[] bytes;
        internal readonly ByteAttribute[] attrs;
        internal readonly FixupCollection fixups;
        internal readonly Dictionary<int, Instruction> instructions;

        public SegmentImage(byte[] bytes, string name)
        {
            this.fixups = new FixupCollection();
            this.bytes = bytes;
            this.attrs = new ByteAttribute[bytes.Length];
            this.fixups = new FixupCollection { Name = name };
            this.instructions = new Dictionary<int, Instruction>();
        }

        public int Length
        {
            get { return bytes.Length; }
        }

        public FixupCollection Fixups
        {
            get { return fixups; }
        }
    }
}

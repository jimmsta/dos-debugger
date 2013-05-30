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

        public override IEnumerable<Segment> Segments
        {
            get
            {
                foreach (SegmentImage segmentImage in segmentImages)
                    yield return segmentImage.Segment;
            }
        }

        protected override ByteAttribute GetByteAttribute(Address address)
        {
            return segmentImages[address.Segment].ByteAttributes[address.Offset];
        }

        protected override void SetByteAttribute(Address address, ByteAttribute attr)
        {
            segmentImages[address.Segment].ByteAttributes[address.Offset] = attr;
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return new ArraySegment<byte>(seg.Data, address.Offset, count);
        }

        public override ArraySegment<byte> GetBytes(Address address)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return new ArraySegment<byte>(seg.Data, address.Offset, seg.Data.Length - address.Offset);
        }

        public override Instruction GetInstruction(Address address)
        {
            SegmentImage seg = segmentImages[address.Segment];
            return seg.Instructions[address.Offset];
        }

        public override void SetInstruction(Address address, Instruction instruction)
        {
            SegmentImage seg = segmentImages[address.Segment];
            seg.Instructions[address.Offset] = instruction;
        }

        public override string FormatAddress(Address address)
        {
            // Note: it is possible that an offset is out of bounds, e.g.
            // when we format the location of an error. But we do not
            // allow segment to be out of bounds.
            //if (!IsAddressValid(address))
            //    throw new ArgumentOutOfRangeException("address");

            return string.Format("{0}+{1:X4}",
                segmentImages[address.Segment].Segment.FullName,
                address.Offset);
        }
    }

    class SegmentImage
    {
        readonly LogicalSegment segment;
        readonly ByteAttribute[] attrs;
        readonly Dictionary<int, Instruction> instructions;

        //internal readonly FixupCollection fixups;

        public SegmentImage(LogicalSegment segment)
        {
            this.segment = segment;

            //this.fixups = new FixupCollection();
            //this.bytes = bytes;
            this.attrs = new ByteAttribute[segment.Data.Length];
            //this.fixups = new FixupCollection { Name = name };
            this.instructions = new Dictionary<int, Instruction>();
        }

        public LogicalSegment Segment
        {
            get { return segment; }
        }

        public int Length
        {
            get { return segment.Data.Length; }
        }

        public ByteAttribute[] ByteAttributes
        {
            get { return attrs; }
        }

        public byte[] Data
        {
            get { return segment.Data; }
        }

        public Dictionary<int, Instruction> Instructions
        {
            get { return instructions; }
        }

        //public FixupCollection Fixups
        //{
        //    get { return fixups; }
        //}
    }
}

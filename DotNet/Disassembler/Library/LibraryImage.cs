using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    public class LibraryImage : BinaryImage
    {
        public LibraryImage()
        {
        }

        public LibrarySegment GetSegment(int index)
        {
            return (LibrarySegment)Segments[index];
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            LibrarySegment seg = GetSegment(address.Segment);
            return new ArraySegment<byte>(seg.Data, address.Offset, count);
        }
        
        public override string FormatAddress(Address address)
        {
            if (address.Segment < 0 || address.Segment >= Segments.Count)
                throw new ArgumentOutOfRangeException("address");

            return string.Format("{0}+{1:X4}",
                Segments[address.Segment].Name,
                address.Offset);
        }
    }

    public class LibrarySegment : Segment
    {
        readonly LogicalSegment segment;
        readonly ByteAttribute[] attrs;

        public LibrarySegment(LogicalSegment segment)
        {
            this.segment = segment;
            this.attrs = new ByteAttribute[segment.Data.Length];
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

        int Segment.Id
        {
            get { return segment.Id; }
        }

        string Segment.Name
        {
            get { return segment.FullName; }
        }

        Range<int> Segment.OffsetBounds
        {
            get { return new Range<int>(0, segment.Length); }
        }
    }
}

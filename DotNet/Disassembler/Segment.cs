using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    public class Segment : ByteBlock
    {
        private UInt16 segmentAddress;

        public UInt16 SegmentAddress { get { return segmentAddress; } }

        public Segment(UInt16 segmentAddress)
        {
            this.segmentAddress = segmentAddress;
        }

        public Segment(UInt16 segmentAddress, LinearPointer start, LinearPointer end)
            : base(start, end)
        {
            this.segmentAddress = segmentAddress;
        }

#if false
        public Pointer StartAddress
        {
            get { return new Pointer(segmentAddress, StartAddress); }
        }
#endif

        // class Segment : MultiRange
        // segment.Bounds.Length = ...
        // segment.Parts = MultiRange (AddRange, RemoveRange, etc.)
        // segment.Parts .TotalLength = ...
        // segment.MinimumAddress = SEG:OFF
        // segment.MaximumAddress = SEG:OFF

        public void Extend(LinearPointer start, LinearPointer end)
        {
            base.Extend(new ByteBlock(start, end));
        }

        public override string ToString()
        {
            if (Length == 0)
                return "(empty)";

            Pointer p1 = new Pointer(segmentAddress, StartAddress);
            Pointer p2 = new Pointer(segmentAddress, EndAddress - 1);
            return string.Format("[{0}, {1}]", p1, p2);
        }
    }
}

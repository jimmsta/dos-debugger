using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    public class Segment : ByteRange
    {
        // private Range<LinearPointer> bounds;
        private UInt16 segmentAddress;

        //public Pointer Start { get; set; }
        //public Pointer End { get; set; }
        public UInt16 SegmentAddress { get { return segmentAddress; } }

        // public Range<LinearPointer> Bounds
        // {
        //     get { return bounds; }
        // }

        public Segment(UInt16 segmentAddress)
        {
            this.segmentAddress = segmentAddress;
        }

        public Segment(UInt16 segmentAddress, LinearPointer start, LinearPointer end)
            : base(start, end)
        {
            this.segmentAddress = segmentAddress;
        }

        public Pointer StartAddress
        {
            get { return new Pointer(segmentAddress, Start); }
        }

        // class Segment : MultiRange
        // segment.Bounds.Length = ...
        // segment.Parts = MultiRange (AddRange, RemoveRange, etc.)
        // segment.Parts .TotalLength = ...
        // segment.MinimumAddress = SEG:OFF
        // segment.MaximumAddress = SEG:OFF

#if false
        /// <summary>
        /// Gets the smallest range that covers this segment. The indices
        /// of the range are offsets within the binary image.
        /// </summary>
        // TBD: we need to subtract the BaseAddress!!
        public Range Bounds
        {
            get
            {
                return new Range(
                    StartAddress.EffectiveAddress,
                    EndAddress.EffectiveAddress);
            }
        }
#endif

        public void Extend(LinearPointer start, LinearPointer end)
        {
            if (start < this.Start)
                this.Start = start;
            if (end > this.End)
                this.End = end;
        }

        public override string ToString()
        {
            if (Length == 0)
                return "(empty)";

            Pointer p1 = new Pointer(segmentAddress, Start);
            Pointer p2 = new Pointer(segmentAddress, End - 1);
            return string.Format("[{0}, {1}]", p1, p2);
        }
    }
}

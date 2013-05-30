using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    public class ExecutableImage : BinaryImage
    {
        readonly byte[] bytes;
        readonly ByteAttribute[] attrs;
        readonly int[] relocatableLocations;
        readonly Address entryPoint;

        // Maps a frame number (before relocation) to a segment id.
        readonly SortedList<UInt16, int> mapFrameToSegment
            = new SortedList<UInt16, int>();

#if false
        /// <summary>
        /// Creates an executable image with only one segment and no
        /// relocation information. This is used with COM file images.
        /// </summary>
        /// <param name="bytes"></param>
        public ExecutableImage(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            if (bytes.Length > 0x10000)
                throw new ArgumentException("Image must not exceed 64KB.");

            this.bytes = bytes;
            this.attrs = new ByteAttribute[bytes.Length];

            // Initialize segmentation info.
            this.relocatableLocations = new int[0];
            //this.segments.Add(0, new DummySegment(null, 0)); // TBD: should be 1000?
        }
#endif

        public ExecutableImage(MZFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            this.bytes = file.Image;
            this.attrs = new ByteAttribute[bytes.Length];

            // Store relocatable locations for future use.
            List<int> relocs = new List<int>();
            foreach (FarPointer location in file.RelocatableLocations)
            {
                int index = location.Segment * 16 + location.Offset;
                if (index >= 0 && index < bytes.Length - 1)
                    relocs.Add(index);
            }
            relocs.Sort();
            this.relocatableLocations = relocs.ToArray();

            // Guess segmentation info from the segment values to be
            // relocated. For example, if a relocatable location contains the
            // word 0x1790, it means that 0x1790 will be a segment that will
            // be accessed some time during the program's execution.
            //
            // Although the SEG:OFF addresses of the relocatable locations
            // themselves also provide clue about the program's segmentation,
            // it is less important because they are not directly referenced
            // in the program. Therefore we ignore them for the moment.
            foreach (int index in relocatableLocations)
            {
                UInt16 frame = BitConverter.ToUInt16(bytes, index);
                mapFrameToSegment[frame] = -1;
            }
            mapFrameToSegment[file.EntryPoint.Segment] = -1;

            // Create a dummy segment for each of the guessed segments.
            // Initially, we set segment.OffsetCoverage to an empty range to
            // indicate that we have no knowledge about the start and end
            // offset of each segment.
            for (int i = 0; i < mapFrameToSegment.Count; i++)
            {
                UInt16 frameNumber = mapFrameToSegment.Keys[i];
                ExecutableSegment segment = new ExecutableSegment(this, i, frameNumber);

                // Compute offset bounds for this segment.
                // The lower bound is off course zero.
                // The upper bound is 15 bytes into the next segment.
                int startIndex = frameNumber * 16;
                if (startIndex < bytes.Length)
                {
                    int offsetLowerBound = 0;
                    int offsetUpperBound = Math.Min(bytes.Length - startIndex, 0x10000);
                    if (i < mapFrameToSegment.Count - 1)
                    {
                        offsetUpperBound = Math.Min(
                            offsetUpperBound, 
                            mapFrameToSegment.Keys[i + 1] * 16 + 15 - startIndex);
                    }
                    segment.SetOffsetBounds(
                        new Range<int>(offsetLowerBound, offsetUpperBound));
                }

                mapFrameToSegment[frameNumber] = i;
                base.Segments.Add(segment);
            }

            this.entryPoint = new Address(
                MapFrameToSegment(file.EntryPoint.Segment), file.EntryPoint.Offset);
        }

        public int Length
        {
            get { return bytes.Length; }
        }

        public int[] RelocatableLocations
        {
            get { return relocatableLocations; }
        }

        public Address EntryPoint
        {
            get { return this.entryPoint; }
        }

        public int MapFrameToSegment(UInt16 frameNumber)
        {
            return mapFrameToSegment[frameNumber];
        }

        public bool IsAddressRelocatable(Address address)
        {
            int index = ToLinearAddress(address);
            if (Array.BinarySearch(relocatableLocations, index) >= 0)
                return true;
            else
                return false;
        }

        public override string FormatAddress(Address address)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            return string.Format("{0:X4}:{1:X4}",
                ((ExecutableSegment)base.Segments[address.Segment]).Frame,
                address.Offset);
        }

        private ExecutableSegment GetSegment(int index)
        {
            return (ExecutableSegment)base.Segments[index];
        }

        private int ToLinearAddress(Address address)
        {
            ExecutableSegment segment = (ExecutableSegment)base.Segments[address.Segment];
            return segment.Frame * 16 + address.Offset;
        }

        protected override void OnBytesAnalyzed(Address startAddress, Address endAddress)
        {
            if (startAddress.Segment != endAddress.Segment)
                throw new ArgumentException();

            ExtendSegmentCoverage(startAddress.Segment, startAddress.Offset, endAddress.Offset);
            base.OnBytesAnalyzed(startAddress, endAddress);
        }

        private void ExtendSegmentCoverage(int segmentIndex, int startOffset, int endOffset)
        {
            ExecutableSegment segment = GetSegment(segmentIndex);
            if (segment.OffsetCoverage.IsSupersetOf(new Range<int>(startOffset, endOffset)))
                return;

            // Extend the segment's offset coverage.
            if (segment.OffsetCoverage.IsEmpty)
            {
                segment.OffsetCoverage = new Range<int>(startOffset, endOffset);
            }
            else
            {
                segment.OffsetCoverage = new Range<int>(
                    Math.Min(segment.OffsetCoverage.Begin, startOffset),
                    Math.Max(segment.OffsetCoverage.End, endOffset));
            }

            // Shrink the offset bounds of its neighboring segments.
            if (segmentIndex > 0)
            {
                ExecutableSegment segBefore = GetSegment(segmentIndex - 1);
                int numBytesOverlap = 
                    (segBefore.Frame * 16 + segBefore.OffsetBounds.End) -
                    (segment.Frame * 16 + segment.OffsetCoverage.Begin);
                if (numBytesOverlap > 0)
                {
                    segBefore.SetOffsetBounds(new Range<int>(
                        segBefore.OffsetBounds.Begin,
                        segBefore.OffsetBounds.End - numBytesOverlap));
                }
            }
            if (segmentIndex < base.Segments.Count - 1)
            {
                ExecutableSegment segAfter = GetSegment(segmentIndex + 1);
                int numBytesOverlap =
                    (segment.Frame * 16 + segment.OffsetCoverage.End) -
                    (segAfter.Frame * 16 + segAfter.OffsetBounds.Begin);
                if (numBytesOverlap > 0)
                {
                    segAfter.SetOffsetBounds(new Range<int>(
                        segAfter.OffsetBounds.Begin + numBytesOverlap,
                        segAfter.OffsetBounds.End));
                }
            }
        }

        public byte[] Data
        {
            get { return bytes; }
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, count);
        }
    }

    /// <summary>
    /// Contains information about a segment in an executable file.
    /// </summary>
    public class ExecutableSegment : Segment
    {
        readonly ExecutableImage image;
        readonly UInt16 frameNumber;
        readonly int id;

        private Range<int> offsetBounds;
        private Range<int> offsetCoverage;

        public ExecutableSegment(ExecutableImage image, int id, UInt16 frameNumber)
        {
            this.id = id;
            this.image = image;
            this.frameNumber = frameNumber;
        }

        public int Id
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return frameNumber.ToString("X4"); }
        }

        public Range<int> OffsetBounds
        {
            get { return offsetBounds; }
        }

        internal void SetOffsetBounds(Range<int> bounds)
        {
            this.offsetBounds = bounds;
        }

        /// <summary>
        /// Gets or sets the range of offsets that are analyzed.
        /// </summary>
        public Range<int> OffsetCoverage
        {
            get { return this.offsetCoverage; }
            set { this.offsetCoverage = value; }
        }

        /// <summary>
        /// Gets the frame number of the canonical frame of this segment,
        /// relative to the beginning of the executable image.
        /// </summary>
        public UInt16 Frame
        {
            get { return frameNumber; }
        }

        public override string ToString()
        {
            return string.Format(
                "seg{0:000}: {1:X4}:{2:X4}-{1:X4}:{3:X4}",
                Id, frameNumber, 
                OffsetBounds.Begin, OffsetBounds.End - 1);
        }
    }
}

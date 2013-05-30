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

        // Maps a frame number (before relocation) to a segment id.
        readonly SortedList<UInt16, DummySegment> segments =
            new SortedList<UInt16, DummySegment>();

        readonly Dictionary<Address, Instruction> instructions =
            new Dictionary<Address, Instruction>();

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
                segments[frame] = null;
            }
            segments[file.EntryPoint.Segment] = null;

            // Create a dummy segment for each of the guessed segments.
            // Initially, we set segment.OffsetCoverage to an empty range to
            // indicate that we have no knowledge about the start and end
            // offset of each segment.
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                UInt16 frameNumber = segments.Keys[i];
                DummySegment segment = new DummySegment(this, i, frameNumber);

                // Compute offset bounds for this segment.
                // The lower bound is off course zero.
                // The upper bound is 15 bytes into the next segment.
                int startIndex = frameNumber * 16;
                if (startIndex < bytes.Length)
                {
                    int offsetLowerBound = 0;
                    int offsetUpperBound = Math.Min(bytes.Length - startIndex, 0x10000);
                    if (i < segments.Count - 1)
                    {
                        offsetUpperBound = Math.Min(
                            offsetUpperBound, segments.Keys[i + 1] * 16 + 15 - startIndex);
                    }
                    segment.OffsetBounds = new Range<int>(
                        offsetLowerBound, offsetUpperBound);
                }

                segments[frameNumber] = segment;
            }
        }

        public int Length
        {
            get { return bytes.Length; }
        }

        public int[] RelocatableLocations
        {
            get { return relocatableLocations; }
        }

        public override IEnumerable<Segment> Segments
        {
            get
            {
                foreach (DummySegment segment in this.segments.Values)
                    yield return segment;
            }
        }

        public int MapFrameToSegment(UInt16 frameNumber)
        {
            return segments[frameNumber].Id;
        }

        public bool IsAddressRelocatable(Address address)
        {
            int index = ToLinearAddress(address);
            if (Array.BinarySearch(relocatableLocations, index) >= 0)
                return true;
            else
                return false;
        }

        private int ToLinearAddress(Address address)
        {
            return segments.Values[address.Segment].Frame * 16 + address.Offset;
        }

        public override bool IsAddressValid(Address address)
        {
            int segmentId = address.Segment;
            if (segmentId < 0 || segmentId >= segments.Count)
                return false;

            DummySegment segment = segments.Values[segmentId];
            if (!segment.OffsetBounds.Contains(address.Offset))
                return false;

            return true;
        }

        protected override ByteAttribute GetByteAttribute(Address address)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            return attrs[ToLinearAddress(address)];
        }

        protected override void SetByteAttribute(Address address, ByteAttribute attr)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            ExtendSegmentCoverage(address);

            attrs[ToLinearAddress(address)] = attr;
        }

        private void ExtendSegmentCoverage(Address address)
        {
            DummySegment segment = segments.Values[address.Segment];
            int offset = address.Offset;

            if (segment.OffsetCoverage.Contains(offset))
                return;

            // Extend the segment's offset coverage.
            if (segment.OffsetCoverage.IsEmpty)
            {
                segment.OffsetCoverage = new Range<int>(offset, offset + 1);
            }
            else
            {
                segment.OffsetCoverage = new Range<int>(
                    Math.Min(segment.OffsetCoverage.Begin, offset),
                    Math.Max(segment.OffsetCoverage.End, offset + 1));
            }

            // Shrink the offset bounds of its neighboring segments.
            int i = segment.Id;
            if (i > 0)
            {
                DummySegment segBefore = segments.Values[i - 1];
                int numBytesOverlap = 
                    (segBefore.Frame * 16 + segBefore.OffsetBounds.End) -
                    (segment.Frame * 16 + segment.OffsetCoverage.Begin);
                if (numBytesOverlap > 0)
                {
                    segBefore.OffsetBounds = new Range<int>(
                        segBefore.OffsetBounds.Begin,
                        segBefore.OffsetBounds.End - numBytesOverlap);
                }
            }
            if (i < segments.Count - 1)
            {
                DummySegment segAfter = segments.Values[i + 1];
                int numBytesOverlap =
                    (segment.Frame * 16 + segment.OffsetCoverage.End) -
                    (segAfter.Frame * 16 + segAfter.OffsetBounds.Begin);
                if (numBytesOverlap > 0)
                {
                    segAfter.OffsetBounds = new Range<int>(
                        segAfter.OffsetBounds.Begin + numBytesOverlap,
                        segAfter.OffsetBounds.End);
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

        public override ArraySegment<byte> GetBytes(Address address)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");

            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, bytes.Length - index);
        }

        public override Instruction GetInstruction(Address address)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");
            return instructions[address];
        }

        public override void SetInstruction(Address address, Instruction instruction)
        {
            if (!IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");
            instructions[address] = instruction;
        }
    }

    public class DummySegment : Segment
    {
        readonly ExecutableImage image;
        readonly UInt16 frameNumber;

        //UInt16 minOffset; 
        //UInt16 maxOffset;

        public DummySegment(ExecutableImage image, int id, UInt16 frameNumber)
        {
            base.Id = id;
            this.image = image;
            this.frameNumber = frameNumber;
            
        }

        public override string Name
        {
            get { return frameNumber.ToString("X4"); }
        }

        public override Range<int> OffsetRange
        {
            get { return OffsetBounds; }
        }

        public Range<int> OffsetBounds { get; set; }

        /// <summary>
        /// Gets or sets the range of offsets that are analyzed.
        /// </summary>
        public Range<int> OffsetCoverage { get; set; }

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

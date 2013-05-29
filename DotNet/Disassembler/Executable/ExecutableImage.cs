using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    public class ExecutableImage : BinaryImage
    {
        readonly byte[] bytes;
        readonly ByteAttribute[] attrs;
        readonly int[] relocatableLocations;

        // Maps a segment number (before relocation) to a dummy segment.
        readonly SortedList<UInt16, DummySegment> segments =
            new SortedList<UInt16, DummySegment>();

        readonly Dictionary<Address, Instruction> instructions =
            new Dictionary<Address, Instruction>();

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
            this.segments.Add(0, new DummySegment(null, 0)); // TBD: should be 1000?
        }

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
            // Initially, we set the ActualSize of each segment to zero,
            // which indicates that we have no knowledge about the start
            // and end offset of each segment.
            for (int i = 0; i < segments.Count; i++)
            {
                UInt16 frameNumber = segments.Keys[i];
                DummySegment segment = new DummySegment(this, frameNumber);
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

        private static int ToLinearAddress(Address address)
        {
            return address.Segment * 16 + address.Offset;
        }

        public override bool IsAddressValid(Address address)
        {
            int index = ToLinearAddress(address);
            return (index >= 0) && (index < bytes.Length);
        }

        protected override ByteAttribute GetByteAttribute(Address address)
        {
            return attrs[ToLinearAddress(address)];
        }

        protected override void SetByteAttribute(Address address, ByteAttribute attr)
        {
            attrs[ToLinearAddress(address)] = attr;
        }

        public byte[] Data
        {
            get { return bytes; }
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            // TODO: we need to maintain the segments here.

            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, count);
        }

        public override ArraySegment<byte> GetBytes(Address address)
        {
            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, bytes.Length - index);
        }

        public override Instruction GetInstruction(Address address)
        {
            return instructions[address];
        }

        public override void SetInstruction(Address address, Instruction instruction)
        {
            instructions[address] = instruction;
        }
    }

    public class DummySegment : Segment
    {
        readonly ExecutableImage image;
        readonly UInt16 frameNumber;

        public DummySegment(ExecutableImage image, UInt16 frameNumber)
        {
            this.image = image;
            this.frameNumber = frameNumber;
        }

        /// <summary>
        /// Gets the frame number of the canonical frame of this segment,
        /// relative to the beginning of the executable image.
        /// </summary>
        public UInt16 Frame
        {
            get { return frameNumber; }
        }
    }
}

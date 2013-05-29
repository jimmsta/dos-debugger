using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    public class Executable : Assembly
    {
        private byte[] data;
        private Address entryPoint;
        private int[] relocatableLocations;

        // Maps a segment number (before relocation) to a dummy segment.
        readonly SortedList<UInt16, DummySegment> segments =
            new SortedList<UInt16, DummySegment>();

        readonly ExecutableImage image;

        public Executable(string fileName)
        {
            MZFile file = new MZFile(fileName);

            this.image = new ExecutableImage(file.Image);

            // Each relocation entry provides a hint of the program's
            // segmentation.
            List<int> relocs = new List<int>();
            foreach (FarPointer location in file.RelocatableLocations)
            {
                int index = location.LinearAddress;
                if (index >= 0 && index < image.Length - 1)
                {
                    relocs.Add(index);
                    segments[location.Segment] = null;
                    UInt16 target = BitConverter.ToUInt16(image.Data, index);
                    segments[target] = null;
                }
            }
            if (file.EntryPoint.LinearAddress < image.Length)
            {
                segments[file.EntryPoint.Segment] = null;
            }

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

            relocs.Sort();
            relocatableLocations = relocs.ToArray();

            this.data = file.Image;
            this.entryPoint = PointerToAddress(file.EntryPoint);
        }

        public BinaryImage Image
        {
            get { return image; }
        }

        private static Address PointerToAddress(FarPointer pointer)
        {
            return new Address(pointer.Segment, pointer.Offset);
        }

        public override Segment GetSegment(int segmentSelector)
        {
            DummySegment segment;
            if (segments.TryGetValue((UInt16)segmentSelector, out segment))
                return segment;
            else
                return null;
        }

        /// <summary>
        /// Gets the entry point address of the executable.
        /// </summary>
        public Address EntryPoint
        {
            get { return entryPoint; }
        }

        public int[] RelocatableLocations
        {
            get { return relocatableLocations; }
        }
    }

    public class DummySegment : Segment
    {
        readonly Executable executable;
        readonly UInt16 frameNumber;

        public DummySegment(Executable executable, UInt16 frameNumber)
        {
            this.executable = executable;
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

#if false
    public class LoadModule : Module
    {
        ImageChunk image;

        public LoadModule(ImageChunk image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this.image = image;
        }

        /// <summary>
        /// Gets the binary image of the load module.
        /// </summary>
        public ImageChunk Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets or sets the initial value of SS register. This value must be
        /// relocated when the image is loaded.
        /// </summary>
        public UInt16 InitialSS { get; set; }

        /// <summary>
        /// Gets or sets the initial value of SP register.
        /// </summary>
        public UInt16 InitialSP { get; set; }

        /// <summary>
        /// Gets or sets the initial value of CS register. This value must be
        /// relocated when the image is loaded.
        /// </summary>
        public UInt16 InitialCS { get; set; }

        /// <summary>
        /// Gets or sets the initial value of IP register.
        /// </summary>
        public UInt16 InitialIP { get; set; }
    }
#endif
}

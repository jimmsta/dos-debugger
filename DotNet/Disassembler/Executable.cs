using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    public class Executable : Assembly
    {
        private byte[] data;
        private Address entryPoint;
        private LoadModule loadModule;
        readonly SortedList<UInt16, DummySegment> segments =
            new SortedList<UInt16, DummySegment>();

        public Executable(string fileName)
        {
            MZFile file = new MZFile(fileName);

            // Each relocation entry provides a hint of the program's
            // segmentation.
            byte[] image = file.Image;
            foreach (FarPointer location in file.RelocatableLocations)
            {
                segments[location.Segment] = null;
                int index = location.Segment * 16 + location.Offset;
                UInt16 target = BitConverter.ToUInt16(image, index);
                segments[target] = null;
            }
            segments[file.EntryPoint.Segment] = null;
            segments[file.StackTop.Segment] = null;

            // Create a dummy segment for each of the guessed segments.
            // Initially, we set the ActualSize of each segment to zero,
            // which indicates that we have no knowledge about the start
            // and end offset of each segment.
            for (int i = 0; i < segments.Count; i++)
            {
                UInt16 frameNumber = segments.Keys[i];
                DummySegment segment = new DummySegment(this);
                segment.Frame = frameNumber;
                segment.Id = segments.Count + 1;
                segments[frameNumber] = segment;
            }

            this.data = file.Image;
            this.entryPoint = new Address(file.EntryPoint.Segment, file.EntryPoint.Offset);
            this.loadModule = new LoadModule(new ImageChunk(data, "LoadModule"));
        }

        public override ImageChunk GetSegment(int segmentSelector)
        {
            return this.segments[(UInt16)segmentSelector].Image;
        }

        public LoadModule LoadModule
        {
            get { return LoadModule; }
        }

        /// <summary>
        /// Gets the entry point address of the executable.
        /// </summary>
        public Address EntryPoint
        {
            get { return entryPoint; }
        }
    }

    public class DummySegment : Segment
    {
        Executable executable;

        public DummySegment(Executable executable)
        {
            this.executable = executable;
        }

        /// <summary>
        /// Gets the frame number of the canonical frame of this segment,
        /// relative to the beginning of the executable image.
        /// </summary>
        public UInt16 Frame { get; set; }

        public override ImageChunk Image
        {
            get { throw new NotImplementedException(); }
        }
    }

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
}

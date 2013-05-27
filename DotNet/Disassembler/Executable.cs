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

        byte[] image;
        ImageChunk.ByteAttribute[] attrs;

        public Executable(string fileName)
        {
            MZFile file = new MZFile(fileName);

            this.image = file.Image;
            this.attrs = new ImageChunk.ByteAttribute[image.Length];

            // Each relocation entry provides a hint of the program's
            // segmentation.
            foreach (FarPointer location in file.RelocatableLocations)
            {
                segments[location.Segment] = null;
                int index = location.Segment * 16 + location.Offset;
                UInt16 target = BitConverter.ToUInt16(image, index);
                segments[target] = null;
            }
            segments[file.EntryPoint.Segment] = null;

            // Create a dummy segment for each of the guessed segments.
            // Initially, we set the ActualSize of each segment to zero,
            // which indicates that we have no knowledge about the start
            // and end offset of each segment.
            for (int i = 0; i < segments.Count; i++)
            {
                UInt16 frameNumber = segments.Keys[i];
                ImageChunk chunk = new ImageChunk(image, attrs, frameNumber * 16, "");
                DummySegment segment = new DummySegment(this, chunk);
                segment.Frame = frameNumber;
                segment.Id = segments.Count + 1;
                segments[frameNumber] = segment;
            }

            this.data = file.Image;
            this.entryPoint = new Address(file.EntryPoint.Segment, file.EntryPoint.Offset);
            this.loadModule = new LoadModule(new ImageChunk(data, "LoadModule"));
        }

        public override Segment GetSegment(int segmentSelector)
        {
            return this.segments[(UInt16)segmentSelector];
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
        ImageChunk image;

        public DummySegment(Executable executable, ImageChunk image)
        {
            this.executable = executable;
            this.image = image;
        }

        /// <summary>
        /// Gets the frame number of the canonical frame of this segment,
        /// relative to the beginning of the executable image.
        /// </summary>
        public UInt16 Frame { get; set; }

        public override ImageChunk Image
        {
            get { return image; }
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

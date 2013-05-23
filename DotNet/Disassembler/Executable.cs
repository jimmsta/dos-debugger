using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2
{
    public class Executable : Assembly
    {
        public LoadModule LoadModule { get; set; }

        /// <summary>
        /// Gets the entry point address of the executable.
        /// </summary>
        public Address EntryPoint
        {
            get
            {
                throw new NotImplementedException();
            }
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

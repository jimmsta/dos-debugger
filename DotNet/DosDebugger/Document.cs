using System;
using System.Collections.Generic;
using System.Text;
using Disassembler;

namespace DosDebugger
{
    class Document
    {
        private Disassembler16 dasm;
        private NavigationPoint<Pointer> nav = new NavigationPoint<Pointer>();
        private BinaryImage image;

        public Disassembler16 Disassembler
        {
            // get { return dasm; }
            set
            {
                dasm = value;
                image = dasm.Image;
            }
        }

        public BinaryImage Image
        {
            get { return image; }
            set { image = value; }
        }

        public NavigationPoint<Pointer> Navigator
        {
            get { return nav; }
        }
    }
}

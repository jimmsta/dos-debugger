using System;
using System.Collections.Generic;
using System.Text;
using Disassembler;
using X86Codec;

namespace DosDebugger
{
    class Document
    {
        private Disassembler16 dasm;
        private NavigationPoint<Pointer> nav = new NavigationPoint<Pointer>();

        public Disassembler16 Disassembler
        {
            get { return dasm; }
            set { dasm = value; }
        }

        public NavigationPoint<Pointer> Navigator
        {
            get { return nav; }
        }
    }
}

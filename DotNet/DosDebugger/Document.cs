using System;
using System.Collections.Generic;
using System.Text;
using Disassembler;

namespace DosDebugger
{
    class Document
    {
        private Disassembler16 dasm;

        public Disassembler16 Disassembler
        {
            get { return dasm; }
            set { dasm = value; }
        }

    }
}

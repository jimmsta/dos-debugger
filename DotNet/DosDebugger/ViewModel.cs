using System;
using System.Collections.Generic;
using System.Text;
using Disassembler;

namespace DosDebugger
{
    class ViewModel
    {
        private Disassembler16 dasm;

        public Disassembler16 Disassembler
        {
            get { return dasm; }
            set { dasm = value; }
        }

        public void ActivateProcedure(Procedure proc)
        {
        }
    }
}

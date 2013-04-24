using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Represents a procedure in an executable.
    /// </summary>
    public class Procedure
    {
        private Pointer entryPoint;
        private MultiRange codeRange = new MultiRange();
        private MultiRange dataRange = new MultiRange();
        private MultiRange byteRange = new MultiRange();

        /// <summary>
        /// Gets or sets the entry point address of the procedure.
        /// </summary>
        public Pointer EntryPoint
        {
            get { return this.entryPoint; }
            set { this.entryPoint = value; }
        }

        public MultiRange CodeRange
        {
            get { return codeRange; }
        }

        public MultiRange DataRange
        {
            get { return dataRange; }
        }

        public MultiRange ByteRange
        {
            get { return byteRange; }
        }
    }

    public class ProcedureEntryPointComparer : IComparer<Procedure>
    {
        public int Compare(Procedure x, Procedure y)
        {
            return x.EntryPoint.CompareTo(y.EntryPoint);
        }
    }
}

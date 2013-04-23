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
        private Range codeRange = new Range();
        private Range dataRange = new Range();
        private Range byteRange = new Range();

        /// <summary>
        /// Gets or sets the entry point address of the procedure.
        /// </summary>
        public Pointer EntryPoint
        {
            get { return this.entryPoint; }
            set { this.entryPoint = value; }
        }

        public Range CodeRange
        {
            get { return codeRange; }
        }

        public Range DataRange
        {
            get { return dataRange; }
        }

        public Range ByteRange
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

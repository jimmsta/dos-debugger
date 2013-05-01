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
        private BinaryImage image;
        private Range bounds;

        //private MultiRange codeRange = new MultiRange();
        //private MultiRange dataRange = new MultiRange();
        //private MultiRange byteRange = new MultiRange();

        public Procedure(BinaryImage image, Pointer entryPoint)
        {
            this.image = image;
            this.EntryPoint = entryPoint;
        }

        /// <summary>
        /// Gets the entry point address of the procedure.
        /// </summary>
        public Pointer EntryPoint { get; private set; }

        public Range Bounds
        {
            get { return bounds; }
        }

#if false
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
#endif

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddBasicBlock(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            // Verify that the bytes have not been assigned to any procedure.
            LinearPointer pos1 = block.Start.LinearAddress;
            LinearPointer pos2 = block.End.LinearAddress;
            for (LinearPointer i = pos1; i < pos2; i++)
            {
                if (image[i].Procedure != null)
                    throw new InvalidOperationException("Some of the bytes are already assigned to a procedure.");
            }

            // Assign the bytes to this procedure.
            for (var i = pos1; i < pos2; i++)
            {
                image[i].Procedure = this;
            }

            // Update the bounds of this procedure.
            if (bounds.Length == 0)
            {
                bounds = new Range(pos1.Address, pos2.Address);
            }
            else
            {
                bounds = new Range(
                    Math.Min(pos1.Address, bounds.Begin),
                    Math.Max(pos2.Address, bounds.End));
            }
        }

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddDataBlock(Pointer start, Pointer end)
        {
            var pos1 = start.LinearAddress;
            var pos2 = end.LinearAddress;
            for (var i = pos1; i < pos2; i++)
            {
                if (image[i].Procedure != null)
                    throw new InvalidOperationException("Some of the bytes are already assigned to a procedure.");
            }

            // Assign the bytes to this procedure.
            for (var i = pos1; i < pos2; i++)
            {
                image[i].Procedure = this;
            }

            // Update the bounds of this procedure.
            if (bounds.Length == 0)
            {
                bounds = new Range(pos1.Address, pos2.Address);
            }
            else
            {
                bounds = new Range(
                    Math.Min(pos1.Address, bounds.Begin),
                    Math.Max(pos2.Address, bounds.End));
            }
        }
    }

#if false
    public class ProcedureEntryPointComparer : IComparer<Procedure>
    {
        public int Compare(Procedure x, Procedure y)
        {
            return x.EntryPoint.CompareTo(y.EntryPoint);
        }
    }
#endif
}

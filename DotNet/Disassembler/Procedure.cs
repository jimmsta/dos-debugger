using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Contains information about a procedure in an executable.
    /// </summary>
    public class Procedure : ByteBlock
    {
        private BinaryImage image;

        //private Range<LinearPointer> bounds;
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

#if false
        public Range<LinearPointer> Bounds
        {
            get { return bounds; }
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
#endif

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        // TODO: what to do if the block is Split ?
        public void AddBasicBlock(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            // Verify that the bytes have not been assigned to any procedure.
            LinearPointer pos1 = block.StartAddress;
            LinearPointer pos2 = block.EndAddress;
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
            this.Extend(block);
        }

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddDataBlock(LinearPointer start, LinearPointer end)
        {
            for (var i = start; i < end; i++)
            {
                if (image[i].Procedure != null)
                    throw new InvalidOperationException("Some of the bytes are already assigned to a procedure.");
            }

            // Assign the bytes to this procedure.
            for (var i = start; i < end; i++)
            {
                image[i].Procedure = this;
            }

            // Update the bounds of this procedure.
            this.Extend(new ByteBlock(start, end));
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

using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Represents a continuous address range of bytes, identified by a start
    /// address (inclusive) and an end address (exclusive). It is not required
    /// that all bytes within this address range logically belong to the
    /// block; instead, the block merely serves as an address boundary.
    /// </summary>
    public class ByteBlock
    {
        /// <summary>
        /// Gets the start address of the range. This address is included in
        /// the range.
        /// </summary>
        public LinearPointer StartAddress { get; protected set; }

        /// <summary>
        /// Gets the end address of the range. This address is excluded from
        /// the range.
        /// </summary>
        public LinearPointer EndAddress { get; protected set; }

        /// <summary>
        /// Gets the number of bytes in the range.
        /// </summary>
        public int Length { get { return EndAddress - StartAddress; } }

        /// <summary>
        /// Returns true if this byte block is empty.
        /// </summary>
        public bool IsEmpty { get { return StartAddress == EndAddress; } }

        /// <summary>
        /// Create an empty range with start and end addresses set to zero.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public ByteBlock()
        {
        }

        /// <summary>
        /// Create a range with the given start and end addresses.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public ByteBlock(LinearPointer start, LinearPointer end)
        {
            if (start > end)
                throw new ArgumentException("start must be less than or equal to end.");

            this.StartAddress = start;
            this.EndAddress = end;
        }

        protected void Extend(ByteBlock block)
        {
            if (block.Length > 0)
            {
                if (this.Length == 0)
                {
                    this.StartAddress = block.StartAddress;
                    this.EndAddress = block.EndAddress;
                }
                else
                {
                    if (block.StartAddress < this.StartAddress)
                        this.StartAddress = block.StartAddress;
                    if (block.EndAddress > this.EndAddress)
                        this.EndAddress = block.EndAddress;
                }
            }
        }
    }
}

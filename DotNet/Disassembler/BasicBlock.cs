using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;

namespace Disassembler2
{
    /// <summary>
    /// Represents a basic block of code.
    /// </summary>
    /// <remarks>
    /// A basic block is a contiguous sequence of instructions such that in a
    /// well-behaved program, if any of these instructions is executed, then
    /// all the rest instructions must be executed.
    /// 
    /// For example, a basic block may begin with an instruction that is the
    /// target of a JMP instruction, continue execution for a few 
    /// instructions, and end with another JMP instruction.
    /// 
    /// In a control flow graph, each basic block can be represented by a
    /// node, and the control flow can be expressed as directed edges linking
    /// these nodes.
    /// 
    /// For the purpose in our application, we do NOT terminate a basic block
    /// when we encounter a CALL instruction. This has the benefit that the
    /// resulting control flow graph won't have too many nodes that merely
    /// call another function. 
    /// </remarks>
    public class BasicBlock
    {
        private ImageChunk image;
        private Range<int> location;

        public ImageChunk Image { get { return image; } }

        internal BasicBlock(ImageChunk image, Range<int> location)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (!image.Bounds.IsSupersetOf(location))
                throw new ArgumentOutOfRangeException("range");

            this.image = image;
            this.location = location;
        }

        public Range<int> Location
        {
            get { return location; }
        }

        /// <summary>
        /// Splits the basic block into two at the given position.
        /// </summary>
        /// <param name="location"></param>
        /// TODO: how does this sync with Procedure.BasicBlocks?
        internal BasicBlock Split(int position)
        {
            if (position <= location.Begin || position >= location.End)
                throw new ArgumentOutOfRangeException("position");
            if (!image[position].IsLeadByte)
                throw new ArgumentException("position must be a lead byte.");

            // Create a new block that covers [location, end).
            BasicBlock newBlock = new BasicBlock(
                image, new Range<int>(position, location.End));

#if false
            // Update the BasicBlock property of bytes in the second block.
            for (var i = location; i < EndAddress; i++)
            {
                image[i].BasicBlock = newBlock;
            }
#endif

            // Update the end position of this block.
            location = new Range<int>(location.Begin, position);

            return newBlock;
        }
    }
}

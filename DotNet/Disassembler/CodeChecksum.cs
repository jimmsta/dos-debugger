using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Computes the checksum of a block of code for the purpose of library
    /// function recognition.
    /// </summary>
    public class CodeChecksum
    {
        byte[] opcodeChecksum;

        private CodeChecksum(byte[] opcodeChecksum)
        {
            this.opcodeChecksum = opcodeChecksum;
        }

        public byte[] OpcodeChecksum
        {
            get { return opcodeChecksum; }
        }

        public static CodeChecksum Compute(Procedure procedure, BinaryImage image)
        {
            using (HashAlgorithm hasher = MD5.Create())
            {
                ComputeMore(hasher, procedure, image);
                hasher.TransformFinalBlock(new byte[0], 0, 0);
                return new CodeChecksum(hasher.Hash);
            }
        }

        private static void ComputeMore(
            HashAlgorithm hasher, Procedure procedure, BinaryImage image)
        {
            // TODO: add the traversal logic into Graph class.
            // or maybe GraphAlgorithms.Traversal(...).

            // Create a queue to simulate breadth-first-search. It doesn't
            // really matter whether DFS or BFS is used as long as we stick
            // to it, but BFS has the benefit that it's easier to understand.
            // Therefore we use it.
            Queue<Address> queue = new Queue<Address>();
            queue.Enqueue(procedure.EntryPoint);

            // Map the entry point address of a basic block to its index
            // in the sequence of blocks visited. Each block that is the
            // target of a none-fall-through control flow edge is assigned
            // an index the first time it is encountered. This index is
            // included in the hash to provide a hint of the graph's
            // structure.
            Dictionary<Address, int> visitOrder = new Dictionary<Address, int>();

            XRefCollection cfg = image.BasicBlocks.ControlFlowGraph.Graph;

            // Traverse the graph.
            while (queue.Count > 0)
            {
                Address source = queue.Dequeue();

                // Check if this block has been visited before. If it has,
                // we just hash its order and work on next one.
                int order;
                if (visitOrder.TryGetValue(source, out order)) // visited
                {
                    ComputeMore(hasher, order);
                    continue;
                }

                // If the block has not been visited, assign a unique order
                // to it, and hash this order.
                order = visitOrder.Count;
                visitOrder.Add(source, order);
                ComputeMore(hasher, order);

                // Next, we hash the instructions in the block. We follow any
                // fall-through edges so that the resulting hash will not be
                // affected by artificial blocks. To see this, consider the
                // following example:
                //
                // MySub:                LibSub1:
                //       mov ax, bx            mov ax, bx
                //                       LibSub2:
                //       mov bx, cx            mov bx, cx
                //       ret                   ret
                //
                // MySub and LibSub1 are identical procedures with three
                // instructions. However, if someone calls into the middle of
                // LibSub1, the block must be split in two and a fall-through
                // edge is created from LibSub1 to LibSub2. If we don't follow
                // the fall-through edge, it will generate a different hash
                // from the left-side one.
                while (true)
                {
                    BasicBlock block = image.BasicBlocks.Find(source);
                    System.Diagnostics.Debug.Assert(block != null);

                    // Hash the instructions in the block. Only the opcode
                    // part of each instruction is hashed; the displacement
                    // and immediate parts are potentially subject to fix-up,
                    // and are therefore ignored in the hash.
                    ComputeMore(hasher, block, image);

                    // Enumerate each block referred to from this block.
                    // We must order the (none-fall-through) outgoing flow
                    // edges in a way that depends only on the graph's
                    // structure and not on the particular arrangement of
                    // target blocks. (Note: this is not a concern if we only
                    // have one none-fall-through outgoing edge; but this may
                    // be of concern if we have multiple outgoing edges, such
                    // as in an indexed jump.)
                    //
                    // TBD: handle multiple outgoing edges.
                    XRef fallThroughEdge = null;
                    XRef nonFallThroughEdge = null;
                    foreach (XRef flow in cfg.GetReferencesFrom(source))
                    {
                        if (flow.Type == XRefType.FallThrough)
                        {
                            if (fallThroughEdge != null)
                                throw new InvalidOperationException("Cannot have more than one fall-through edge.");
                            fallThroughEdge = flow;
                        }
                        else
                        {
                            if (nonFallThroughEdge != null)
                                throw new InvalidOperationException("Cannot have more than one non-fall-through edge.");
                            nonFallThroughEdge = flow;
                        }
                    }

                    // Hash the special flow type and add target to queue.
                    if (nonFallThroughEdge != null)
                    {
                        ComputeMore(hasher, (int)nonFallThroughEdge.Type);
                        queue.Enqueue(nonFallThroughEdge.Target);
                    }
                    
                    // Fall through to the next block if any.
                    if (fallThroughEdge != null)
                    {
                        source = fallThroughEdge.Target;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void ComputeMore(HashAlgorithm hasher, int data)
        {
            // TODO: make this thread local to save the byte array allocation.
            byte[] bytes = BitConverter.GetBytes(data);
            hasher.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
        }

        /// <summary>
        /// Computes the checksum of a basic block.
        /// </summary>
        private static void ComputeMore(
            HashAlgorithm hasher, BasicBlock basicBlock, BinaryImage image)
        {
            ArraySegment<byte> code = image.GetBytes(basicBlock.Location, basicBlock.Length);
            int index = code.Offset;
            // TODO: maybe we should subclass X86Codec.Instruction to provide
            // rich functionalities???
            foreach (Instruction instruction in basicBlock.GetInstructions(image))
            {
                ComputeMore(hasher, code.Array, index, instruction);
                index += instruction.EncodedLength;
            }
        }

        private static void ComputeMore(
            HashAlgorithm hasher, byte[] code, int startIndex, IEnumerable<Instruction> instructions)
        {
            if (instructions == null)
                throw new ArgumentNullException("instructions");

            // Hash the opcode part of each instruction in the sequence.
            int index = startIndex;
            foreach (Instruction instruction in instructions)
            {
                ComputeMore(hasher, code, index, instruction);
                index += instruction.EncodedLength;
            }
        }

        private static void ComputeMore(
            HashAlgorithm hasher, byte[] code, int startIndex, Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            // Find the opcode part.
            // TODO: in X86Codec, since a fixable location always comes after
            // prefix+opcode+modrm+sib, we should put the fixable location as
            // a property of the instruction instead of the operand.
            int opcodeLength = instruction.EncodedLength;
            foreach (Operand operand in instruction.Operands)
            {
                if (operand.FixableLocation.Length > 0)
                    opcodeLength = Math.Min(opcodeLength, operand.FixableLocation.StartOffset);
            }

            // Since the opcode uniquely determines the displacement and
            // immediate format, we only need to hash the opcode part and
            // don't need to hash dummy zeros for the remaining part of the
            // instruction.
            hasher.TransformBlock(code, startIndex, opcodeLength, code, startIndex);
        }
    }
}

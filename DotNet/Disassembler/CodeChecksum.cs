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

            // Create a stack to simulate depth-first-search. It doesn't
            // matter whether DFS or BFS is used as long as we use the same
            // traversal order for library function and executable function.
            // Since DFS is simpler (can use deque), we use it.
            Stack<Address> queue = new Stack<Address>();
            queue.Push(procedure.EntryPoint);

            // Map the entry point address of a basic block to its index
            // in the sequence of blocks visited.
            Dictionary<Address, int> visitedOrder = new Dictionary<Address, int>();

            // Traverse the graph.
            while (queue.Count > 0)
            {
                Address source = queue.Pop();
                BasicBlock block = image.BasicBlocks.Find(source);
                System.Diagnostics.Debug.Assert(block != null);

                // Find the visit order of this block, and hash this order.
                int order;
                if (visitedOrder.TryGetValue(source, out order)) // visited
                {
                    ComputeMore(hasher, order);
                    continue;
                }
                order = visitedOrder.Count;
                visitedOrder.Add(source, order);
                ComputeMore(hasher, order);

                // Hash the instructions in the basic block. Only the opcode
                // part of each instruction is hashed; the displacement and
                // immediate parts are potentially subject to fix-up, and are
                // therefore ignored in the hash.
                ComputeMore(hasher, block, image);

                // Enumerate each block referred to from this block.
                // We must order the outgoing edges by type to make sure
                // the traversal produce the same order every time.
                // If the outgoing edges are of the same type, we
                // order them by their hash values. (Note: this will lead to 
                // recursion) or alternatively we throw an exception.
                //
                // TODO: control flow graph should order the edges by
                // xref type already.
                XRefCollection cfg = image.BasicBlocks.ControlFlowGraph.Graph;
                List<XRef> flows = new List<XRef>(cfg.GetReferencesFrom(block.Location));
                flows.Sort(XRef.CompareByPriority); // may rename to CompareByType

                foreach (XRef flow in flows)
                {
                    // Hash the flow type.
                    ComputeMore(hasher, (int)flow.Type);

                    // Add the target address into the queue.
                    queue.Push(flow.Target);
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

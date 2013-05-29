using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
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
    /// 
    /// A basic block is always contained in a single segment.
    /// </remarks>
    public class BasicBlock
    {
        readonly Address location;
        readonly int length;
        readonly BasicBlockType type;
        readonly CodeFeatures features;

#if false
        internal BasicBlock(ImageChunk image, Range<int> range)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (!image.Bounds.IsSupersetOf(range))
                throw new ArgumentOutOfRangeException("range");

            this.location = new Address(image, range.Begin);
            this.length = range.End - range.Begin;
        }
#endif

        public BasicBlock(Address begin, Address end, BasicBlockType type, BinaryImage image)
        {
            if (begin.Segment != end.Segment)
                throw new ArgumentException("Basic block must be on the same segment.");
            this.location = begin;
            this.length = end.Offset - begin.Offset;
            this.type = type;
            this.features = ComputeFeatures(image, begin, end);
        }

        public Address Location
        {
            get { return location; }
        }

        public int Length
        {
            get { return length; }
        }

        public BasicBlockType Type
        {
            get { return type; }
        }

        public Range<Address> Bounds
        {
            get { return new Range<Address>(location, location + length); }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Bounds, Type);
        }

        public CodeFeatures Features
        {
            get { return features; }
        }

        private static CodeFeatures ComputeFeatures(
            BinaryImage image, Address startAddress, Address endAddress)
        {
            CodeFeatures features = CodeFeatures.None;
            for (Address p = startAddress; p!=endAddress;)
            {
                Instruction instruction = image.GetInstruction(p);

                switch (instruction.Operation)
                {
                    case Operation.INT:
                    case Operation.INTO:
                        features |= CodeFeatures.HasInterrupt;
                        break;
                    case Operation.RET:
                        features |= CodeFeatures.HasRETN;
                        break;
                    case Operation.RETF:
                        features |= CodeFeatures.HasRETF;
                        break;
                    case Operation.IRET:
                        features |= CodeFeatures.HasIRET;
                        break;
                    case Operation.FCLEX:
                        features |= CodeFeatures.HasFpu;
                        break;
                }

                p += instruction.EncodedLength;
            }
            return features;
        }
    }

    /// <summary>
    /// Specifies the type of a basic block.
    /// </summary>
    public enum BasicBlockType
    {
        Unknown = 0,

        /// <summary>
        /// Indicates that the basic block ends prematurally because of an
        /// error encountered during analysis.
        /// </summary>
        Broken,

        /// <summary>
        /// Indicates that the basic block ends because the instruction
        /// following it is some jump target and hence starts another block.
        /// </summary>
        FallThrough,

        /// <summary>
        /// Indicates that the basic block ends because of an unconditional
        /// JMP/JMPF instruction.
        /// </summary>
        Jump,

        /// <summary>
        /// Indicates that the basic block ends because of a branch
        /// instruction, such as Jcc, LOOPZ, etc.
        /// </summary>
        Branch,

        /// <summary>
        /// Indicates that the basic block ends because of a CALL/CALLF/INT
        /// instruction.
        /// </summary>
        Call,

        /// <summary>
        /// Indicates that the basic block ends because of a RET/RETF/IRET
        /// instruction.
        /// </summary>
        Return,

        /// <summary>
        /// Indicates that the basic block ends becuase of a HLT instruction.
        /// </summary>
        Halt,
    }

    public class BasicBlockCollection : ICollection<BasicBlock>
    {
        readonly List<BasicBlock> blocks = new List<BasicBlock>();
        readonly RangeDictionary<Address, BasicBlock> map =
            new RangeDictionary<Address, BasicBlock>();
        readonly XRefCollection controlFlowGraph = new XRefCollection();

        public BasicBlockCollection()
        {
        }

        public void Add(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            if (blocks.Contains(block))
                throw new ArgumentException("Block already exists in the collection.");

            this.blocks.Add(block);
            this.map.Add(block.Bounds, block);
        }

        /// <summary>
        /// Finds a basic block that covers the given address. Returns null
        /// if the address is not covered by any basic block.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public BasicBlock Find(Address address)
        {
            return map.GetValueOrDefault(
                new Range<Address>(address, address + 1));
        }

        /// <summary>
        /// Splits an existing basic block into two. This basic block must
        /// be in the collection.
        /// </summary>
        /// <param name="block"></param>
        public BasicBlock[] SplitBasicBlock(BasicBlock block, Address cutoff, BinaryImage image)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            if (!block.Bounds.Contains(cutoff))
                throw new ArgumentOutOfRangeException("cutoff");
            if (cutoff == block.Location)
                return null;

            int k = blocks.IndexOf(block);
            if (k < 0)
                throw new ArgumentException("Block must be within the collection.");

            // Create two blocks.
            var range = block.Bounds;
            BasicBlock block1 = new BasicBlock(range.Begin, cutoff, BasicBlockType.FallThrough, image);
            BasicBlock block2 = new BasicBlock(cutoff, range.End, block.Type, image);

            // Replace the big block from this collection and add the newly
            // created smaller blocks.
            blocks[k] = block1;
            blocks.Add(block2);

            // Update lookup map.
            map.Remove(block.Bounds);
            map.Add(block1.Bounds, block1);
            map.Add(block2.Bounds, block2);

            // Return the two basic blocks.
            return new BasicBlock[2] { block1, block2 };
        }

        #region ICollection Interface Implementation

        public void Clear()
        {
            blocks.Clear();
            map.Clear();
            controlFlowGraph.Clear();
        }

        public bool Contains(BasicBlock item)
        {
            if (item == null)
                return false;
            else
                return Find(item.Location) == item;
        }

        public void CopyTo(BasicBlock[] array, int arrayIndex)
        {
            blocks.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return blocks.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(BasicBlock item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<BasicBlock> GetEnumerator()
        {
            return blocks.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void AddControlFlowGraphEdge(
            BasicBlock source, BasicBlock target, XRef xref)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            if (xref == null)
                throw new ArgumentNullException("xref");

            // TODO: verify that the basic blocks exist in this collection.
            XRef xFlow = new XRef(
                type: xref.Type,
                source: source.Location,
                target: target.Location,
                dataLocation: xref.Source
            );
            controlFlowGraph.Add(xFlow);
        }

        public IEnumerable<BasicBlock> GetSuccessors(BasicBlock source)
        {
            foreach (XRef xref in controlFlowGraph.GetReferencesFrom(source.Location))
            {
                yield return Find(xref.Target);
            }
        }
    }
}

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
    // TODO: we might want to make BasicBlockInfo struct to store, and then
    //       make BasicBlock a wrapper around it with rich functionalities.
    public class BasicBlock
    {
        readonly Address location;
        readonly int length;
        readonly BasicBlockType type;
        readonly CodeFeatures features;
        
        public BasicBlock(Address begin, Address end, BasicBlockType type, BinaryImage image)
        {
            if (begin.Segment != end.Segment)
                throw new ArgumentException("Basic block must be on the same segment.");
            this.location = begin;
            this.length = end.Offset - begin.Offset;
            this.type = type;
            this.features = CodeFeaturesHelper.GetFeatures(GetInstructions(image));
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

        public IEnumerable<Instruction> GetInstructions(BinaryImage image)
        {
            for (Address p = this.location; p != this.location + length; )
            {
                Instruction instruction = image.Instructions.Find(p);
                yield return instruction;
                p += instruction.EncodedLength;
            }
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
        public static int TimesSplit = 0;

        // Consider using a specialized AddressDictionary or
        // AddressSortedDictionary, or TwoTierDictionary, or
        // TwoTierSortedDictionary, to make the implementation
        // cleaner.

        // Use a two-tier data structure to store the blocks.
        // First, since a basic block must be on a single segment,
        // we group the blocks by segments.
        // Then, we use a SortedList<int, int> to map the starting offset
        // of a block to the block's index in 'blocks'. It would have been
        // better to use SortedDictionary for better insertion, but
        // unfortunately that one doesn't support binary search, so
        // there is no way to quickly find a block that covers a byte.
        readonly List<SortedList<int, int>> map = 
            new List<SortedList<int, int>>();

        // Keep a list of blocks so that we don't have to implement
        // IEnumerable ourselves. Strictly speaking this is redundant.
        readonly List<BasicBlock> blocks = new List<BasicBlock>();

        readonly ControlFlowGraph controlFlowGraph;

        public BasicBlockCollection()
        {
            controlFlowGraph = new ControlFlowGraph(this);
        }

        public bool Contains(BasicBlock item)
        {
            if (item == null)
                return false;

            int segment = item.Location.Segment;
            if (segment < 0 || segment >= map.Count)
                return false;

            int index;
            if (!map[segment].TryGetValue(item.Location.Offset, out index))
                return false;

            return (blocks[index] == item);
        }

        public void Add(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            if (this.Contains(block))
                throw new ArgumentException("Block already exists in the collection.");

            int segment = block.Location.Segment;
            while (segment >= map.Count)
                map.Add(new SortedList<int, int>());

            int index = this.blocks.Count;
            this.blocks.Add(block);
            this.map[segment].Add(block.Location.Offset, index);
        }

        /// <summary>
        /// Finds a basic block that covers the given address. Returns null
        /// if the address is not covered by any basic block.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public BasicBlock Find(Address address)
        {
            int segment = address.Segment;
            if (segment < 0 || segment >= map.Count)
                return null;

            int k = map[segment].Keys.BinarySearch(address.Offset, Comparer<int>.Default.Compare);
            if (k >= 0)
                return blocks[map[segment].Values[k]];

            k = ~k;
            if (k == 0)
                return null;

            BasicBlock block = blocks[map[segment].Values[k - 1]];
            if (block.Bounds.Contains(address))
                return block;
            else
                return null;
        }

        /// <summary>
        /// Splits an existing basic block into two. This basic block must
        /// be in the collection.
        /// </summary>
        /// <param name="block"></param>
        public BasicBlock[] SplitBasicBlock(BasicBlock block, Address cutoff, BinaryImage image)
        {
            ++TimesSplit;
            if (block == null)
                throw new ArgumentNullException("block");
            if (!block.Bounds.Contains(cutoff))
                throw new ArgumentOutOfRangeException("cutoff");
            if (cutoff == block.Location)
                return null;

            int segment = block.Location.Segment;
            if (segment < 0 || segment >= map.Count)
                throw new ArgumentException("Block must be within the collection.");

            int index;
            if (!map[segment].TryGetValue(block.Location.Offset, out index))
                throw new ArgumentException("Block must be within the collection.");

            // Create two blocks.
            var range = block.Bounds;
            BasicBlock block1 = new BasicBlock(range.Begin, cutoff, BasicBlockType.FallThrough, image);
            BasicBlock block2 = new BasicBlock(cutoff, range.End, block.Type, image);

            // Replace the big block from this collection and add the newly
            // created smaller blocks.
            blocks[index] = block1;
            blocks.Add(block2);

            // Update lookup map.
            map[segment].Add(block2.Location.Offset, blocks.Count - 1);

            // Return the two basic blocks.
            return new BasicBlock[2] { block1, block2 };
        }

        #region ICollection Interface Implementation

        public void Clear()
        {
            blocks.Clear();
            map.Clear();
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

        public ControlFlowGraph ControlFlowGraph
        {
            get { return controlFlowGraph; }
        }
    }
}

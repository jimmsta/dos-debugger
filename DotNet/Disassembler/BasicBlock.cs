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
        readonly ResolvedAddress location;
        readonly int length;

        internal BasicBlock(ImageChunk image, Range<int> range)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (!image.Bounds.IsSupersetOf(range))
                throw new ArgumentOutOfRangeException("range");

            this.location = new ResolvedAddress(image, range.Begin);
            this.length = range.End - range.Begin;
        }

        public ResolvedAddress Location
        {
            get { return location; }
        }

        public Range<int> Bounds
        {
            get { return new Range<int>(location.Offset, location.Offset + length); }
        }

        public int Length
        {
            get { return length; }
        }
    }

    public class BasicBlockCollection : ICollection<BasicBlock>
    {
        readonly List<BasicBlock> blocks = new List<BasicBlock>();
        readonly Dictionary<ImageChunk, RangeDictionary<int, BasicBlock>> map =
            new Dictionary<ImageChunk, RangeDictionary<int, BasicBlock>>();
        readonly XRefCollection controlFlowGraph = new XRefCollection();

        public BasicBlockCollection()
        {
        }

        private RangeDictionary<int, BasicBlock> GetSubMap(ImageChunk image, bool autoAdd)
        {
            RangeDictionary<int, BasicBlock> subMap;
            if (!map.TryGetValue(image, out subMap) && autoAdd)
            {
                subMap = new RangeDictionary<int, BasicBlock>(0, image.Length);
                map.Add(image, subMap);
            }
            return subMap;
        }

        private RangeDictionary<int, BasicBlock> GetSubMap(BasicBlock block)
        {
            return GetSubMap(block.Location.Image, true);
        }

        public void Add(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            if (blocks.Contains(block))
                throw new ArgumentException("Block already exists.");

            this.blocks.Add(block);

            GetSubMap(block).Add(block.Bounds, block);
        }

        public BasicBlock Find(ResolvedAddress address)
        {
            var subMap = GetSubMap(address.Image, false);
            if (subMap != null)
                return subMap.GetValueOrDefault(new Range<int>(address.Offset, address.Offset + 1));
            else
                return null;
        }

        /// <summary>
        /// Splits an existing basic block into two. This basic block must
        /// be in the collection.
        /// </summary>
        /// <param name="block"></param>
        public void SplitBasicBlock(BasicBlock block, ResolvedAddress cutoff)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            if (cutoff.Image != block.Location.Image)
                throw new ArgumentException("Cutoff position must be within the block.");

            Range<int> range = new Range<int>(
                block.Location.Offset, block.Location.Offset + block.Length);
            int pos = cutoff.Offset;
            if (pos <= range.Begin || pos >= range.End)
                throw new ArgumentOutOfRangeException("cutoff");
            if (!cutoff.ImageByte.IsLeadByte)
                throw new ArgumentException("cutoff must be a lead byte.");

            int k = blocks.IndexOf(block);
            if (k < 0)
                throw new ArgumentException("Block must be within the collection.");

            // Create two blocks.
            Range<int> range1 = new Range<int>(range.Begin, pos);
            Range<int> range2 = new Range<int>(pos, range.End);
            BasicBlock block1 = new BasicBlock(block.Location.Image, range1);
            BasicBlock block2 = new BasicBlock(block.Location.Image, range2);

            // Remove the big block from this collection and add the newly
            // created smaller blocks.
            blocks[k] = block1;
            blocks.Add(block2);

            // Update lookup map.
            var subMap = GetSubMap(block);
            subMap.Remove(block.Bounds);
            subMap.Add(block1.Bounds, block1);
            subMap.Add(block2.Bounds, block2);
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
            return blocks.Contains(item);
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

            throw new NotImplementedException();
#if false
            // TODO: verify that the basic blocks exist in this collection.
            XRef xFlow = new XRef(
                type: xref.Type,
                source: source.Location,
                target: target.Location,
                dataLocation: xref.Source
            );
            controlFlowGraph.Add(xFlow);
#endif
        }
    }
}

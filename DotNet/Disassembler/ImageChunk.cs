using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler2
{
    /// <summary>
    /// Contains information about a contiguous chunk of bytes in a binary
    /// image. The bytes may contain code, data, and unknown bytes. In
    /// particular, any fix-up information is associated.
    /// </summary>
    public class ImageChunk
    {
        byte[] image;
        ByteAttribute[] attrs;
        FixupCollection fixups;
        RangeDictionary<int, Procedure> procedures;
        RangeDictionary<int, BasicBlock> basicBlocks;

        public ImageChunk(int length)
            : this(new byte[length])
        {
        }

        /// <summary>
        /// Creates an image chunk with the supplied binary data.
        /// </summary>
        /// <param name="image"></param>
        public ImageChunk(byte[] image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this.image = image;
            this.attrs = new ByteAttribute[image.Length];
            this.fixups = new FixupCollection();
            this.procedures = new RangeDictionary<int, Procedure>(0, image.Length);
            this.basicBlocks = new RangeDictionary<int, BasicBlock>(0, image.Length);
        }

        /// <summary>
        /// Gets the binary image data.
        /// </summary>
        public byte[] Data
        {
            get { return image; }
        }

        public ByteAttribute[] Attributes
        {
            get { return attrs; }
        }

        public ImageByte this[int index]
        {
            get { return new ImageByte(this, index); }
        }

        public int Length
        {
            get { return image.Length; }
        }

        /// <summary>
        /// Gets the fixups defined on this chunk.
        /// </summary>
        public FixupCollection Fixups
        {
            get { return fixups; }
        }

        /// <summary>
        /// Decodes an instruction at the given offset, applying associated
        /// fix-up information if present.
        /// </summary>
        /// <returns>The decoded instruction.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If offset refers to
        /// a location outside of the image.</exception>
        public Instruction DecodeInstruction(int offset)
        {
            if (offset < 0 || offset >= image.Length)
                throw new ArgumentOutOfRangeException("offset");

            Instruction instruction = X86Codec.Decoder.Decode(
                image, offset, CpuMode.RealAddressMode);

            // Find the first fixup that covers the instruction. If no
            // fix-up covers the instruction, find the closest fix-up
            // that comes after.
            int fixupIndex = fixups.BinaryLocate(offset);
            if (fixupIndex < 0)
                fixupIndex = ~fixupIndex;

            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (fixupIndex >= fixups.Count) // no more fixups
                    break;

                Fixup fixup = fixups[fixupIndex];
                if (fixup.StartIndex >= offset + instruction.EncodedLength) // past end
                    break;

                Operand operand = instruction.Operands[i];
                if (operand is RelativeOperand)
                {
                    RelativeOperand opr = (RelativeOperand)operand;
                    int start = offset + opr.Offset.Location.StartOffset;
                    int end = start + opr.Offset.Location.Length;

                    if (fixup.StartIndex >= end)
                        continue;

                    if (fixup.StartIndex != start || fixup.EndIndex != end)
                        throw new BrokenFixupException(fixup);

                    instruction.Operands[i] = new SymbolicRelativeOperand(fixup.Target);
                    ++fixupIndex;

                    //instruction.Operands[i] = new SourceAwareRelativeOperand(
                    //    (RelativeOperand)instruction.Operands[i],
                    //    address + instruction.EncodedLength);
                }
            }

            if (fixupIndex < fixups.Count &&
                fixups[fixupIndex].StartIndex < offset + instruction.EncodedLength)
            {
                throw new BrokenFixupException(fixups[fixupIndex]);
            }
            return instruction;
        }

        public RangeDictionary<int, Procedure> Procedures
        {
            get { return this.procedures; }
        }

        public RangeDictionary<int, BasicBlock> BasicBlocks
        {
            get { return this.basicBlocks; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This is a bit-field, described as below:
        /// 
        ///   7   6   5   4   3   2   1   0
        /// +---+---+---+---+---+---+---+---+
        /// | - | - | FL| F | L | - |  TYPE |
        /// +---+---+---+---+---+---+---+---+
        /// 
        /// -   : reserved
        /// TYPE: 00 = unknown
        ///       01 = padding
        ///       10 = code
        ///       11 = data
        /// L (LeadByte): 0 = not a lead byte
        ///               1 = is lead byte of code or data
        /// F (Fix-up):   0 = no fix-up info
        ///               1 = has fix-up info
        /// </remarks>
        public struct ByteAttribute
        {
            byte attr;

            public ByteType Type
            {
                get { return (ByteType)(attr & 0x3); }
                set { attr = (byte)((attr & ~3) | ((int)value & 3)); }
            }

            public bool IsLeadByte
            {
                get { return (attr & 0x08) != 0; }
                set
                {
                    if (value)
                        attr |= 0x08;
                    else
                        attr &= 0xF7;
                }
            }

            public bool HasFixup
            {
                get { return (attr & 0x10) != 0; }
                set
                {
                    if (value)
                        attr |= 0x10;
                    else
                        attr &= 0xEF;
                }
            }
        }

        /// <summary>
        /// Provides methods to access the fixups defined on this image chunk.
        /// </summary>
        public class FixupCollection : IList<Fixup>
        {
            List<Fixup> fixups;
            bool done;

            public FixupCollection()
            {
                this.fixups = new List<Fixup>();
                this.done = false;
            }

            public void Seal()
            {
                if (!done)
                {
                    fixups.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
                    for (int i = 1; i < fixups.Count; i++)
                    {
                        if (fixups[i - 1].EndIndex > fixups[i].StartIndex)
                            throw new InvalidOperationException("Some fixups overlap.");
                    }
                    done = true;
                }
            }

            /// <summary>
            /// Finds the fixup associated with the given position. If no
            /// fixup is found, find the first one that comes after that
            /// position.
            /// </summary>
            /// <param name="offset"></param>
            /// <returns></returns>
            public int BinaryLocate(int offset)
            {
                //fixups.BinarySearch(
                //return fixups[offset];
                throw new NotImplementedException();
            }

            public int IndexOf(Fixup item)
            {
                throw new NotSupportedException();
            }

            public void Insert(int index, Fixup item)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public Fixup this[int index]
            {
                get
                {
                    if (!done)
                        throw new InvalidOperationException("Cannot access the collection until Seal() is called.");
                    return fixups[index];
                }
                set { throw new NotSupportedException(); }
            }

            public void Add(Fixup item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");
                fixups.Add(item);
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(Fixup item)
            {
                throw new NotSupportedException();
            }

            public void CopyTo(Fixup[] array, int arrayIndex)
            {
                if (!done)
                {
                    throw new InvalidOperationException("Cannot access the collection until Seal() is called.");
                }
                fixups.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return fixups.Count; }
            }

            public bool IsReadOnly
            {
                get
                {
                    if (done)
                        return true;
                    else
                        return false;
                }
            }

            public bool Remove(Fixup item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<Fixup> GetEnumerator()
            {
                if (!done)
                {
                    throw new InvalidOperationException("Cannot access the collection until Seal() is called.");
                }   
                return fixups.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    /// <summary>
    /// Provides methods to retrieve the properties of a byte in an image.
    /// This is a wrapper class that is generated on the fly.
    /// </summary>
    public class ImageByte
    {
        ImageChunk image;
        int index;

        public ImageByte(ImageChunk image, int index)
        {
            this.image = image;
            this.index = index;
        }

        public byte Value
        {
            get { return image.Data[index]; }
        }

        public ByteType Type
        {
            get { return image.Attributes[index].Type; }
        }

        public bool IsLeadByte
        {
            get { return image.Attributes[index].IsLeadByte; }
        }

        public Procedure Procedure
        {
            get { return image.Procedures.GetValueOrDefault(index); }
        }

        public BasicBlock BasicBlock
        {
            get { return image.BasicBlocks.GetValueOrDefault(index); }
        }
    }


#if false
    
            // Create a BinaryImage with the code.
            BinaryImage image = new BinaryImage(codeSegment.Data, new Pointer(0, 0));

            // Disassemble the instructions literally. Note that this should
            // be improved, but we don't do that yet.
            var addr = image.BaseAddress;
            for (var i = image.StartAddress; i < image.EndAddress; )
            {
                var instruction = image.DecodeInstruction(addr);

                // An operand may have zero or one component that may be
                // fixed up. Check this.
                for (int k = 0; k < instruction.Operands.Length; k++)
                {
                    var operand = instruction.Operands[k];
                    if (operand is RelativeOperand)
                    {
                        var opr = (RelativeOperand)operand;
                        var loc = opr.Offset.Location;
                        int j = i - image.StartAddress + loc.StartOffset;
                        int fixupIndex = codeSegment.DataFixups[j];
                        if (fixupIndex != 0)
                        {
                            FixupDefinition fixup = codeSegment.Fixups[fixupIndex - 1];
                            if (fixup.DataOffset != j)
                                continue;

                            var target = new SymbolicTarget(fixup, module);
                            instruction.Operands[k] = new SymbolicRelativeOperand(target);
                            System.Diagnostics.Debug.WriteLine(instruction.ToString());
                        }
                    }
                }

                image.CreatePiece(addr, addr + instruction.EncodedLength, ByteType.Code);
                image[addr].Instruction = instruction;
                addr = addr.Increment(instruction.EncodedLength);

                // TODO: we need to check more accurately.

#if false
                // Check if any bytes covered by this instruction has a fixup
                // record associated with it. Note that an instruction might
                // have multiple fixup records associated with it, such as 
                // in a far call.
                for (int j = 0; j < instruction.EncodedLength; j++)
                {
                    int fixupIndex = codeSegment.DataFixups[i - image.StartAddress + j];
                    if (fixupIndex != 0)
                    {
                        FixupDefinition fixup = codeSegment.Fixups[fixupIndex - 1];
                        if (fixup.DataOffset != i - image.StartAddress + j)
                            continue;

                        if (fixup.Target.Method == FixupTargetSpecFormat.ExternalPlusDisplacement ||
                            fixup.Target.Method == FixupTargetSpecFormat.ExternalWithoutDisplacement)
                        {
                            var extIndex = fixup.Target.IndexOrFrame;
                            var extName = module.ExternalNames[extIndex - 1];
                            var disp = fixup.Target.Displacement;

                            System.Diagnostics.Debug.WriteLine(string.Format(
                                "{0} refers to {1}+{2} : {3}",
                                instruction, extName, disp, fixup.Location));
                        }
                    }
                }
#endif
#endif


    /// <summary>
    /// Defines the type of a byte in an executable image.
    /// </summary>
    public enum ByteType
    {
        /// <summary>
        /// The byte is not analyzed and its type is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The byte is a padding byte (usually 0x90, NOP) used to align the
        /// next instruction or data item on a word or dword boundary.
        /// </summary>
        Padding = 1,

        /// <summary>
        /// The byte is part of an instruction.
        /// </summary>
        Code = 2,

        /// <summary>
        /// The byte is part of a data item.
        /// </summary>
        Data = 3,
    }
}

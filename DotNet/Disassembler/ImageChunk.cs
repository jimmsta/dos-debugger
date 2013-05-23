using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Util.Data;
using X86Codec;

// Note: we might as well equate ImageChunk with Segment.
// This will significantly reduce the complexity of our model.
// For an executable, we just dynamically adjust the segment
// boundaries.
namespace Disassembler2
{
    /// <summary>
    /// Contains information about a contiguous chunk of bytes in a binary
    /// image. The bytes may contain code, data, and unknown bytes. In
    /// particular, any fix-up information is associated.
    /// </summary>
    public class ImageChunk
    {
        readonly byte[] image;
        readonly ByteAttribute[] attrs;
        readonly FixupCollection fixups;
        readonly Dictionary<int, Instruction> instructions;

        readonly RangeDictionary<int, Procedure> procedureMapping;

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
            this.procedureMapping = new RangeDictionary<int, Procedure>();
            this.instructions = new Dictionary<int, Instruction>();
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

        public Range<int> Bounds
        {
            get { return new Range<int>(0, image.Length); }
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
            int fixupIndex = fixups.BinarySearch(offset);
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

        /// <summary>
        /// Returns true if all the bytes within the given range are of the
        /// given type.
        /// </summary>
        public bool CheckByteType(int offset, int length, ByteType type)
        {
            for (int i = offset; i < offset + length; i++)
            {
                if (attrs[i].Type != type)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Marks a continuous range of bytes as an atomic item of the given
        /// type.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void UpdateByteType(int offset, int length, ByteType type)
        {
            if (type != ByteType.Code &&
                type != ByteType.Data &&
                type != ByteType.Padding)
                throw new ArgumentException("type is invalid.", "type");
            if (offset < 0 || offset > image.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0 || length > image.Length - offset)
                throw new ArgumentOutOfRangeException("length");
            if (length == 0)
                return;

#if false
            if (start.Segment != end.Segment)
                throw new ArgumentException("start and end must be in the same segment.");
#endif

            if (!CheckByteType(offset,length,ByteType.Unknown))
                throw new ArgumentException("[start, end) overlaps with analyzed bytes.");

            // Mark the byte range as 'type'.
            for (int i = offset; i < offset + length; i++)
            {
                //attr[i].Address = start + (i - pos1);
                attrs[i].Type = type;
                attrs[i].IsLeadByte = false;
            }
            attrs[offset].IsLeadByte = true;

#if false
            // Update the segment bounds.
            Segment segment = FindSegment(start.Segment);
            if (segment == null)
            {
                segment = new Segment(start.Segment, start.LinearAddress, end.LinearAddress);
                segments.Add(start.Segment, segment);
            }
            else
            {
                // TODO: modify this to use MultiRange.
                segment.Extend(start.LinearAddress, end.LinearAddress);
            }
            return piece;
#endif
        }

        public RangeDictionary<int, Procedure> ProcedureMapping
        {
            get { return this.procedureMapping; }
        }

        //public RangeDictionary<int, BasicBlock> BasicBlockMapping
        //{
        //    get { return this.basicBlockMapping; }
        //}

        public Dictionary<int, Instruction> Instructions
        {
            get { return instructions; }
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
            get
            {
                return image.ProcedureMapping.GetValueOrDefault(
                    new Range<int>(index, index + 1));
            }
        }

        //public BasicBlock BasicBlock
        //{
        //    get { return image.BasicBlockMapping.GetValueOrDefault(index); }
        //}

        public Instruction Instruction
        {
            get { return image.Instructions[index]; }
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

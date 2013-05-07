using System;
using System.Text;

namespace X86Codec
{
#if false
    /// <summary>
    /// Defines a location within an instruction.
    /// </summary>
    public struct LocationWithinInstruction
    {
    }
#endif

    /// <summary>
    /// Represents an operand of an instruction.
    /// </summary>
    public abstract class Operand
    {
        /// <summary>
        /// Specifies the location of the operand within the instruction,
        /// expressed as a byte offset to the beginning of the instruction.
        /// This only applies to the displacement and immediate part; it does
        /// not include part of the operand encoded in prefix, opcode, modrm,
        /// or SIB byte.
        /// </summary>
        /// <remarks>
        /// Depending on the operand type, these values are defined as below:
        /// 
        /// Operand Type            StartOffset     EndOffset       Example
        /// Register                0               0               AX
        /// Memory with disp        disp-start      disp-end        [BX+4]
        /// Memory w/o disp         0               0               [BX+BI]
        /// Relative                offset start    offset end      +4
        /// Immediate (explicit)    imm start       imm end         20h
        /// Immediate (implicit)    0               0               SHL AX,1
        /// Pointer                 pointer start   pointer end     2920:7654
        /// </remarks>
        public byte EncodedPosition { get; private set; }
        public byte EncodedLength { get; private set; }

        protected Operand(int encodedPosition, int encodedLength)
        {
            this.EncodedPosition = (byte)encodedPosition;
            this.EncodedLength = (byte)encodedLength;
        }

        /// <summary>
        /// Converts an unsigned integer to hexidecimal string of the form
        /// "0f43h" or "5".
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        internal static string FormatImmediate(UInt64 number)
        {
            if (number < 10)
            {
                return number.ToString();
            }
            else
            {
                string s = string.Format("0{0:x}h", number);
                if (s[1] > '9')
                    return s;
                else
                    return s.Substring(1);
            }
        }
    }

    /// <summary>
    /// Represents an immediate operand. An immediate may take 8, 16, or 32
    /// bits, and is always sign-extended to 32 bits and stored internally.
    /// </summary>
    public class ImmediateOperand : Operand
    {
        private int value;
        public CpuSize Size;

        public ImmediateOperand(int value, CpuSize size, int position, int length)
            : base(position, length)
        {
            this.value = value;
            this.Size = size;
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Converts an immediate to a string in Intel syntax.
        /// </summary>
        /// <returns>The converted string.</returns>
        public override string ToString()
        {
            // Encode in decimal if the value is a single digit.
            if (value > -10 && value < 10)
                return value.ToString();

            // Encode in hexidecimal format such as 0f34h.
            switch (Size)
            {
                case CpuSize.Use8Bit: return FormatImmediate((byte)value);
                case CpuSize.Use16Bit: return FormatImmediate((ushort)value);
                default: return FormatImmediate((uint)value);
            }
        }
    }

    /// <summary>
    /// Represents a register operand.
    /// </summary>
    public class RegisterOperand : Operand
    {
        public static Register Make(RegisterType type, int number, CpuSize size, int offset)
        {
            int value =
                ((int)type << (int)Register._TypeShift) |
                (number << (int)Register._NumberShift) |
                ((int)size << (int)Register._SizeShift) |
                (offset << (int)Register._OffsetShift);
            return (Register)value;
        }

        public static Register Make(RegisterType type, int number, CpuSize size)
        {
            return Make(type, number, size, 0);
        }

        public static Register Resize(Register register, CpuSize newSize)
        {
            int reg = (int)register & ~(int)Register._SizeMask;
            reg |= (int)newSize << (int)Register._SizeShift;
            return (Register)reg;
        }

        public Register Register { get; set; }

        public RegisterOperand()
            : base(0, 0)
        {
        }

        public RegisterOperand(Register register)
            : base(0, 0)
        {
            this.Register = register;
        }

        public RegisterOperand(Register register, CpuSize newSize)
            : base(0, 0)
        {
            int reg = (int)register & ~(int)Register._SizeMask;
            reg |= (int)newSize << (int)Register._SizeShift;
            this.Register = (Register)reg;
        }

        public RegisterOperand(RegisterType type, int number, CpuSize size)
            : this(type, number, size, 0)
        {
        }

        public RegisterOperand(RegisterType type, int number, CpuSize size, RegisterOffset offset)
            : base(0, 0)
        {
            int value =
                ((int)type << (int)Register._TypeShift) |
                (number << (int)Register._NumberShift) |
                ((int)size << (int)Register._SizeShift) |
                ((int)offset << (int)Register._OffsetShift);
            this.Register = (Register)value;
        }

        public RegisterType Type
        {
            get
            {
                return (RegisterType)((int)(Register & Register._TypeMask)
                    >> (int)Register._TypeShift);
            }
        }

        public int Number
        {
            get
            {
                return (int)(Register & Register._NumberMask)
                    >> (int)Register._NumberShift;
            }
        }

        /// <summary>
        /// Gets the size (in bytes) of the register.
        /// </summary>
        public CpuSize Size
        {
            get
            {
                return (CpuSize)((int)(Register & Register._SizeMask)
                    >> (int)Register._SizeShift);
            }
        }

        /// <summary>
        /// Gets the offset (in bytes) of the register relative to the
        /// physical register. This is 1 for AH,BH,CH,DH and 0 for all
        /// other registers.
        /// </summary>
        public RegisterOffset Offset
        {
            get
            {
                return (RegisterOffset)((int)(Register & Register._OffsetMask)
                    >> (int)Register._OffsetShift);
            }
        }

        public override string ToString()
        {
            return this.Register.ToString();
        }
    }

    /// <summary>
    /// Represents a memory address operand of the form
    /// [segment:base+index*scaling+displacement].
    /// </summary>
    public class MemoryOperand : Operand
    {
        public CpuSize Size; // size of the operand in bytes
        public Register Segment;
        public Register Base;
        public Register Index;
        public byte Scaling = 1; // scaling factor; must be one of 1, 2, 4
        public int Displacement; // sign-extended

        public MemoryOperand()
            : base(0, 0)
        {
        }

        /// <summary>
        /// Formats a memory operand in the form "dword ptr es:[ax+si*4+10]".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            CpuSize size = Size;
            string prefix =
                (size == CpuSize.Use8Bit) ? "BYTE" :
                (size == CpuSize.Use16Bit) ? "WORD" :
                (size == CpuSize.Use32Bit) ? "DWORD" :
                (size == CpuSize.Use64Bit) ? "QWORD" :
                (size == CpuSize.Use128Bit) ? "DQWORD" : "";

            StringBuilder s = new StringBuilder();
            if (prefix != "")
            {
                s.Append(prefix);
                s.Append(" PTR ");
            }

            if (Segment != Register.None)
            {
                s.Append(Segment.ToString());
                s.Append(':');
            }
            s.Append('[');
            if (Base == Register.None) // only displacement
            {
                s.Append(FormatImmediate((UInt32)Displacement));
            }
            else // base+index*scale+displacement
            {
                s.Append(Base.ToString());
                if (Index != Register.None)
                {
                    s.Append('+');
                    s.Append(Index.ToString());
                    if (Scaling != 1)
                    {
                        s.Append('*');
                        s.Append(Scaling.ToString());
                    }
                }
                if (Displacement > 0) // e.g. [BX+1]
                {
                    s.Append('+');
                    s.Append(FormatImmediate((uint)Displacement));
                }
                else if (Displacement < 0) // e.g. [BP-2]
                {
                    s.Append('-');
                    s.Append(FormatImmediate((uint)(-Displacement)));
                }
            }
            s.Append(']');
            return s.ToString();
        }
    }

    /// <summary>
    /// Represents an address as a relative offset to EIP.
    /// </summary>
    public class RelativeOperand : Operand
    {
        private int offset;

        public RelativeOperand(int offset, int position, int length)
            : base(position, length)
        {
            this.offset = offset;
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public override string ToString()
        {
            string s = offset.ToString();
            if (offset >= 0)
                s = '+' + s;
            return s;
        }
    }

#if false
    /// <summary>
    /// Represents a far pointer consisting of a segment address and an offset
    /// within the segment.
    /// </summary>
    public struct FarPointer
    {
        public UInt16 Segment;
        public UInt64 Offset;
    }
#endif

    public class PointerOperand : Operand
    {
        public UInt16 Segment { get; private set; }
        public UInt64 Offset { get; private set; }

        public PointerOperand(UInt16 segment, UInt64 offset, int position, int length)
            : base(position, length)
        {
            this.Segment = segment;
            this.Offset = offset;
        }

        public override string ToString()
        {
            //if (opr->size == OPR_32BIT)
            return string.Format("{0:X4}:{1:X4}",
                Segment, (UInt16)Offset);
        }

        public Pointer Value
        {
            get { return new Pointer(this.Segment, (UInt16)this.Offset); }
        }
    }
}

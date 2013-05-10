using System;
using System.Text;

namespace X86Codec
{
    /// <summary>
    /// Represents an operand of an instruction.
    /// </summary>
    public abstract class Operand
    {
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

        /// <summary>
        /// Represents the location within an instruction, expressed as an
        /// offset to the beginning of the instruction.
        /// </summary>
        public struct Location
        {
            public byte StartOffset { get; private set; }
            public byte Length { get; private set; }

            public Location(byte startOffset, byte length)
                : this()
            {
                StartOffset = startOffset;
                Length = length;
            }
        }

        /// <summary>
        /// Wraps a value of type T with its location within the instruction.
        /// </summary>
        /// <typeparam name="T">Type of the value to wrap.</typeparam>
        /// <remarks>
        /// The location is expressed as a byte offset to the beginning of
        /// the instruction. Depending on the type of operand, the location
        /// may be used in the following ways:
        /// 
        /// Operand Type            Property        Example
        /// ---------------------------------------------------------------
        /// Register                (not used)      XOR   AX, AX
        /// Memory with disp        displacement    LEA   AX, [BX+4]
        /// Memory w/o disp         (not used)      MOV   AX, [BX+BI]
        /// Relative                Offset          JMP   +4
        /// Immediate (explicit)    Immediate       MOV   AX, 20h
        /// Immediate (implicit)    (not used)      SHL   AX, 1
        /// Pointer                 Segment,Offset  CALLF 2920:7654
        /// </remarks>
        public struct LocationAware<T>
        {
            public Location Location { get; private set; }
            public T Value { get; private set; }

            public LocationAware(Location location, T value)
                : this()
            {
                this.Location = location;
                this.Value = value;
            }

            public LocationAware(T value)
                : this()
            {
                this.Value = value;
            }
        }
    }

    /// <summary>
    /// Represents an immediate operand. An immediate may take 8, 16, or 32
    /// bits, and is always sign-extended to 32 bits and stored internally.
    /// </summary>
    public class ImmediateOperand : Operand
    {
        public LocationAware<int> Immediate { get; set; }
        public CpuSize Size;

        public ImmediateOperand(LocationAware<int> immediate, CpuSize size)
        {
            this.Immediate = immediate;
            this.Size = size;
        }

        public ImmediateOperand(int value, CpuSize size)
        {
            this.Immediate = new LocationAware<int>(value);
            this.Size = size;
        }

        /// <summary>
        /// Converts an immediate to a string in Intel syntax.
        /// </summary>
        /// <returns>The converted string.</returns>
        public override string ToString()
        {
            // Encode in decimal if the value is a single digit.
            if (Immediate.Value > -10 && Immediate.Value < 10)
                return Immediate.Value.ToString();

            // Encode in hexidecimal format such as 0f34h.
            switch (Size)
            {
                case CpuSize.Use8Bit: return FormatImmediate((byte)Immediate.Value);
                case CpuSize.Use16Bit: return FormatImmediate((ushort)Immediate.Value);
                default: return FormatImmediate((uint)Immediate.Value);
            }
        }
    }

    /// <summary>
    /// Represents a register operand.
    /// </summary>
    public class RegisterOperand : Operand
    {
        public Register Register { get; private set; }

        public RegisterOperand(Register register)
        {
            this.Register = register;
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
        public CpuSize Size { get; set; } // size of the operand in bytes
        public Register Segment { get; set; }
        public Register Base { get; set; }
        public Register Index { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor. Must be one of 1, 2, 4.
        /// </summary>
        public byte Scaling { get; set; }

        public LocationAware<int> Displacement { get; set; } // sign-extended

        public MemoryOperand()
        {
            this.Scaling = 1;
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
                s.Append(FormatImmediate((UInt16)Displacement.Value));
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
                if (Displacement.Value > 0) // e.g. [BX+1]
                {
                    s.Append('+');
                    s.Append(FormatImmediate((uint)Displacement.Value));
                }
                else if (Displacement.Value < 0) // e.g. [BP-2]
                {
                    s.Append('-');
                    s.Append(FormatImmediate((uint)(-Displacement.Value)));
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
        public LocationAware<int> Offset { get; private set; }

        public RelativeOperand(LocationAware<int> offset)
        {
            this.Offset = offset;
        }

        public override string ToString()
        {
            return Offset.Value.ToString("+#;-#");
        }
    }

    public class PointerOperand : Operand
    {
        public LocationAware<UInt16> Segment { get; private set; }
        public LocationAware<UInt32> Offset { get; private set; }

        public PointerOperand(LocationAware<UInt16> segment, LocationAware<UInt32> offset)
        {
            this.Segment = segment;
            this.Offset = offset;
        }

        public override string ToString()
        {
            return string.Format("{0:X4}:{1:X4}", Segment.Value, Offset.Value);
        }
    }
}

using System;
using System.Text;

namespace X86Codec
{
    public class InstructionFormatter
    {
        public virtual string FormatInstruction(Instruction instruction)
        {
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            StringBuilder sb = new StringBuilder();

            // Format group 1 (LOCK/REPZ/REPNZ) prefix.
            if ((instruction.Prefix & Prefixes.Group1) != 0)
            {
                sb.Append((instruction.Prefix & Prefixes.Group1).ToString().ToLowerInvariant());
                sb.Append(' ');
            }

            // Format mnemonic.
            sb.Append(instruction.Operation.ToString().ToLowerInvariant());

            // Format operands.
            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                sb.Append(' ');
                sb.Append(FormatOperand(instruction.Operands[i]));
            }
            return sb.ToString();
        }

        //protected virtual void FormatPrefix(StringBuilder sb, Prefixes prefix)
        //{
        //    sb.Append((prefix & Prefixes.Group1).ToString());
        //}

        //protected virtual void FormatMnemonic(StringBuilder sb, 

        public virtual string FormatOperand(Operand operand)
        {
            if (operand is ImmediateOperand)
                return FormatOperand((ImmediateOperand)operand);
            else if (operand is RegisterOperand)
                return FormatOperand((RegisterOperand)operand);
            else if (operand is MemoryOperand)
                return FormatOperand((MemoryOperand)operand);
            else if (operand is RelativeOperand)
                return FormatOperand((RelativeOperand)operand);
            else if (operand is PointerOperand)
                return FormatOperand((PointerOperand)operand);
            else
                throw new ArgumentException("Unsupported operand type.");
        }

        public virtual string FormatOperand(ImmediateOperand operand)
        {
            int value = operand.Immediate.Value;

            // Encode in decimal if the value is a single digit.
            if (value > -10 && value < 10)
                return value.ToString();

            // Encode in hexidecimal format such as 0f34h.
            switch (operand.Size)
            {
                case CpuSize.Use8Bit: value &= 0xFF; break;
                case CpuSize.Use16Bit: value &= 0xFFFF; break;
            }
            string s = value.ToString("x");
            if (s[0] > '9')
                return '0' + s + 'h';
            else
                return s + 'h';
        }

        public virtual string FormatOperand(RegisterOperand operand)
        {
            return operand.Register.ToString().ToLowerInvariant();
        }

        public virtual string FormatOperand(MemoryOperand operand)
        {
            CpuSize size = operand.Size;
            string prefix =
                (size == CpuSize.Use8Bit) ? "byte" :
                (size == CpuSize.Use16Bit) ? "word" :
                (size == CpuSize.Use32Bit) ? "dword" :
                (size == CpuSize.Use64Bit) ? "qword" :
                (size == CpuSize.Use128Bit) ? "dqword" : "";

            StringBuilder sb = new StringBuilder();
            if (prefix != "")
            {
                sb.Append(prefix);
                sb.Append(" ptr ");
            }

            if (operand.Segment != Register.None)
            {
                FormatRegister(sb, operand.Segment);
                sb.Append(':');
            }

            string strDisplacement = FormatFixableLocation(operand, operand.Displacement.Location);
            
            sb.Append('[');
            if (operand.Base == Register.None) // only displacement
            {
                if (strDisplacement != null)
                    sb.Append(strDisplacement);
                else
                    FormatNumber(sb, (UInt16)operand.Displacement.Value);
            }
            else // base+index*scale+displacement
            {
                FormatRegister(sb, operand.Base);
                if (operand.Index != Register.None)
                {
                    sb.Append('+');
                    FormatRegister(sb, operand.Index);
                    if (operand.Scaling != 1)
                    {
                        sb.Append('*');
                        sb.Append(operand.Scaling);
                    }
                }

                if (strDisplacement != null)
                {
                    sb.Append('+');
                    sb.Append(strDisplacement);
                }
                else
                {
                    int displacement = operand.Displacement.Value;
                    if (displacement > 0) // e.g. [BX+1]
                    {
                        sb.Append('+');
                        FormatNumber(sb, (uint)displacement);
                    }
                    else if (displacement < 0)
                    {
                        sb.Append('-');
                        FormatNumber(sb, (uint)-displacement);
                    }
                }
            }
            sb.Append(']');
            return sb.ToString();
        }

        public virtual string FormatOperand(RelativeOperand operand)
        {
            return operand.Offset.Value.ToString("+#;-#");
        }

        public virtual string FormatOperand(PointerOperand operand)
        {
            return string.Format("{0:X4}:{1:X4}",
                operand.Segment.Value,
                operand.Offset.Value);
        }

        protected virtual string FormatFixableLocation(Operand operand, Operand.Location location)
        {
            return null;
        }

        protected static void FormatRegister(StringBuilder sb, Register register)
        {
            sb.Append(register.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Formats an unsigned integer in one of the following formats:
        /// 3 -- for single decimal digit number
        /// 45h -- for hexidecimal number starting with a digit
        /// 0ffch -- for hexidecimal number starting with an alphabet
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="number"></param>
        protected static void FormatNumber(StringBuilder sb, UInt32 number)
        {
            if (number < 10)
            {
                sb.Append(number);
            }
            else
            {
                string s = number.ToString("x");
                if (s[0] > '9')
                    sb.Append('0');
                sb.Append(s);
                sb.Append('h');
            }
        }

        public static readonly InstructionFormatter Default =
            new InstructionFormatter();
    }

    // class InstructionFormatter_Intel
    // class InstructionFormatter_Att
}

using System;
using System.Text;

namespace X86Codec
{
    /// <summary>
    /// Represents a decoded x86 instruction.
    /// </summary>
    public class Instruction
    {
        /// <summary>
        /// Gets or sets the encoded length (in bytes) of the instruction.
        /// </summary>
        public int EncodedLength { get; set; }

        /// <summary>
        /// Gets or sets the (legacy) prefixes of the instruction.
        /// </summary>
        public Prefixes Prefix { get; set; }

        /// <summary>
        /// Gets or sets the operation performed by this instruction.
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>Gets or sets the operands of this instruction.</summary>
        /// <remarks>An X86 instruction may use up to four operands.</remarks>
        public Operand[] Operands { get; set; }

        /// <summary>
        /// Converts the instruction to a string using the default formatter.
        /// </summary>
        /// <returns>The formatted instruction.</returns>
        public override string ToString()
        {
            return InstructionFormatter.Default.FormatInstruction(this);
        }
    }

    /// <summary>
    /// Provides methods to read the components of an encoded instruction.
    /// An encoded instruction has the following components:
    /// 
    ///  LegacyPrefixes + Opcode + ModR/M + SIB + Displacement + Immediate
    ///  0-4              1-3      0-1      0-1   0,1,2,4        0,1,2,4
    /// </summary>
    internal class InstructionReader
    {
        byte[] code;
        int startIndex;
        int count;

        int opcodeOffset;
        int modrmOffset;
        int currentOffset;

        public int Position
        {
            get { return this.currentOffset; }
        }

        /// <summary>
        /// Creates an instruction reader that reads the given portion of a
        /// byte array.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public InstructionReader(byte[] code, int startIndex, int count)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            if (startIndex < 0 || startIndex >= code.Length)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count < 0 || startIndex + count > code.Length)
                throw new ArgumentOutOfRangeException("count");

            this.code = code;
            this.startIndex = startIndex;
            this.count = count;
        }

        /// <summary>
        /// Gets the next byte in the instruction.
        /// </summary>
        /// <returns>The next byte in the instruction.</returns>
        public byte PeekByte()
        {
            if (currentOffset >= count)
                throw new InvalidInstructionException("Reading past the end of the code buffer.");
            return code[startIndex + currentOffset];
        }

        /// <summary>
        /// Consumes the next byte as part of the instruction prefix.
        /// </summary>
        public void ConsumePrefixByte()
        {
            if (currentOffset >= count)
                throw new InvalidOperationException("Not enough bytes to consume.");
            currentOffset += 1;
            opcodeOffset = currentOffset;
            modrmOffset = currentOffset;
        }

        public void ConsumeOpcodeByte()
        {
            if (currentOffset >= count)
                throw new InvalidOperationException("Not enough bytes to consume.");
            currentOffset += 1;
            modrmOffset = currentOffset;
        }

        public ModRM GetModRM()
        {
            if (currentOffset == modrmOffset)
            {
                if (currentOffset >= count)
                    throw new InvalidInstructionException("Reading past the end of the code buffer.");
                currentOffset++;
            }
            return new ModRM(code[startIndex + modrmOffset]);
        }

        /// <summary>
        /// Consumes the next byte, word, or dword.
        /// </summary>
        /// <returns>The next byte, word, or dword, sign-extended.</returns>
        public Operand.LocationAware<int> ReadImmediate(CpuSize size)
        {
            int pos = this.Position;
            int value;
            if (size == CpuSize.Use8Bit)
                value = (sbyte)ReadByte();
            else if (size == CpuSize.Use16Bit)
                value = ReadInt16();
            else if (size == CpuSize.Use32Bit)
                value = ReadInt32();
            else
                throw new ArgumentException("size must be 1, 2, or 4.");

            return new Operand.LocationAware<int>(
                new Operand.Location((byte)pos, (byte)size),
                value);
        }

        private byte ReadByte()
        {
            if (currentOffset + 1 > count)
            {
                throw new InvalidInstructionException("Reading past the end of the code buffer.");
            }
            byte value = code[startIndex + currentOffset];
            currentOffset += 1;
            return value;
        }

        private short ReadInt16()
        {
            if (currentOffset + 2 > count)
            {
                throw new InvalidInstructionException("Reading past the end of the code buffer.");
            }
            short value = BitConverter.ToInt16(code, startIndex + currentOffset);
            currentOffset += 2;
            return value;
        }

        private int ReadInt32()
        {
            if (currentOffset + 4 > count)
            {
                throw new InvalidInstructionException("Reading past the end of the code buffer.");
            }
            int value = BitConverter.ToInt32(code, startIndex + currentOffset);
            currentOffset += 4;
            return value;
        }
    }

    struct ModRM
    {
        public byte Value { get; set; }

        public ModRM(byte value)
            : this()
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the MOD part of a ModR/M byte. The returned value is in the
        /// range 0 to 3.
        /// </summary>
        public int MOD
        {
            get { return (Value >> 6) & 0x3; }
        }

        /// <summary>
        /// Gets the REG part of a ModR/M byte. The returned value is in the
        /// range 0 to 7.
        /// </summary>
        public int REG
        {
            get { return (Value >> 3) & 0x7; }
        }

        /// <summary>
        /// Gets the RM part of a ModR/M byte (0-7).
        /// </summary>
        public int RM
        {
            get { return Value & 0x7; }
        }
    }

    /// <summary>
    /// Defines the condition values (tttn) used in a Jcc instruction.
    /// The values must correspond to their encoded values in an instruction.
    /// See Intel Reference, Volume 2, Appendix B, Table B-10.
    /// </summary>
    public enum Condition
    {
        O = 0,
        NO = 1,
        B = 2,
        NAE = B,
        NB = 3,
        AE = NB,
        E = 4,
        Z = E,
        NE = 5,
        NZ = NE,
        BE = 6,
        NA = BE,
        NBE = 7,
        A = NBE,
        S = 8,
        NS = 9,
        P = 10,
        PE = P,
        NP = 11,
        PO = NP,
        L = 12,
        NGE = L,
        NL = 13,
        GE = NL,
        LE = 14,
        NG = LE,
        NLE = 15,
        G = NLE
    }

    /// <summary>
    /// Defines logical constants for instruction prefixes. Each prefix takes
    /// a different bit, so it's easy to test the presence of a given prefix.
    /// In addition, prefixes from the same group have a common bit set, which
    /// simplifies group testing.
    /// 
    /// Note that more than one prefixes may share the same bit; therefore the
    /// actual interpretatio of a prefix should depend on the instruction 
    /// being modified.
    /// </summary>
    [Flags]
    public enum Prefixes
    {
        None = 0,

        // Group 1: lock and repeat prefixes
        Group1 = 0x07,
        LOCK = 0x01,         /* F0 */
        REPNZ = 0x02,         /* F2 */
       // REPNE = REPNZ,
        REP = 0x04,         /* F3 */
        //REPZ = REP,
        //REPE = REPZ,

        /* Group 2: segment override or branch hints */
        Group2 = 0x01f8,
        ES = 0x0008,       /* 26 */
        CS = 0x0010,       /* 2E */
        SS = 0x0020,       /* 36 */
        DS = 0x0040,       /* 3E */
        FS = 0x0080,       /* 64 */
        GS = 0x0100,       /* 65 */
        BranckTaken = CS, /* 2E */
        BranchNotTaken = DS, /* 3E */

        /* Group 3: operand-size override */
        Group3 = 0x0200,
        OperandSizeOverride = 0x0200,  /* 66 */

        /* Group 4: address-size override */
        Group4 = 0x0400,
        AddressSizeOverride = 0x0400   /* 67 */
    }

    /// <summary>
    /// Thrown when an instruction cannot be encoded or decoded.
    /// </summary>
    public class InvalidInstructionException : Exception
    {
        public InvalidInstructionException()
        {
        }

        public InvalidInstructionException(string message)
            : base(message)
        {
        }
    }
}

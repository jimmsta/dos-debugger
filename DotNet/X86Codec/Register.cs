using System;

namespace X86Codec
{
    /// <summary>
    /// Represents an x86 register.
    /// </summary>
    public struct Register
    {
        private RegisterId id;

        private Register(RegisterId id)
        {
#if DEBUG
            if (id.ToString().StartsWith("_"))
            {
                throw new ArgumentException(id.ToString() + " is not a valid register.");
            }
#endif
            this.id = id;
        }

        public Register(RegisterType type, int number, CpuSize size)
        {
            id = (RegisterId)(number | ((int)type << 4) | ((int)size - 1) << 8);
        }

        /// <summary>
        /// Gets the type of the register.
        /// </summary>
        public RegisterType Type
        {
            get { return (RegisterType)(((int)id >> 4) & 0xF); }
        }

        /// <summary>
        /// Gets the ordinal number of the register within its type.
        /// </summary>
        public int Number
        {
            get { return (int)id & 0xF; }
        }

        /// <summary>
        /// Gets the size (in bytes) of the register.
        /// </summary>
        public CpuSize Size
        {
            get { return (CpuSize)((((int)id >> 8) & 0xFF) + 1); }
        }

        public Register Resize(CpuSize newSize)
        {
            int newId = (int)id & 0xFF | (((int)newSize - 1) << 8);
            return new Register((RegisterId)newId);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public static bool operator ==(Register x, Register y)
        {
            return x.id == y.id;
        }

        public static bool operator !=(Register x, Register y)
        {
            return x.id != y.id;
        }

        public override bool Equals(object obj)
        {
            return (obj is Register) && (this == (Register)obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static readonly Register None = new Register(RegisterId.None);
        public static readonly Register FLAGS = new Register(RegisterId.FLAGS);

        public static readonly Register AL = new Register(RegisterId.AL);
        public static readonly Register CL = new Register(RegisterId.CL);
        public static readonly Register DL = new Register(RegisterId.DL);
        public static readonly Register BL = new Register(RegisterId.BL);
        public static readonly Register AH = new Register(RegisterId.AH);
        public static readonly Register CH = new Register(RegisterId.CH);
        public static readonly Register DH = new Register(RegisterId.DH);
        public static readonly Register BH = new Register(RegisterId.BH);

        public static readonly Register AX = new Register(RegisterId.AX);
        public static readonly Register CX = new Register(RegisterId.CX);
        public static readonly Register DX = new Register(RegisterId.DX);
        public static readonly Register BX = new Register(RegisterId.BX);
        public static readonly Register SP = new Register(RegisterId.SP);
        public static readonly Register BP = new Register(RegisterId.BP);
        public static readonly Register SI = new Register(RegisterId.SI);
        public static readonly Register DI = new Register(RegisterId.DI);

        public static readonly Register EAX = new Register(RegisterId.EAX);
        public static readonly Register ECX = new Register(RegisterId.ECX);
        public static readonly Register EDX = new Register(RegisterId.EDX);
        public static readonly Register EBX = new Register(RegisterId.EBX);
        public static readonly Register ESP = new Register(RegisterId.ESP);
        public static readonly Register EBP = new Register(RegisterId.EBP);
        public static readonly Register ESI = new Register(RegisterId.ESI);
        public static readonly Register EDI = new Register(RegisterId.EDI);

        public static readonly Register ES = new Register(RegisterId.ES);
        public static readonly Register CS = new Register(RegisterId.CS);
        public static readonly Register SS = new Register(RegisterId.SS);
        public static readonly Register DS = new Register(RegisterId.DS);
        public static readonly Register FS = new Register(RegisterId.FS);
        public static readonly Register GS = new Register(RegisterId.GS);

        public static readonly Register ST0 = new Register(RegisterId.ST0);
        public static readonly Register ST1 = new Register(RegisterId.ST1);
        public static readonly Register ST2 = new Register(RegisterId.ST2);
        public static readonly Register ST3 = new Register(RegisterId.ST3);
        public static readonly Register ST4 = new Register(RegisterId.ST4);
        public static readonly Register ST5 = new Register(RegisterId.ST5);
        public static readonly Register ST6 = new Register(RegisterId.ST6);
        public static readonly Register ST7 = new Register(RegisterId.ST7);
    }

    /// <summary>
    /// Defines the ID of each x86 register in a specific way.
    /// </summary>
    /// <remarks>
    /// Each addressible register in the x86 architecture is assigned a unique
    /// ID that can be stored in 16-bits. For performance reasons, the value
    /// of the identifier are constructed as follows:
    /// 
    ///   15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0
    ///  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    ///  |         SIZE minus 1          |     TYPE      |    NUMBER     |
    ///  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    ///
    /// TYPE specifies the type of the register, such as general purpose,
    /// segment, flags, etc. Its value corresponds to an enumerated value
    /// defined in RegisterType.
    /// 
    /// NUMBER is the ordinal number of the register within its type. This
    /// number is defined by its opcode encoding; for example, general-
    /// purpose registers are numbered in the order of AX, CX, DX, BX.
    ///
    /// SIZE specifies the size (in bytes) of the register, minus one. Thus
    /// by encoding SIZE in 8 bits, it's capable of representing a length
    /// from 1 byte up to 256 bytes, adequate for any register.
    /// </remarks>
    internal enum RegisterId : ushort
    {
        /// <summary>
        /// Indicates that either a register is not used, or the default
        /// register should be used.
        /// </summary>
        None = 0,

        /* Special registers. */
        IP = RT.SPECIAL | 1 | RS.WORD,
        EIP = RT.SPECIAL | 1 | RS.DWORD,
        FLAGS = RT.SPECIAL | 2 | RS.WORD,
        EFLAGS = RT.SPECIAL | 2 | RS.DWORD,
        RFLAGS = RT.SPECIAL | 2 | RS.QWORD,
        MXCSR = RT.SPECIAL | 3 | RS.DWORD,

        // General purpose registers.
        // See Table B-2 to B-5 in Intel Reference, Volume 2, Appendix B.

        /* ad-hoc registers */
        AH = RT.HIGHBYTE | RS.BYTE | 0,
        CH = RT.HIGHBYTE | RS.BYTE | 1,
        DH = RT.HIGHBYTE | RS.BYTE | 2,
        BH = RT.HIGHBYTE | RS.BYTE | 3,

        /* byte registers */
        AL = RT.GENERAL | RS.BYTE | 0,
        CL = RT.GENERAL | RS.BYTE | 1,
        DL = RT.GENERAL | RS.BYTE | 2,
        BL = RT.GENERAL | RS.BYTE | 3,
        SPL = RT.GENERAL | RS.BYTE | 4,
        BPL = RT.GENERAL | RS.BYTE | 5,
        SIL = RT.GENERAL | RS.BYTE | 6,
        DIL = RT.GENERAL | RS.BYTE | 7,
        R8L = RT.GENERAL | RS.BYTE | 8,
        R9L = RT.GENERAL | RS.BYTE | 9,
        R10L = RT.GENERAL | RS.BYTE | 10,
        R11L = RT.GENERAL | RS.BYTE | 11,
        R12L = RT.GENERAL | RS.BYTE | 12,
        R13L = RT.GENERAL | RS.BYTE | 13,
        R14L = RT.GENERAL | RS.BYTE | 14,
        R15L = RT.GENERAL | RS.BYTE | 15,

        /* word registers */
        AX = RT.GENERAL | RS.WORD | 0,
        CX = RT.GENERAL | RS.WORD | 1,
        DX = RT.GENERAL | RS.WORD | 2,
        BX = RT.GENERAL | RS.WORD | 3,
        SP = RT.GENERAL | RS.WORD | 4,
        BP = RT.GENERAL | RS.WORD | 5,
        SI = RT.GENERAL | RS.WORD | 6,
        DI = RT.GENERAL | RS.WORD | 7,
        R8W = RT.GENERAL | RS.WORD | 8,
        R9W = RT.GENERAL | RS.WORD | 9,
        R10W = RT.GENERAL | RS.WORD | 10,
        R11W = RT.GENERAL | RS.WORD | 11,
        R12W = RT.GENERAL | RS.WORD | 12,
        R13W = RT.GENERAL | RS.WORD | 13,
        R14W = RT.GENERAL | RS.WORD | 14,
        R15W = RT.GENERAL | RS.WORD | 15,

        /* dword registers */
        EAX = RT.GENERAL | RS.DWORD | 0,
        ECX = RT.GENERAL | RS.DWORD | 1,
        EDX = RT.GENERAL | RS.DWORD | 2,
        EBX = RT.GENERAL | RS.DWORD | 3,
        ESP = RT.GENERAL | RS.DWORD | 4,
        EBP = RT.GENERAL | RS.DWORD | 5,
        ESI = RT.GENERAL | RS.DWORD | 6,
        EDI = RT.GENERAL | RS.DWORD | 7,
        R8D = RT.GENERAL | RS.DWORD | 8,
        R9D = RT.GENERAL | RS.DWORD | 9,
        R10D = RT.GENERAL | RS.DWORD | 10,
        R11D = RT.GENERAL | RS.DWORD | 11,
        R12D = RT.GENERAL | RS.DWORD | 12,
        R13D = RT.GENERAL | RS.DWORD | 13,
        R14D = RT.GENERAL | RS.DWORD | 14,
        R15D = RT.GENERAL | RS.DWORD | 15,

        /* qword registers */
        RAX = RT.GENERAL | RS.QWORD | 0,
        RCX = RT.GENERAL | RS.QWORD | 1,
        RDX = RT.GENERAL | RS.QWORD | 2,
        RBX = RT.GENERAL | RS.QWORD | 3,
        RSP = RT.GENERAL | RS.QWORD | 4,
        RBP = RT.GENERAL | RS.QWORD | 5,
        RSI = RT.GENERAL | RS.QWORD | 6,
        RDI = RT.GENERAL | RS.QWORD | 7,
        R8 = RT.GENERAL | RS.QWORD | 8,
        R9 = RT.GENERAL | RS.QWORD | 9,
        R10 = RT.GENERAL | RS.QWORD | 10,
        R11 = RT.GENERAL | RS.QWORD | 11,
        R12 = RT.GENERAL | RS.QWORD | 12,
        R13 = RT.GENERAL | RS.QWORD | 13,
        R14 = RT.GENERAL | RS.QWORD | 14,
        R15 = RT.GENERAL | RS.QWORD | 15,

        /* Segment registers. See Volume 2, Appendix B, Table B-8. */
        ES = RT.SEGMENT | RS.WORD | 0,
        CS = RT.SEGMENT | RS.WORD | 1,
        SS = RT.SEGMENT | RS.WORD | 2,
        DS = RT.SEGMENT | RS.WORD | 3,
        FS = RT.SEGMENT | RS.WORD | 4,
        GS = RT.SEGMENT | RS.WORD | 5,

        /* FPU */
        ST0 = RT.ST | RS.LONGDOUBLE | 0,
        ST1 = RT.ST | RS.LONGDOUBLE | 1,
        ST2 = RT.ST | RS.LONGDOUBLE | 2,
        ST3 = RT.ST | RS.LONGDOUBLE | 3,
        ST4 = RT.ST | RS.LONGDOUBLE | 4,
        ST5 = RT.ST | RS.LONGDOUBLE | 5,
        ST6 = RT.ST | RS.LONGDOUBLE | 6,
        ST7 = RT.ST | RS.LONGDOUBLE | 7,

        /* Control registers (eee). See Volume 2, Appendix B, Table B-9. */
        CR0 = RT.CONTROL | RS.WORD | 0,
        CR2 = RT.CONTROL | RS.WORD | 2,
        CR3 = RT.CONTROL | RS.WORD | 3,
        CR4 = RT.CONTROL | RS.WORD | 4,

        /* Debug registers (eee). See Volume 2, Appendix B, Table B-9. */
        DR0 = RT.DEBUG | RS.WORD | 0,
        DR1 = RT.DEBUG | RS.WORD | 1,
        DR2 = RT.DEBUG | RS.WORD | 2,
        DR3 = RT.DEBUG | RS.WORD | 3,
        DR6 = RT.DEBUG | RS.WORD | 6,
        DR7 = RT.DEBUG | RS.WORD | 7,

        /* MMX registers. */
        MM0 = RT.MM | RS.QWORD | 0,
        MM1 = RT.MM | RS.QWORD | 1,
        MM2 = RT.MM | RS.QWORD | 2,
        MM3 = RT.MM | RS.QWORD | 3,
        MM4 = RT.MM | RS.QWORD | 4,
        MM5 = RT.MM | RS.QWORD | 5,
        MM6 = RT.MM | RS.QWORD | 6,
        MM7 = RT.MM | RS.QWORD | 7,

        /* XMM registers. */
        XMM0 = RT.XMM | RS.DQWORD | 0,
        XMM1 = RT.XMM | RS.DQWORD | 1,
        XMM2 = RT.XMM | RS.DQWORD | 2,
        XMM3 = RT.XMM | RS.DQWORD | 3,
        XMM4 = RT.XMM | RS.DQWORD | 4,
        XMM5 = RT.XMM | RS.DQWORD | 5,
        XMM6 = RT.XMM | RS.DQWORD | 6,
        XMM7 = RT.XMM | RS.DQWORD | 7,
        XMM8 = RT.XMM | RS.DQWORD | 8,
        XMM9 = RT.XMM | RS.DQWORD | 9,
        XMM10 = RT.XMM | RS.DQWORD | 10,
        XMM11 = RT.XMM | RS.DQWORD | 11,
        XMM12 = RT.XMM | RS.DQWORD | 12,
        XMM13 = RT.XMM | RS.DQWORD | 13,
        XMM14 = RT.XMM | RS.DQWORD | 14,
        XMM15 = RT.XMM | RS.DQWORD | 15,
    }

    /// <summary>
    /// (Abbreviation for RegisterType) Auxiliary enum used to simplify
    /// definition of RegisterId enum members. We need to split this out
    /// to make sure RegisterId's ToString() and Parse() work properly.
    /// </summary>
    internal enum RT
    {
        SPECIAL = RegisterType.Special << 4,
        GENERAL = RegisterType.General << 4,
        HIGHBYTE = RegisterType.HighByte << 4,
        SEGMENT = RegisterType.Segment << 4,
        ST = RegisterType.Fpu << 4,
        CONTROL = RegisterType.Control << 4,
        DEBUG = RegisterType.Debug << 4,
        MM = RegisterType.MMX << 4,
        XMM = RegisterType.XMM << 4,
    }

    /// <summary>
    /// (Abbreviation for RegisterSize) Auxiliary enum used to simplify
    /// definition of RegisterId enum members. We need to split this out
    /// to make sure RegisterId's ToString() and Parse() work properly.
    /// </summary>
    internal enum RS
    {
        BYTE = (1 - 1) << 8,
        WORD = (2 - 1) << 8,
        DWORD = (4 - 1) << 8,
        QWORD = (8 - 1) << 8,
        LONGDOUBLE = (10 - 1) << 8,
        DQWORD = (16 - 1) << 8,
    }

    /// <summary>
    /// Defines the type of a physical register.
    /// </summary>
    public enum RegisterType
    {
        /// <summary>
        /// Indicates that a register is not used.
        /// </summary>
        None,

        /// <summary>Special purpose registers, such as FLAGS.</summary>
        Special,

        /// <summary>General purpose registers, such as EAX.</summary>
        General,

        /// <summary>High byte of general purpose registers (AH-DH).</summary>
        HighByte,

        /// <summary>Segment registers, such as CS.</summary>
        Segment,

        /// <summary>FPU registers ST(0) - ST(7).</summary>
        Fpu,

        /// <summary>Control registers.</summary>
        Control,

        /// <summary>Debug registers.</summary>
        Debug,

        MMX,
        XMM,
        //YMM,
    }
}

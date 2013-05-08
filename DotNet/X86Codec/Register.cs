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

        _SPECIAL = RegisterType.Special << 4,
        _GENERAL = RegisterType.General << 4,
        _HIGHBYTE = RegisterType.HighByte << 4,
        _SEGMENT = RegisterType.Segment << 4,
        _ST = RegisterType.Fpu << 4,
        _CONTROL = RegisterType.Control << 4,
        _DEBUG = RegisterType.Debug << 4,
        _MM = RegisterType.MMX << 4,
        _XMM = RegisterType.XMM << 4,

        _BYTE = 1 << 8,
        _WORD = 2 << 8,
        _DWORD = 4 << 8,
        _QWORD = 8 << 8,
        _LONGDOUBLE = 10 << 8,
        _DQWORD = 16 << 8,

        /* Special registers. */
        IP = _SPECIAL | 1 | _WORD,
        EIP = _SPECIAL | 1 | _DWORD,
        FLAGS = _SPECIAL | 2 | _WORD,
        EFLAGS = _SPECIAL | 2 | _DWORD,
        RFLAGS = _SPECIAL | 2 | _QWORD,
        MXCSR = _SPECIAL | 3 | _DWORD,

        // General purpose registers.
        // See Table B-2 to B-5 in Intel Reference, Volume 2, Appendix B.

        /* ad-hoc registers */
        AH = _HIGHBYTE | _BYTE | 0,
        CH = _HIGHBYTE | _BYTE | 1,
        DH = _HIGHBYTE | _BYTE | 2,
        BH = _HIGHBYTE | _BYTE | 3,

        /* byte registers */
        AL = _GENERAL | _BYTE | 0,
        CL = _GENERAL | _BYTE | 1,
        DL = _GENERAL | _BYTE | 2,
        BL = _GENERAL | _BYTE | 3,
        SPL = _GENERAL | _BYTE | 4,
        BPL = _GENERAL | _BYTE | 5,
        SIL = _GENERAL | _BYTE | 6,
        DIL = _GENERAL | _BYTE | 7,
        R8L = _GENERAL | _BYTE | 8,
        R9L = _GENERAL | _BYTE | 9,
        R10L = _GENERAL | _BYTE | 10,
        R11L = _GENERAL | _BYTE | 11,
        R12L = _GENERAL | _BYTE | 12,
        R13L = _GENERAL | _BYTE | 13,
        R14L = _GENERAL | _BYTE | 14,
        R15L = _GENERAL | _BYTE | 15,

        /* word registers */
        AX = _GENERAL | _WORD | 0,
        CX = _GENERAL | _WORD | 1,
        DX = _GENERAL | _WORD | 2,
        BX = _GENERAL | _WORD | 3,
        SP = _GENERAL | _WORD | 4,
        BP = _GENERAL | _WORD | 5,
        SI = _GENERAL | _WORD | 6,
        DI = _GENERAL | _WORD | 7,
        R8W = _GENERAL | _WORD | 8,
        R9W = _GENERAL | _WORD | 9,
        R10W = _GENERAL | _WORD | 10,
        R11W = _GENERAL | _WORD | 11,
        R12W = _GENERAL | _WORD | 12,
        R13W = _GENERAL | _WORD | 13,
        R14W = _GENERAL | _WORD | 14,
        R15W = _GENERAL | _WORD | 15,

        /* dword registers */
        EAX = _GENERAL | _DWORD | 0,
        ECX = _GENERAL | _DWORD | 1,
        EDX = _GENERAL | _DWORD | 2,
        EBX = _GENERAL | _DWORD | 3,
        ESP = _GENERAL | _DWORD | 4,
        EBP = _GENERAL | _DWORD | 5,
        ESI = _GENERAL | _DWORD | 6,
        EDI = _GENERAL | _DWORD | 7,
        R8D = _GENERAL | _DWORD | 8,
        R9D = _GENERAL | _DWORD | 9,
        R10D = _GENERAL | _DWORD | 10,
        R11D = _GENERAL | _DWORD | 11,
        R12D = _GENERAL | _DWORD | 12,
        R13D = _GENERAL | _DWORD | 13,
        R14D = _GENERAL | _DWORD | 14,
        R15D = _GENERAL | _DWORD | 15,

        /* qword registers */
        RAX = _GENERAL | _QWORD | 0,
        RCX = _GENERAL | _QWORD | 1,
        RDX = _GENERAL | _QWORD | 2,
        RBX = _GENERAL | _QWORD | 3,
        RSP = _GENERAL | _QWORD | 4,
        RBP = _GENERAL | _QWORD | 5,
        RSI = _GENERAL | _QWORD | 6,
        RDI = _GENERAL | _QWORD | 7,
        R8 = _GENERAL | _QWORD | 8,
        R9 = _GENERAL | _QWORD | 9,
        R10 = _GENERAL | _QWORD | 10,
        R11 = _GENERAL | _QWORD | 11,
        R12 = _GENERAL | _QWORD | 12,
        R13 = _GENERAL | _QWORD | 13,
        R14 = _GENERAL | _QWORD | 14,
        R15 = _GENERAL | _QWORD | 15,

        /* Segment registers. See Volume 2, Appendix B, Table B-8. */
        ES = _SEGMENT | _WORD | 0,
        CS = _SEGMENT | _WORD | 1,
        SS = _SEGMENT | _WORD | 2,
        DS = _SEGMENT | _WORD | 3,
        FS = _SEGMENT | _WORD | 4,
        GS = _SEGMENT | _WORD | 5,

        /* FPU */
        ST0 = _ST | _LONGDOUBLE | 0,
        ST1 = _ST | _LONGDOUBLE | 1,
        ST2 = _ST | _LONGDOUBLE | 2,
        ST3 = _ST | _LONGDOUBLE | 3,
        ST4 = _ST | _LONGDOUBLE | 4,
        ST5 = _ST | _LONGDOUBLE | 5,
        ST6 = _ST | _LONGDOUBLE | 6,
        ST7 = _ST | _LONGDOUBLE | 7,

        /* Control registers (eee). See Volume 2, Appendix B, Table B-9. */
        CR0 = _CONTROL | _WORD | 0,
        CR2 = _CONTROL | _WORD | 2,
        CR3 = _CONTROL | _WORD | 3,
        CR4 = _CONTROL | _WORD | 4,

        /* Debug registers (eee). See Volume 2, Appendix B, Table B-9. */
        DR0 = _DEBUG | _WORD | 0,
        DR1 = _DEBUG | _WORD | 1,
        DR2 = _DEBUG | _WORD | 2,
        DR3 = _DEBUG | _WORD | 3,
        DR6 = _DEBUG | _WORD | 6,
        DR7 = _DEBUG | _WORD | 7,

        /* MMX registers. */
        MM0 = _MM | _QWORD | 0,
        MM1 = _MM | _QWORD | 1,
        MM2 = _MM | _QWORD | 2,
        MM3 = _MM | _QWORD | 3,
        MM4 = _MM | _QWORD | 4,
        MM5 = _MM | _QWORD | 5,
        MM6 = _MM | _QWORD | 6,
        MM7 = _MM | _QWORD | 7,

        /* XMM registers. */
        XMM0 = _XMM | _DQWORD | 0,
        XMM1 = _XMM | _DQWORD | 1,
        XMM2 = _XMM | _DQWORD | 2,
        XMM3 = _XMM | _DQWORD | 3,
        XMM4 = _XMM | _DQWORD | 4,
        XMM5 = _XMM | _DQWORD | 5,
        XMM6 = _XMM | _DQWORD | 6,
        XMM7 = _XMM | _DQWORD | 7,
        XMM8 = _XMM | _DQWORD | 8,
        XMM9 = _XMM | _DQWORD | 9,
        XMM10 = _XMM | _DQWORD | 10,
        XMM11 = _XMM | _DQWORD | 11,
        XMM12 = _XMM | _DQWORD | 12,
        XMM13 = _XMM | _DQWORD | 13,
        XMM14 = _XMM | _DQWORD | 14,
        XMM15 = _XMM | _DQWORD | 15,
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

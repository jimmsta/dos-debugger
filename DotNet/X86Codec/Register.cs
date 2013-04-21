using System;

namespace X86Codec
{
    /// <summary>
    /// Represents a register.
    /// </summary>
    /// <remarks>
    /// Each logical register in the x86 architecture is assigned a unique
    /// 16-bit identifier. For performance considerations, the values of 
    /// these identifiers are constructed as follows:
    /// 
    ///   15  14  13  12  11  10   9   8   7   6   5   4   3   2   1   0
    ///  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    ///  | offset|         size          |     type      |    number     |
    ///  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    ///
    /// TYPE corresponds to an enumerated value defined in RegisterType.
    /// NUMBER is a number between 0 and 15. TYPE and NUMBER combined defines
    /// a physical register on the CPU, such as RAX. All logical registers on
    /// the same physical register have the same TYPE and NUMBER values.
    ///
    /// SIZE and OFFSET defines a logical register that can be referenced in
    /// an instruction, such as AX or AH. OFFSET is typically 0, which
    /// indicates the lowerest SIZE*8 bits of the register. The only cases
    /// where OFFSET is non-zero are AH-DH, where OFFSET=1 and indicates high
    /// byte.
    /// 
    /// NUMBER is chosen to be equal to their encoded values where possible.
    /// The exceptions are AH-DH.
    /// </remarks>
    public enum Register : ushort
    {
        /// <summary>
        /// Indicates that either a register is not used, or the default
        /// register should be used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies the bits that are used to represent the number of the
        /// physical register among the same type of registers.
        /// </summary>
        _NumberMask = 0xf,
        _NumberShift = 0,

        /// <summary>
        /// Specifies the bits that are used to represent the type of the
        /// register.
        /// </summary>
        _TypeMask = 0xf0,
        _TypeShift = 4,
        _SPECIAL = RegisterType.Special << 4,
        _GENERAL = RegisterType.General << 4,
        _SEGMENT = RegisterType.Segment << 4,

        /// <summary>
        /// Specifies the bits that are used to represent the size (in bytes)
        /// of a logical register.
        /// </summary>
        _SizeMask = 0x3f00,
        _SizeShift = 8,
        _BYTE = 0x0100,
        _WORD = 0x0200,
        _DWORD = 0x0400,
        _QWORD = 0x0800,
        _DQWORD = 0x1000,

        /// <summary>
        /// Specifies the bits that are used to indicate the offset of a
        /// logical register.
        /// </summary>
        _OffsetMask = 0xc000,
        _OffsetShift = 14,
        _HIBYTE = RegisterOffset.HighByte << 14,

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
        AH = _GENERAL | _BYTE | 0 | _HIBYTE,
        CH = _GENERAL | _BYTE | 1 | _HIBYTE,
        DH = _GENERAL | _BYTE | 2 | _HIBYTE,
        BH = _GENERAL | _BYTE | 3 | _HIBYTE,

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

#if false
        /* Control registers (eee). See Volume 2, Appendix B, Table B-9. */
        R_CR0 = REG_MAKE_W(CONTROL, 0),
        R_CR2 = REG_MAKE_W(CONTROL, 2),
        R_CR3 = REG_MAKE_W(CONTROL, 3),
        R_CR4 = REG_MAKE_W(CONTROL, 4),

        /* Debug registers (eee). See Volume 2, Appendix B, Table B-9. */
        R_DR0 = REG_MAKE_W(DEBUG, 0),
        R_DR1 = REG_MAKE_W(DEBUG, 1),
        R_DR2 = REG_MAKE_W(DEBUG, 2),
        R_DR3 = REG_MAKE_W(DEBUG, 3),
        R_DR6 = REG_MAKE_W(DEBUG, 6),
        R_DR7 = REG_MAKE_W(DEBUG, 7),

        /* MMX registers. */
        R_MM0 = REG_MAKE_Q(MMX, 0),
        R_MM1 = REG_MAKE_Q(MMX, 1),
        R_MM2 = REG_MAKE_Q(MMX, 2),
        R_MM3 = REG_MAKE_Q(MMX, 3),
        R_MM4 = REG_MAKE_Q(MMX, 4),
        R_MM5 = REG_MAKE_Q(MMX, 5),
        R_MM6 = REG_MAKE_Q(MMX, 6),
        R_MM7 = REG_MAKE_Q(MMX, 7),

        /* XMM registers. */
        R_XMM0 = REG_MAKE_DQ(XMM, 0),
        R_XMM1 = REG_MAKE_DQ(XMM, 1),
        R_XMM2 = REG_MAKE_DQ(XMM, 2),
        R_XMM3 = REG_MAKE_DQ(XMM, 3),
        R_XMM4 = REG_MAKE_DQ(XMM, 4),
        R_XMM5 = REG_MAKE_DQ(XMM, 5),
        R_XMM6 = REG_MAKE_DQ(XMM, 6),
        R_XMM7 = REG_MAKE_DQ(XMM, 7),
        R_XMM8 = REG_MAKE_DQ(XMM, 8),
        R_XMM9 = REG_MAKE_DQ(XMM, 9),
        R_XMM10 = REG_MAKE_DQ(XMM, 10),
        R_XMM11 = REG_MAKE_DQ(XMM, 11),
        R_XMM12 = REG_MAKE_DQ(XMM, 12),
        R_XMM13 = REG_MAKE_DQ(XMM, 13),
        R_XMM14 = REG_MAKE_DQ(XMM, 14),
        R_XMM15 = REG_MAKE_DQ(XMM, 15),
#endif
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

        /// <summary>Special purpose registers.</summary>
        Special,

        /// <summary>General purpose registers, such as EAX.</summary>
        General,

        /// <summary>Segment registers, such as CS.</summary>
        Segment,

        // <summary>Control registers.</summary>
        //Control,

        //Debug,
        //MMX,
        //XMM,
        //YMM,
    }

    public enum RegisterOffset
    {
        None = 0,
        HighByte = 1,
    }
}

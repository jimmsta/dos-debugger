using System;
using System.ComponentModel;

namespace X86Codec
{
    /// <summary>
    /// Defines the operation of an instructions.
    /// </summary>
    /// <remarks>
    /// (1) We might as well append operand type to mnemonic to make certain
    ///     applications easier.
    /// (2) We might as well enumerate all legacy prefixes (REP/REPZ/REPNZ),
    ///     because they are only a few predefined applications.
    /// </remarks>
    public enum Operation
    {
        None = 0,

        // ------------------------------------------------------------------
        // The following instructions are compatible with 8086.
        // See Intel Manual 20.1.3.
        //
        // However, it appears that the list in Intel Manual is not complete.
        // We therefore refer to the following sources for the complete 8086
        // instruction set:
        // http://www.electronics.dit.ie/staff/tscarff/8086_instruction_set/8086_instruction_set.html
        // http://en.wikipedia.org/wiki/X86_instruction_listings#Original_8086.2F8088_instructions
        // ------------------------------------------------------------------

        // Data transfer instructions:
        [Description("Moves data from second operand to first operand.")]
        [FlagsAffected(CpuFlags.None)]
        MOV,

        [Description("Exchanges the contents of two operands.")]
        [FlagsAffected(CpuFlags.None)]
        XCHG,

        [Description("Loads a far pointer from memory into DS and first operand.")]
        [FlagsAffected(CpuFlags.None)]
        LDS,

        [Description("Loads a far pointer from memory into ES and first operand.")]
        [FlagsAffected(CpuFlags.None)]
        LES,

        // Arithmetic instructions
        [Description("Adds second operand to first operand.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        ADD,

        [Description("Adds second operand and CF to first operand.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        ADC,

        [Description("Subtracts second operand from first operand.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        SUB,

        [Description("Subtracts sum of second operand and CF from first operand.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        SBB,

        [Description("Unsigned multiply: AX ← AL * SRC.")]
        [FlagsAffected(CpuFlags.CF | CpuFlags.OF,
            UndefinedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.AF | CpuFlags.PF)]
        MULB,

        [Description("Unsigned multiply: DX:AX ← AX * SRC.")]
        [FlagsAffected(CpuFlags.CF | CpuFlags.OF,
            UndefinedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.AF | CpuFlags.PF)]
        MULW,

        [Description("Signed multiply: AX ← AL * SRC.")]
        [FlagsAffected(CpuFlags.CF | CpuFlags.OF,
            UndefinedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.AF | CpuFlags.PF)]
        IMULB,

        [Description("Signed multiply: DX:AX ← AX * SRC.")]
        [FlagsAffected(CpuFlags.CF | CpuFlags.OF,
            UndefinedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.AF | CpuFlags.PF)]
        IMULW,

        [Description("Unsigned divide AX by operand: AL ← Quotient, AH ← Remainder," +
            "or, Unsigned divide DX:AX by operand: AX ← Quotient, DX ← Remainder.")]
        [FlagsAffected(UndefinedFlags = CpuFlags.StatusFlags)]
        DIV,

        [Description("Signed divide AX by operand: AL ← Quotient, AH ← Remainder," +
            "or, Signed divide DX:AX by operand: AX ← Quotient, DX ← Remainder.")]
        [FlagsAffected(UndefinedFlags = CpuFlags.StatusFlags)]
        IDIV,

        [Description("Increments operand by 1.")]
        [FlagsAffected(CpuFlags.StatusFlagsExceptCF)]
        INC,

        [Description("Decrements operand by 1.")]
        [FlagsAffected(CpuFlags.StatusFlagsExceptCF)]
        DEC,

        [Description("Subtracts operand 2 from operand 1 without storing the result.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        CMP,

        [Description("Subtracts the operand from 0 and store in it.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        NEG,

        // Logical instructions
        [Description("Performs bitwise AND and stores the result in first operand.")]
        [FlagsAffected(
            AffectedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.PF,
            ClearedFlags = CpuFlags.OF | CpuFlags.CF,
            UndefinedFlags = CpuFlags.AF)]
        AND,

        [Description("Performs bitwise OR and stores the result in first operand.")]
        [FlagsAffected(
            AffectedFlags = CpuFlags.SF | CpuFlags.ZF | CpuFlags.PF,
            ClearedFlags = CpuFlags.OF | CpuFlags.CF,
            UndefinedFlags = CpuFlags.AF)]
        OR,

        XOR, NOT,

        // Decimal instructions
        DAA, DAS, AAA, AAS, AAM, AAD,

        // Stack instructions:
        PUSH, POP,

        // Type conversion instructions:
        [Description("Sets DX:AX to sign-extend of AX.")]
        CWD, // same opcode as CDQ
        [Description("Sets AX to sign-extend of AL.")]
        CBW, // same opcode as CWDE

        // Shift and rotate instructions:
        SAL, SHL, SHR, SAR, ROL, ROR, RCL, RCR,

        // TEST instruction:
        [Description("Computes bit-wise AND of two operands and sets SF, ZF and PF accordingly.")]
        TEST,

        // Control instructions
        JMP, JMPF,
        [Description("Jump if CX = 0.")]
        JCXZ,
        [Description("Jump if overflow (OF = 1).")]
        JO,
        [Description("Jump if not overflow (OF = 0).")]
        JNO,
        [Description("Jump if below (CF = 1).")]
        JB,
        [Description("Jump if above or equal (CF = 0).")]
        JAE,
        [Description("Jump if equal (ZF = 1).")]
        JE,
        [Description("Jump if not equal (ZF = 0).")]
        JNE,
        [Description("Jump if below or equal (CF = 1 or ZF = 1).")]
        JBE,
        [Description("Jump if above (CF = 0 and ZF = 0).")]
        JA,
        [Description("Jump if sign (SF = 1).")]
        JS,
        [Description("Jump if not sign (SF = 0).")]
        JNS,
        [Description("Jump if parity (PF = 1).")]
        JP,
        [Description("Jump if not parity (PF = 0).")]
        JNP,
        [Description("Jump if less (SF ≠ OF).")]
        JL,
        [Description("Jump if greater or equal (SF = OF).")]
        JGE,
        [Description("Jump if less or equal (ZF = 1 or SF ≠ OF).")]
        JLE,
        [Description("Jump if greater (ZF = 0 and SF = OF).")]
        JG,

        [Description("Near call.")]
        CALL,
        [Description("Far call.")]
        CALLF,
        [Description("Near return.")]
        RET,
        [Description("Far return.")]
        RETF,
        [Description("Decrements CX, and then jump if CX ≠ 0.")]
        LOOP,
        [Description("Decrements CX, and then jump if CX ≠ 0 and ZF = 1.")]
        LOOPZ,
        [Description("Decrements CX, and then jump if CX ≠ 0 and ZF = 0.")]
        LOOPNZ,

        // Interrupt instructions
        [Description("Raises an interrupt.")]
        INT,
        [Description("Raises interrupt 4 if OF = 1.")]
        INTO,
        IRET,

        // FLAGS control instructions:
        [Description("Sets carry flag: CF ← 1.")]
        STC,
        [Description("Clears carry flag: CF ← 0.")]
        CLC,
        [Description("Complements carry flag: CF ← !CF.")]
        CMC,
        [Description("Sets direction flag: DF ← 1.")]
        STD,
        [Description("Clears direction flag: DF ← 0.")]
        CLD,
        [Description("Sets interrupt flag: IF ← 1.")]
        STI,
        [Description("Clears interrupt flag: IF ← 0.")]
        CLI,
        [Description("Loads flags into AH: AH ← SF:ZF:0:AF:0:PF:1:CF.")]
        LAHF,
        [Description("Stores AH into flags: SF:ZF:0:AF:0:PF:1:CF ← AH.")]
        SAHF,
        [Description("Pushes FLAGS onto the stack.")]
        PUSHF,
        [Description("Pops a word from the stack and stores it in FLAGS.")]
        POPF,

        // I/O instructions:
        [Description("Inputs byte from the given I/O port into AL.")]
        [FlagsAffected(CpuFlags.None)]
        INB,

        [Description("Inputs word from the given I/O port into AX.")]
        [FlagsAffected(CpuFlags.None)]
        INW,

        [Description("Outputs byte in AL to the given I/O port.")]
        [FlagsAffected(CpuFlags.None)]
        OUTB,

        [Description("Outputs word in AX to the given I/O port.")]
        [FlagsAffected(CpuFlags.None)]
        OUTW,

        // String instructions:
        [Description("Compares byte at DS:[SI] with byte at ES:[DI], and sets status flags accordingly.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        CMPSB,

        [Description("Compares word at DS:[SI] with word at ES:[DI], and sets status flags accordingly.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        CMPSW,

        [Description("Loads byte at DS:[SI] into AL.")]
        [FlagsAffected(CpuFlags.None)]
        LODSB,

        [Description("Loads byte at DS:[SI] into AL.")]
        [FlagsAffected(CpuFlags.None)]
        LODSW,

        [Description("Moves byte from DS:[SI] to ES:[DI].")]
        [FlagsAffected(CpuFlags.None)]
        MOVSB,

        [Description("Moves word from DS:[SI] to ES:[DI].")]
        [FlagsAffected(CpuFlags.None)]
        MOVSW,

        [Description("Compares AL with byte at ES:[DI], and sets status flags accordingly.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        SCASB,

        [Description("Compares AX with word at ES:[DI], and sets status flags accordingly.")]
        [FlagsAffected(CpuFlags.StatusFlags)]
        SCASW,

        [Description("Stores AL at ES:[DI].")]
        [FlagsAffected(CpuFlags.None)]
        STOSB,

        [Description("Stores AX at ES:[DI].")]
        [FlagsAffected(CpuFlags.None)]
        STOSW,

        // Special memory load:
        [Description("Computes effective address of second operand and stores it into first operand.")]
        LEA,
        [Description("Sets AL to memory byte DS:[BX + unsigned AL].")]
        XLATB,

        // LOCK prefix.
        // Repeat prefixes REP, REPE, REPZ, REPNE, and REPNZ.

        // Processor control:
        [Description("Stops instruction execution and puts processor in HALT state.")]
        HLT,
        [Description("Performs no operation.")]
        NOP,

        // Note: the WAIT/FWAIT instruction can be treated either as
        // 8086 or as 8087.

        // ------------------------------------------------------------------
        // The following instructions are added in 80186/80188. See
        // http://en.wikipedia.org/wiki/X86_instruction_listings#Added_with_80186.2F80188
        // ------------------------------------------------------------------

        PUSHA,
        POPA,
        BOUND,

        INS,
        OUTS,

        ENTER,
        LEAVE,

        // ------------------------------------------------------------------
        // The following instructions are added in 80286. See
        // http://en.wikipedia.org/wiki/X86_instruction_listings#Added_with_80286
        // ------------------------------------------------------------------

        ARPL,
        /* CLTS */
        /* LAR */
        /* LGDT */
        /* LIDT */
        LLDT,
        /* LMSW */
        /* LSL */
        LTR,
        /* SGDT */
        /* SIDT */
        SLDT,
        /* SMSW */
        STR,
        VERR,
        VERW,

        // ------------------------------------------------------------------
        // The following instructions are defined here purely because I come
        // across them when writing the decoder. It's not supported in any
        // ------------------------------------------------------------------

        // Three-operand IMUL; I don't think this is supported by 8086. Is it?
        IMUL3,

        XABORT,
        XBEGIN,

        // ------------------------------------------------------------------
        // The following instructions are supported by the 8087 processor.
        // See http://en.wikipedia.org/wiki/X86_instruction_listings#Original_8087_instructions
        // ------------------------------------------------------------------

        // Note: this list is not checked.

        FWAIT,

        // 5.2.1 x87 FPU Data Transfer Instructions:
        FLD, FST, FSTP,
        FILD, FIST, FISTP, FISTTP,
        FBLD, FBSTP,
        FXCH,
        FCMOVE, FCMOVNE, FCMOVB, FCMOVBE, FCMOVNB, FCMOVNBE, FCMOVU, FCMOVNU,

        // 5.2.2 x87 FPU Basic Arithmetic Instructions:
        FADD, FADDP, FIADD,
        FSUB, FSUBP, FISUB, FSUBR, FSUBRP, FISUBR,
        FMUL, FMULP, FIMUL, FDIV,
        FDIVP, FIDIV, FDIVR, FDIVRP, FIDIVR,
        FPREM,
        FPREM1,
        FABS,
        FCHS,
        FRNDINT,
        FSCALE,
        FSQRT,
        FXTRACT,

        // 5.2.3 x87 FPU Comparison Instructions:
        FCOM, FCOMP, FCOMPP,
        FUCOM, FUCOMP, FUCOMPP,
        FICOM, FICOMP,
        FCOMI, FUCOMI, FCOMIP, FUCOMIP,
        FTST,
        FXAM,

        // 5.2.4 x87 FPU Transcendental Instructions:
        FSIN, FCOS, FSINCOS,
        FPTAN, FPATAN,
        F2XM1,
        FYL2X,
        FYL2XP1,

        // 5.2.5 x87 FPU Load Constants Instructions:
        FLD1, FLDZ, FLDPI, FLDL2E, FLDLN2, FLDL2T, FLDLG2,

        // 5.2.6 x87 FPU Control Instructions:
        FINCSTP, FDECSTP, FFREE,
        FINIT, FNINIT,
        FCLEX, FNCLEX,
        FSTCW, FNSTCW, FLDCW,
        FSTENV, FNSTENV, FLDENV,
        FSAVE, FNSAVE, FRSTOR,
        FSTSW, FNSTSW,
        FNOP,
    }
}

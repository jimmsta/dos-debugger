using System;

namespace X86Codec
{
    /// <summary>
    /// Defines the operation of an instructions.
    /// </summary>
    public enum Operation
    {
        None = 0,

        /* Basic instructions */
        HLT,
        NOP,

        /* Bitwise logical operations */
        AND,
        OR,
        XOR,
        NOT,

        /* Bitwise shift */
        SHL,
        SHR,
        SAL,
        SAR,

        /* Bitwise rotation */
        ROL,
        ROR,
        RCL,
        RCR,

        /* Unary integer arithmetic */
        INC,
        DEC,
        NEG,

        /* Binary integer arithmetic */
        ADD,
        SUB,
        ADC,
        SBB,
        MUL,
        IMUL,
        DIV,
        IDIV,

        /* Decimal integer adjustment */
        DAA,
        DAS,
        AAA,
        AAS,
        AAM,
        AAD,

        /* Comparison and basic control flow */
        CMP,
        TEST,
        JMP,
        LOOP,
        LOOPZ,
        LOOPNZ,

        /* Conditional jump */
        JO,
        JNO,
        JB,
        JNB,
        JE,
        JNE,
        JBE,
        JNBE,
        JS,
        JNS,
        JP,
        JNP,
        JL,
        JNL,
        JLE,
        JNLE,

        /* Manipulating FLAGS */
        CMC,
        CLC,
        STC,
        CLD,
        STD,
        LAHF,
        SAHF,
        PUSHF,
        POPF,

        /* Calls and interrupts */
        CALL,
        RET,
        INT,
        INTO,
        IRET,

        /* Data movement */
        MOV,
        XCHG,
        PUSH,
        POP,

        /* Address calculation */
        LDS,
        LES,
        LEA,
        XLATB,

        /* Data extension */
        CWD,
        CDQ,
        CBW,
        CWDE,

        /* IO */
        IN,
        INS,
        OUT,
        OUTS,

        /* Misc */
        PUSHA,
        POPA,
        BOUND,
        ARPL,

        CMPS,
        STOS,
        SCAS,
        MOVS,
        LODS,
        FWAIT,
        RETN,
        RETF,
        ENTER,
        LEAVE,
        XLAT,
        LOOPNE,
        LOOPE,
        JCXZ,
        CLI,
        STI,

        SLDT,
        STR,
        VERR,
        VERW,
        LLDT,
        LTR,
        CALLN,
        CALLF,
        JMPN,
        JMPF,
        XABORT,
        XBEGIN,

        /* 8087 FPU instructions */
        FCLEX,
        FSTSW,
    }
}

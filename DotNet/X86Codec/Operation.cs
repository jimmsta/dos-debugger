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
        LOOP,
        LOOPZ,
        LOOPNZ,

        /* Conditional jump */
        JO,
        JNO,
        JB,
        JAE,
        JE,
        JNE,
        JBE,
        JA,
        JS,
        JNS,
        JP,
        JNP,
        JL,
        JGE,
        JLE,
        JG,

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
        CALLF,
        JMP,
        JMPF,
        RET,
        RETF,
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
        
        XABORT,
        XBEGIN,

        /* 8087 FPU instructions */

        // 5.2.1 x87 FPU Data Transfer Instructions:
        FLD, FST, FSTP,
        FILD, FIST, FISTP, FISTTP,
        FBLD, FBSTP,
        FXCH,
        FCMOVE, FCMOVNE, FCMOVB, FCMOVBE, FCMOVNB, FCMOVNBE, FCMOVU, FCMOVNU,

        // x87 FPU Basic Arithmetic Instructions:
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

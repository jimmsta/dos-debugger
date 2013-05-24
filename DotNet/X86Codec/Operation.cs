using System;
using System.ComponentModel;

namespace X86Codec
{
    /// <summary>
    /// Defines the operation of an instructions.
    /// </summary>
    public enum Operation
    {
        None = 0,

        // ----------------------------------------------------------------
        // The following instructions are compatible with 8086.
        // See Intel Manual 20.1.3.
        // ----------------------------------------------------------------

        // Data transfer instructions:
        [Description("Moves data from second operand to first operand.")]
        MOV,
        [Description("Exchanges the contents of two operands.")]
        XCHG,
        [Description("Loads a far pointer from memory into DS and first operand.")]
        LDS,
        [Description("Loads a far pointer from memory into ES and first operand.")]
        LES,

        // Arithmetic instructions
        [Description("Adds second operand to first operand. Affects OF, SF, ZF, AF, CF, PF.")]
        ADD,
        [Description("Adds second operand and CF to first operand.")]
        ADC,
        [Description("Subtracts second operand from first operand.")]
        SUB,
        [Description("Subtracts sum of second operand and CF from first operand.")]
        SBB,
        [Description("Unsigned multiplication.")]
        MUL, 
        [Description("Signed multiplication.")]
        IMUL, 
        
        DIV, IDIV, INC, DEC, CMP, NEG,

        // Logical instructions
        AND, OR, XOR, NOT,

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
        [Description("Loads flags into AH: AH ← SF:ZF:0:AF:0:PF:1:CF.")]
        LAHF,
        [Description("Stores AH into flags: SF:ZF:0:AF:0:PF:1:CF ← AH.")]
        SAHF, 
        [Description("Pushes FLAGS onto the stack.")]
        PUSHF, 
        [Description("Pops a word from the stack and stores it in FLAGS.")]
        POPF,

        // I/O instructions:
        [Description("Inputs value from the I/O port.")]
        IN, INS, OUT, OUTS,

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

        // ----------------------------------------------------------------
        // The following instructions are not compatible with 8086.
        // Therefore, we don't support them for the moment.
        // ----------------------------------------------------------------

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
        [Description("Jump if CX = 0.")]
        JCXZ,
        [Description("Clears interrupt flag: IF ← 0.")]
        CLI,
        [Description("Sets interrupt flag: IF ← 1.")]
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

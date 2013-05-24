using System;
using System.Globalization;

namespace X86Codec
{
    public class CpuProfile
    {
    }

    /// <summary>
    /// Size constants. These values MUST be defined to be the equivalent
    /// number of bytes.
    /// </summary>
    public enum CpuSize : ushort
    {
        Default = 0,
        Use8Bit = 1,
        Use16Bit = 2,
        Use32Bit = 4,
        Use64Bit = 8,
        Use80Bit = 10,
        Use14Bytes = 14,
        Use128Bit = 16,
        Use28Bytes = 28,
        Use256Bit = 32,
    }

    public enum CpuMode
    {
        Default = 0,

        /// <summary>
        /// Native state of a 32-bit processor.
        /// </summary>
        ProtectedMode,

        /// <summary>
        /// A protected mode attribute that can be enabled for any task to
        /// directly execute "real-address mode" 8086 software in a protected,
        /// multi-tasking environment.
        /// </summary>
        Virtual8086Mode,

        /// <summary>
        /// This mode implements the programming environment of the Intel
        /// 8086 processor with extensions (such as the ability to switch to
        /// protected or system management mode). The processor is placed in
        /// real-address mode following power-up or a reset.
        /// </summary>
        RealAddressMode,

        /// <summary>
        /// This mode provides an operating system or executive with a 
        /// transparent mechanism for implementing platform-specific functions
        /// such as power management and system security. The processor enters
        /// SMM when the external SMM interrupt pin (SMI#) is activated or an
        /// SMI is received from the advanced programmable interrupt
        /// controller (APIC).
        /// </summary>
        SystemManagementMode,

        /// <summary>
        /// Similar to 32-bit protected mode. The compability mode permits
        /// most legacy 16-bit and 32-bit applications to run without
        /// re-compilation under a 64-bit operating system. Legacy
        /// applications that run in Virtual 8086 mode or use hardware task
        /// management will not work in this mode.
        /// </summary>
        CompatibilityMode,

        /// <summary>
        /// The 64-bit mode enables a 64-bit operating system to run 
        /// applications written to access 64-bit linear address space.
        /// </summary>
        X64Mode,
    }

    /// <summary>
    /// Defines each bit in the FLAGS register.
    /// </summary>
    [Flags]
    public enum CpuFlags
    {
        None = 0,

        /// <summary>
        /// Carry flag: set if an arithmetic operation generates a carry or
        /// a borrow out of the most-significant bit of the result; cleared
        /// otherwise. This flag indicates an overflow condition for
        /// unsigned-integer arithmetic. It is also used in multiple-precision
        /// arithmetic.
        /// </summary>
        CF = 1 << 0,

        /// <summary>
        /// Parity flag: set if the least-significant byte of the result
        /// contains an even number of 1 bits; cleared otherwise.
        /// </summary>
        PF = 1 << 2,

        /// <summary>
        /// Adjust flag: set if an arithmetic operation generates a carry or
        /// a borrow out of bit 3 of the result; cleared otherwise. This flag
        /// is used in binary-coded decimal (BCD) arithmetic.
        /// </summary>
        AF = 1 << 4,

        /// <summary>
        /// Zero flag: set if the result is zero; cleared otherwise.
        /// </summary>
        ZF = 1 << 6,

        /// <summary>
        /// Sign flag: set equal to the most-significant bit of the result,
        /// which is the sign bit of a signed integer. (0 indicates a positive
        /// value and 1 indicates a negative value.)
        /// </summary>
        SF = 1 << 7,

        /// <summary>Trap flag.</summary>
        TF = 1 << 8,

        /// <summary>Interrupt flag.</summary>
        IF = 1 << 9,

        /// <summary>Direction flag.</summary>
        DF = 1 << 10,

        /// <summary>
        /// Overflow flag: set if the integer result is too large a positive
        /// number or too small a negative number (excluding the sign-bit) to
        /// fit in the destination operand; cleared otherwise. This flag
        /// indicates an overflow condition for signed-integer arithmetic.
        /// </summary>
        OF = 1 << 11,

        /// <summary>
        /// Flags that indicate the results of arithmetic instructions, such
        /// as the ADD, SUB, MUL, and DIV instructions.
        /// </summary>
        StatusFlags = CF | PF | AF | ZF | SF | OF,

        StatusFlagsExceptCF = PF | AF | ZF | SF | OF,
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class FlagsAffectedAttribute : Attribute
    {
        public CpuFlags AffectedFlags { get; set; }
        public CpuFlags UndefinedFlags { get; set; }
        public CpuFlags ClearedFlags { get; set; }

        public FlagsAffectedAttribute()
        {
        }

        public FlagsAffectedAttribute(CpuFlags affectedFlags)
        {
            this.AffectedFlags = affectedFlags;
        }
    }
}

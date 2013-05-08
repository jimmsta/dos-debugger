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
}

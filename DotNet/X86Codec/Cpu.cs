using System;
using System.Globalization;

namespace X86Codec
{
    public class CpuProfile
    {
    }

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
    /// Represents a far pointer consisting of segment and offset components.
    /// For the moment, we only support 16-bit far pointers.
    /// </summary>
    public struct Pointer
    {
        private UInt16 segment;
        private UInt16 offset;

        /// <summary>
        /// Creates a far pointer with the given segment and offset values.
        /// </summary>
        /// <param name="segment">Segment address.</param>
        /// <param name="offset">Offset within segment.</param>
        public Pointer(UInt16 segment, UInt16 offset)
            : this()
        {
            this.segment = segment;
            this.offset = offset;
        }

        /// <summary>
        /// Gets or sets the segment address.
        /// </summary>
        public UInt16 Segment
        {
            get { return segment; }
            set { segment = value; }
        }

        /// <summary>
        /// Gets or sets the offset within the segment.
        /// </summary>
        public UInt16 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        /// <summary>
        /// Gets the linear (physical) address pointed to by this pointer.
        /// </summary>
        /// <exception cref="AddressWrappedException">
        /// If the linear address is greater than or equal to 1MB.
        /// </exception>
        public int LinearAddress
        {
            get
            {
                int linearAddress = segment * 16 + offset;
                if (linearAddress >= 0x100000)
                    throw new AddressWrappedException();
                return linearAddress;
            }
        }

        public override string ToString()
        {
            return string.Format("{0:X4}:{1:X4}", segment, offset);
        }

        public static Pointer Parse(string s)
        {
            Pointer ptr;
            if (!TryParse(s, out ptr))
                throw new ArgumentException("s");
            return ptr;
        }

        public static bool TryParse(string s, out Pointer pointer)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            pointer = new Pointer();

            int k = s.IndexOf(':');
            if (k <= 0 || k >= s.Length - 1)
                return false;

            if (!UInt16.TryParse(
                    s.Substring(0, k),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.segment))
                return false;

            if (!UInt16.TryParse(
                    s.Substring(k + 1),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.offset))
                return false;

            return true;
        }

        /// <summary>
        /// Increments the offset by the given amount, allowing it to wrap
        /// around 0xFFFF.
        /// </summary>
        /// <param name="increment">The amount to increment. A negative value
        /// specifies decrement.</param>
        /// <returns>The incremented pointer, possibly wrapped.</returns>
        public Pointer IncrementWithWrapping(int increment)
        {
            return new Pointer(segment, (ushort)(offset + increment));
        }

        /// <summary>
        /// Increments the offset by the given amount.
        /// </summary>
        /// <param name="increment">The amount to increment. A negative value
        /// specifies decrement.</param>
        /// <returns>The incremented pointer</returns>
        /// <exception cref="AddressWrappedException">If the offset would be
        /// wrapped around 0xFFFF.</exception>
        public Pointer Increment(int increment)
        {
            if ((increment > 0 && increment > 0xFFFF - offset) ||
                (increment < 0 && increment < -(int)offset))
            {
                throw new AddressWrappedException();
            }
            // TODO: check result.LinearAddress.
            return IncrementWithWrapping(increment);
        }

        /// <summary>
        /// Same as p.Increment(increment).
        /// </summary>
        /// <param name="p"></param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static Pointer operator +(Pointer p, int increment)
        {
            return p.Increment(increment);
        }

        /// <summary>
        /// Represents an invalid pointer value (FFFF:FFFF).
        /// </summary>
        public static readonly Pointer Invalid = new Pointer(0xFFFF, 0xFFFF);

        /// <summary>
        /// Returns true if two pointers have the same segment and offset
        /// values.
        /// </summary>
        /// <param name="a">First pointer.</param>
        /// <param name="b">Second pointer.</param>
        /// <returns></returns>
        public static bool operator ==(Pointer a, Pointer b)
        {
            return (a.segment == b.segment) && (a.offset == b.offset);
        }

        /// <summary>
        /// Returns true unless two pointers have the same segment and offset
        /// values.
        /// </summary>
        /// <param name="a">First pointer.</param>
        /// <param name="b">Second pointer.</param>
        /// <returns></returns>
        public static bool operator !=(Pointer a, Pointer b)
        {
            return (a.segment != b.segment) || (a.offset != b.offset);
        }

        /// <summary>
        /// Returns true if two pointers have the same segment and offset
        /// values.
        /// </summary>
        public override bool Equals(object obj)
        {
            return (obj is Pointer) && (this == (Pointer)obj);
        }

        public override int GetHashCode()
        {
            return this.LinearAddress.GetHashCode();
        }
    }

    public class AddressWrappedException : Exception
    {
    }
}

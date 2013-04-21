using System;
using System.Globalization;

namespace X86Codec
{
    public enum CpuSize : ushort
    {
        Default = 0,
        Use8Bit = 1,
        Use16Bit = 2,
        Use32Bit = 4,
        Use64Bit = 8,
        Use128Bit = 16,
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
    /// Represents an absolute address with segment and offset.
    /// For the moment, we only support 16-bit pointers.
    /// </summary>
    public struct Pointer : IComparable<Pointer>
    {
        private UInt16 segment;
        private UInt16 offset;

        public Pointer(UInt16 segment, UInt16 offset)
            : this()
        {
            this.segment = segment;
            this.offset = offset;
        }

        public UInt16 Segment
        {
            get { return segment; }
            set { segment = value; }
        }

        public UInt16 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int EffectiveAddress
        {
            get { return segment * 16 + offset; }
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

            if (s.Length != 9)
                return false;
            if (s[4] != ':')
                return false;

            if (!UInt16.TryParse(
                    s.Substring(0, 4),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.segment))
                return false;

            if (!UInt16.TryParse(
                    s.Substring(5, 4),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.offset))
                return false;

            return true;
        }

        public static Pointer operator +(Pointer p, int increment)
        {
            return new Pointer(p.segment, (ushort)(p.offset + increment));
        }

        /// <summary>
        /// Returns the difference between the effective address between two
        /// pointers. The pointers are allowed to be from different segments.
        /// </summary>
        /// <param name="a">First pointer.</param>
        /// <param name="b">Second pointer.</param>
        /// <returns>The difference between the effective addresses.</returns>
        public static int operator -(Pointer a, Pointer b)
        {
#if false
            if (a.segment != b.segment)
            {
                throw new InvalidOperationException(
                    "Cannot find the difference between two pointers from different segments.");
            }
#endif
            return a.EffectiveAddress - b.EffectiveAddress;
        }

        public static readonly Pointer Invalid = new Pointer(0xFFFF, 0xFFFF);

        public int CompareTo(Pointer other)
        {
            int cmp = (int)this.segment - (int)other.segment;
            if (cmp == 0)
                cmp = (int)this.offset - (int)other.offset;
            return cmp;
            // return this.EffectiveAddress - other.EffectiveAddress;
        }

        public static bool operator ==(Pointer a, Pointer b)
        {
            return (a.segment == b.segment) && (a.offset == b.offset);
        }

        public static bool operator !=(Pointer a, Pointer b)
        {
            return (a.segment != b.segment) || (a.offset != b.offset);
        }

        public override bool Equals(object obj)
        {
            return (obj is Pointer) && (this == (Pointer)obj);
        }

        public override int GetHashCode()
        {
            return this.EffectiveAddress.GetHashCode();
        }
    }
}

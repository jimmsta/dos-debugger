using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Represents a logical object that can be used as an address referent
    /// relative to which logical addresses can be defined.
    /// </summary>
    public interface IAddressReferent
    {
        /// <summary>
        /// Gets a string representation of the address referent. This label
        /// is not necessarily physical or unique; for example, it could be
        /// something like "fopen._TEXT" or "_strcpy".
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Resolves the data address of the referent.
        /// </summary>
        /// <returns>
        /// The resolved address of the referent, or ResolvedAddress.Invalid
        /// if this referent cannot be resolved.
        /// </returns>
        Address Resolve();
    }

#if false
    /// <summary>
    /// Represents a logical address in an assembly, expressed as an address
    /// referent plus a 16-bit displacement. A logical address must be within
    /// the same frame as the address referent.
    /// </summary>
    /// <remarks>
    /// Multiple logical addresses may resolve to the same ResolvedAddress.
    /// </remarks>
    public struct LogicalAddress
    {
        private readonly IAddressReferent referent;
        private readonly UInt16 displacement;

        /// <summary>
        /// Creates a logical address with the given address referent and
        /// displacement.
        /// </summary>
        /// <param name="referent"></param>
        /// <param name="offset"></param>
        public LogicalAddress(IAddressReferent referent, UInt16 displacement)
            : this()
        {
            if (referent == null)
                throw new ArgumentNullException("referent");

            this.referent = referent;
            this.displacement = displacement;
        }

        /// <summary>
        /// Gets the referent of this logical address.
        /// </summary>
        public IAddressReferent Referent
        {
            get { return referent; }
        }

        /// <summary>
        /// Gets the displacement relative to the referent.
        /// </summary>
        public UInt16 Displacement
        {
            get { return displacement; }
        }

        // TODO: we should eliminate this, and use IAddressReferent::Resolve() only.
        public Address ResolvedAddress
        {
            get
            {
                if (this == Invalid)
                    return Address.Invalid;

                Address address = Referent.Resolve();
                return address + this.Displacement;
            }
        }

        /// <summary>
        /// Increments the logical address by the given amount, or throws an
        /// exception if address wrapping would occur.
        /// </summary>
        /// <param name="increment">The amount to increment.</param>
        /// <returns>The incremented logical address.</returns>
        /// <exception cref="AddressWrappedException">If adding the increment
        /// would wrap the displacement around 0xFFFF or 0.</exception>
        public LogicalAddress Increment(int increment)
        {
            if (increment > 0xFFFF - (int)displacement ||
                increment < -(int)displacement)
            {
                throw new AddressWrappedException();
            }
            return this.IncrementWithWrapping(increment);
        }

        /// <summary>
        /// Increments the logical address by the given amount, allowing
        /// address wrapping to occur.
        /// </summary>
        /// <param name="increment">The amount to increment.</param>
        /// <returns>
        /// The incremented (and possibly wrapped) logical address.
        /// </returns>
        public LogicalAddress IncrementWithWrapping(int increment)
        {
            return new LogicalAddress(referent, (UInt16)(displacement + increment));
        }

#if false
        public ImageChunk Image
        {
            get { return ResolvedAddress.Image; }
        }

        public int ImageOffset
        {
            get { return ResolvedAddress.Offset; }
        }

        public ImageByte ImageByte
        {
            get { return this.Image[this.ImageOffset]; }
        }
#endif
        
        public static bool operator ==(LogicalAddress a, LogicalAddress b)
        {
            return (a.Referent == b.Referent) && (a.Displacement == b.Displacement);
        }

        public static bool operator !=(LogicalAddress a, LogicalAddress b)
        {
            return (a.Referent != b.Referent) || (a.Displacement != b.Displacement);
        }

        public override bool Equals(object obj)
        {
            return (obj is LogicalAddress) && (this == (LogicalAddress)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (this == Invalid)
                return "(null)";
            else
                return string.Format("{0}+{1:X4}", referent, displacement);
        }

        /// <summary>
        /// Represents an invalid (null) logical address.
        /// </summary>
        public static readonly LogicalAddress Invalid = new LogicalAddress();

        public static int CompareByLexical(LogicalAddress a, LogicalAddress b)
        {
            int cmp = a.Referent.GetHashCode().CompareTo(b.Referent.GetHashCode());
            if (cmp == 0)
                cmp = a.Displacement.CompareTo(b.Displacement);
            return cmp;
        }
    }
#endif

    public class AddressWrappedException : Exception
    {
    }

    /// <summary>
    /// Represents an address in an assembly. The address is uniquely
    /// identified its segment id and its offset within the segment.
    /// </summary>
    /// <remarks>
    /// The segment id is the one-based index of the segment that contains
    /// the address, among all segments in an assembly. A segment id of zero
    /// indicates an invalid segment, which is used as a guard against
    /// accidental programming error.
    /// 
    /// The offset is an integer that references a byte within the segment.
    /// While 16 bits is adequate to address a byte, it is not sufficient to
    /// express an 'end' index, which is often used in ranges. Therefore, we
    /// use 32 bits to store the offset. This also allows for future extension
    /// to 32-bit disassembly.
    ///
    /// Addresses in different segments are not related. However, certain
    /// tasks will be much simplified if a total order is defined on the
    /// addresses. Examples:
    /// 
    /// 1) When displaying all the instructions in a LoadModule, it is useful
    ///    to arrange the segments sequentially in the order that they appear
    ///    in the image.
    /// 2) When using a RangeDictionary to associate ranges of bytes to a
    ///    value, it is easier to use one single RangeDictionary than using
    ///    a separate RangeDictionary for each individual segment.
    ///    
    /// Therefore, we define a lexical order on the addresses, by first
    /// ordering by segment id, and then ordering by offset. This simple
    /// solution is adequate for the requirements above.
    /// </remarks>
    public struct Address : IComparable<Address>
    {
        readonly int segment;
        readonly int offset;

        public Address(int segment, int offset)
        {
            this.segment = segment;
            this.offset = offset;
        }

        /// <summary>
        /// Gets the id of the segment that contains this address. A value of
        /// zero indicates an invalid segment (and thus an invalid address).
        /// </summary>
        public int Segment
        {
            get { return segment; }
        }

        /// <summary>
        /// Gets the offset of this address within the segment.
        /// </summary>
        public int Offset
        {
            get { return offset; }
        }

        public static bool operator ==(Address a, Address b)
        {
            return (a.segment == b.segment) && (a.offset == b.offset);
        }

        public static bool operator !=(Address a, Address b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return (obj is Address) && (this == (Address)obj);
        }

        public override int GetHashCode()
        {
            return (segment << 16) | offset;
        }

        /// <summary>
        /// Represents an invalid (null) address.
        /// </summary>
        public static readonly Address Invalid = new Address(-1, -1);

        /// <summary>
        /// Compares this address to another address lexicographically; that
        /// is, first the segment selectors are compared, then the offsets
        /// are compared.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Address other)
        {
            int cmp = this.segment.CompareTo(other.segment);
            if (cmp == 0)
                cmp = this.offset.CompareTo(other.offset);
            return cmp;
        }

        /// <summary>
        /// Increments the offset by the given amount. The resulting offset
        /// may be negative or greater than 0xFFFF.
        /// </summary>
        public static Address operator +(Address address, int increment)
        {
            return new Address(address.segment, address.offset + increment);
        }

        /// <summary>
        /// Decrements the offset by the given amount. The resulting offset
        /// may be negative or greater than 0xFFFF.
        /// </summary>
        public static Address operator -(Address address, int decrement)
        {
            return new Address(address.segment, address.offset - decrement);
        }

        public override string ToString()
        {
            return string.Format("seg{0:0000}:{1:X4}", segment, offset);
        }
    }

    public struct PhysicalAddress : IAddressReferent
    {
        public UInt16 Frame { get; private set; }
        public UInt16 Offset { get; private set; }

        public PhysicalAddress(UInt16 frame, UInt16 offset)
            : this()
        {
            this.Frame = frame;
            this.Offset = offset;
        }

        string IAddressReferent.Label
        {
            get { return string.Format("X4:X4", Frame, Offset); }
        }

        public Address Resolve()
        {
            throw new NotSupportedException();
        }
    }

#if false
    /// <summary>
    /// Represents an address expressed as base:offset. This is used in
    /// situations where the distinction between base and offset is
    /// significant, such as in an load module.
    /// </summary>
    public struct Pointer
    {
        private readonly UInt16 _base;
        private readonly UInt16 _offset;

        public Pointer(UInt16 Base, UInt16 Offset)
        {
            this._base = Base;
            this._offset = Offset;
        }

        /// <summary>
        /// Gets the base (frame) component of the pointer.
        /// </summary>
        public UInt16 Base
        {
            get { return _base; }
        }

        /// <summary>
        /// Gets the offset component of the pointer.
        /// </summary>
        public UInt16 Offset
        {
            get { return _offset; }
        }

        public int LinearAddress
        {
            get { return _base * 16 + _offset; }
        }

        public override string ToString()
        {
            return string.Format("{0:X4}:{1:X4}", _base, _offset);
        }

#if false
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
#endif

        /// <summary>
        /// Returns true if two pointers have the same base and offset values.
        /// </summary>
        /// <param name="a">First pointer.</param>
        /// <param name="b">Second pointer.</param>
        /// <returns></returns>
        public static bool operator ==(Pointer a, Pointer b)
        {
            return (a._base == b._base) && (a._offset == b._offset);
        }

        /// <summary>
        /// Returns true unless two pointers have the same base and offset
        /// values.
        /// </summary>
        /// <param name="a">First pointer.</param>
        /// <param name="b">Second pointer.</param>
        /// <returns></returns>
        public static bool operator !=(Pointer a, Pointer b)
        {
            return !(a == b);
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
#endif
}

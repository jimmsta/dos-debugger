using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2
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
        ResolvedAddress Resolve();
    }

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

        public ResolvedAddress ResolvedAddress
        {
            get
            {
                ResolvedAddress address = Referent.Resolve();
                return new ResolvedAddress(address.Image, address.Offset + this.Displacement);
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

    public class AddressWrappedException : Exception
    {
    }

    /// <summary>
    /// Represents a unique address in an assembly, expressed as an offset
    /// within a specific image chunk. Note that a ResolvedAddress is not
    /// related to the physical address of an image when loaded into memory.
    /// </summary>
    public struct ResolvedAddress
    {
        private readonly ImageChunk image;
        private readonly int offset;

        public ResolvedAddress(ImageChunk image, int offset)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            
            this.image = image;
            this.offset = offset;
        }

        /// <summary>
        /// Gets the image that contains this resolved address.
        /// </summary>
        public ImageChunk Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the offset of this address within the image.
        /// </summary>
        public int Offset
        {
            get { return offset; }
        }

        public bool IsValid
        {
            get { return (Offset >= 0) && (Offset < Image.Length); }
        }

        public ImageByte ImageByte
        {
            get { return Image[Offset]; }
        }

        public static bool operator ==(ResolvedAddress a, ResolvedAddress b)
        {
            return (a.Image == b.Image) && (a.Offset == b.Offset);
        }

        public static bool operator !=(ResolvedAddress a, ResolvedAddress b)
        {
            return (a.Image != b.Image) || (a.Offset != b.Offset);
        }

        public override bool Equals(object obj)
        {
            return (obj is ResolvedAddress) && (this == (ResolvedAddress)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Represents an invalid (null) resolved address.
        /// </summary>
        public static readonly ResolvedAddress Invalid = new ResolvedAddress();
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

        public ResolvedAddress Resolve()
        {
            throw new NotSupportedException();
        }
    }
}

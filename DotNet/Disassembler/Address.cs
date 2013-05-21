using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2
{
    /// <summary>
    /// Represents a logical object that can be used as an address reference.
    /// The address does not have to be physical. For example, a logical
    /// segment is addressible, but an immediate or EAX register is not.
    /// </summary>
    public interface IAddressable // may rename to IAddressReferent
    {
        /// <summary>
        /// Gets a string representation of the object's address. This address
        /// is not necessarily physical; for example, it can be something like
        /// "fopen._TEXT" or "_strcpy".
        /// </summary>
        string Label { get; }

        ResolvedAddress Resolve();
    }

    public struct PhysicalAddress : IAddressable
    {
        public UInt16 Frame { get; private set; }
        public UInt16 Offset { get; private set; }

        public PhysicalAddress(UInt16 frame, UInt16 offset)
            : this()
        {
            this.Frame = frame;
            this.Offset = offset;
        }

        string IAddressable.Label
        {
            get { return string.Format("X4:X4", Frame, Offset); }
        }

        public ResolvedAddress Resolve()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Represents a logical address in an assembly, expressed as referent +
    /// displacement. The displacement may be positive, zero, or negative.
    /// Multiple logical addresses may resolve to the same ResolvedAddress.
    /// </summary>
    public struct LogicalAddress
    {
        public IAddressable Referent { get; private set; }
        public int Displacement { get; private set; }

        public LogicalAddress(IAddressable referent, int offset)
            : this()
        {
            if (referent == null)
                throw new ArgumentNullException("referent");

            this.Referent = referent;
            this.Displacement = offset;
        }

        public ResolvedAddress Resolve()
        {
            ResolvedAddress address = Referent.Resolve();
            return new ResolvedAddress(address.Image, address.Offset + this.Displacement);
        }

        public static readonly LogicalAddress Invalid = new LogicalAddress();

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
    }

    /// <summary>
    /// Represents a unique address in an assembly, expressed as an offset
    /// within a specific image chunk. Note that a ResolvedAddress is not
    /// related to the physical address of an image when loaded into memory.
    /// </summary>
    public struct ResolvedAddress
    {
        public ImageChunk Image { get; private set; }
        public int Offset { get; private set; }

        public ResolvedAddress(ImageChunk image, int offset)
            : this()
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this.Image = image;
            this.Offset = offset;
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
    }
}

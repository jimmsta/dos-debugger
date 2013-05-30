using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Represents a segment in a binary image. A segment is a contiguous
    /// block of bytes that are addressible by the same segment selector.
    /// This segment selector is a zero-based index that identifies the
    /// segment within the binary image.
    /// </summary>
    public abstract class Segment : IAddressReferent // ?? should we implement IAddressReferent?
    {
        /// <summary>
        /// Gets the zero-based index of the segment within the binary image.
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        /// Gets the display name of the segment.
        /// </summary>
        public abstract string Name { get; }

#if false
        /// <summary>
        /// Gets the length of the segment. This is equal to the offset of
        /// the maximum addressible byte, plus one. Note that not all bytes
        /// within 0 to Length are necessarily valid.
        /// </summary>
        public abstract int Length { get; }
#endif

        /// <summary>
        /// Gets the range of addressible offsets within this segment. All
        /// bytes within this range must be accessible, i.e. they will have
        /// IsAddressValid() return true.
        /// 
        /// The return value may change overtime, but it can only shrink
        /// and must never grow. This ensures that the return value from
        /// the very first call to OffsetBounds can be used to allocate a
        /// sufficiently large buffer.
        /// 
        /// The returned range does not necessarily start from offset zero.
        /// </summary>
        public abstract Range<int> OffsetBounds { get; }

        protected virtual string GetLabel()
        {
            throw new NotImplementedException();
        }

        string IAddressReferent.Label
        {
            get { return GetLabel(); }
        }

        public virtual Address Resolve()
        {
            return new Address(this.Id, 0);
        }
    }

    public enum SegmentType
    {
        /// <summary>
        /// Indicates a logical segment.
        /// </summary>
        /// <remarks>
        /// A logical segment is not associated with a canonical frame, and
        /// therefore does not have a frame number. The offset within a
        /// logical segment is relative to the beginning of the segment,
        /// and may be changed at run-time; therefore it is not meaningful
        /// to address an offset relative to the segment; only self-relative
        /// addressing should be used.
        /// 
        /// A logical segment may be combined with other logical segments to
        /// form a relocatable segment.
        /// </remarks>
        Logical,

        /// <summary>
        /// Indicates a relocatable segment.
        /// </summary>
        /// <remarks>
        /// A relocatable segment is associated with a canonical frame,
        /// which is the frame with the largest frame number that contains
        /// the segment. This frame number is meaningful, but it is subject
        /// to relocation when the image is loaded into memory.
        /// 
        /// An offset within a relocatable segment is relative to the
        /// canonical frame that contains the segment, and NOT relative to
        /// the beginning of the segment. To avoid confusion, it is convenient
        /// to think of a relocatable segment as always starting at paragraph
        /// boundary, though in practice the first few bytes may actually be
        /// used by a previous segment.
        /// </remarks>
        Relocatable,
    }

    public class SegmentCollection : List<Segment>
    {
        public T Get<T>(int index) where T : Segment
        {
            return (T)base[index];
        }
    }
}

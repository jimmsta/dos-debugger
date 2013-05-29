using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Represents a segment in a binary image. A segment is a contiguous
    /// block of bytes that are addressible with the same segment selector.
    /// This segment selector is also called "SegmentId" here.
    ///
    /// A relocatable segment in an executable, or a logical segment in an
    /// object library.
    /// </summary>
    public abstract class Segment : IAddressReferent // ?? should we implement IAddressReferent?
    {
        private int id;

        /// <summary>
        /// Gets or sets the ID of the segment. This ID uniquely identifies
        /// the segment within its containing assembly. This ID may be set
        /// only once.
        /// </summary>
        public int Id
        {
            get { return id; }
            set
            {
                if (id != 0)
                    throw new InvalidOperationException();
                id = value;
            }
        }

        //public abstract ImageChunk Image { get; }

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
}

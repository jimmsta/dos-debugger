using System;
using System.Collections.Generic;
using Util.Data;

namespace Disassembler
{
    /// <summary>
    /// Provides information about a segment in a binary image.
    /// </summary>
    /// <remarks>
    /// A segment is a contiguous block of bytes that are addressible by the
    /// same segment selector. This segment selector is a zero-based index
    /// that identifies the segment within the binary image.
    /// </remarks>
    public interface Segment
    {
        /// <summary>
        /// Gets the zero-based index of the segment within the binary image.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the display name of the segment.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the range of accessible offsets within this segment. All
        /// bytes within this range will have IsAddressValid() return true.
        /// 
        /// The returned range may change overtime, but it can only shrink
        /// and must never grow. This ensures that the return value from
        /// any call to OffsetBounds can be used to allocate a sufficiently
        /// large buffer for succeeding operations.
        /// 
        /// The returned range does not necessarily start from offset zero.
        /// </summary>
        Range<int> OffsetBounds { get; }
    }

#if false
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
#endif

    /// <summary>
    /// Represents a collection of segments. This collection can only be
    /// added to and queried. Existing items in the collection cannot be
    /// modified.
    /// </summary>
    public class SegmentCollection : IList<Segment>
    {
        readonly List<Segment> segments = new List<Segment>();

        public T Get<T>(int index) where T : Segment
        {
            return (T)segments[index];
        }

        public Segment this[int index]
        {
            get { return segments[index]; }
            set { throw new NotSupportedException(); }
        }

        public bool Contains(Segment item)
        {
            return segments.Contains(item);
        }

        public void CopyTo(Segment[] array, int arrayIndex)
        {
            segments.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return segments.Count; }
        }

        public int IndexOf(Segment item)
        {
            return segments.IndexOf(item);
        }

        public void Add(Segment segment)
        {
            if (segment == null)
                throw new ArgumentNullException("segment");
            if (segment.Id != segments.Count)
                throw new ArgumentException("The segment to add has inconsistent id.");

            segments.Add(segment);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        void ICollection<Segment>.Clear()
        {
            throw new NotSupportedException();
        }

        void IList<Segment>.Insert(int index, Segment item)
        {
            throw new NotSupportedException();
        }

        void IList<Segment>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public bool Remove(Segment item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<Segment> GetEnumerator()
        {
            return segments.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

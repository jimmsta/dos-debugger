using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Util.Data;
using X86Codec;

namespace Disassembler2
{
    /// <summary>
    /// Contains information about a fix-up to be applied to a given range of
    /// bytes in a binary image.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Fixup
    {
        /// <summary>
        /// Gets or sets the start index to apply the fix-up, relative to the
        /// beginning of an image.
        /// </summary>
        public int StartIndex { get; internal set; }

        public int EndIndex
        {
            get { return StartIndex + Length; }
        }

#if false
        /// <summary>
        /// Gets the location to fix up.
        /// </summary>
        public Range<int> Location
        {
            get { return location; }
        }
#endif

        /// <summary>
        /// Gets or sets the type of data to fix in that location.
        /// </summary>
        public FixupLocationType LocationType { get; internal set; }

        /// <summary>
        /// Gets the number of bytes to fix.
        /// </summary>
        public int Length
        {
            get { return GetLengthFromLocationType(LocationType); }
        }

        /// <summary>
        /// Gets or sets the fix-up mode.
        /// </summary>
        public FixupMode Mode { get; internal set; }

        /// <summary>
        /// Gets or sets the fix-up target.
        /// </summary>
        public SymbolicTarget Target { get; internal set; }

        /// <summary>
        /// Gets or sets the target frame relative to which to apply the
        /// fix up.
        /// </summary>
        public FixupFrame Frame { get; internal set; }

        public override string ToString()
        {
            return string.Format(
                "Type={0},Mode={1},Target={2}",
                LocationType, Mode, Target);
        }

        /// <summary>
        /// Gets the number of bytes to fix up. This is inferred from the
        /// LocationType property.
        /// </summary>
        private static int GetLengthFromLocationType(FixupLocationType type)
        {
            switch (type)
            {
                case FixupLocationType.LowByte:
                    return 1;
                case FixupLocationType.Offset:
                case FixupLocationType.Base:
                    return 2;
                case FixupLocationType.Pointer:
                    return 4;
                default:
                    return 0;
            }
        }
    }

    public enum FixupMode : byte
    {
        SelfRelative = 0,
        SegmentRelative = 1
    }

    /// <summary>
    /// Specifies the type of data to fix up in that location.
    /// </summary>
    public enum FixupLocationType : byte
    {
        /// <summary>The fixup location type is unknown.</summary>
        Unknown,

        /// <summary>
        /// 8-bit displacement or low byte of 16-bit offset.
        /// </summary>
        LowByte,

        /// <summary>16-bit offset.</summary>
        Offset,

        /// <summary>16-bit base.</summary>
        Base,

        /// <summary>32-bit pointer (16-bit base:16-bit offset).</summary>
        Pointer,
    }

    public struct FixupFrame
    {
        public FixupFrameMethod Method { get; internal set; }

        /// <summary>
        /// Gets or sets the INDEX of the SEG/GRP/EXT item that is used as
        /// the referent to find the frame. This is used only if Method is
        /// one of 0, 1, or 2. If Method is 3, then this field contains an
        /// absolute frame number. If Method is 4-7, this field is not used.
        /// </summary>
        public UInt16 IndexOrFrame { get; internal set; }

        public override string ToString()
        {
            switch (Method)
            {
                case FixupFrameMethod.SegmentIndex:
                    return string.Format("SEG({0})", IndexOrFrame);
                case FixupFrameMethod.GroupIndex:
                    return string.Format("GRP({0})", IndexOrFrame);
                case FixupFrameMethod.ExternalIndex:
                    return string.Format("EXT({0})", IndexOrFrame);
                case FixupFrameMethod.ExplicitFrame:
                    return string.Format("{0:X4}", IndexOrFrame);
                case FixupFrameMethod.UseLocation:
                    return "LOCATION";
                case FixupFrameMethod.UseTarget:
                    return "TARGET";
                default:
                    return "(invalid)";
            }
        }
    }

    public enum FixupFrameMethod : byte
    {
        /// <summary>
        /// The FRAME is the canonical frame of the LSEG (logical segment)
        /// identified by Index.
        /// </summary>
        SegmentIndex = 0,

        /// <summary>
        /// The FRAME is the canonical frame of the group identified by Index.
        /// </summary>
        GroupIndex = 1,

        /// <summary>
        /// The FRAME is determined according to the External Name's PUBDEF
        /// record. There are three cases:
        /// a) If there is an associated group with the symbol, the canonical
        ///    frame of that group is used; otherwise,
        /// b) If the symbol is defined relative to some LSEG, the canonical
        ///    frame of the LSEG is used.
        /// c) If the symbol is defined at an absolute address, the frame of
        ///    this absolute address is used.
        /// </summary>
        ExternalIndex = 2,

        /// <summary>
        /// The FRAME is specified explicitly by a number. This method is not
        /// supported by any linker.
        /// </summary>
        ExplicitFrame = 3,

        /// <summary>
        /// The FRAME is the canonic FRAME of the LSEG containing LOCATION.
        /// If the location is defined by an absolute address, the frame 
        /// component of that address is used.
        /// </summary>
        UseLocation = 4,

        /// <summary>
        /// The FRAME is determined by the TARGET's segment, group, or
        /// external index.
        /// </summary>
        UseTarget = 5,
    }

    public class BrokenFixupException : InvalidInstructionException
    {
        public Fixup Fixup { get; private set; }

        public BrokenFixupException(Fixup fixup)
        {
            this.Fixup = fixup;
        }
    }

    /// <summary>
    /// Provides methods to access the fixups defined on this image chunk.
    /// </summary>
    public class FixupCollection : IList<Fixup>
    {
        readonly List<Fixup> fixups = new List<Fixup>();

        public FixupCollection()
        {
        }

        /// <summary>
        /// Finds the fixup associated with the given position. If no
        /// fixup is found, find the first one that comes after that
        /// position.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int BinarySearch(int offset)
        {
            int k = fixups.BinarySearch(offset, CompareFixupWithOffset);
            while (k > 0 && CompareFixupWithOffset(fixups[k - 1], offset) == 0)
                k--;
            return k;
        }

        private static int CompareFixupWithOffset(Fixup fixup, int offset)
        {
            if (fixup.StartIndex > offset)
                return 1;
            else if (fixup.EndIndex > offset)
                return 0;
            else
                return -1;
        }

        public int IndexOf(Fixup item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, Fixup item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public Fixup this[int index]
        {
            get { return fixups[index]; }
            set { throw new NotSupportedException(); }
        }

        public void Add(Fixup fixup)
        {
            if (fixup == null)
                throw new ArgumentNullException("fixup");

            int k = BinarySearch(fixup.StartIndex);
            if (k >= 0) // already exists
            {
                System.Diagnostics.Debug.WriteLine("FixupCollection: Overlaps with an existing fixup.");
                return;
            }

            k = ~k;
            if (k > 0 && fixups[k - 1].EndIndex > fixup.StartIndex ||
                k < fixups.Count && fixup.EndIndex > fixups[k].StartIndex)
            {
                System.Diagnostics.Debug.WriteLine("FixupCollection: Overlaps with an existing fixup.");
                return;
            }
            fixups.Insert(k, fixup);
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(Fixup item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Fixup[] array, int arrayIndex)
        {
            fixups.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return fixups.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Fixup item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<Fixup> GetEnumerator()
        {
            return fixups.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Util.Data;
using X86Codec;

namespace Disassembler
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
        public FileFormats.Omf.FixupMode Mode { get; internal set; }

        /// <summary>
        /// Gets or sets the fix-up target.
        /// </summary>
        public SymbolicTarget Target { get; internal set; }

        /// <summary>
        /// Gets or sets the target frame relative to which to apply the
        /// fix up.
        /// </summary>
        public FileFormats.Omf.FixupFrame Frame { get; internal set; }

        public override string ToString()
        {
            return string.Format(
                "[{0:X4},{1:X4}) => {2} of {3}",
                StartIndex, EndIndex, LocationType, Target);
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

    public class BrokenFixupException : InvalidInstructionException
    {
        public Fixup Fixup { get; private set; }

        public BrokenFixupException(Fixup fixup)
        {
            this.Fixup = fixup;
        }
    }

    /// <summary>
    /// Provides methods to access the fixups defined on an image chunk.
    /// </summary>
    public class FixupCollection : IList<Fixup>
    {
        readonly List<Fixup> fixups = new List<Fixup>();

        public FixupCollection()
        {
        }

        /// <summary>
        /// Gets or sets the name for this fixup collection.
        /// </summary>
        public string Name { get; set; }

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
                WarnOverlap(fixups[k], fixup);
                return;
            }

            k = ~k;
            if (k > 0 && fixups[k - 1].EndIndex > fixup.StartIndex)
            {
                WarnOverlap(fixups[k - 1], fixup);
                return;
            }
            if (k < fixups.Count && fixup.EndIndex > fixups[k].StartIndex)
            {
                WarnOverlap(fixups[k], fixup);
                return;
            }
            fixups.Insert(k, fixup);
        }

        private void WarnOverlap(Fixup existing, Fixup newone)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(
                "FixupCollection({2}): Overlaps with an existing fixup: existing={0}, new={1}.",
                existing, newone, this.Name));
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

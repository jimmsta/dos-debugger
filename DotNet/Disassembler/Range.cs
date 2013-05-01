using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Disassembler
{
#if false
    /// <summary>
    /// Represents a collection of disjoint ranges.
    /// </summary>
    public class MultiRange
    {
        private LinkedList<Range> ranges = new LinkedList<Range>();

        public MultiRange()
        {
        }

        /// <summary>
        /// Returns true if this multi-range is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return ranges.Count == 0; }
        }

        /// <summary>
        /// Gets the smallest range that covers this multi-range.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the multi-range
        /// is empty.</exception>
        public Range BoundingRange
        {
            get
            {
                if (this.IsEmpty)
                {
                    throw new InvalidOperationException(
                        "Cannot get BoundingRange of an empty multi-range.");
                }
                return new Range(ranges.First.Value.Begin, ranges.Last.Value.End);
            }
        }

        /// <summary>
        /// Gets the disjoint ranges that constitute this multi-range. The
        /// ranges are stored in acsending order.
        /// </summary>
        /// TODO: we need to change to something like ReadOnlyCollection.
        public LinkedList<Range> Intervals
        {
            get { return ranges; }
        }

        /// <summary>
        /// Gets the total number of elements in the range.
        /// </summary>
        public int Length
        {
            get
            {
                int size = 0;
                foreach (Range range in ranges)
                {
                    size += range.Length;
                }
                return size;
            }
        }

        // TODO: we need to improve this signature and its implementation.
        public void AddInterval(int begin, int end)
        {
            if (end < begin)
                throw new ArgumentException("'end' must be greater than or equal to 'begin'.");
            if (begin == end) // empty interval
                return;

            if (ranges.Count == 0)
            {
                ranges.AddFirst(new Range(begin, end));
            }
            else if (ranges.Last.Value.End == begin)
            {
                ranges.Last.Value = new Range(ranges.Last.Value.Begin, end);
            }
            else if (ranges.Last.Value.End < begin)
            {
                ranges.AddLast(new Range(begin, end));
            }
            else if (ranges.First.Value.Begin == end)
            {
                ranges.First.Value = new Range(begin, ranges.First.Value.End);
            }
            else if (ranges.First.Value.Begin > end)
            {
                ranges.AddFirst(new Range(begin, end));
            }
            else
            {
                LinkedListNode<Range> node;

                // Find the first interval where begin <= Interval.End.
                // This interval will be extended to include end-1.
                node = ranges.First;
                while (!(begin <= node.Value.End))
                    node = node.Next;

                // Extend node.End to include 'end'-1.
                if (begin < node.Value.Begin)
                {
                    if (end < node.Value.Begin)
                    {
                        ranges.AddBefore(node, new Range(begin, end));
                    }
                    else if (end == node.Value.Begin)
                    {
                        node.Value = new Range(begin, node.Value.End);
                    }
                }
                while (node.Value.End < end)
                {
                    if (node.Next == null ||
                        node.Next.Value.Begin > end)
                    {
                        // Extend this interval and exit.
                        node.Value = new Range(node.Value.Begin, end);
                        break;
                    }
                    else
                    {
                        // Merge this interval and the next interval.
                        node.Value = new Range(node.Value.Begin, node.Next.Value.End);
                        ranges.Remove(node.Next);
                    }
                }
            }
        }
    }
#endif

#if false
    /// <summary>
    /// Represents a C++/STL-style range [begin, end) where 'begin' and 'end'
    /// are of type T where T is comparable.
    /// </summary>
    public struct Range
    {
        private int begin;
        private int end;

        /// <summary>
        /// Gets or sets the begin-index of the interval.
        /// </summary>
        public int Begin { get { return begin; } }

        /// <summary>
        /// Gets or sets the end-index, i.e. one past the last element in
        /// the interval.
        /// </summary>
        public int End { get { return end; } }

        /// <summary>
        /// Gets the length of the range, i.e. (end - begin).
        /// </summary>
        public int Length
        {
            get { return end - begin; }
        }

        /// <summary>
        /// Creates a range with the given begin and end indices.
        /// </summary>
        /// <param name="begin">Begin index, inclusive.</param>
        /// <param name="end">End index, exclusive.</param>
        /// <exception cref="ArgumentException">If begin is greater than end.
        /// </exception>
        public Range(int begin, int end)
            : this()
        {
            if (end < begin)
                throw new ArgumentException("'end' must be greater than or equal to 'begin'.");

            this.begin = begin;
            this.end = end;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1})", begin, end);
        }
    }
#else
    /// <summary>
    /// Represents a C++/STL-style range [begin, end) where 'begin' and 'end'
    /// are of type T where T is comparable.
    /// </summary>
    public struct Range<T> where T : IComparable<T>
    {
        private T begin;
        private T end;

        /// <summary>
        /// Creates a half-open range with the given start and end points.
        /// The start point is included in the range, but the end point is
        /// excluded from the range.
        /// </summary>
        /// <param name="begin">The start point, inclusive.</param>
        /// <param name="end">The end point, exclusive.</param>
        /// <exception cref="ArgumentException">If start is greater than end.
        /// </exception>
        public Range(T begin, T end)
            : this()
        {
            if (begin.CompareTo(end) > 0)
                throw new ArgumentException("'begin' must be less than or equal to 'end'.");

            this.begin = begin;
            this.end = end;
        }

        /// <summary>
        /// Gets the begin-index of the range, inclusive.
        /// </summary>
        public T Begin { get { return begin; } }

        /// <summary>
        /// Gets the end-index of the range, exclusive.
        /// </summary>
        public T End { get { return end; } }

        /// <summary>
        /// Returns true if the range is empty, i.e. Begin == End.
        /// </summary>
        public bool IsEmpty
        {
            get { return begin.CompareTo(end) == 0; }
        }

        /// <summary>
        /// Converts the range into a string.
        /// </summary>
        /// <returns>A string in the format "[start, end)".</returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1})", begin, end);
        }
    }
#endif
}

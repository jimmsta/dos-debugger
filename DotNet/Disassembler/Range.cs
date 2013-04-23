using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Represents a collection of disjoint integer intervals.
    /// </summary>
    public class Range
    {
        private LinkedList<Interval> intervals = new LinkedList<Interval>();

        public Range()
        {
        }

        /// <summary>
        /// Gets the disjoint intervals that constitute this range. The
        /// intervals are returned in order.
        /// </summary>
        public IEnumerable<Interval> Intervals
        {
            get { return intervals; }
        }

        /// <summary>
        /// Gets the total size, in bytes, of the range.
        /// </summary>
        public int Length
        {
            get
            {
                int size = 0;
                foreach (Interval interval in intervals)
                {
                    size += interval.Length;
                }
                return size;
            }
        }

        /// <summary>
        /// Gets the number of disjoint intervals in the range.
        /// </summary>
        public int IntervalCount
        {
            get { return intervals.Count; }
        }

        public void AddInterval(int begin, int end)
        {
            if (end < begin)
                throw new ArgumentException("'end' must be greater than or equal to 'begin'.");
            if (begin == end) // empty interval
                return;

            if (begin == 0x267)
            {
                int kk = 1;
            }

            if (intervals.Count == 0)
            {
                intervals.AddFirst(new Interval(begin, end));
            }
            else if (intervals.Last.Value.End == begin)
            {
                intervals.Last.Value = new Interval(intervals.Last.Value.Begin, end);
            }
            else if (intervals.Last.Value.End < begin)
            {
                intervals.AddLast(new Interval(begin, end));
            }
            else if (intervals.First.Value.Begin == end)
            {
                intervals.First.Value = new Interval(begin, intervals.First.Value.End);
            }
            else if (intervals.First.Value.Begin > end)
            {
                intervals.AddFirst(new Interval(begin, end));
            }
            else
            {
                LinkedListNode<Interval> node;

                // Find the first interval where begin <= Interval.End.
                // This interval will be extended to include end-1.
                node = intervals.First;
                while (!(begin <= node.Value.End))
                    node = node.Next;

                // Extend node.End to include 'end'-1.
                if (begin < node.Value.Begin)
                {
                    if (end < node.Value.Begin)
                    {
                        intervals.AddBefore(node, new Interval(begin, end));
                    }
                    else if (end == node.Value.Begin)
                    {
                        node.Value = new Interval(begin, node.Value.End);
                    }
                }
                while (node.Value.End < end)
                {
                    if (node.Next == null ||
                        node.Next.Value.Begin > end)
                    {
                        // Extend this interval and exit.
                        node.Value = new Interval(node.Value.Begin, end);
                        break;
                    }
                    else
                    {
                        // Merge this interval and the next interval.
                        node.Value = new Interval(node.Value.Begin, node.Next.Value.End);
                        intervals.Remove(node.Next);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a C++/STL-style interval [begin, end).
    /// </summary>
    public struct Interval
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
        /// Gets the length of the interval.
        /// </summary>
        public int Length
        {
            get { return end - begin; }
        }

        public Interval(int begin, int end)
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
}

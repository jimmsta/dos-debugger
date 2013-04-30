using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Represents a min-priority queue. This priority queue is internally
    /// implemented by a binary heap.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// A binary heap is a complete binary tree that satisfies the additional
    /// condition that every node is smaller than or equal to its two child
    /// nodes. Below is an example heap with 6 elements:
    /// 
    ///         [0]
    ///        /   \
    ///     [1]     [2]
    ///    /   \   /   \
    ///   [3] [4] [5]  _
    /// </remarks>
    public class PriorityQueue<T> : ICollection<T>
    {
        IComparer<T> comparer;
        List<T> list;

        /// <summary>
        /// Creates a priority queue using the given comparer to compare for
        /// priority. Since this is a min-priority queue, a smaller element
        /// has a higher priority.
        /// </summary>
        /// <param name="comparer"></param>
        public PriorityQueue(IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            this.list = new List<T>();
            this.comparer = comparer;
        }

        /// <summary>
        /// Creates a priority queue using the default comparer for T to
        /// compare for priority. Since this is a min-priority queue, a
        /// smaller element has a higher priority.
        /// </summary>
        public PriorityQueue()
            : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Gets the number of elements in the priority queue.
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        public bool IsEmpty
        {
            get { return list.Count == 0; }
        }

        /// <summary>
        /// Clears the priority queue.
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// Gets the first (smallest) element in the priority queue without
        /// removing it.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the queue is empty.
        /// </exception>
        public T Peek()
        {
            if (list.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");
            return list[0];
        }

        /// <summary>
        /// Swaps node i with node j.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void Swap(int i, int j)
        {
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        /// <summary>
        /// Adds an item into the queue.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            // Add item to the end of the heap.
            int k = list.Count;
            list.Add(item);

            // Swap the item with its parent repeatedly until its
            // parent is smaller than the item.
            while (k > 0)
            {
                int parent = (k - 1) / 2;
                if (comparer.Compare(list[parent], list[k]) > 0)
                {
                    Swap(k, parent);
                    k = parent;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// Removes and returns the first (smallest) element in the priority
        /// queue.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the queue is empty.
        /// </exception>
        public T Dequeue()
        {
            if (list.Count == 0)
                throw new InvalidOperationException("The priority queue is empty.");

            T item = list[0];
            Swap(0, list.Count - 1);
            list.RemoveAt(list.Count - 1);
            if (list.Count == 0)
                return item;

            int k = 0;
            int count=list.Count;

            // Swap node[k] with its child until in order.
            while (true)
            {
                int left = 2 * k + 1;
                int right = 2 * k + 2;

                int smallest = k;
                if (left < count && comparer.Compare(list[left], list[smallest]) < 0)
                    smallest = left;
                if (right < count && comparer.Compare(list[right], list[smallest]) < 0)
                    smallest = right;

                if (smallest != k)
                {
                    Swap(k, smallest);
                    k = smallest;
                }
                else
                    break;
            }
            return item;
        }

        #region ICollection<T> Interface Implementation

        public void Add(T item)
        {
            Enqueue(item);
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion
    }
}

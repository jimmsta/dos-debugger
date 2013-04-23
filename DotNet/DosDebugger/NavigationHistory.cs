using System;
using System.Collections.Generic;
using System.Text;

namespace DosDebugger
{
    /// <summary>
    /// Keeps track of the navigation history.
    /// </summary>
    class NavigationHistory<T>
    {
        private LinkedList<T> history = new LinkedList<T>();
        private LinkedListNode<T> current = null;

        public NavigationHistory()
        {
        }

        public void Clear()
        {
            this.history.Clear();
            this.current = null;
            RaiseChanged();
        }

        public bool CanGoBackward
        {
            get { return current != null && current.Previous != null; }
        }

        public bool CanGoForward
        {
            get { return current != null && current.Next != null; }
        }

        public T GoBackward()
        {
            if (!CanGoBackward)
                throw new InvalidOperationException("Cannot go backward.");

            current = current.Previous;
            RaiseChanged();
            return current.Value;
        }

        public T GoForward()
        {
            if (!CanGoForward)
                throw new InvalidOperationException("Cannot go forward.");

            current = current.Next;
            RaiseChanged();
            return current.Value;
        }

        public void GoTo(T pos)
        {
            if (current == null)
            {
                current = history.AddFirst(pos);
                RaiseChanged();
                return;
            }

            if (pos.Equals(current.Value))
            {
                return;
            }
            if (current.Next != null && pos.Equals(current.Next.Value))
            {
                current = current.Next;
                RaiseChanged();
                return;
            }

            while (current.Next != null)
            {
                history.RemoveLast();
            }
            current = history.AddAfter(current, pos);
            RaiseChanged();
        }

        public event EventHandler Changed;

        private void RaiseChanged()
        {
            if (Changed != null)
                Changed(this, null);
        }
    }
}

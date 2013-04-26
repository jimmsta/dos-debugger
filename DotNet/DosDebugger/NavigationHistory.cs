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

    /// <summary>
    /// Represents a token that keeps track of the current navigation
    /// location. Multiple objects can "connect" to this token to change the
    /// navigation location and be notified of such changes.
    /// </summary>
    class NavigationPoint<T>
    {
        T location;

        public T Location
        {
            get { return location; }
        }

        public void SetLocation(T location, object source)
        {
            LocationChangedEventArgs<T> e = new LocationChangedEventArgs<T>();
            e.OldLocation = this.location;
            e.NewLocation = location;
            e.Source = source;

            this.location = location;
            if (LocationChanged != null)
            {
                LocationChanged(this, e);
            }
        }

        public event EventHandler<LocationChangedEventArgs<T>> LocationChanged;
    }

    class LocationChangedEventArgs<T> : EventArgs
    {
        public T NewLocation { get; set; }
        public T OldLocation { get; set; }
        public object Source { get; set; }
    }
}

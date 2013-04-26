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
        private List<T> history = new List<T>();
        private int current = -1;

        public NavigationHistory()
        {
        }

        /// <summary>
        /// Clears the navigation history.
        /// </summary>
        public void Clear()
        {
            this.history.Clear();
            this.current = -1;
        }

        /// <summary>
        /// Gets the current location.
        /// </summary>
        public T Current
        {
            get
            {
                if (history.Count == 0)
                    throw new InvalidOperationException("The history is empty.");
                return history[current];
            }
        }

        /// <summary>
        /// Returns the historical locations starting from the most recent
        /// location back to the first location.
        /// </summary>
        public IEnumerable<T> History
        {
            get
            {
                for (int index = current - 1; index >= 0; index--)
                {
                    yield return history[index];
                }
            }
        }

        /// <summary>
        /// Checks whether we can move the given number of steps backward
        /// or forward within the history.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool CanMove(int offset)
        {
            int i = current + offset;
            return (i >= 0) && (i < history.Count);
        }

        public T Peek(int offset)
        {
            if (!CanMove(offset))
                throw new ArgumentException("The given offset is out of range.");

            return history[current + offset];
        }

        /// <summary>
        /// Moves the given number of steps backward or forward within the
        /// history.
        /// </summary>
        /// <param name="offset"></param>
        public void Move(int offset)
        {
            if (!CanMove(offset))
                throw new InvalidOperationException("Cannot move the given offset.");

            current += offset;
        }

        /// <summary>
        /// Adds a location after the current position.
        /// </summary>
        /// <param name="location"></param>
        public void Add(T location)
        {
            if (history.Count > 0)
            {
                history.RemoveRange(current + 1, history.Count - current - 1);
            }
            history.Add(location);
            current++;
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

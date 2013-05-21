using System;
using System.Collections.Generic;
using Util.Data;

namespace Disassembler2
{
    /// <summary>
    /// Represents a cross-reference between code and code or code and data.
    /// A xref between code and code is analog to an edge in a Control Flow 
    /// Graph.
    /// </summary>
    public class XRef : IGraphEdge<LogicalAddress>
    {
        /// <summary>
        /// Gets the target address being referenced. This may be set to
        /// <code>LogicalAddress.Invalid</code> if the target address cannot
        /// be determined, such as in a dynamic jump or call.
        /// </summary>
        public LogicalAddress Target { get; private set; }

        /// <summary>
        /// Gets the source address that refers to target. This may be set to
        /// <code>LogicalAddress.Invalid</code> if the source address cannot
        /// be determined, such as in the entry routine of a program.
        /// </summary>
        public LogicalAddress Source { get; private set; }

        /// <summary>
        /// Gets the type of this cross-reference.
        /// </summary>
        public XRefType Type { get; private set; }

        /// <summary>
        /// Gets the address of the associated data item. This is relevant
        /// if Type is NearIndexedJump or FarIndexedJump, where DataLocation
        /// contains the address of the jump table entry.
        /// </summary>
        public LogicalAddress DataLocation { get; private set; }

        /// <summary>
        /// Returns true if this xref is dynamic, i.e. its Target address
        /// contains <code>LogicalAddress.Invalid</code>.
        /// </summary>
        public bool IsDynamic
        {
            get { return Target == LogicalAddress.Invalid; }
        }

        public XRef()
        {
            this.Source = LogicalAddress.Invalid;
            this.Target = LogicalAddress.Invalid;
            this.DataLocation = LogicalAddress.Invalid;
        }

        public XRef(XRefType type, LogicalAddress source, LogicalAddress target, LogicalAddress dataLocation)
        {
            this.Source = source;
            this.Target = target;
            this.Type = type;
            this.DataLocation = dataLocation;
        }

        public XRef(XRefType type, LogicalAddress source, LogicalAddress target)
            : this(type, source, target, LogicalAddress.Invalid)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1} ({2})", Source, Target, Type);
        }

#if false
        /// <summary>
        /// Compares two XRef objects by source, target, and data location,
        /// in descending priority.
        /// </summary>
        public static int CompareByLocation(XRef x, XRef y)
        {
            int cmp = x.Source.LinearAddress.CompareTo(y.Source.LinearAddress);
            if (cmp == 0)
                cmp = x.Target.LinearAddress.CompareTo(y.Target.LinearAddress);
            if (cmp == 0)
                cmp = x.DataLocation.LinearAddress.CompareTo(y.DataLocation.LinearAddress);
            return cmp;
        }
#endif

        /// <summary>
        /// Compares two XRef objects by priority (precedence). An XRef object
        /// with a smaller numeric Type value has higher precedence, and 
        /// compare smaller (as in a min-priority queue).
        /// </summary>
        public static int CompareByPriority(XRef x, XRef y)
        {
            return (int)x.Type - (int)y.Type;
        }
    }

    /// <summary>
    /// Defines types of cross-references. The numeric values of the enum
    /// members are in decreasing order of their priority in analysis.
    /// </summary>
    public enum XRefType
    {
#if false
        /// <summary>
        /// User specified entry point (such as program start).
        /// </summary>
        UserSpecified,
#endif

        /// <summary>
        /// Indicates that the XRef object is invalid.
        /// </summary>
        None = 0,

        /// <summary>
        /// A JMPN instruction refers to this location.
        /// </summary>
        NearJump,

        /// <summary>
        /// A JMPF instruction refers to this location.
        /// </summary>
        FarJump,

        /// <summary>
        /// A CALLN instruction refers to this location.
        /// </summary>
        NearCall,

        /// <summary>
        /// A CALLF instruction refers to this location.
        /// </summary>
        FarCall,

        /// <summary>
        /// A Jcc instruction refers to this location. In the x86 instruction
        /// set, a conditional jump is always near and always relative (i.e.
        /// its target address can always be determined).
        /// </summary>
        ConditionalJump,

        /// <summary>
        /// A JUMP instruction where the jump target address is given by
        /// a memory location (such as jump table). ??????
        /// </summary>
        //IndirectJump,

#if false
    XREF_RETURN_FROM_CALL      = 5,    
    XREF_RETURN_FROM_INTERRUPT = 6,    
#endif

        /// <summary>
        /// A JMPN instruction refers to this location indirectly through
        /// a word-sized jump table entry. The address of the jump table
        /// entry is stored in the DataLocation field of the XRef object.
        /// </summary>
        NearIndexedJump,

        /// <summary>
        /// A JMPF instruction refers to this location indirectly through
        /// a dword-sized jump table entry. The address of the jump table
        /// entry is stored in the DataLocation field of the XRef object.
        /// </summary>
        /* FarIndexedJump, */
    }

    /// <summary>
    /// Maintains a collection of cross references and provides methods to
    /// find xrefs by source or target efficiently.
    /// </summary>
    public class XRefCollection : ICollection<XRef>
    {
        //private Range<LinearPointer> addressRange;
        private Graph<LogicalAddress, XRef> graph;

        /// <summary>
        /// Creates a cross reference collection.
        /// </summary>
        public XRefCollection()
        {
            //this.addressRange = addressRange;
            //this.graph = new Graph<LinearPointer, XRef>(XRef.CompareByLocation);
            this.graph = new Graph<LogicalAddress, XRef>();
        }

        /// <summary>
        /// Clears all the cross references stored in this collection.
        /// </summary>
        public void Clear()
        {
            graph.Clear();
        }

        /// <summary>
        /// Gets the number of cross references in this collection.
        /// </summary>
        public int Count
        {
            get { return graph.Edges.Count; }
        }

        public void Add(XRef xref)
        {
            if (xref == null)
            {
                throw new ArgumentNullException("xref");
            }
            if (xref.Source == LogicalAddress.Invalid &&
                xref.Target == LogicalAddress.Invalid)
            {
                throw new ArgumentException("can't have both source and target invalid");
            }

            graph.AddEdge(xref);

            // Raise the XRefAdded event.
            if (XRefAdded != null)
            {
                LogicalXRefAddedEventArgs e = new LogicalXRefAddedEventArgs(xref);
                XRefAdded(this, e);
            }
        }

        /// <summary>
        /// Gets a list of xrefs with dynamic target (i.e. target == Invalid).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XRef> GetDynamicReferences()
        {
            return graph.GetIncomingEdges(LogicalAddress.Invalid);
        }

        /// <summary>
        /// Gets all cross references that points to 'target', in the order
        /// that they were added.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<XRef> GetReferencesTo(LogicalAddress target)
        {
            return graph.GetIncomingEdges(target);
        }

        /// <summary>
        /// Gets all cross references that points from 'source'.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<XRef> GetReferencesFrom(LogicalAddress source)
        {
            return graph.GetOutgoingEdges(source);
        }

        public event EventHandler<LogicalXRefAddedEventArgs> XRefAdded;

        #region ICollection Interface Implementation

        public bool Contains(XRef item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(XRef[] array, int arrayIndex)
        {
            graph.Edges.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(XRef item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<XRef> GetEnumerator()
        {
            return graph.Edges.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class LogicalXRefAddedEventArgs : EventArgs
    {
        public XRef XRef { get; private set; }

        public LogicalXRefAddedEventArgs(XRef xref)
        {
            this.XRef = xref;
        }
    }


}

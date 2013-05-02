#if true
#define SPARSE_XREF_COLLECTION
#else
#undef SPARSE_XREF_COLLECTION
#endif

using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;
using System.Diagnostics;

namespace Disassembler
{
    /// <summary>
    /// Represents a cross-reference between code and code or code and data.
    /// A xref between code and code is analog to an edge in a Control Flow 
    /// Graph.
    /// </summary>
    public class XRef
    {
        /// <summary>
        /// Gets the target address being referenced. This may be
        /// <code>Pointer.Invalid</code> if the target address cannot be
        /// determined, such as in a dynamic jump or call.
        /// </summary>
        public Pointer Target { get; private set; }

        /// <summary>
        /// Gets the source address that refers to target. This may be
        /// <code>Pointer.Invalid</code> if the source address cannot be
        /// determined, such as in the entry routine of a program.
        /// </summary>
        public Pointer Source { get; private set; }

        /// <summary>
        /// Gets the type of this cross-reference.
        /// </summary>
        public XRefType Type { get; private set; }

        /// <summary>
        /// Gets the address of the associated data item. This is relevant
        /// if Type is NearIndexedJump or FarIndexedJump, where DataLocation
        /// contains the address of the jump table entry.
        /// </summary>
        public Pointer DataLocation { get; private set; }

        /// <summary>
        /// Returns true if this xref is dynamic, i.e. its Target address
        /// contains <code>Pointer.Invalid</code>.
        /// </summary>
        public bool IsDynamic
        {
            get { return Target == Pointer.Invalid; }
        }

        public XRef()
        {
            this.Source = Pointer.Invalid;
            this.Target = Pointer.Invalid;
            this.DataLocation = Pointer.Invalid;
        }

        public XRef(XRefType type, Pointer source, Pointer target, Pointer dataLocation)
        {
            this.Source = source;
            this.Target = target;
            this.Type = type;
            this.DataLocation = dataLocation;
        }

        public XRef(XRefType type, Pointer source, Pointer target)
            : this(type, source, target, Pointer.Invalid)
        {
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1} ({2})", Source, Target, Type);
        }

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
        //private BinaryImage image;
        private Range<LinearPointer> addressRange;

        /// <summary>
        /// List of cross references contained in this collection. This list
        /// have the same number of items as ListNext.
        /// </summary>
        private List<XRef> ListData = new List<XRef>();

        /// <summary>
        /// Contains the node link for each cross reference in xrefs array.
        /// That is, <code>ListData[ListNext[i].Incoming].Target = ListData[i].Target</code>,
        /// and <code>ListData[ListNext[i].Outgoing].Source = ListData[i].Source</code>.
        /// </summary>
        private List<XRefLink> ListLink = new List<XRefLink>();

        /// <summary>
        /// Contains the index of the first xref pointing to or from each
        /// address in the image. That is, 
        /// <code>xrefs[ListHead[addr].Incoming].Target = addr</code>, and
        /// <code>xrefs[ListHead[addr].Outgoing].Source = addr</code>.
        /// An extra item is placed at the end to represent dynamic xrefs,
        /// i.e. those with source or target equal to FFFF:FFFF.
        /// </summary>
        /// <remarks>
        /// If the XRef collection is sparse, we might as well use a 
        /// Dictionary(Of LinearPointer, XRefLink) to store ListHead.
        /// </remarks>
#if SPARSE_XREF_COLLECTION
        private Dictionary<LinearPointer, XRefLink> ListHead;
#else
        private XRefLink[] ListHead;
        private readonly int DynamicIndex;
#endif

        /// <summary>
        /// Represents a node in the cross reference map. For performance
        /// reason, the actual node data (XRef object) is placed separately
        /// in the NodeData[] list. Therefore this structure only contains
        /// the node link fields.
        /// </summary>
        struct XRefLink
        {
            /// <summary>
            /// Index of the next xref node that points to Target; -1 if none.
            /// </summary>
            public int NextIncoming;

            /// <summary>
            /// Index of the next xref node that points from Source; -1 if none.
            /// </summary>
            public int NextOutgoing;
        }

        /// <summary>
        /// Creates a cross reference collection for the given image.
        /// </summary>
        /// <param name="image"></param>
        public XRefCollection(Range<LinearPointer> addressRange)
        {
            this.addressRange = addressRange;
#if SPARSE_XREF_COLLECTION
            this.ListHead = new Dictionary<LinearPointer, XRefLink>();
#else
            this.DynamicIndex = addressRange.End - addressRange.Begin;
            this.ListHead = new XRefLink[DynamicIndex + 1];
#endif
            Clear();
        }

        /// <summary>
        /// Clears all the cross references stored in this collection.
        /// </summary>
        public void Clear()
        {
            ListData.Clear();
            ListLink.Clear();
#if SPARSE_XREF_COLLECTION
            ListHead.Clear();
#else
            for (int i = 0; i < ListHead.Length; i++)
            {
                ListHead[i].NextIncoming = -1;
                ListHead[i].NextOutgoing = -1;
            }
#endif
        }

        /// <summary>
        /// Gets the number of cross references in this collection.
        /// </summary>
        public int Count
        {
            get { return ListData.Count; }
        }

#if SPARSE_XREF_COLLECTION
#else
        private int PointerToOffset(LinearPointer address)
        {
            if (!addressRange.Contains(address))
            {
                throw new ArgumentOutOfRangeException("address");
            }
            return address - addressRange.Begin;
        }

        private int PointerToOffset(Pointer address)
        {
            if (address == Pointer.Invalid)
                return DynamicIndex;
            else
                return PointerToOffset(address.LinearAddress);
        }
#endif

        private XRefLink GetListHead(LinearPointer address)
        {
#if SPARSE_XREF_COLLECTION
            XRefLink headLink;
            if (!ListHead.TryGetValue(address, out headLink))
            {
                headLink.NextOutgoing = -1;
                headLink.NextIncoming = -1;
            }
            return headLink;
#else
            return ListHead[PointerToOffset(address)];
#endif
        }

        private XRefLink GetListHead(Pointer address)
        {
#if SPARSE_XREF_COLLECTION
            XRefLink headLink;
            if (!ListHead.TryGetValue(address.LinearAddress, out headLink))
            {
                headLink.NextOutgoing = -1;
                headLink.NextIncoming = -1;
            }
            return headLink;
#else
            return ListHead[PointerToOffset(address)];
#endif
        }

        public void Add(XRef xref)
        {
            if (xref == null)
                throw new ArgumentNullException("xref");
            if (xref.Source == Pointer.Invalid && xref.Target == Pointer.Invalid)
                throw new ArgumentException("can't have both source and target invalid");

            int nodeIndex = ListData.Count;

            XRefLink nodeLink = new XRefLink();

#if SPARSE_XREF_COLLECTION
            LinearPointer iSource = xref.Source.LinearAddress;
            XRefLink headLink = GetListHead(xref.Source);
            nodeLink.NextOutgoing = headLink.NextOutgoing;
            headLink.NextOutgoing = nodeIndex;
            ListHead[iSource] = headLink;

            LinearPointer iTarget = xref.Target.LinearAddress;
            headLink = GetListHead(xref.Target);
            nodeLink.NextIncoming = headLink.NextIncoming;
            headLink.NextIncoming = nodeIndex;
            ListHead[iTarget] = headLink;
#else
            int iSource = PointerToOffset(xref.Source);
            nodeLink.NextOutgoing = ListHead[iSource].NextOutgoing;
            ListHead[iSource].NextOutgoing = nodeIndex;

            int iTarget = PointerToOffset(xref.Target);
            nodeLink.NextIncoming = ListHead[iTarget].NextIncoming;
            ListHead[iTarget].NextIncoming = nodeIndex;
#endif

            // Since XRefLink is a struct, we must add it after updating all
            // its fields.
            ListLink.Add(nodeLink);
            ListData.Add(xref);
        }

        /// <summary>
        /// Gets a list of xrefs with dynamic target (i.e. target == Invalid).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XRef> GetDynamicReferences()
        {
            for (int i = GetListHead(Pointer.Invalid).NextIncoming; 
                 i >= 0; 
                 i = ListLink[i].NextIncoming)
            {
                Debug.Assert(ListData[i].Target == Pointer.Invalid);
                yield return ListData[i];
            }
        }

        /// <summary>
        /// Gets all cross references that points to 'target', in the order
        /// that they were added.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerable<XRef> GetReferencesTo(LinearPointer target)
        {
            for (int i = GetListHead(target).NextIncoming; i >= 0; i = ListLink[i].NextIncoming)
            {
                Debug.Assert(ListData[i].Target.LinearAddress == target);
                yield return ListData[i];
            }
        }

        /// <summary>
        /// Gets all cross references that points from 'source'.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<XRef> GetReferencesFrom(LinearPointer source)
        {
            for (int i = GetListHead(source).NextOutgoing; i >= 0; i = ListLink[i].NextOutgoing)
            {
                Debug.Assert(ListData[i].Source.LinearAddress == source);
                yield return ListData[i];
            }
        }

        public bool Contains(XRef item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(XRef[] array, int arrayIndex)
        {
            ListData.CopyTo(array, arrayIndex);
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
            return ListData.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ListData.GetEnumerator();
        }
    }
}

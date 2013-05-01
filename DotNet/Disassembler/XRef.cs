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
        /// Gets or sets the target address being referenced. This may be
        /// <code>Pointer.Invalid</code> if the target address cannot be
        /// determined, such as in a dynamic jump or call.
        /// </summary>
        public Pointer Target { get; set; }

        /// <summary>
        /// Gets or sets the source address that refers to target. This may
        /// be <code>Pointer.Invalid</code> if the source address cannot be
        /// determined, such as in the entry routine of a program.
        /// </summary>
        public Pointer Source { get; set; }

        /// <summary>
        /// Gets or sets the type of cross-reference.
        /// </summary>
        public XRefType Type { get; set; }

        /// <summary>
        /// Gets or sets the address of the associated data item. This is
        /// only relevant if Type is NearIndexedJump or FarIndexedJump, where
        /// DataLocation contains the address of the jump table entry.
        /// </summary>
        public Pointer DataLocation { get; set; }

        public XRef()
        {
            this.Source = Pointer.Invalid;
            this.Target = Pointer.Invalid;
            this.DataLocation = Pointer.Invalid;
            this.Type = XRefType.UserSpecified;
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1} ({2})", Source, Target, Type);
        }
    }

    /// <summary>
    /// Defines types of cross-references. The numeric values of the enum
    /// members are in decreasing order of their priority in analysis.
    /// </summary>
    public enum XRefType
    {
        /// <summary>
        /// User specified entry point (such as program start).
        /// </summary>
        UserSpecified,

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

    public class XRefLocationComparer : IComparer<XRef>
    {
        public int Compare(XRef x, XRef y)
        {
            int cmp = x.Source.LinearAddress.CompareTo(y.Source.LinearAddress);
            if (cmp == 0)
                cmp = x.Target.LinearAddress.CompareTo(y.Target.LinearAddress);
            if (cmp == 0)
                cmp = x.DataLocation.LinearAddress.CompareTo(y.DataLocation.LinearAddress);
            return cmp;
        }
    }

    /// <summary>
    /// Maintains a collection of cross references and provides methods to
    /// find xrefs by source or target efficiently.
    /// </summary>
    public class XRefCollection : ICollection<XRef>
    {
        private BinaryImage image;

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
        private XRefLink[] ListHead;

        private readonly int DynamicIndex;

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
        public XRefCollection(BinaryImage image)
        {
            this.image = image;
            this.DynamicIndex = image.Length;
            this.ListHead = new XRefLink[image.Length + 1];
            Clear();
        }

        /// <summary>
        /// Clears all the cross references stored in this collection.
        /// </summary>
        public void Clear()
        {
            ListData.Clear();
            ListLink.Clear();
            for (int i = 0; i < ListHead.Length; i++)
            {
                ListHead[i].NextIncoming = -1;
                ListHead[i].NextOutgoing = -1;
            }
        }

        /// <summary>
        /// Gets the number of cross references in this collection.
        /// </summary>
        public int Count
        {
            get { return ListData.Count; }
        }

        private int PointerToOffset(LinearPointer address)
        {
            int i = address - image.StartAddress;
            if (i < 0 || i >= image.Length)
            {
                throw new ArgumentException(string.Format(
                    "The address {0} is out of the range of the image.",
                    address));
            }
            return i;
        }

        private int PointerToOffset(Pointer address)
        {
            if (address == Pointer.Invalid)
                return DynamicIndex;

            int i = address.LinearAddress - image.StartAddress;
            if (i < 0 || i >= image.Length)
            {
                throw new ArgumentException(string.Format(
                    "The address {0} is out of the range of the image.",
                    address));
            }
            return i;
        }

        public void Add(XRef xref)
        {
            if (xref == null)
                throw new ArgumentNullException("xref");
            if (xref.Source == Pointer.Invalid && xref.Target == Pointer.Invalid)
                throw new ArgumentException("can't have both source and target invalid");

            int nodeIndex = ListData.Count;

            XRefLink nodeLink = new XRefLink();

            int iSource = PointerToOffset(xref.Source);
            nodeLink.NextOutgoing = ListHead[iSource].NextOutgoing;
            ListHead[iSource].NextOutgoing = nodeIndex;

            int iTarget = PointerToOffset(xref.Target);
            nodeLink.NextIncoming = ListHead[iTarget].NextIncoming;
            ListHead[iTarget].NextIncoming = nodeIndex;

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
            for (int i = ListHead[0].NextIncoming; i >= 0; i = ListLink[i].NextIncoming)
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
        public XRef[] GetReferencesTo(LinearPointer target)
        {
            int k = PointerToOffset(target);

            // Count the number of items to return.
            int n = 0;
            for (int i = ListHead[k].NextIncoming; i >= 0; i = ListLink[i].NextIncoming)
            {
                Debug.Assert(ListData[i].Target.LinearAddress == target);
                n++;
            }

            // Fill the result in reverse order of the linked list.
            XRef[] result = new XRef[n];
            for (int i = ListHead[k].NextIncoming; i >= 0; i = ListLink[i].NextIncoming)
            {
                result[--n] = ListData[i];
            }
            return result;
        }

        /// <summary>
        /// Gets all cross references that points from 'source'.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<XRef> GetReferencesFrom(LinearPointer source)
        {
            int k = PointerToOffset(source);
            for (int i = ListHead[k].NextOutgoing; i >= 0; i = ListLink[i].NextOutgoing)
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

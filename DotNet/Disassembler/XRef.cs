using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

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
        /// Gets or sets the target address being referenced.
        /// </summary>
        public Pointer Target { get; set; }

        /// <summary>
        /// Gets or sets the source address that refers to target.
        /// </summary>
        public Pointer Source { get; set; }

        /// <summary>
        /// Gets or sets the type of cross-reference.
        /// </summary>
        public XRefType Type { get; set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1} ({2})", Source, Target, Type);
        }
    }

    /// <summary>
    /// Maintains a collection of cross references and provides easy access
    /// to xrefs by type and location.
    /// </summary>
    public class XRefManager
    {
        private List<XRef> xrefs;

        public XRefManager()
        {
            xrefs = new List<XRef>();
        }

        public void Add(XRef xref)
        {
            xrefs.Add(xref);
        }

        public IEnumerable<XRef> FindReferencesTo(Pointer target)
        {
            foreach (XRef xref in xrefs)
            {
                if (xref.Target == target)
                    yield return xref;
            }
        }

        public IEnumerable<XRef> FindReferencesFrom(Pointer source)
        {
            foreach (XRef xref in xrefs)
            {
                if (xref.Source == source)
                    yield return xref;
            }
        }
    }

    /// <summary>
    /// Defines types of cross-references.
    /// </summary>
    public enum XRefType
    {
        /// <summary>
        /// User specified entry point (such as program start).
        /// </summary>
        UserSpecified,

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
        /// A JMP instruction refers to this location.
        /// </summary>
        NearJump,

        /// <summary>
        /// A JMPF instruction refers to this location.
        /// </summary>
        FarJump,

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
        /// The word at the Target location appears to contain the relative
        /// offset to a JMPN instruction that refers to a jump-table. The
        /// location of the JMPN instruction is stored in Source.
        /// </summary>
        NearJumpTableEntry,

        /// <summary>
        /// A JMPN instruction refers to this location indirectly through
        /// a word-sized jump table entry. The address of the jump table
        /// entry is stored in the DataReference field of the XRef object.
        /// </summary>
        //NearIndexedJump,

        NearJumpTableTarget,

#if false
    ENTRY_FAR_JUMP_TABLE        = 8,    /* the dword at this location appears to represent
                                         * an absolute address (seg:ptr) of a JUMP FAR
                                         * instruction.
                                         */
#endif
    }

}

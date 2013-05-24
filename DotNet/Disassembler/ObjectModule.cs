using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler2
{
    /// <summary>
    /// Represents an object module, which contains the binary image as well
    /// as associated fix-up information, symbol definitions, logical segment
    /// definitions, and segment group definitions.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Browsable(true)]
    public class ObjectModule : Module
    {
        readonly List<LogicalSegment> segments = new List<LogicalSegment>();
        readonly List<SegmentGroup> groups = new List<SegmentGroup>();
        readonly List<DefinedSymbol> definedNames = new List<DefinedSymbol>();
        readonly List<ExternalSymbol> externalNames = new List<ExternalSymbol>();
        readonly List<SymbolAlias> aliases = new List<SymbolAlias>();

        public Omf.Record[] Records { get; internal set; }

        /// <summary>
        /// Gets the name of the object module in the library.
        /// </summary>
        /// <remarks>
        /// This name is defined by the LIBMOD subrecord of COMENT.
        /// </remarks>
        [Browsable(true)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the source file name of the object module.
        /// </summary>
        /// <remarks>
        /// This name is defined in the THEADR record.
        /// </remarks>
        [Browsable(true)]
        public string SourceName { get; internal set; }

        /// <summary>
        /// Gets a list of logical segments defined in this module.
        /// </summary>
        /// <remarks>
        /// A logical segment is defined by a SEGDEF record.
        /// </remarks>
        public List<LogicalSegment> Segments
        {
            get { return segments; }
        }

        /// <summary>
        /// Gets a list of segment groups defined in this module.
        /// </summary>
        /// <remarks>
        /// A segment group is defined by a GRPDEF record.
        /// </remarks>
        public List<SegmentGroup> Groups
        {
            get { return groups; }
        }

        /// <summary>
        /// Gets a list of external symbols used by this module.
        /// </summary>
        /// <remarks>
        /// An external symbol is defined by one of the following records:
        /// EXTDEF  -- refers to public names in other modules
        /// LEXTDEF -- refers to a local name defined in this module
        /// CEXTDEF -- refers to a COMDAT name defined in another module 
        ///            (by COMDEF) or in this module (by LCOMDEF)
        /// </remarks>
        public List<ExternalSymbol> ExternalNames
        {
            get { return externalNames; }
        }

        /// <summary>
        /// Gets a list of defined symbols defined in this module.
        /// </summary>
        public List<DefinedSymbol> DefinedNames
        {
            get { return definedNames; }
        }

        /// <summary>
        /// Gets a list of symbol aliases defined in this module.
        /// </summary>
        /// <remarks>
        /// A symbol alias is defined by the ALIAS record.
        /// </remarks>
        [Browsable(true)]
        public List<SymbolAlias> Aliases
        {
            get { return aliases; }
        }

        public override string ToString()
        {
            if (this.Name == null)
                return this.SourceName;
            else
                return string.Format("{0} ({1})", this.Name, this.SourceName);
        }
    }
}

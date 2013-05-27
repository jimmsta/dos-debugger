using System;
using System.Text;
using Disassembler;

namespace WpfDebugger
{
    /// <summary>
    /// Represents a custom URI used to address a byte in an assembly.
    /// </summary>
    /// <remarks>
    /// An AssemblyUri has the following format:
    ///
    /// ddd://assembly/type/name/offset
    ///
    /// All parts are mandatory except 'offset', which defaults to zero. The
    /// URI components are explained below.
    /// 
    /// "ddd":
    ///   Protocol; stands for "_dos _debugger and _decompiler".
    ///
    /// assembly:
    ///   Format: "exe" N   or   "lib" N
    ///   where N is a zero-based index that uniquely identifies an assembly
    ///   within the session. N monotonically increases for each assembly
    ///   opened in the session.
    ///   
    ///   We use an index to represent an assembly so that we don't need to
    ///   worry about escaping file names or handling duplicate file names.
    ///   
    ///   The 'assembly' part serves as the base address from which bytes
    ///   within the assembly can be references without explicitly specifying
    ///   the assembly.
    ///   
    /// type & name:
    ///   Specifies an IAddressReferent object. Supported types and their
    ///   formats are described below.
    ///   
    ///   'type'='seg' -- segment.
    ///   'name' must be one of the following:
    ///     (1) decimal number: specifies the one-based index that uniquely
    ///         identifies the segment within the assembly.
    ///     (2) module.name: fully qualified name of the segment. Neither
    ///         module nor name may contain a dot. If multiple matches are
    ///         found, throws UriFormatException.
    ///     (3) name: (without dot) if the name is unique within all modules,
    ///         specifies that segment; otherwise, throws UriFormatException.
    ///   'offset' is relative to the beginning of the segment.
    ///   
    ///   'type'='sym' -- symbol.
    ///   'name' specifies the symbol name.
    ///   'offset' is relative to the location of the symbol.
    ///    
    ///   'type'='sub' -- procedure.
    ///   'name' specifies the name of the procedure.
    ///   'offset' is relative to the entry point of the procedure.
    /// 
    ///   'type'='*' -- wildcard.
    ///   'name' specifies one of the above names, and the handler tries to
    ///   resolve it.
    ///   
    /// offset:
    ///   Hexidecimal offset of the location within the segment. No prefix
    ///   or suffix should be added to indicate that it is hexidecimal. 
    ///   Leading zeros are not mandatory. Negative offsets are not supported.
    ///   
    ///   If the offset specifies a position in the middle of an instruction
    ///   or data item, the handler automatically chooses a suitable position
    ///   instead to display the location.
    ///   
    /// It is helpful to think of a URI as one would expect to input and get
    /// a result in a GOTO dialog.
    /// </remarks>
    /// <example>
    /// ddd://exe1/seg/1/0              first byte of segment 1
    /// ddd://lib1/seg/_ctype._TEXT/0   first byte of _TEXT segment of _ctype module
    /// ddd://lib2/sym/_strcpy/0        starting address of symbol '_strcpy'
    /// ddd://exe2/sub/sub_01234/0      entry point of procedure 'sub_01234'
    /// ddd://lib3/*/__exit             search for something named __exit
    /// </example>
    public class AssemblyUri : Uri
    {
        readonly Assembly assembly;
        readonly IAddressReferent referent;
        readonly int offset;

        public AssemblyUri(string uriString)
            : base(uriString)
        {
        }

        public AssemblyUri(Assembly assembly, IAddressReferent referent, int offset)
            : base(MakeUriString(assembly, referent, offset))
        {
            this.assembly = assembly;
            this.referent = referent;
            this.offset = offset;
        }

        public Assembly Assembly
        {
            get { return assembly; }
        }

        public IAddressReferent Referent
        {
            get { return referent; }
        }

        public int Offset
        {
            get { return offset; }
        }

        private static string MakeUriString(
            Assembly assembly, IAddressReferent referent, int offset)
        {
            StringBuilder sb = new StringBuilder();

            if (assembly != null) // absolute uri
            {
                sb.AppendFormat("ddd://{0}{1}/",
                    assembly is Executable ? "exe" : "lib",
                    assembly.GetHashCode());
            }

            if (referent == null) // no referent; must be relative uri
            {
                if (assembly != null)
                    throw new ArgumentException("Cannot specify assembly without specifying referent.");
            }
            else if (referent is Segment)
            {
                sb.AppendFormat("seg/{0}/", ((Segment)referent).Id);
            }
            else
            {
                throw new ArgumentException("Unsupported referent type.");
            }

            sb.Append(offset.ToString("X4"));
            return sb.ToString();
        }
    }
}

using System;
using System.Text;
using Disassembler2;

namespace WpfDebugger
{
    /// <summary>
    /// Represents a custom URI that can be used to address a byte in an
    /// assembly. See Remarks for details.
    /// </summary>
    /// <remarks>
    /// An AssemblyUri has the following format:
    ///
    /// ddd://assembly/segment/offset
    ///
    /// All parts are mandatory. Each part is explained below.
    /// 
    /// "ddd":
    ///   Protocal; stands for "_dos _debugger and _decompiler".
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
    /// segment: 
    ///   Must be one of the following:
    ///   
    ///   1) decimal number: specifies the zero-based index that uniquely
    ///      identifies the segment within the assembly.
    ///   2) module.name: fully qualified name of the segment. Neither module
    ///      nor name may contain a dot. If multiple matches are found,
    ///      throws a UriFormatException.
    ///   3) name: (without dot) if the name is unique within all modules,
    ///      specifies that segment; otherwise, throws a UriFormatException.
    ///      
    /// offset:
    ///   Hexidecimal offset of the location within the segment. No prefix
    ///   or suffix should be added to indicate that it is hexidecimal. 
    ///   Leading zeros are not mandatory.
    ///   
    ///   If the offset specifies a position in the middle of an instruction
    ///   or data item, the handler automatically chooses a suitable position
    ///   instead to display the location.
    /// </remarks>
    /// <example>
    /// ddd://lib1/_ctype._TEXT/0000    _TEXT segment of _ctype module
    /// ddd://exe1/seg001/0005          address seg001:0005
    /// </example>
    // TODO: we might as well use logical uri, such as
    // ddd://exe1/ seg/seg001   or   _ctype._TEXT
    //             sym/_strcpy
    //             sub/sub17283/7845
    //             lab/loc12345
    //             */name-to-search-for
    //    the three character in between must implement IAddressReferent
    //    to be precise, change each to #N where N is zero-based decimal
    // 
    // i.e. it is the same as one would expect in a goto
    // 
    public class AssemblyUri : Uri
    {
        public AssemblyUri(string uriString)
            : base(uriString)
        {
        }

        public AssemblyUri(Assembly assembly, IAddressReferent referent, int offset)
            : base(MakeUriString(assembly, referent, offset))
        {
        }

        public AssemblyUri(Assembly assembly, ResolvedAddress address)
            : base(MakeUriString(assembly, address))
        {
        }

        private static string MakeUriString(
            Assembly assembly, IAddressReferent referent, int offset)
        {
            StringBuilder sb = new StringBuilder();

            if (assembly != null) // absolute uri
            {
                sb.AppendFormat("ddd://{0}{1}/",
                    assembly is Executable ? "exe" : "lib",
                    0);
            }

            if (referent != null) // has referent
            {
                if (referent is LogicalSegment)
                {
                    sb.Append("seg/");
                }
                else
                {
                    throw new ArgumentException("Unsupported referent type.");
                }
            }
            else
            {
                if (assembly != null)
                    throw new ArgumentException("Cannot specify assembly without specifying referent.");
            }

            sb.Append(offset.ToString("X4"));
            return sb.ToString();
        }

        private static string MakeUriString(Assembly assembly, ResolvedAddress address)
        {
            return string.Format(
                "ddd://{0}{1}/{2}/{3:X4}",
                assembly is Executable ? "exe" : "lib",
                0,
                "unnamed_segment",
                address.Offset);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Contains information about the (low-level) signature of a function.
    /// </summary>
    public class FunctionSignature
    {
        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the calling convention of the function.
        /// </summary>
        public CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Gets or sets the total number of bytes of parameters, including
        /// those passed in registers. If this information is not available
        /// (such as in cdecl), the value is -1.
        /// </summary>
        public int ParametersSize { get; set; }
    }
    
    /// <summary>
    /// Defines the calling convention of a function.
    /// </summary>
    /// <remarks>
    /// For a detailed list of the calling conventions of different compilers
    /// and systems, see http://en.wikipedia.org/wiki/X86_calling_conventions.
    /// </remarks>
    public enum CallingConvention
    {
        Unknown = 0,

        /// <summary>
        /// Parameters passed on the stack from right to left; caller cleans
        /// the stack.
        /// </summary>
        CDecl = 1,

        /// <summary>
        /// Parameters passed on the stack from left to right; callee cleans
        /// the stack. This convention is mainly used on 16-bit systems.
        /// </summary>
        Pascal = 2,

        /// <summary>
        /// Parameters passed in AX, DX, and then pushed to the stack LTR.
        /// The return address is passed in BX. Callee cleans the stack.
        /// </summary>
        FastCall = 3,
    }

    public static class NameMangler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>
        /// The name decoration of a C function is described at
        /// http://en.wikipedia.org/wiki/Name_mangling#C_name_decoration_in_Microsoft_Windows
        /// </remarks>
        public static FunctionSignature Demangle(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (s.Length <= 1)
                return null;

            if (s[0] == '_')
            {
                int at = s.IndexOf('@');
                if (at == -1) // _f
                {
                    return new FunctionSignature
                    {
                        CallingConvention = CallingConvention.CDecl,
                        Name = s.Substring(1),
                        ParametersSize = -1
                    };
                }

                int paramSize;
                if (Int32.TryParse(s.Substring(at + 1), out paramSize)) // _f@4
                {
                    return new FunctionSignature
                    {
                        CallingConvention = CallingConvention.Pascal,
                        Name = s.Substring(1, at - 1),
                        ParametersSize = paramSize
                    };
                }

                return null;
            }

            if (s[0] == '@') // @f@4 -- fast call
            {
                int at = s.IndexOf('@', 1);
                if (at >= 0)
                {
                    int paramSize;
                    if (Int32.TryParse(s.Substring(at + 1), out paramSize))
                    {
                        return new FunctionSignature
                        {
                            CallingConvention = CallingConvention.FastCall,
                            Name = s.Substring(1, at - 1),
                            ParametersSize = paramSize
                        };
                    }
                }
                return null;
            }

            if (s[0] == '?')
            {
                return DemangleCpp(s);
            }

            return null;
        }

        /// <remarks>
        /// The name mangling method of VC++ is described at
        /// http://en.wikipedia.org/wiki/Visual_C%2B%2B_name_mangling
        /// </remarks>
        private static FunctionSignature DemangleCpp(string s)
        {
            return null;
        }
    }
}

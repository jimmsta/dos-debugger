using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Represents an executable file or library file to be disassembled.
    /// </summary>
    /// <remarks>
    /// The object model looks like this:
    /// 
    /// Assembly                Executable              Library
    /// (physical)
    /// |-- Module1             LoadModule(1)           ObjectModule(*)
    /// |-- Module2
    ///     |-- Segment1        All segments share      Each segment has
    ///     |-- Segmeng2        the same image chunk    its own image chunk
    ///     |-- Segment3
    ///     |-- ...
    /// |-- Module3
    /// |-- ...
    /// * Note: An assembly actually bypasses the Module layer to manage
    ///         segments directly.
    /// (logical)
    /// |-- Procedures
    ///     |-- Procedure1      procedures should       procedures may
    ///     |-- Procedure2      not cross segments      cross segments
    ///     |-- ...
    /// |-- Symbols
    ///     |-- Symbol1         deduced from analysis   directly read
    ///     |-- Symbol2
    ///     |-- ...
    /// 
    /// </remarks>
    public abstract class Assembly
    {
#if false
        readonly XRefCollection crossReferences = new XRefCollection();
        readonly BasicBlockCollection basicBlocks = new BasicBlockCollection();
        readonly ProcedureCollection procedures = new ProcedureCollection();
        readonly ErrorCollection errors = new ErrorCollection();
#endif
        readonly ModuleCollection modules = new ModuleCollection();
        //readonly Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

        public Assembly()
        {
        }

#if false
        public XRefCollection CrossReferences
        {
            get { return crossReferences; }
        }

        public BasicBlockCollection BasicBlocks
        {
            get { return basicBlocks; }
        }

        public ProcedureCollection Procedures
        {
            get { return procedures; }
        }

        public ErrorCollection Errors
        {
            get { return errors; }
        }
#endif

        public ModuleCollection Modules
        {
            get { return modules; }
        }

        /// <summary>
        /// Returns the binary image of this assembly.
        /// </summary>
        public abstract BinaryImage GetImage();
    }

    /// <summary>
    /// Represents a module in an assembly. For an executable, there is only
    /// one module which is the LoadModule, so this is not very interesting;
    /// for a library, it contains multiple ObjectModules.
    /// </summary>
    public abstract class Module
    {
    }

    public class ModuleCollection : List<Module>
    {
    }
}

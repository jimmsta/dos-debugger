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
        readonly XRefCollection crossReferences = new XRefCollection();
        readonly BasicBlockCollection basicBlocks = new BasicBlockCollection();
        readonly ProcedureCollection procedures = new ProcedureCollection();
        readonly ErrorCollection errors = new ErrorCollection();

        readonly ModuleCollection modules = new ModuleCollection();
        //readonly Dictionary<int, Segment> segments = new Dictionary<int, Segment>();

        public Assembly()
        {
        }

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

        public ModuleCollection Modules
        {
            get { return modules; }
        }

        /// <summary>
        /// Returns the binary image of this assembly.
        /// </summary>
        public abstract BinaryImage GetImage();

#if false
        /// <summary>
        /// Finds the segment with the given segment selector.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public abstract Segment GetSegment(int segmentId)
        {
            return segments[segmentSelector];
        }
#endif

#if false
        /// <summary>
        /// Finds the segment with the given segment selector.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public void AddSegment(int segmentSelector, Segment segment)
        {
            segments.Add(segmentSelector, segment);
        }
#endif
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

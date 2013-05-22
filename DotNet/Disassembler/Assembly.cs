using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2
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
    ///     |-- Segmeng2        the same image          its own image
    ///     |-- Segment3        buffer                  buffer
    ///     |-- ...
    /// |-- Module3
    /// |-- ...
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
    public class Assembly
    {
        private readonly XRefCollection crossReferences;
        private readonly ProcedureCollection procedures;
        private readonly ModuleCollection modules;

        public Assembly()
        {
            this.crossReferences = new XRefCollection();
            this.procedures = new ProcedureCollection(this);
            this.modules = new ModuleCollection();
        }

        public XRefCollection CrossReferences
        {
            get { return crossReferences; }
        }

        public ProcedureCollection Procedures
        {
            get { return procedures; }
        }

        public ModuleCollection Modules
        {
            get { return modules; }
        }
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

    /// <summary>
    /// Contains information about a procedure in an assembly (executable or
    /// library).
    /// </summary>
    // As this class interacts a lot with ProcedureCollection, it's easier
    // to think of this class as a 'wrapper' around a procedure defined in
    // ProcedureCollection/ProcedureMap.
    public class Procedure
    {
        private LogicalAddress entryPoint;
        private string name;

        //private BinaryImage image;
        //private ProcedureCollection procMap;
        public CallType CallType { get; set; } // near or far

        /// <summary>
        /// Creates a procedure with the given entry point.
        /// </summary>
        /// <param name="entryPoint">Entry point of the procedure.</param>
        public Procedure(LogicalAddress entryPoint)
        {
            this.entryPoint = entryPoint;
            //this.image = image;
            //this.procMap = procMap;
        }

        /// <summary>
        /// Gets or sets the name of the procedure. Though not required, this
        /// name should be unique within the assembly, otherwise it may cause
        /// confusion to the user.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Gets the (logical) entry point address of the procedure.
        /// </summary>
        public LogicalAddress EntryPoint
        {
            get { return this.entryPoint; }
        }

#if false
        /// <summary>
        /// Gets the linear address range of this procedure. The procedure
        /// may not cover every byte in this address range, and the entry
        /// point may not be at the beginning of the address range.
        /// </summary>
        public Range<LinearPointer> AddressRange
        {
            get { return new Range<LinearPointer>(this.StartAddress, this.EndAddress); }
        }
#endif

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        // TODO: what to do if the block is Split ?
        public void AddBasicBlock(BasicBlock block)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            // Verify that the bytes have not been assigned to any procedure.
#if false
            LinearPointer pos1 = block.StartAddress;
            LinearPointer pos2 = block.EndAddress;
            for (var i = pos1; i < pos2; i++)
            {
                if (image[i].Procedure != null)
                    throw new InvalidOperationException("Some of the bytes are already assigned to a procedure.");
            }

            // Assign the bytes to this procedure.
            for (var i = pos1; i < pos2; i++)
            {
                image[i].Procedure = this;
            }

            // Update the bounds of this procedure.
            this.Extend(block);
            this.Size += (pos2 - pos1);

            // Go through each instructions in this basic block, and update
            // the feature of this procedure. For example, if there is an
            // "INT 21h" instruction, then it looks like this procedure
            // interacts with the OS directly, and is therefore probably a
            // library function rather than a user function.
            ProcedureFeatures features = ProcedureFeatures.None;
            for (var i = pos1; i < pos2; )
            {
                Instruction instruction = image.DecodeInstruction(image[i].Address);
                switch (instruction.Operation)
                {
                    case Operation.INT:
                        features |= ProcedureFeatures.HasInterrupt;
                        break;
                    case Operation.FCLEX:
                        features |= ProcedureFeatures.HasFpu;
                        break;
                }
                i += instruction.EncodedLength;
            }
            this.Features |= features;
#endif
        }

        //public ProcedureFeatures Features { get; private set; }
        //public int Size { get; private set; }

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddDataBlock(LogicalAddress location, int length)
        {
#if false
            for (var i = start; i < end; i++)
            {
                if (image[i].Procedure != null)
                    throw new InvalidOperationException("Some of the bytes are already assigned to a procedure.");
            }

            // Assign the bytes to this procedure.
            for (var i = start; i < end; i++)
            {
                image[i].Procedure = this;
            }

            // Update the bounds of this procedure.
            this.Extend(new ByteBlock(start, end));
            this.Size += (end - start);
#endif
        }

#if false
        /// <summary>
        /// Enumerates the procedures that calls this procedure. The
        /// procedures are returned in order of their entry point address.
        /// Each procedure is returned only once.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Procedure> GetCallers()
        {
#if false
            SortedDictionary<LinearPointer, Procedure> procList =
                new SortedDictionary<LinearPointer, Procedure>();

            foreach (XRef xCall in procMap.callGraph.GetReferencesTo(EntryPoint.LinearAddress))
            {
                Procedure caller = procMap.Find(xCall.Source.LinearAddress);
                Debug.Assert(caller != null);
                procList[caller.EntryPoint.LinearAddress] = caller;
            }

            return procList.Values;
#else
            Pointer last = Pointer.Invalid;
            foreach (XRef xCall in procMap.callGraph.GetReferencesTo(EntryPoint.LinearAddress))
            {
                if (xCall.Source != last)
                {
                    Procedure caller = procMap.Find(xCall.Source.LinearAddress);
                    Debug.Assert(caller != null);
                    yield return caller;
                    last = xCall.Source;
                }
            }
#endif
        }

        /// <summary>
        /// Enumerates the procedures called by this procedure.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Procedure> GetCallees()
        {
#if false
            SortedDictionary<LinearPointer, Procedure> procList =
                new SortedDictionary<LinearPointer, Procedure>();

            foreach (XRef xCall in procMap.callGraph.GetReferencesFrom(EntryPoint.LinearAddress))
            {
                Procedure callee = procMap.Find(xCall.Target.LinearAddress);
                Debug.Assert(callee != null);
                procList[callee.EntryPoint.LinearAddress] = callee;
            }

            return procList.Values;
#else
            Pointer last = Pointer.Invalid;
            foreach (XRef xCall in procMap.callGraph.GetReferencesFrom(EntryPoint.LinearAddress))
            {
                if (xCall.Target != last)
                {
                    Procedure callee = procMap.Find(xCall.Target.LinearAddress);
                    Debug.Assert(callee != null);
                    yield return callee;
                    last = xCall.Target;
                }
            }
#endif
        }
#endif
    }

#if false
    public class ProcedureEntryPointComparer : IComparer<Procedure>
    {
        public int Compare(Procedure x, Procedure y)
        {
            return x.EntryPoint.CompareTo(y.EntryPoint);
        }
    }
#endif

    /// <summary>
    /// Specifies whether a function call is a near call or far call.
    /// </summary>
    // TODO: merge this with FunctionSignature.
    public enum CallType
    {
        Unknown = 0,
        Near = 1,
        Far = 2,
    }

    /// <summary>
    /// Specifies the features of the procedure. This is usually used for
    /// informational purpose, and should not be taken too seriously.
    /// </summary>
    [Flags]
    public enum ProcedureFeatures
    {
        None = 0,
        HasInterrupt = 1,
        HasFpu = 2,
    }

    /// <summary>
    /// Maintains a collection of procedures within an assembly and keeps
    /// track of their interdependence dynamically.
    /// </summary>
    public class ProcedureCollection : ICollection<Procedure>
    {
        //private Assembly assembly;

        /// <summary>
        /// Dictionary that maps the (resolved) entry point address of a
        /// procedure to the corresponding Procedure object.
        /// </summary>
        private readonly Dictionary<ResolvedAddress, Procedure> procMap
            = new Dictionary<ResolvedAddress, Procedure>();

        /// <summary>
        /// Maintains a call graph of the procedures in this collection.
        /// </summary>
        /// <remarks>
        /// For each function call, the disassembler generates a xref object
        /// with the following fields:
        /// 
        ///   Source  = logical address of the CALL or CALLF instruction
        ///   Target  = logical address of the target procedure
        ///   Type    = NearCall or FarCall
        ///   AuxData = not used; potentially could be used to store the data
        ///             address of a dynamic call instruction
        /// 
        /// In our call graph, we only keep track of the entry point of the
        /// calling procedure and the called procedure; that is, we discard
        /// information about which instruction generates the call. Therefore
        /// the above xref is transformed into the following xref and stored:
        /// 
        ///   Source  = entry point address of the calling procedure
        ///   Target  = entry point address of the called procedure
        ///   Type    = NearCall or FarCall
        ///   AuxData = not used, but could be set to the address of the
        ///             CALL/CALLF instruction
        /// 
        /// The reason that we don't store the address of the actual
        /// CALL/CALLF instruction is because there may be multiple CALLs
        /// between two procedures and keeping track all (or any) of them
        /// in real time is not really useful.
        /// </remarks>
        internal XRefCollection callGraph;

        public ProcedureCollection(Assembly assembly)
        {
            this.callGraph = new XRefCollection();
            assembly.CrossReferences.XRefAdded += CrossReferences_XRefAdded;
        }

        /// <summary>
        /// Raised after the disassembler adds a new xref to the assembly.
        /// We update our call graph in this handler.
        /// </summary>
        private void CrossReferences_XRefAdded(object sender, LogicalXRefAddedEventArgs e)
        {
            XRef x = e.XRef;
            if ((x.Type == XRefType.NearCall || x.Type == XRefType.FarCall) &&
                (x.Source != LogicalAddress.Invalid && x.Target != LogicalAddress.Invalid))
            {
                Procedure caller = x.Source.ImageByte.Procedure;
                //Procedure callee = Find(entryPoint: x.Target.LinearAddress);
                Procedure callee = x.Target.ImageByte.Procedure;
                if (caller == null || callee == null)
                {
                    throw new InvalidOperationException(
                        "A function call cross reference was added, but the functions are not defined.");
                }
                System.Diagnostics.Debug.Assert(callee.EntryPoint == x.Target);
                // TBD: how can we make sure the logical addresses agree?

                XRef xCall = new XRef(
                    type: x.Type,
                    source: caller.EntryPoint,
                    target: x.Target,
                    dataLocation: x.Source
                );
                callGraph.Add(xCall);
            }
        }

        /// <summary>
        /// Finds a procedure at the given entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns>A Procedure object with the given entry point if found,
        /// or null otherwise.</returns>
        public Procedure Find(LogicalAddress entryPoint)
        {
            Procedure proc;
            if (procMap.TryGetValue(entryPoint.ResolvedAddress, out proc))
                return proc;
            else
                return null;
        }

        /// <summary>
        /// Creates a procedure at the given entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns></returns>
        public Procedure Create(LogicalAddress entryPoint)
        {
            ResolvedAddress address = entryPoint.ResolvedAddress;
            if (procMap.ContainsKey(address))
            {
                throw new InvalidOperationException(
                    "A procedure already exists with the given entry point address.");
            }

            Procedure proc = new Procedure(entryPoint);
            procMap.Add(address, proc);
            return proc;
        }

        #region ICollection implementation

        public void Add(Procedure item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Procedure item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Procedure[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return procMap.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(Procedure item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Procedure> GetEnumerator()
        {
            return procMap.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

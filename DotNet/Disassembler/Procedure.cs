using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    // TBD: if multiple procedures share a basic block (known as "function
    // chunk"), it is a bit messy when we generate a call graph or display
    // the procedure size statistic. On the other hand, it might be easy
    // to generate a procedure checksum.
    //
    // Since a procedure chunk is shared code, while a procedure is itself
    // shared code, a procedure chunk is not much different than procedures
    // in terms of their purpose. The only difference is whether it is
    // invoked by a CALL or JUMP (or possibly fall-through). It is therefore
    // natural to create a "dummy" procedure type called "procedure chunk".
    // This ensures every basic block belongs to only one procedure, which
    // is nice because it simplifies code and also simplifies call graph.
    //
    // Now in terms of procedure checksum computing, we can simply treat
    // a procedure chunk as a procedure; that is, we compute the chunk's
    // checksum and include that when computing the procedure's checksum.


    /// <summary>
    /// Contains information about a procedure in an assembly (executable or
    /// library). The procedure is uniquely identified by its resolved entry
    /// point address. If the same entry point is called with different
    /// logical addresses, they are stored in Aliases.
    /// </summary>
    public class Procedure
    {
        readonly Address entryPoint;
        readonly Dictionary<BasicBlock, BasicBlock> basicBlocks =
            new Dictionary<BasicBlock, BasicBlock>();

        private CodeFeatures features = CodeFeatures.None;
        private string name; // TODO: add Names property to store aliases

        /// <summary>
        /// Creates a procedure with the given entry point.
        /// </summary>
        /// <param name="entryPoint">Entry point of the procedure.</param>
        public Procedure(Address entryPoint)
        {
            this.entryPoint = entryPoint;
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

        public ReturnType ReturnType { get; set; } // RETN/RETF/IRET

        /// <summary>
        /// Gets the entry point address of the procedure.
        /// </summary>
        public Address EntryPoint
        {
            get { return this.entryPoint; }
        }
        
        public ICollection<BasicBlock> BasicBlocks
        {
            get { return basicBlocks.Keys; }
        }

        public void AddBasicBlock(BasicBlock block)
        {
            basicBlocks.Add(block, block);
            this.features |= block.Features;
        }

        /// <summary>
        /// Gets the size, in bytes, of the procedure. This is the total size
        /// of its basic blocks. This size does NOT include data.
        /// </summary>
        public int Size
        {
            get
            {
                int size = 0;
                foreach (BasicBlock block in BasicBlocks)
                {
                    size += block.Length;
                }
                return size;
            }
        }

        public CodeFeatures Features
        {
            get { return features; }
        }

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddDataBlock(Address location, int length)
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

        /// <summary>
        /// Indicate
        /// </summary>
        Near = 1,
        Far = 2,
        Interrupt = 3,
    }

    /// <summary>
    /// Specifies the ways a procedure returns. It is possible, though rare,
    /// for a procedure to have multiple ways of return; therefore we define
    /// the enum as Flags.
    /// </summary>
    [Flags]
    public enum ReturnType
    {
        /// <summary>
        /// Indicates that the procedure never returns. Possible causes are:
        /// - the procedure terminates the program through e.g. int 21h;
        /// - the procedure contains a HLT instruction;
        /// - the procedure is an infinite loop;
        /// - the procedure calls a procedure that never returns.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that the procedure returns with an RETN instruction.
        /// </summary>
        Near = 1,

        /// <summary>
        /// Indicates that the procedure returns with an RETF instruction.
        /// </summary>
        Far = 2,

        /// <summary>
        /// Indicates that the procedure returns with an IRET instruction.
        /// </summary>
        Interrupt = 4,

        // Chunk
    }

    /// <summary>
    /// Maintains a collection of procedures within an assembly and keeps
    /// track of their interdependence dynamically.
    /// </summary>
    public class ProcedureCollection : ICollection<Procedure>
    {
        /// <summary>
        /// Dictionary that maps the entry point address of a procedure to
        /// the corresponding Procedure object.
        /// </summary>
        readonly Dictionary<Address, Procedure> procMap
            = new Dictionary<Address, Procedure>();

        readonly CallGraph callGraph;

        public ProcedureCollection()
        {
            this.callGraph = new CallGraph(this);
        }

        /// <summary>
        /// Finds the procedure at the given entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns>A Procedure object with the given entry point if found,
        /// or null otherwise.</returns>
        public Procedure Find(Address entryPoint)
        {
            Procedure proc;
            if (procMap.TryGetValue(entryPoint, out proc))
                return proc;
            else
                return null;
        }

        #region ICollection implementation

        public void Add(Procedure procedure)
        {
            if (procedure == null)
                throw new ArgumentNullException("procedure");

            if (procMap.ContainsKey(procedure.EntryPoint))
            {
                throw new ArgumentException(
                    "A procedure already exists with the given entry point address.");
            }
            procMap.Add(procedure.EntryPoint, procedure);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Procedure item)
        {
            if (item == null)
                return false;
            else
                return Find(item.EntryPoint) == item;
        }

        public void CopyTo(Procedure[] array, int arrayIndex)
        {
            this.procMap.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return procMap.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
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

        public CallGraph CallGraph
        {
            get { return callGraph; }
        }
    }
}

#if false
        /// <summary>
        /// Raised when a new xref is added to the binary image. We update
        /// our call graph in this handler.
        /// </summary>
        private void CrossReferences_XRefAdded(object sender, XRefAddedEventArgs e)
        {
            XRef x = e.XRef;
            if ((x.Type == XRefType.NearCall || x.Type == XRefType.FarCall) &&
                (x.Source != Pointer.Invalid && x.Target != Pointer.Invalid))
            {
                Procedure caller = image[x.Source].Procedure; // image.FindProcedure(x.Source.LinearAddress);
                Procedure callee = Find(entryPoint: x.Target.LinearAddress);
                if (caller == null || callee == null)
                {
                    throw new InvalidOperationException(
                        "A function call cross reference was added, but the functions are not defined.");
                }
                Debug.Assert(callee.EntryPoint.LinearAddress == x.Target.LinearAddress);

                XRef xCall = new XRef(
                    type: x.Type,
                    source: caller.EntryPoint,
                    target: x.Target,
                    dataLocation: x.Source
                );
                callGraph.Add(xCall);
            }
        }
#endif
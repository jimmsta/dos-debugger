using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;
using System.Diagnostics;

namespace Disassembler
{
    /// <summary>
    /// Contains information about a procedure in an executable.
    /// </summary>
    public class Procedure : ByteBlock
    {
        private BinaryImage image;

        //private Range<LinearPointer> bounds;
        //private MultiRange codeRange = new MultiRange();
        //private MultiRange dataRange = new MultiRange();
        //private MultiRange byteRange = new MultiRange();

        public Procedure(BinaryImage image, Pointer entryPoint)
        {
            this.image = image;
            this.EntryPoint = entryPoint;
        }

        /// <summary>
        /// Gets the entry point address of the procedure.
        /// </summary>
        public Pointer EntryPoint { get; private set; }

#if false
        public Range<LinearPointer> Bounds
        {
            get { return bounds; }
        }

        public MultiRange CodeRange
        {
            get { return codeRange; }
        }

        public MultiRange DataRange
        {
            get { return dataRange; }
        }

        public MultiRange ByteRange
        {
            get { return byteRange; }
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
        }

        /// <summary>
        /// Adds a basic block to the procedure.
        /// </summary>
        /// <param name="block"></param>
        public void AddDataBlock(LinearPointer start, LinearPointer end)
        {
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
    /// Maintains a collection of procedures and keeps track of their
    /// interdependence dynamically.
    /// </summary>
    public class ProcedureCollection : ICollection<Procedure>
    {
        BinaryImage image;

        /// <summary>
        /// Dictionary that maps the entry point (linear) address of a
        /// procedure to a Procedure object.
        /// </summary>
        private SortedList<LinearPointer, Procedure> procMap
            = new SortedList<LinearPointer, Procedure>();

        /// <summary>
        /// Maintains a call graph of the procedures in this collection.
        /// </summary>
        /// <remarks>
        /// For each function call, the disassembler generates a xref object
        /// with the following fields:
        /// 
        ///   Source  = CS:IP address of the CALL or CALLF instruction
        ///   Target  = CS:IP address of the target procedure
        ///   Type    = NearCall or FarCall
        ///   AuxData = not used; potentially could be used to store the data
        ///             address of a dynamic call instruction
        /// 
        /// In our call graph, we only keep track of the entry point of the
        /// calling procedure and the called procedure. So the above xref
        /// is transformed into the following xref and stored:
        /// 
        ///   Source  = (CS:IP) entry point address of the calling procedure
        ///   Target  = (CS:IP) entry point address of the called procedure
        ///   Type    = NearCall or FarCall
        ///   AuxData = not used, but could be set to the address of the
        ///             CALL/CALLF instruction
        /// 
        /// The reason that we don't store the address of the exact
        /// CALL/CALLF instruction is because there may be multiple CALLs
        /// between two procedures and keeping track all (or any) of them
        /// in real time is not useful.
        /// </remarks>
        XRefCollection callGraph;

        public ProcedureCollection(BinaryImage image)
        {
            this.image = image;
            this.callGraph = new XRefCollection(image.AddressRange);
            image.CrossReferences.XRefAdded += CrossReferences_XRefAdded;
        }

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

        /// <summary>
        /// Finds a procedure at the given entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <returns>A Procedure object with the given entry point if found,
        /// or null otherwise.</returns>
        public Procedure Find(LinearPointer entryPoint)
        {
            Procedure proc;
            if (procMap.TryGetValue(entryPoint, out proc))
                return proc;
            else
                return null;
        }

        public void Add(Procedure proc)
        {
            if (procMap.ContainsKey(proc.EntryPoint.LinearAddress))
            {
                throw new InvalidOperationException(
                    "A procedure already exists at the given entry point.");
            }

            procMap.Add(proc.EntryPoint.LinearAddress, proc);
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
            get { throw new NotImplementedException(); }
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
            return procMap.Values.GetEnumerator();
        }
    }
}

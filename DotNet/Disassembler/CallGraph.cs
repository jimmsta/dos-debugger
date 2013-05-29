using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Maintains a call graph among a collection of the procedures.
    /// </summary>
    /// <remarks>
    /// For each procedure call, the disassembler generates a xref object
    /// with the following fields:
    /// 
    ///   Source  = Address of the CALL or CALLF instruction
    ///   Target  = Address of the entry point of the target procedure
    ///   Type    = NearCall or FarCall
    ///   AuxData = not used; potentially could be used to store the data
    ///             address of a dynamic call instruction
    /// 
    /// In our call graph, we keep track of the entry point of the calling
    /// procedure and the called procedure, as well as the location of the
    /// CALL/CALLF instruction. Therefore the above xref is transformed
    /// into the following xref and stored:
    /// 
    ///   Source  = entry point address of the calling procedure
    ///   Target  = entry point address of the called procedure
    ///   Type    = NearCall or FarCall
    ///   AuxData = Address of the CALL/CALLF instruction
    /// 
    /// As a result of the above arrangement, each CALL/CALLF instruction
    /// generates one edge in the call graph, and there may be multiple edges
    /// between two procedures if it is called more than once. When the call
    /// graph is displayed to the user, it is often desirable to keep just
    /// one edge between any pair of procedures.
    /// </remarks>
    public class CallGraph
    {
        // TODO: need to supply comparer.
        readonly XRefCollection graph = new XRefCollection();
        readonly ProcedureCollection procedures;

        public CallGraph(ProcedureCollection procedures)
        {
            this.procedures = procedures;
        }

        public void AddEdge(Procedure caller, Procedure callee, XRef xref)
        {
            if (caller == null)
                throw new ArgumentNullException("caller");
            if (callee == null)
                throw new ArgumentNullException("callee");
            if (xref == null)
                throw new ArgumentNullException("xref");

            System.Diagnostics.Debug.Assert(procedures.Contains(caller));
            System.Diagnostics.Debug.Assert(procedures.Contains(callee));

            // TBD: check that the xref indeed refers to these two
            // procedures.
            XRef xCall = new XRef(
                type: xref.Type,
                source: caller.EntryPoint,
                target: callee.EntryPoint,
                dataLocation: xref.Source
            );
            graph.Add(xCall);
        }

        /// <summary>
        /// Gets the procedures that calls a given procedure.
        /// </summary>
        /// <param name="procedure">Procedure whose callers to find.</param>
        /// <returns>
        /// The calling procedures in order of their entry point address.
        /// Each calling procedure is returned only once.
        /// </returns>
        public IEnumerable<Procedure> GetCallers(Procedure procedure)
        {
            Address last = Address.Invalid;
            foreach (XRef xCall in graph.GetReferencesTo(procedure.EntryPoint))
            {
                if (xCall.Source != last)
                {
                    Procedure caller = procedures.Find(xCall.Source);
                    System.Diagnostics.Debug.Assert(caller != null);
                    yield return caller;
                    last = xCall.Source;
                }
            }
        }

        /// <summary>
        /// Gets the procedures called by a given procedure.
        /// </summary>
        /// <param name="procedure">Procedure whose callees to find.</param>
        /// <returns>
        /// The called procedures in order of their entry point address.
        /// Each called procedure is returned only once.
        /// </returns>
        public IEnumerable<Procedure> GetCallees(Procedure procedure)
        {
            Address last = Address.Invalid;
            foreach (XRef xCall in graph.GetReferencesFrom(procedure.EntryPoint))
            {
                if (xCall.Target != last)
                {
                    Procedure callee = procedures.Find(xCall.Target);
                    System.Diagnostics.Debug.Assert(callee != null);
                    yield return callee;
                    last = xCall.Target;
                }
            }
        }
    }
}

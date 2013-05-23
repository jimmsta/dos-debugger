using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;
using Util.Data;

namespace Disassembler2
{
    /// <summary>
    /// Provides methods to disassemble and analyze 16-bit x86 binary code.
    /// </summary>
    public class Disassembler16New
    {
        readonly Assembly program;

        public Disassembler16New(Assembly program)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            this.program = program;
        }

        /// <summary>
        /// Analyzes code starting from the given location. That location
        /// should be the entry point of a procedure, or otherwise the
        /// analysis may not work correctly.
        /// </summary>
        /// <param name="entryPoint">Specifies the location to start analysis.
        /// This location is relative to the beginning of the image.</param>
        /// <param name="entryType">Type of entry, should usually be JMP or
        /// CALL.</param>
        public void Analyze(ResolvedAddress entryPoint)
        {
            GenerateBasicBlocks(entryPoint);
            GenerateControlFlowGraph();
            GenerateProcedures();
            CheckSegmentOverlaps();

#if false
            /* Sort the XREFs built from the above analyses by target address. 
             * After this is done, the client can easily list the disassembled
             * instructions with xrefs sequentially in physical order.
             */
            VECTOR_QSORT(d->entry_points, compare_xrefs_by_target_and_source);
#endif
        }

        /// <summary>
        /// Analyzes code starting from the given location, and create basic
        /// blocks iteratively.
        /// </summary>
        public void GenerateBasicBlocks(ResolvedAddress entryPoint)
        {
            ResolvedAddress address = entryPoint;

            // Maintain a queue of basic block entry points to analyze. At
            // the beginning, only the user-specified entry point is in the
            // queue. As we encounter b/c/j instructions during the course
            // of analysis, we push the target addresses to the queue of
            // entry points to be analyzed later.
            PriorityQueue<XRef> xrefQueue =
                new PriorityQueue<XRef>(XRef.CompareByPriority);

            // Maintain a list of all procedure calls (with known target)
            // encountered during the analysis. After we finish analyzing
            // all the basic blocks, we update the list of procedures.
            // List<XRef> xrefCalls = new List<XRef>();

            // Create a a dummy xref entry using the user-supplied starting
            // address.
            xrefQueue.Enqueue(new XRef(
                type: XRefType.None,
                source: ResolvedAddress.Invalid,
                target: entryPoint
            ));

            // Analyze each cross reference in order of their priority.
            // In particular, if the xref is an indexed jump, we delay its
            // processing until we have processed all other types of xrefs.
            // This reduces the chance that we process past the end of a
            // jump table.
            while (!xrefQueue.IsEmpty)
            {
                XRef entry = xrefQueue.Dequeue();

                // Handle jump table entry, whose Target == Invalid.
                if (entry.Type == XRefType.NearIndexedJump)
                {
                    System.Diagnostics.Debug.Assert(entry.Target == ResolvedAddress.Invalid);

                    // Fill the Target field to make it a static xref.
                    entry = ProcessJumpTableEntry(entry, xrefQueue);
                    if (entry == null) // end of jump table
                        continue;
                }

                // Skip other dynamic xrefs.
                if (entry.Target == ResolvedAddress.Invalid)
                {
                    program.CrossReferences.Add(entry);
                    continue;
                }

                // Process the basic block starting at the target address.
                BasicBlock block = AnalyzeBasicBlock(entry, xrefQueue);
                if (block != null)
                {
                    //int count = block.Length;
                    //int baseOffset = PointerToOffset(entry.Target);
                    //proc.CodeRange.AddInterval(baseOffset, baseOffset + count);
                    //proc.ByteRange.AddInterval(baseOffset, baseOffset + count);
                    //for (int j = 0; j < count; j++)
                    //{
                    //    image[baseOffset + j].Procedure = proc;
                    //}
#if false
                    proc.AddBasicBlock(block);
#endif
                }

                program.CrossReferences.Add(entry);
            }
        }

        /// <summary>
        /// Generates control flow graph from existing xrefs.
        /// </summary>
        private void GenerateControlFlowGraph()
        {
            foreach (XRef xref in program.CrossReferences)
            {
                // Skip xrefs with unknown source (e.g. user-specified entry
                // point) or target (e.g. dynamic call or jump).
                if (xref.Source == ResolvedAddress.Invalid ||
                    xref.Target == ResolvedAddress.Invalid)
                    continue;

                // Find the basic blocks that owns the source location
                // and target location.
                BasicBlock sourceBlock = program.BasicBlocks.Find(xref.Source);
                BasicBlock targetBlock = program.BasicBlocks.Find(xref.Target);
                System.Diagnostics.Debug.Assert(sourceBlock != null);
                System.Diagnostics.Debug.Assert(targetBlock != null);

                // Create a directed edge from the source basic block to
                // the target basic block.
                program.BasicBlocks.AddControlFlowGraphEdge(
                    sourceBlock, targetBlock, xref);
            }
        }

        private void GenerateProcedures()
        {
            foreach (XRef xref in program.CrossReferences)
            {
                ResolvedAddress entryPoint = xref.Target;
                CallType callType = 
                    (xref.Type == XRefType.NearCall) ?
                    CallType.Near : CallType.Far;

                // If there is already a procedure defined at the given
                // entry point, perform some sanity checks.
                // TBD: should check and emit a message if two procedures
                // are defined at the same ResolvedAddress but with different
                // logical address.
                Procedure proc = program.Procedures.Find(entryPoint);
                if (proc != null)
                {
                    if (proc.CallType != callType)
                    {
                        AddError(entryPoint, ErrorCode.InconsistentCall,
                            "Procedure at entry point {0} has inconsistent call type.",
                            entryPoint);
                    }
                    // add call graph
                    continue;
                }

                // Create a procedure at the entry point. The entry point must
                // be the first byte of a basic block, or otherwise some flow
                // analysis error must have occurred. On the other hand, note
                // that multiple procedures may share one or more basic blocks
                // as part of their implementation.
                proc = new Procedure(entryPoint);
                proc.Name = "TBD";
                proc.CallType = callType;

                program.Procedures.Add(proc);
            }

            // Second pass: update the call graph.

        }

        private void GenerateCallGraph()
        {
#if false
            foreach (XRef xref in program.CrossReferences)
            {
                LogicalAddress entryPoint = xref.Target;
                CallType callType =
                    (xref.Type == XRefType.NearCall) ?
                    CallType.Near : CallType.Far;

                // If there is already a procedure defined at that entry
                // point, perform some sanity checks.
                // TBD: should check and emit a message if two procedures
                // are defined at the same ResolvedAddress but with different
                // logical address.
                Procedure proc = program.Procedures.Find(entryPoint);
                if (proc != null)
                {
                    if (proc.CallType != callType)
                    {
                        AddError(entryPoint, ErrorCode.InconsistentCall,
                            "Procedure at entry point {0} has inconsistent call type.",
                            entryPoint);
                    }
                    // add call graph
                    continue;
                }

                // Create a procedure at the entry point. The entry point must
                // be the first byte of a basic block, or otherwise some flow
                // analysis error must have occurred. On the other hand, note
                // that multiple procedures may share one or more basic blocks
                // as part of their implementation.
                proc = new Procedure(entryPoint);
                proc.Name = "TBD";
                proc.CallType = callType;

                program.Procedures.Add(proc);
            }
#endif
        }

        /// <summary>
        /// Adds all basic blocks, starting from the procedure's entry point,
        /// to the procedure's owning blocks. Note that multiple procedures
        /// may share one or more basic blocks.
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="xrefs"></param>
        private void MapBasicBlocksToProcedures(Procedure proc, XRefCollection xrefs)
        {
#if false
            // TODO: introduce ProcedureAlias, so that we don't need to
            // analyze the same procedure twice.

            LogicalAddress entryPoint = proc.EntryPoint;
            ResolvedAddress address = entryPoint.ResolvedAddress;
            // TODO: we need to make BasicBlock dependent on ResolvedAddress
            BasicBlock block = address.Image.BasicBlocks.GetValueOrDefault(address.Offset);

#endif
        }

        /// <summary>
        /// Checks for segment overlaps and emits error messages for
        /// overlapping segments.
        /// </summary>
        private void CheckSegmentOverlaps()
        {
#if false
            // Check for segment overlaps.
            Segment lastSegment = null;
            foreach (Segment segment in image.Segments)
            {
                if (lastSegment != null && segment.StartAddress < lastSegment.EndAddress)
                {
                    AddError(segment.StartAddress.ToFarPointer(segment.SegmentAddress),
                        ErrorCategory.Error,
                        "Segment {0:X4} overlaps with segment {1:X4}.",
                        lastSegment.SegmentAddress, segment.SegmentAddress);
                }
                lastSegment = segment;
            }
#endif
        }

        /// <summary>
        /// Fills the Target of an IndexedJump xref heuristically by plugging
        /// in the jump target stored in DataLocation and performing various
        /// sanity checks.
        /// </summary>
        /// <param name="entry">A xref of type IndexedJump whose Target field
        /// is Invalid.</param>
        /// <param name="xrefs">Collection to add a new dynamic IndexedJump
        /// xref to, if any.</param>
        /// <returns>The updated xref, or null if the jump table ends.</returns>
        private XRef ProcessJumpTableEntry(XRef entry, ICollection<XRef> xrefs)
        {
#if true
            return null;
#else
            System.Diagnostics.Debug.Assert(
                entry.Type == XRefType.NearIndexedJump &&
                entry.Target == LogicalAddress.Invalid,
                "Entry must be NearIndexedJump with unknown target");

            // Verify that the location that supposedly stores the jump table
            // entry is not analyzed as anything else. If it is, it indicates
            // that the jump table ends here.
            LinearPointer b = entry.DataLocation.LinearAddress;
            if (image[b].Type != ByteType.Unknown ||
                image[b + 1].Type != ByteType.Unknown)
                return null;

            // If the data location looks like in another segment, stop.
            if (image.LargestSegmentThatStartsBefore(b)
                > entry.Source.Segment)
            {
                return null;
            }

            // TBD: it's always a problem if CS:IP wraps. We need a more
            // general way to detect and fix it. For this particular case,
            // we need to check that the jump target is within the space
            // of this segment.
            if (entry.DataLocation.Offset >= 0xFFFE)
            {
                AddError(entry.DataLocation, ErrorCategory.Error,
                    "Jump table is too big (jumped from {0}).",
                    entry.Source);
                return null;
            }

            // Find the target address of the jump table entry.
            ushort jumpOffset = image.GetUInt16(b);
            Pointer jumpTarget = new Pointer(entry.Source.Segment, jumpOffset);

            // Check that the target address looks valid. If it doesn't, it
            // probably indicates that the jump table ends here.
            if (!image.IsAddressValid(jumpTarget.LinearAddress))
                return null;

            // If the jump target is outside the range of the current segment
            // but inside the range of a later segment, it likely indicates
            // that the jump table ends here.
            // TBD: this heuristic is kind of a hack... we should do better.
#if true
            if (image.LargestSegmentThatStartsBefore(jumpTarget.LinearAddress)
                > entry.Source.Segment)
            {
                return null;
            }
#endif

            // BUG: We really do need to check that the destination
            // is valid. If not, we should stop immediately.
            if (!(image[jumpTarget].Type == ByteType.Unknown ||
                  image[jumpTarget].Type == ByteType.Code &&
                  image[jumpTarget].IsLeadByte))
                return null;

            // ...

            // Mark DataLocation as data and add it to the owning procedure's
            // byte range.
            Piece piece = image.CreatePiece(
                entry.DataLocation, entry.DataLocation + 2, ByteType.Data);
            Procedure proc = image[entry.Source].Procedure;
            proc.AddDataBlock(piece.StartAddress, piece.EndAddress);

            // Add a dynamic xref from the JMP instruction to the next jump
            // table entry.
            xrefs.Add(new XRef(
                type: XRefType.NearIndexedJump,
                source: entry.Source,
                target: Pointer.Invalid,
                dataLocation: entry.DataLocation + 2
            ));

            // Return the updated xref with Target field filled.
            return new XRef(
                type: XRefType.NearIndexedJump,
                source: entry.Source,
                target: jumpTarget,
                dataLocation: entry.DataLocation
            );
#endif
        }

        /// <summary>
        /// Analyzes a contiguous sequence of instructions that form a basic
        /// block. The termination conditions include end-of-input, analyzed
        /// code/data, or any of the following instructions: RET, IRET, JMP,
        /// HLT.
        /// </summary>
        /// <param name="start">Address to begin analysis.</param>
        /// <returns>
        /// A new BasicBlock if one was created during the analysis.
        /// If analysis failed or an existing block was split into two,
        /// returns null.
        /// </returns>
        // TODO: should be roll-back the entire basic block if we 
        // encounters an error on our way? maybe not.
        private BasicBlock AnalyzeBasicBlock(XRef start, ICollection<XRef> xrefs)
        {
            ResolvedAddress pos = start.Target;
            ImageChunk image = pos.Image;
            ImageByte b = pos.ImageByte;

            // Check if the entry address is already analyzed.
            if (b.Type != ByteType.Unknown)
            {
                // Fail if we ran into data or padding while expecting code.
                if (b.Type != ByteType.Code)
                {
                    AddError(pos, ErrorCode.RanIntoData,
                        "XRef target is in the middle of data (referred from {0})",
                        start.Source);
                    return null;
                }

                // Now the byte was previously analyzed as code. We must have
                // already created a basic block.
                BasicBlock block = program.BasicBlocks.Find(pos);
                System.Diagnostics.Debug.Assert(block != null);
                
                // If the block starts at this address, we're done.
                if (block.Location == pos)
                    return null;

                // TBD: recover the following in some way...
#if false
                    if (image[b.BasicBlock.StartAddress].Address.Segment != pos.Segment)
                    {
                        AddError(pos, ErrorCategory.Error,
                            "Ran into the middle of a block [{0},{1}) from another segment " +
                            "when processing block {2} referred from {3}",
                            b.BasicBlock.StartAddress, b.BasicBlock.EndAddress,
                            start.Target, start.Source);
                        return null;
                    }
#endif

                // Now split the existing basic block into two. This requires
                // that the cut-off point is instruction boundary.
                if (!b.IsLeadByte)
                {
                    AddError(pos, ErrorCode.RanIntoCode,
                        "XRef target is in the middle of an instruction (referred from {0})",
                        start.Source);
                    return null;
                }
                program.BasicBlocks.SplitBasicBlock(block, pos);
                return null;
            }

            // Analyze each instruction in sequence until we encounter
            // analyzed code, flow instruction, or an error condition.
            while (true)
            {
                // Decode an instruction at this location.
                ResolvedAddress insnPos = pos;
                Instruction insn;
                try
                {
                    insn = pos.Image.DecodeInstruction(pos.Offset);
                }
                catch (BrokenFixupException ex)
                {
                    AddError(pos, ErrorCode.BrokenFixup, "Broken fix-up: {0}", ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    AddError(pos, ErrorCode.InvalidInstruction, "Bad instruction: {0}", ex.Message);
                    break;
                }

                // Create a code piece for this instruction.
                if (!pos.Image.CheckByteType(pos.Offset, insn.EncodedLength, ByteType.Unknown))
                {
                    AddError(pos, ErrorCode.OverlappingInstruction,
                        "Ran into the middle of code when processing block {0} referred from {1}",
                        start.Target, start.Source);
                    break;
                }

                // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                // if pos.off + count > 0xFFFF. This is probably not intended.
                try
                {
                    pos.Image.UpdateByteType(
                        pos.Offset, insn.EncodedLength, ByteType.Code);
                    pos.Image.Instructions.Add(pos.Offset, insn);
                    //pos = pos.Increment(insn.EncodedLength); // TODO: check address wrapping
                    pos = new ResolvedAddress(pos.Image, pos.Offset + insn.EncodedLength);
                }
                //catch (AddressWrappedException)
                catch (Exception)
                {
                    AddError(pos, ErrorCode.AddressWrapped,
                        "CS:IP wrapped when processing block {1} referred from {2}",
                        start.Target, start.Source);
                    break;
                }

                // Check if this instruction terminates the block.
                if (insn.Operation == Operation.RET ||
                    insn.Operation == Operation.RETF ||
                    insn.Operation == Operation.HLT)
                    break;

                // Analyze BCJ (branch, jump, call) instructions. Such an
                // instruction will create a cross reference.
                XRef xref = AnalyzeFlowInstruction(insnPos, insn);
                if (xref != null)
                {
                    xrefs.Add(xref);

                    // If the instruction is a conditional jump, add xref to
                    // the 'no-jump' branch.
                    // TODO: adding a no-jump xref causes confusion when we
                    // browse xrefs in the disassembly listing window. Is it
                    // truely necessary to add these xrefs?
                    if (xref.Type == XRefType.ConditionalJump)
                    {
                        xrefs.Add(new XRef(
                            type: XRefType.ConditionalJump,
                            source: insnPos,
                            target: pos
                        ));
                    }

                    // Finish basic block unless this is a CALL instruction.
                    if (xref.Type == XRefType.ConditionalJump ||
                        xref.Type == XRefType.NearJump ||
                        xref.Type == XRefType.FarJump ||
                        xref.Type == XRefType.NearIndexedJump)
                        break;
                }

                // If the new location is already analyzed as code, create a
                // control-flow edge from the previous block to the existing
                // block, and we are done.
                if (pos.ImageByte.Type == ByteType.Code)
                {
                    System.Diagnostics.Debug.Assert(pos.ImageByte.IsLeadByte);
                    break;
                }
            }

            // Create a basic block unless we failed on the first instruction.
            if (pos.Offset > start.Target.Offset)
            {
                Range<int> blockLocation = new Range<int>(
                   start.Target.Offset, pos.Offset);
                BasicBlock block = new BasicBlock(pos.Image, blockLocation);
                program.BasicBlocks.Add(block);
            }
            return null;
        }

        /// <summary>
        /// Analyzes an instruction and returns a xref if the instruction is
        /// one of the branch/call/jump instructions. Note that the 'no-jump'
        /// branch of a conditional jump instruction is not returned. The
        /// caller must manually create such a xref if needed.
        /// </summary>
        /// <param name="instruction">The instruction to analyze.</param>
        /// <returns>XRef if the instruction is a b/c/j instruction; 
        /// null otherwise.</returns>
        /// TBD: address wrapping if IP is above 0xFFFF is not handled. It should be.
        private XRef AnalyzeFlowInstruction(ResolvedAddress start, Instruction instruction)
        {
            Operation op = instruction.Operation;

            // Find the type of branch/call/jump instruction being processed.
            //
            // Note: If the instruction is a conditional jump, we assume that
            // the condition may be true or false, so that both "jump" and 
            // "no jump" is a reachable branch. If the code is malformed such
            // that either branch will never be executed, the analysis may not
            // work correctly.
            //
            // Note: If the instruction is a function call, we assume that the
            // subroutine being called will return. If the subroutine never
            // returns the analysis may not work correctly.
            XRefType bcjType;
            switch (op)
            {
                case Operation.JO:
                case Operation.JNO:
                case Operation.JB:
                case Operation.JAE:
                case Operation.JE:
                case Operation.JNE:
                case Operation.JBE:
                case Operation.JA:
                case Operation.JS:
                case Operation.JNS:
                case Operation.JP:
                case Operation.JNP:
                case Operation.JL:
                case Operation.JGE:
                case Operation.JLE:
                case Operation.JG:
                case Operation.JCXZ:
                case Operation.LOOP:
                case Operation.LOOPZ:
                case Operation.LOOPNZ:
                    bcjType = XRefType.ConditionalJump;
                    break;

                case Operation.JMP:
                    bcjType = XRefType.NearJump;
                    break;

                case Operation.JMPF:
                    bcjType = XRefType.FarJump;
                    break;

                case Operation.CALL:
                    bcjType = XRefType.NearCall;
                    break;

                case Operation.CALLF:
                    bcjType = XRefType.FarCall;
                    break;

                default:
                    // Not a b/c/j instruction; do nothing.
                    return null;
            }

            // Create a cross-reference depending on the type of operand.
            if (instruction.Operands[0] is RelativeOperand) // near jump/call to relative address
            {
                RelativeOperand opr = (RelativeOperand)instruction.Operands[0];
                return new XRef(
                    type: bcjType,
                    source: start,
                    target: start.IncrementWithWrapping(instruction.EncodedLength + opr.Offset.Value)
                );
            }

            if (instruction.Operands[0] is PointerOperand) // far jump/call to absolute address
            {
                // The story is very different when we take into account
                // fix-up information. If the target is an absolute address,
                // we indeed cannot process further.
                PointerOperand opr = (PointerOperand)instruction.Operands[0];
#if false
                return new XRef(
                    type: bcjType,
                    source: start,
                    target: new Pointer(opr.Segment.Value, (UInt16)opr.Offset.Value)
                );
#else
                return new XRef(
                    type: bcjType,
                    source: start,
                    target: ResolvedAddress.Invalid
                );
#endif
            }

            if (instruction.Operands[0] is MemoryOperand) // indirect jump/call
            {
                MemoryOperand opr = (MemoryOperand)instruction.Operands[0];

                // Handle static near jump table. We recognize a jump table 
                // heuristically if the instruction looks like the following:
                //
                //   jmpn word ptr cs:[bx+3782h] 
                //
                // That is, it meets the requirements that
                //   - the instruction is JMPN
                //   - the jump target is a word-ptr memory location
                //   - the memory location has CS prefix
                //   - a base register (e.g. bx) specifies the entry index
                //
                // Note that a malformed executable may create a jump table
                // not conforming to the above rules, or create a non-jump 
                // table that conforms to the above rules. We do not deal with
                // these cases for the moment.
                if (op == Operation.JMP &&
                    opr.Size == CpuSize.Use16Bit &&
                    opr.Segment == Register.CS &&
                    opr.Base != Register.None &&
                    opr.Index == Register.None)
                {
#if false
                    return new XRef(
                        type: XRefType.NearIndexedJump,
                        source: start,
                        target: Pointer.Invalid,
                        dataLocation: new Pointer(start.Segment, (UInt16)opr.Displacement.Value)
                    );
#else
                    return new XRef(
                        type: XRefType.NearJump,
                        source: start,
                        target: ResolvedAddress.Invalid
                        );
#endif
                }
            }

            // Other jump/call targets that we cannot recognize.
            AddError(start, ErrorCode.DynamicTarget,
                "Cannot determine target of {0} instruction.", op);

            return new XRef(
                type: bcjType,
                source: start,
                target: ResolvedAddress.Invalid
            );
        }

#if false
        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
#endif

        private void AddError(
            ResolvedAddress location, ErrorCode errorCode,
            string format, params object[] args)
        {
            program.Errors.Add(new Error(location, errorCode, string.Format(format, args)));
        }

#if false
        private void AddError(
            LogicalAddress location, ErrorCategory category,
            string format, params object[] args)
        {
            AddError(location, ErrorCode.GenericError, format, args);
        }

        private void AddError(LogicalAddress location, string format, params object[] args)
        {
            AddError(location, ErrorCategory.Error, format, args);
        }
#endif

        public static void Disassemble(Assembly assembly, ResolvedAddress entryPoint)
        {
            Disassembler16New dasm = new Disassembler16New(assembly);
            dasm.Analyze(entryPoint);
        }
    }
}

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
        private Assembly program;

#if false
        /// <summary>
        /// Maintains a queue of pending code entry points to analyze. At the
        /// beginning, there is only one entry point, which is the program
        /// entry point specified by the user. As we encounter branch 
        /// instructions (JMP, CALL, or Jcc) on the way, we push the target 
        /// addresses to the queue of entry points, so that they can be 
        /// analyzed later.
        /// </summary>
        //private List<XRef> globalXRefs;
        //private XRefCollection xrefCollection ;
#endif

        public Disassembler16New(Assembly program)
        {
            if (program == null)
                throw new ArgumentNullException("program");

            this.program = program;
        }

        /// <summary>
        /// Gets the assembly being disassembled.
        /// </summary>
        public Assembly Assembly
        {
            get { return program; }
        }

#if false
        /// <summary>
        /// Converts a SEG:OFF pointer to its offset within the image. The
        /// returned value may pass the end of the image.
        /// </summary>
        /// <param name="location">A pointer to convert.</param>
        /// <returns>The offset within the image.</returns>
        public static int PointerToOffset(Pointer location)
        {
            return location.Segment * 16 + location.Offset;
        }
#endif

        /// <summary>
        /// Analyzes code starting from the given location. That location
        /// should be the entry point of a procedure, or otherwise the
        /// analysis may not work correctly.
        /// </summary>
        /// <param name="entryPoint">Specifies the location to start analysis.
        /// This location is relative to the beginning of the image.</param>
        /// <param name="entryType">Type of entry, should usually be JMP or
        /// CALL.</param>
        public void Analyze(LogicalAddress entryPoint, XRefType entryType)
        {
            ResolvedAddress address = entryPoint.ResolvedAddress;
            if (!address.IsValid)
                throw new ArgumentOutOfRangeException("entryPoint");

            PriorityQueue<XRef> xrefQueue =
                new PriorityQueue<XRef>(XRef.CompareByPriority);

            // Create a a dummy xref entry using the user-supplied starting
            // address.
            xrefQueue.Enqueue(new XRef(
                type: entryType,
                source: LogicalAddress.Invalid,
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

                // Handle jump table entry (where Target == Invalid).
                if (entry.Type == XRefType.NearIndexedJump)
                {
                    System.Diagnostics.Debug.Assert(entry.Target == LogicalAddress.Invalid);

                    // Fill the Target field to make it a static xref.
                    entry = ProcessJumpTableEntry(entry, xrefQueue);
                    if (entry == null) // end of jump table
                        continue;
                }

                // Skip other dynamic xrefs.
                if (entry.Target == LogicalAddress.Invalid)
                {
                    program.CrossReferences.Add(entry);
                    continue;
                }

                Procedure proc;

                // Handle function call.
                if (entry.Type == XRefType.NearCall ||
                    entry.Type == XRefType.FarCall)
                {
                    CallType callType = (entry.Type == XRefType.NearCall) ?
                        CallType.Near : CallType.Far;

                    // If a procedure with that entry point has already been
                    // defined, perform some sanity checks but no need to
                    // analyze again.
                    proc = program.Procedures.Find(entry.Target);
                    if (proc != null)
                    {
                        if (proc.CallType != callType)
                        {
                            AddError(entry.Target, ErrorCategory.Error,
                                "Procedure {0} has inconsistent call type.",
                                proc.EntryPoint);
                        }
                        program.CrossReferences.Add(entry);
                        continue;
                    }

                    // Create a new Procedure object with that entry point.
                    // TODO: we may be calling into the middle of an already
                    // defined procedure. This can happen if two procedures
                    // share a chunk of code. We need to handle this later.
                    proc = program.Procedures.Create(entry.Target);
                    proc.CallType = (entry.Type == XRefType.NearCall) ?
                        CallType.Near : CallType.Far;
                }
                else
                {
                    proc = address.ImageByte.Procedure;
                    // TBD: how do we know this is not null?
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
                    proc.AddBasicBlock(block);
                }

                program.CrossReferences.Add(entry);
            }

            // Update the segment statistics.
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
        /// Analyzes a continuous sequence of instructions that form a basic
        /// block. The termination conditions include end-of-input, analyzed
        /// code/data, or any of the following instructions: RET, IRET, JMP,
        /// HLT.
        /// </summary>
        /// <param name="start">Address to begin analysis.</param>
        /// <param name="jumps">Jump instructions are added to this list.</param>
        /// <param name="calls">Call instructions are added to this list.</param>
        /// <param name="dynamicJumps">Jump instructions with a dynamic target
        /// are added to this list.</param>
        /// <returns>
        /// A new BasicBlock if one was created during the analysis.
        /// If analysis failed or an existing block was split into two,
        /// returns null.
        /// </returns>
        // TODO: should be roll-back the entire basic block if we 
        // encounters an error on our way? maybe not.
        private BasicBlock AnalyzeBasicBlock(XRef start, ICollection<XRef> xrefs)
        {
            LogicalAddress pos = start.Target;
            ImageByte b = pos.ImageByte;

            // Check if we are running into the middle of code or data. This
            // can only happen when we process the first instruction in the
            // block.
            if (b.Type != ByteType.Unknown && !b.IsLeadByte) // TODO: handle padding byte
            {
                AddError(pos,
                    "XRef target is in the middle of code/data (referred from {0})",
                    start.Source);
                return null;
            }

            // Check if this location is already analyzed as code.
            if (b.Type == ByteType.Code)
            {
                // Now we are already covered by a basic block. If the
                // basic block *starts* from this address, do nothing.
                // Otherwise, split the basic block into two.
                if (b.BasicBlock.Location.Begin == pos.ImageOffset)
                {
                    return null;
                }
                else
                {
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
                    BasicBlock newBlock = b.BasicBlock.Split(pos.ImageOffset);
                    return null; // newBlock;
                }
            }

            // Analyze each instruction in sequence until we encounter
            // analyzed code, flow instruction, or an error condition.
            while (true)
            {
                // Decode an instruction at this location.
                LogicalAddress insnPos = pos;
                Instruction insn;
                try
                {
                    insn = pos.Image.DecodeInstruction(pos.ImageOffset);
                }
                catch (Exception ex)
                {
                    AddError(pos, "Bad instruction: {0}", ex.Message);
                    break;
                }

                // Create a code piece for this instruction.
                if (!pos.Image.CheckByteType(pos.ImageOffset, insn.EncodedLength, ByteType.Unknown))
                {
                    AddError(pos, 
                        "Ran into the middle of code when processing block {0} referred from {1}",
                        start.Target, start.Source);
                    break;
                }

                // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                // if pos.off + count > 0xFFFF. This is probably not intended.
                try
                {
                    pos.Image.UpdateByteType(
                        pos.ImageOffset, insn.EncodedLength, ByteType.Code);
                    pos = pos.Increment(insn.EncodedLength); // TODO: check address wrapping
                }
                //catch (AddressWrappedException)
                catch (Exception)
                {
                    AddError(pos,
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
            if (pos.ReferentOffset > start.Target.ReferentOffset)
            {
                Range<int> blockLocation = new Range<int>(
                   start.Target.ImageOffset, pos.ImageOffset);
                BasicBlock block = new BasicBlock(pos.Image, blockLocation);
                pos.Image.BasicBlocks.Add(blockLocation, block);
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
        private XRef AnalyzeFlowInstruction(LogicalAddress start, Instruction instruction)
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
                    target: LogicalAddress.Invalid
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
                        target: LogicalAddress.Invalid
                        );
#endif
                }
            }

            // Other jump/call targets that we cannot recognize.
            AddError(start, ErrorCategory.Message,
                "Cannot determine target of {0} instruction.", op);

            return new XRef(
                type: bcjType,
                source: start,
                target: LogicalAddress.Invalid
            );
        }

#if false
        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
#endif

        private void AddError(
            LogicalAddress location, ErrorCategory category,
            string format, params object[] args)
        {
            //image.Errors.Add(new Error(location, string.Format(format, args), category));
        }

        private void AddError(LogicalAddress location, string format, params object[] args)
        {
            AddError(location, ErrorCategory.Error, format, args);
        }
    }

    public class Error
    {
        public ErrorCategory Category { get; private set; }
        public LogicalAddress Location { get; private set; }
        public string Message { get; private set; }

        public Error(LogicalAddress location, string message, ErrorCategory category)
        {
            this.Category = category;
            this.Location = location;
            this.Message = message;
        }

        public Error(LogicalAddress location, string message)
            : this(location, message, ErrorCategory.Error)
        {
        }

        public static int CompareByLocation(Error x, Error y)
        {
            return LogicalAddress.CompareByLexical(x.Location, y.Location);
        }
    }

    [Flags]
    public enum ErrorCategory
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Message = 4,
    }
}

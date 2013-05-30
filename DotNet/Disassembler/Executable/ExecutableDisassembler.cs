using System;
using System.Collections.Generic;
using System.Text;
using Util.Data;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Provides methods to disassemble a 16-bit executable without symbol
    /// information.
    /// </summary>
    public class ExecutableDisassembler : DisassemblerBase
    {
        readonly Executable executable;

        public ExecutableDisassembler(Executable executable)
            : base(executable.Image)
        {
            this.executable = executable;
        }

        public override Assembly Assembly
        {
            get { return executable; }
        }

        /// <summary>
        /// Gets the executable being disassembled.
        /// </summary>
        public Executable Executable
        {
            get { return executable; }
        }

        protected override Instruction DecodeInstruction(Address address)
        {
            // Check if the address is relocatable. If it is, it
            // cannot be an instruction ah...
            if (((ExecutableImage)image).IsAddressRelocatable(address))
            {
                AddError(address, ErrorCode.BrokenFixup,
                    "Cannot decode an instruction at a relocatable location.");
                return null;
            }
            return base.DecodeInstruction(address);
        }

        protected override Address ResolveFlowInstructionTarget(PointerOperand operand)
        {
            // TBD: need to perform mapping from segment address to segment id.
            // TBD: need to check fixups and avoid absolute calls.

            int segment = executable.Image.MapFrameToSegment(operand.Segment.Value);
            return new Address(segment, (int)operand.Offset.Value);

            //int segmentId = executable.GetSegment((int)operand.Segment.Value);
            //return new Address(segmentId, (int)operand.Offset.Value);
            //return new Address(operand.Segment.Value, (int)operand.Offset.Value);
        }

        protected override void GenerateProcedures()
        {
            base.GenerateProcedures();
            foreach (Procedure proc in Procedures)
            {
                int offset = proc.EntryPoint.Segment * 16 + proc.EntryPoint.Offset;
                proc.Name = string.Format("sub_{0:X5}", offset);
            }
        }

#if false
        /// <summary>
        /// Checks for segment overlaps and emits error messages for
        /// overlapping segments.
        /// </summary>
        private void CheckSegmentOverlaps()
        {
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
        }
#endif

#if false
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
            System.Diagnostics.Debug.Assert(
                entry.Type == XRefType.NearIndexedJump &&
                entry.Target == Pointer.Invalid,
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
        }
#endif

#if false
        private BasicBlock AnalyzeBasicBlock(XRef start, ICollection<XRef> xrefs)
        {
            Pointer pos = start.Target;

            // Check if we are running into the middle of code or data. This
            // can only happen when we process the first instruction in the
            // block.
            if (image[pos].Type != ByteType.Unknown && !image[pos].IsLeadByte)
            {
                AddError(pos, ErrorCategory.Error,
                    "XRef target is in the middle of code/data (referred from {0})",
                    start.Source);
                return null;
            }

            // Check if this location is already analyzed as code.
            if (image[pos].Type == ByteType.Code)
            {
                ByteProperties b = image[pos];

                // Now we are already covered by a basic block. If the
                // basic block *starts* from this address, do nothing.
                // Otherwise, split the basic block into two.
                if (b.BasicBlock.StartAddress == pos.LinearAddress)
                {
                    return null;
                }
                else
                {
                    if (image[b.BasicBlock.StartAddress].Address.Segment != pos.Segment)
                    {
                        AddError(pos, ErrorCategory.Error,
                            "Ran into the middle of a block [{0},{1}) from another segment " +
                            "when processing block {2} referred from {3}",
                            b.BasicBlock.StartAddress, b.BasicBlock.EndAddress,
                            start.Target, start.Source);
                        return null;
                    }
                    BasicBlock newBlock = b.BasicBlock.Split(pos.LinearAddress);
                    return null; // newBlock;
                }
            }

            // Analyze each instruction in sequence until we encounter
            // analyzed code, flow instruction, or an error condition.
            while (true)
            {
                // Decode an instruction at this location.
                Pointer insnPos = pos;
                Instruction insn;
                try
                {
                    insn = image.DecodeInstruction(pos);
                }
                catch (Exception ex)
                {
                    AddError(pos, ErrorCategory.Error, "Bad instruction: {0}", ex.Message);
                    break;
                }

                // Create a code piece for this instruction.
                if (!image.CheckByteType(pos, pos + insn.EncodedLength, ByteType.Unknown))
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
                    Piece piece = image.CreatePiece(pos, pos + insn.EncodedLength, ByteType.Code);
                    pos += insn.EncodedLength;
                }
                catch (AddressWrappedException)
                {
                    AddError(pos, ErrorCategory.Error,
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
                if (image[pos].Type == ByteType.Code)
                {
                    System.Diagnostics.Debug.Assert(image[pos].IsLeadByte);
                    break;
                }
            }

            // Create a basic block unless we failed on the first instruction.
            if (pos.LinearAddress > start.Target.LinearAddress)
                return image.CreateBasicBlock(start.Target.LinearAddress, pos.LinearAddress);
            else
                return null;
        }
#endif

#if false
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
        private XRef AnalyzeFlowInstruction(Pointer start, Instruction instruction)
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
                PointerOperand opr = (PointerOperand)instruction.Operands[0];
                return new XRef(
                    type: bcjType,
                    source: start,
                    target: new Pointer(opr.Segment.Value, (UInt16)opr.Offset.Value)
                );
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
                    return new XRef(
                        type: XRefType.NearIndexedJump,
                        source: start,
                        target: Pointer.Invalid,
                        dataLocation: new Pointer(start.Segment, (UInt16)opr.Displacement.Value)
                    );
                }
            }

            // Other jump/call targets that we cannot recognize.
            AddError(start, ErrorCategory.Message,
                "Cannot determine target of {0} instruction.", op);

            return new XRef(
                type: bcjType,
                source: start,
                target: Pointer.Invalid
            );
        }
#endif

        public override void Analyze()
        {
            base.Analyze(executable.Image.EntryPoint);
        }

#if false
        public static void Disassemble(Executable executable, Address entryPoint)
        {
            ExecutableDisassembler dasm = new ExecutableDisassembler(executable);
            dasm.Analyze(entryPoint);
        }
#endif
    }

}

#if false

typedef struct dasm_jump_table_t
{
    dasm_farptr_t insn_pos; /* location of the jump instruction */
    dasm_farptr_t start;    /* location of the start of the jump table */
    dasm_farptr_t current;  /* location of the next jump entry to process */
} dasm_jump_table_t;



/* Print statistics about the number of bytes analyzed. */
void dasm_stat(x86_dasm_t *d)
{
    size_t b;
    size_t total = d->image_size, code = 0, data = 0, insn = 0;

    for (b = 0; b < total; b++)
    {
        if ((d->attr[b] & ATTR_TYPE) == TYPE_CODE)
        {
            ++code;
            if (d->attr[b] & ATTR_BOUNDARY)
                ++insn;
        }
        else if ((d->attr[b] & ATTR_TYPE) == TYPE_DATA)
            ++data;
    }

    fprintf(stderr, "Image size: %d bytes\n", total);
    fprintf(stderr, "Code size : %d bytes\n", code);
    fprintf(stderr, "Data size : %d bytes\n", data);
    fprintf(stderr, "# Instructions: %d\n", insn);

    fprintf(stderr, "Jump tables: %d\n", VECTOR_SIZE(d->jump_tables));
}

static int verbose = 0;


static int compare_xrefs_by_target_and_source(const dasm_xref_t *a, const dasm_xref_t *b)
{
    int cmp = (int)FARPTR_TO_OFFSET(a->target) - (int)FARPTR_TO_OFFSET(b->target);
    if (cmp == 0)
        cmp = (int)FARPTR_TO_OFFSET(a->source) - (int)FARPTR_TO_OFFSET(b->source);
    return cmp;
}

static int compare_xrefs_by_target(const dasm_xref_t *a, const dasm_xref_t *b)
{
    return (int)FARPTR_TO_OFFSET(a->target) - (int)FARPTR_TO_OFFSET(b->target);
}


/* Returns the next xref that refers to the given target address. */
const dasm_xref_t * 
dasm_enum_xrefs(
    x86_dasm_t *d,              /* disassembler object */
    uint32_t target_offset,     /* absolute address of target byte */
    const dasm_xref_t *prev)    /* previous xref; NULL for first one */   
{
    const dasm_xref_t *first = VECTOR_DATA(d->entry_points);
    const dasm_xref_t *xref;

    /* If target is -1, return the next xref without filtering target. */
    if (target_offset == (uint32_t)(-1))
    {
        xref = (prev == NULL)? first : prev + 1;
        return (xref < first + VECTOR_SIZE(d->entry_points))? xref : NULL;
    }

    /* If prev is NULL, find the first xref that matches the target. */
    if (prev == NULL) 
    {
        dasm_xref_t match;
        match.target.seg = (uint16_t)(target_offset >> 4);
        match.target.off = (uint16_t)(target_offset & 0xf);
        
        xref = bsearch(&match, first, VECTOR_SIZE(d->entry_points), sizeof(match),
            compare_xrefs_by_target);
        if (xref == NULL)
            return NULL;

        /* If there are multiple matches, bsearch() may return any one of
         * them. So we need to move the pointer to the first one.
         */
        while (xref > first && FARPTR_TO_OFFSET(xref[-1].target) == target_offset)
            --xref;
        return xref;
    }

    /* Return the next xref if it matches target_offset. */
    xref = prev + 1;
    if (xref < first + VECTOR_SIZE(d->entry_points) &&
        FARPTR_TO_OFFSET(xref->target) == target_offset)
        return xref;
    else
        return NULL;
}

#endif

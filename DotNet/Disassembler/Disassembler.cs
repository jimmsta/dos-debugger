using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Provides methods to disassemble and analyze 16-bit x86 binary code.
    /// </summary>
    public class Disassembler16
    {
        private BinaryImage image;

        /// <summary>
        /// Maintains a queue of pending code entry points to analyze. At the
        /// beginning, there is only one entry point, which is the program
        /// entry point specified by the user. As we encounter branch 
        /// instructions (JMP, CALL, or Jcc) on the way, we push the target 
        /// addresses to the queue of entry points, so that they can be 
        /// analyzed later.
        /// </summary>
        private List<XRef> globalXRefs;

        /// <summary>
        /// Maintains a dictionary that maps the entry point offset of a
        /// procedure to a Procedure object.
        /// </summary>
        private SortedList<int, Procedure> procedures 
            = new SortedList<int, Procedure>();

        /// <summary>
        /// Maintains a dictionary that maps a 16-bit segment number to a
        /// Segment object. 
        /// Ideally we should have each segment starting at offset 0, but
        /// this may not be the case in an executable.
        /// </summary>
        private SortedList<UInt16, Segment> segments
            = new SortedList<ushort, Segment>();

        /// <summary>
        /// -Maintains a dictionary that maps an offset to an Error object
        /// -where the error occurred at this offset.
        /// </summary>
        private List<Error> errors = new List<Error>();

        public Disassembler16(byte[] image, Pointer baseAddress)
        {
            this.image = new BinaryImage(image, baseAddress);
            this.globalXRefs = new List<XRef>();
        }

        /// <summary>
        /// Gets the executable image being disassembled.
        /// </summary>
        public BinaryImage Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the base address of the executable image. This address 
        /// always has zero offset.
        /// </summary>
        public Pointer BaseAddress
        {
            get { return image.BaseAddress; }
        }

        /// <summary>
        /// Gets a collection of analyzed segments. The segments are returned
        /// in order of their 16-bit segment number.
        /// </summary>
        public ICollection<Segment> Segments
        {
            get { return segments.Values; }
        }

        public Segment FindSegment(ushort seg)
        {
            return segments[seg];
        }

        /// <summary>
        /// Gets a collection of analyzed procedures. The procedures are
        /// returned in order of their entry point offset.
        /// </summary>
        public ICollection<Procedure> Procedures
        {
            get { return procedures.Values; }
        }

        public Error[] Errors
        {
            get { return errors.ToArray(); }
        }

        public IEnumerable<XRef> GetReferencesTo(Pointer location)
        {
            foreach (XRef xref in globalXRefs)
            {
                if (xref.Target == location)
                    yield return xref;
            }
        }

        /// <summary>
        /// Converts a CS:IP pointer to its offset within the executable
        /// image. Note that different CS:IP pointers may correspond to the
        /// same offset.
        /// </summary>
        /// <param name="location">A pointer to convert.</param>
        /// <returns>The offset within the executable image.</returns>
        public int PointerToOffset(Pointer location)
        {
            return image.PointerToOffset(location);
        }

        /// <summary>
        /// Analyzes code starting from the given location. That location
        /// must be the entry point of a procedure, or otherwise the analysis
        /// may not work correctly.
        /// </summary>
        /// <param name="start">Entry point of a procedure.</param>
        /// <param name="recursive">Whether to analyze functions called by
        /// this procedure.</param>
        public void Analyze(Pointer start)
        {
            // Create a a dummy xref entry using the user-supplied starting
            // address.
            List<XRef> xrefs = new List<XRef>();
            xrefs.Add(new XRef
            {
                Target = start,
                Source = Pointer.Invalid,
                Type = XRefType.FarCall,
            });

            List<XRef> jumpTables = new List<XRef>();

            int iJumpTable = 0;
            int i = 0;
            while (true)
            {
                // Analyze each procedure in the list of procedures.
                for (; i < xrefs.Count; i++)
                {
                    XRef entry = xrefs[i];

                    // Process dynamic xrefs (i.e. whose Target is Invalid).
                    if (entry.Target == Pointer.Invalid)
                    {
                        // If this xref is an indexed jump, add it to the list
                        // of jump table entries to process later.
                        if (entry.Type == XRefType.NearIndexedJump)
                        {
                            jumpTables.Add(entry);
                            continue;
                        }

                        // Skip other types of dynamic xrefs.
                        continue;
                    }

                    // If this xref is a function call, analyze the function
                    // if the entry point has not yet analyzed yet.
                    if (entry.Type == XRefType.NearCall ||
                        entry.Type == XRefType.FarCall)
                    {
                        if (!procedures.ContainsKey(PointerToOffset(entry.Target)))
                        {
                            // Create a procedure at this location.
                            Procedure proc = new Procedure();
                            proc.EntryPoint = entry.Target;

                            AnalyzeProcedure(proc, entry, xrefs);

                            // Mark the procedure as processed.
                            procedures[PointerToOffset(entry.Target)] = proc;
                        }
                        continue;
                    }

                    // If this xref is an indexed jump, process the
                    // procedure again.
                    if (entry.Type == XRefType.NearIndexedJump)
                    {
                        // Find out which procedure entry.Source belongs to.
                        Procedure proc = image[entry.Source].Procedure;
                        AnalyzeProcedure(proc, entry, xrefs);
                    }
                }

                // Process any entries in the jump table.
                if (iJumpTable >= jumpTables.Count)
                    break;

                ProcessJumpTableEntry(jumpTables[iJumpTable], xrefs);
                //System.Diagnostics.Debug.WriteLine("Processing jump table at " + entry.Source.ToString());
                iJumpTable++;
            }

            // Add the cross references to the global xref list.
            globalXRefs.AddRange(xrefs);

            // Update the segment statistics.
            UpdateSegmentInformation();

#if false
#if false
    fprintf(stderr, "\n-- Statistics after initial analysis --\n");
    dasm_stat(d);
#endif
            /* Sort the XREFs built from the above analyses by target address. 
             * After this is done, the client can easily list the disassembled
             * instructions with xrefs sequentially in physical order.
             */
            VECTOR_QSORT(d->entry_points, compare_xrefs_by_target_and_source);
#endif
        }

        /// <summary>
        /// Updates segmentation information of the executable. This method
        /// must be called AFTER all the procedures have been analyzed, as
        /// it simply performs a byte-by-byte review of the executable.
        /// </summary>
        private void UpdateSegmentInformation()
        {
            segments.Clear();
            for (int i = 0; i < image.Length; i++)
            {
                ByteProperties b = image[i];
                if (b.Address != Pointer.Invalid)
                {
                    ushort seg = b.Address.Segment;
                    if (!segments.ContainsKey(seg))
                    {
                        segments.Add(seg, new Segment
                        {
                            StartAddress = b.Address,
                            EndAddress = b.Address + 1,
                        });
                    }
                    else
                    {
                        segments[seg].EndAddress = b.Address + 1;
                    }
                }
            }

            // Check for segment overlaps.
            for (int i = 0; i < segments.Count; i++)
            {
                Segment s1 = segments.Values[i];
                for (int j = i + 1; j < segments.Count; j++)
                {
                    Segment s2 = segments.Values[j];
                    if (s1.EndAddress.EffectiveAddress >
                        s2.StartAddress.EffectiveAddress)
                    {
                        errors.Add(new Error(
                            s2.StartAddress,
                            string.Format("Segment {0:X4} overlaps with segment {1:X4}.",
                            s1.StartAddress.Segment, s2.StartAddress.Segment)));
                    }
                }
            }
        }

        /// <summary>
        /// Analyzes a procedure starting from the given location. This
        /// location should be within the procedure, but is not necessarily
        /// the entry point of the procedure.
        /// 
        /// The analysis does not recurse into procedures called by this
        /// procedure.
        /// </summary>
        /// <param name="start">Address to start analysis.</param>
        /// <param name="xrefs">List of procedures called by this 
        /// procedure.</param>
        private void AnalyzeProcedure(Procedure proc, XRef start, List<XRef> xrefs)
        {
            List<XRef> localXrefs = new List<XRef>();

            // Create a dummy xref for the starting address.
            localXrefs.Add(new XRef
            {
                Source = start.Source,
                Target = start.Target,
                Type = XRefType.UserSpecified
            });
            
            // Process each entry point in the queue until there are no more
            // entry points left.
            for (int i = 0; i < localXrefs.Count; i++)
            {
                XRef entry = localXrefs[i];

                // Skip dynamic xrefs, function calls, and jump tables.
                if (entry.Target == Pointer.Invalid ||
                    entry.Type == XRefType.NearCall ||
                    entry.Type == XRefType.FarCall)
                {
                    continue;
                }
                //if (entry.Type == XRefType.NearJumpTableEntry)
                //{
                //    continue;
                //}

                // Process this code block assuming no jumps are taken and
                // function calls and interrupts all return.
                BasicBlock block = AnalyzeBasicBlock(entry, localXrefs);
                if (block != null)
                {
                    int count = block.Length;
                    int baseOffset = PointerToOffset(entry.Target);
                    proc.CodeRange.AddInterval(baseOffset, baseOffset + count);
                    proc.ByteRange.AddInterval(baseOffset, baseOffset + count);
                    for (int j = 0; j < count; j++)
                    {
                        image[baseOffset + j].Procedure = proc;
                    }
                }
            }

            // Append the local XRef list to the global xref list.
            localXrefs.RemoveAt(0);
            xrefs.AddRange(localXrefs);
        }

        private void ProcessJumpTableEntry(XRef entry, ICollection<XRef> xrefs)
        {
            if (entry.Type != XRefType.NearIndexedJump ||
                entry.Target != Pointer.Invalid)
                throw new ArgumentException("xref type mismatch.");

            // If the target refers to a jump table entry, we delay its
            // processing until we have processed all other types of
            // cross-references. This reduces the chance that we process
            // past the end of the jump table.

            // Process this one. If the jump target is already analyzed, it
            // probably means the jump table entry is fake, i.e. the jump
            // table ends one entry before.
            int b = PointerToOffset(entry.DataLocation);
            if (image[b].Type != ByteType.Unknown ||
                image[b + 1].Type != ByteType.Unknown)
                return;

            // If the jump target is outside the range of the current segment
            // but inside the range of another segment, it likely indicates
            // that the jump table entry is fake, i.e. the jump table ends on
            // the previous entry.
            // ...

            // Add this data item to its owning procedure's byte range.
            Procedure proc = image[entry.Source].Procedure;
            proc.DataRange.AddInterval(b, b + 2);
            proc.ByteRange.AddInterval(b, b + 2);

            // Mark the memory location specified by the jump table entry
            // as data.
#if false
            image.CreatePiece(entry.DataLocation, entry.DataLocation + 2, ByteType.Data);
#else
            image[b] = new ByteProperties
            {
                Type = ByteType.Data,
                IsLeadByte = true,
                Address = entry.DataLocation,
                Procedure = proc,
            };
            image[b + 1] = new ByteProperties
            {
                Type = ByteType.Data,
                IsLeadByte = false,
                Address = entry.DataLocation + 1,
                Procedure = proc,
            };
#endif

            // Read the jump offset.
            ushort jumpOffset = image.GetUInt16(b);

            // BUG: We really do need to check that the destination
            // is valid. If not, we should stop immediately.
            // This whole jump table thing needs to be improved.

            // Add a xref from the JMP instruction to the jump target.
            xrefs.Add(new XRef
            {
                Source = entry.Source,
                Target = new Pointer(entry.Source.Segment, jumpOffset),
                DataLocation = entry.DataLocation,
                Type = XRefType.NearIndexedJump
            });

            // Add a dynamic xref from the JMP instruction to the next jump
            // table entry.
            // BUG: We actually should do this in the handler for the
            // NearJumpTableTarget xref, and only add this if that
            // call succeeded.
            xrefs.Add(new XRef
            {
                Source = entry.Source,
                Target = Pointer.Invalid,
                DataLocation = entry.DataLocation + 2,
                Type = XRefType.NearIndexedJump
            });
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
        /// <returns>The number of bytes successfully analyzed as code.</returns>
        // TODO: should be roll-back the entire basic block if we 
        // encounters an error on our way? maybe not.
        private BasicBlock AnalyzeBasicBlock(XRef start, ICollection<XRef> xrefs)
        {
            Pointer pos = start.Target;

            // Check if we are running into the middle of code or data. This
            // can only happen when we process the first instruction in the
            // block.
            if (image[pos].Type != ByteType.Unknown && !image[pos].IsLeadByte)
            {
                errors.Add(new Error(pos, string.Format(
                    "XRef target is in the middle of code/data (referred from {0})",
                    start.Source)));
                return null;
            }

            // Check if this location is already analyzed as code.
            if (image[pos].Type == ByteType.Code)
            {
                ByteProperties b = image[pos];

                // Now we are already covered by a basic block. If the
                // basic block *starts* from this address, do nothing.
                // Otherwise, split the basic block into two.
                if (b.BasicBlock.Start.EffectiveAddress == pos.EffectiveAddress)
                {
                    return null;
                }
                else
                {
                    if (b.BasicBlock.Start.Segment != pos.Segment)
                    {
                        errors.Add(new Error(pos, string.Format(
                            "Ran into the middle of a block [{0},{1}) from another segment " +
                            "when processing block {2} referred from {3}",
                            b.BasicBlock.Start, b.BasicBlock.End,
                            start.Target, start.Source)));
                        return null;
                    }
                    BasicBlock newBlock = b.BasicBlock.Split(pos);
                    return newBlock;
                }
            }

            // Analyze each instruction in sequence until we encounter
            // analyzed code, flow instruction, or an error condition.
            while (true)
            {
                // Decode an instruction at this location.
                Instruction insn;
                try
                {
                    insn = image.DecodeInstruction(pos);
                }
                catch (Exception ex)
                {
                    errors.Add(new Error(pos, "Bad instruction: " + ex.Message));
                    break;
                }

                // Create a code piece for this instruction.
                if (!image.CheckByteType(pos, pos + insn.EncodedLength, ByteType.Unknown))
                {
                    errors.Add(new Error(pos, string.Format(
                        "Ran into the middle of code when processing block {0} referred from {1}",
                        start.Target, start.Source)));
                    break;
                }

                Piece piece = image.CreatePiece(pos, pos + insn.EncodedLength, ByteType.Code);

                // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                // if pos.off + count > 0xFFFF. This is probably not intended.
                if (pos.Offset + insn.EncodedLength > 0xFFFF)
                {
                    errors.Add(new Error(pos, string.Format(
                        "CS:IP wrapped when processing block {1} referred from {2}",
                        start.Target, start.Source)));
                    break;
                }
                pos += insn.EncodedLength;

                // Check if this instruction terminates the block.
                if (insn.Operation == Operation.RET ||
                    insn.Operation == Operation.RETF ||
                    insn.Operation == Operation.HLT)
                    break;

                // Analyze BCJ (branch, jump, call) instructions. Such an
                // instruction will create a cross reference.
                XRef xref = AnalyzeFlowInstruction(insn);
                if (xref != null)
                {
                    xrefs.Add(xref);

                    // If the instruction is a conditional jump, add xref to
                    // the 'no-jump' branch.
                    if (xref.Type == XRefType.ConditionalJump)
                    {
                        xrefs.Add(new XRef
                        {
                            Type = XRefType.ConditionalJump,
                            Source = insn.Location,
                            Target = pos
                        });
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
            if (pos.EffectiveAddress > start.Target.EffectiveAddress)
                return image.CreateBasicBlock(start.Target, pos);
            else
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
        private XRef AnalyzeFlowInstruction(Instruction instruction)
        {
            Pointer start = instruction.Location;
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
                return new XRef
                {
                    Source = start,
                    Target = start + instruction.EncodedLength + opr.Offset,
                    Type = bcjType
                };
            }
            
            if (instruction.Operands[0] is PointerOperand) // far jump/call to absolute address
            {
                PointerOperand opr = (PointerOperand)instruction.Operands[0];
                return new XRef
                {
                    Source = start,
                    Target = opr.Value,
                    Type = bcjType
                };
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
                    return new XRef
                    {
                        Source = start,
                        Target = Pointer.Invalid,
                        DataLocation = new Pointer(start.Segment, (UInt16)opr.Displacement),
                        Type = XRefType.NearIndexedJump
                    };
                }
            }

            // Other jump/call targets that we cannot recognize.
            AddError(start, ErrorCategory.Message,
                "Cannot determine target of {0} instruction.", op);

            return new XRef
            {
                Source = start,
                Target = Pointer.Invalid,
                Type = bcjType
            };
        }

#if false
        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
#endif

        private void AddError(
            Pointer location, ErrorCategory category,
            string format, params object[] args)
        {
            errors.Add(new Error(location, string.Format(format, args), category));
        }
    }

    public class Error
    {
        public ErrorCategory Category { get; private set; }
        public Pointer Location { get; private set; }
        public string Message { get; private set; }

        public Error(Pointer location, string message, ErrorCategory category)
        {
            this.Category = category;
            this.Location = location;
            this.Message = message;
        }

        public Error(Pointer location, string message)
            : this(location, message, ErrorCategory.Error)
        {
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

    public class ErrorLocationComparer : IComparer<Error>
    {
        public int Compare(Error x, Error y)
        {
            return x.Location.ToString().CompareTo(y.Location.ToString());
        }
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

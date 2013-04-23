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
        private byte[] image;
        private ByteAttribute[] attr;
        private UInt16[] byteSegment;
        private Procedure[] byteToProcedure;

        /// <summary>
        /// Maintains a queue of pending code entry points to analyze. At the
        /// beginning, there is only one entry point, which is the program
        /// entry point specified by the user. As we encounter branch 
        /// instructions (JMP, CALL, or Jcc) on the way, we push the target 
        /// addresses to the queue of entry points, so that they can be 
        /// analyzed later.
        /// </summary>
        private List<XRef> globalXRefs;
        private Pointer baseAddress;

        /// <summary>
        /// Maintains a dictionary that maps the entry point address of a
        /// procedure (expressed in offset) to a Procedure object.
        /// </summary>
        private Dictionary<int, Procedure> procedures = new Dictionary<int, Procedure>();

        private List<Error> errors = new List<Error>();

        public Disassembler16(byte[] image, Pointer baseAddress)
        {
            this.image = image;
            this.baseAddress = baseAddress;
            this.attr = new ByteAttribute[image.Length];
            this.byteSegment = new ushort[image.Length]; // TBD
            this.byteToProcedure = new Procedure[image.Length]; // TBD
            this.globalXRefs = new List<XRef>();
        }

        /// <summary>
        /// Gets the executable image being disassembled.
        /// </summary>
        public byte[] Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the base address of the executable image. This address 
        /// always has zero offset.
        /// </summary>
        public Pointer BaseAddress
        {
            get { return baseAddress; }
        }

        /// <summary>
        /// Gets the attributes of each byte in the executable image.
        /// </summary>
        public ByteAttribute[] ByteAttributes
        {
            get { return attr; }
        }

        public UInt16[] ByteSegments
        {
            get { return byteSegment; }
        }

        /// <summary>
        /// Gets the entry points of analyzed procedures.
        /// </summary>
        public Procedure[] Procedures
        {
            get
            {
                Procedure[] procs = new Procedure[procedures.Count];
                procedures.Values.CopyTo(procs, 0);
                Array.Sort(procs, new ProcedureEntryPointComparer());
                return procs;
            }
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
            return location - baseAddress;
        }

        public Pointer OffsetToPointer(int offset)
        {
            if (offset < 0 || offset >= attr.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (!attr[offset].IsBoundary)
                return Pointer.Invalid;

            int baseSegment = this.baseAddress.Segment;
            UInt16 seg = byteSegment[offset];
            return new Pointer(seg, (UInt16)(offset - (seg - baseSegment) * 16));
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
                Type = XRefType.FunctionCall,
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

                    // If the target of this xref is dynamic, do nothing.
                    if (entry.Target == Pointer.Invalid)
                        continue;

                    // If this xref refers to a jump table entry, add it to
                    // the list of jump table entries to process later.
                    if (entry.Type == XRefType.NearJumpTableEntry)
                    {
                        jumpTables.Add(entry);
                        continue;
                    }

                    // If this xref is a function call, analyze the function
                    // if the entry point has not yet analyzed yet.
                    if (entry.Type == XRefType.FunctionCall)
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

                    // If this xref is NearJumpTableTarget, process the
                    // procedure again.
                    if (entry.Type == XRefType.NearJumpTableTarget)
                    {
                        // Find out which procedure entry.Source belongs to.
                        Procedure proc = byteToProcedure[PointerToOffset(entry.Source)];
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
                    entry.Type == XRefType.FunctionCall)
                    continue;
                if (entry.Type == XRefType.NearJumpTableEntry)
                {
                    continue;
                }

                // Process this code block assuming no jumps are taken and
                // function calls and interrupts all return.
                int count = AnalyzeCodeBlock(entry, localXrefs);
                if (count > 0)
                {
                    int baseOffset = PointerToOffset(entry.Target);
                    proc.CodeRange.AddInterval(baseOffset, baseOffset + count);
                    proc.ByteRange.AddInterval(baseOffset, baseOffset + count);
                    for (int j = 0; j < count; j++)
                    {
                        byteToProcedure[baseOffset + j] = proc;
                    }
                }
            }

            // Append the local XRef list to the global xref list.
            localXrefs.RemoveAt(0);
            xrefs.AddRange(localXrefs);
        }

        private void ProcessJumpTableEntry(XRef entry, ICollection<XRef> xrefs)
        {
            if (entry.Type != XRefType.NearJumpTableEntry)
                throw new ArgumentException("xref type mismatch.");

            // If the target refers to a jump table entry, we delay its
            // processing until we have processed all other types of
            // cross-references. This reduces the chance that we process
            // past the end of the jump table.
            //if (entry.Source.ToString() == "3668:6516")
            //{
            //    int kk = 1;
            //}

            // Process this one.
            int b = entry.Target - baseAddress;
            if (attr[b].Type != ByteType.Unknown ||
                attr[b + 1].Type != ByteType.Unknown)
                return;

            // Mark the memory location specified by the jump table
            // entry as data.
            attr[b].Type = ByteType.Data;
            attr[b].IsBoundary = true;
            attr[b + 1].Type = ByteType.Data;
            attr[b + 1].IsBoundary = false;
            byteSegment[b] = entry.Target.Segment;

            // Add this data item to its owning procedure's byte range.
            Procedure proc = byteToProcedure[PointerToOffset(entry.Source)];
            proc.DataRange.AddInterval(b, b + 2);
            proc.ByteRange.AddInterval(b, b + 2);
            byteToProcedure[b] = proc;
            byteToProcedure[b + 1] = proc;

            // Read the jump offset.
            ushort jumpOffset = BitConverter.ToUInt16(image, b);

            // Add a xref from the dynamic JMP instruction to the jump 
            // target.
            xrefs.Add(new XRef
            {
                Source = entry.Source,
                Target = new Pointer(entry.Source.Segment, jumpOffset),
                Type = XRefType.NearJumpTableTarget
            });

            // Add a xref from the JMP instruction to the next jump
            // table entry.
            xrefs.Add(new XRef
            {
                Source = entry.Source,
                Target = entry.Target + 2,
                Type = XRefType.NearJumpTableEntry
            });
        }

        /// <summary>
        /// Analyzes a continuous block of code until end-of-input, analyzed
        /// code/data, or any of the following instructions: RET, IRET, JMP,
        /// HLT. Conditional jumps and calls are stored but they do not
        /// terminate the block.
        /// </summary>
        /// <param name="start">Address of first instruction in the block.
        /// </param>
        /// <param name="jumps">Jump instructions are added to this list.</param>
        /// <param name="calls">Call instructions are added to this list.</param>
        /// <param name="dynamicJumps">Jump instructions with a dynamic target
        /// are added to this list.</param>
        /// <returns>The number of bytes successfully analyzed as code.</returns>
        private int AnalyzeCodeBlock(XRef start, ICollection<XRef> xrefs)
        {
            Pointer pos = start.Target;
            int count = 0;
            while (true)
            {
                Instruction insn;

                // Decode an instruction at this location.
                string errMsg;
                DecodeResult ret = TryDecodeInstruction(pos, out insn, out errMsg);
                switch (ret)
                {
                    case DecodeResult.AlreadyAnalyzed:
                        return count;
                    case DecodeResult.UnexpectedData:
                    case DecodeResult.UnexpectedCode:
                        errors.Add(new Error(pos, string.Format(
                            "Ran into {0} when processing block {1} referred from {2}",
                            ret, start.Target, start.Source)));
                        return count;
                    case DecodeResult.BadInstruction:
                        errors.Add(new Error(pos, "Bad instruction: " + errMsg));
                        return count;
                }
                count += insn.EncodedLength;

                // Check if this instruction terminates the block.
                switch (insn.Operation)
                {
                    case Operation.RET:
                    case Operation.RETF:
                    case Operation.HLT:
                        return count;
                }

                // Analyze BCJ (branch, jump, call) instructions. Such an
                // instruction will create a cross reference.
                XRef xref = AnalyzeFlowInstruction(insn);
                if (xref != null)
                {
                    xrefs.Add(xref);
                    switch (xref.Type)
                    {
                        case XRefType.FunctionCall:
                        case XRefType.ConditionalJump:
                            break;
                        case XRefType.UnconditionalJump:
                        case XRefType.NearJumpTableEntry:
                            return count;
                        default:
                            throw new ArgumentException("Unexpected.");
                    }
                }

                // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                // if pos.off + count > 0xFFFF. This is probably not intended but
                // technically allowed. So we allow for this for the moment.
                if (pos.Offset + insn.EncodedLength > 0xFFFF)
                    throw new InvalidOperationException("Instruction wrapped!");
                pos += insn.EncodedLength;
            }
        }

        /// <summary>
        /// Represents the result of trying to decode an instruction at a
        /// given location.
        /// </summary>
        enum DecodeResult
        {
            /// <summary>
            /// The instruction is successfully decoded.
            /// </summary>
            OK = 0,

            /// <summary>
            /// The byte is already analyzed and is marked as the first byte
            /// of an instruction.
            /// </summary>
            AlreadyAnalyzed,

            /// <summary>
            /// The bytes at the given range do not form a valid instruction.
            /// </summary>
            BadInstruction,

            /// <summary>
            /// The byte, or an instruction if decoded, runs into bytes
            /// previously analyzed and marked as data.
            /// </summary>
            UnexpectedData,

            /// <summary>
            /// The byte, or an instruction if decoded, runs into bytes
            /// previously analyzed and marked as code. However, the byte
            /// itself is not previously analyzed to be the start of an
            /// instruction.
            /// </summary>
            UnexpectedCode,
        }

        /// <summary>
        /// Try decode an instruction at the given location.
        /// </summary>
        /// <param name="start">The address to decode.</param>
        /// <param name="instruction">On return, stores the decoded
        /// instruction if successful, or null if failed.</param>
        /// <returns>One of the status codes.</returns>
        DecodeResult TryDecodeInstruction(Pointer start, out Instruction instruction, out string errMsg)
        {
            errMsg = null;
            instruction = null;
            int b = start - baseAddress;

            // TODO: we actually need to make sure that the full instruction
            // is within bound, not just the first byte.
            if (b >= image.Length)
                return DecodeResult.BadInstruction;

            // If the byte to analyze is already marked as data, return a
            // conflict status.
            if (attr[b].Type == ByteType.Data)
                return DecodeResult.UnexpectedData;

            // If the byte to analyze is already marked as code, check that
            // it was treated as the first byte of an instruction. Otherwise,
            // return a conflict status.
            if (attr[b].Type == ByteType.Code)
            {
                if (attr[b].IsBoundary)
                    return DecodeResult.AlreadyAnalyzed;
                else
                    return DecodeResult.UnexpectedCode;
            }

            // Try decode an instruction at this location.
            try
            {
                instruction = X86Codec.Decoder.Decode(image, b, start, CpuMode.RealAddressMode);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return DecodeResult.BadInstruction;
            }

            // Check that the entire instruction covers unprocessed bytes.
            // If any byte in the area is already processed, return an error.
            for (int j = 1; j < instruction.EncodedLength; j++)
            {
                if (attr[b + j].IsProcessed)
                {
                    return attr[b + j].Type == ByteType.Code ?
                        DecodeResult.UnexpectedCode : DecodeResult.UnexpectedData;
                }
            }

            // Mark the bytes covered by the instruction as code.
            for (int j = 0; j < instruction.EncodedLength; j++)
            {
                attr[b + j].Type = ByteType.Code;
                //d->attr[b + i] &= ~ATTR_BOUNDARY;
            }
            attr[b].IsBoundary = true;

            // Record the segment of the first byte.
            byteSegment[b] = start.Segment;

            return DecodeResult.OK;
        }

        /// <summary>
        /// Analyzes a branch/call/jump instruction and returns a xref.
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
                    bcjType = XRefType.ConditionalJump;
                    break;

                case Operation.JMP:
                case Operation.JMPF:
                    bcjType = XRefType.UnconditionalJump;
                    break;

                case Operation.CALL:
                case Operation.CALLF:
                    bcjType = XRefType.FunctionCall;
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
                        Target = new Pointer(start.Segment, (UInt16)opr.Displacement),
                        Type = XRefType.NearJumpTableEntry
                    };
                }
            }

            // Other jump/call targets that we cannot recognize.
            errors.Add(new Error(start, string.Format(
                "Cannot determine target of {0} instruction.",
                instruction.Operation.ToString().ToUpperInvariant())));

            return new XRef
            {
                Source = start,
                Target = Pointer.Invalid,
                Type = bcjType
            };
        }

        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
    }

    public class Error
    {
        public Pointer Location { get; set; }
        public string Message { get; set; }

        public Error(Pointer location, string message)
        {
            this.Location = location;
            this.Message = message;
        }
    }

    /// <summary>
    /// Defines the type of a byte in an executable image.
    /// </summary>
    public enum ByteType
    {
        /// <summary>
        /// The byte is not yet analyzed and its attribute is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The byte is scheduled for analysis.
        /// </summary>
        Pending,

        /// <summary>
        /// The byte is part of an instruction.
        /// </summary>
        Code = 2,

        /// <summary>
        /// The byte is part of a data item.
        /// </summary>
        Data = 3,
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

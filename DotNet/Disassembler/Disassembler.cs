using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Provides methods to disassemble and analyze x86 binary code.
    /// </summary>
    public class Disassembler
    {
        private byte[] image;
        private ByteAttributes[] attr;

        /// <summary>
        /// Maintains a queue of pending code entry points to analyze. At the
        /// beginning, there is only one entry point, which is the program
        /// entry point specified by the user. As we encounter branch 
        /// instructions (JMP, CALL, or Jcc) on the way, we push the target 
        /// addresses to the queue of entry points, so that they can be 
        /// analyzed later.
        /// </summary>
        private List<XRef> entryPoints;
        private FarPointer16 baseAddress;

        /// <summary>
        /// Maintains a dictionary that maps the entry point address of a
        /// procedure to a boolean value that indicates whether the procedure
        /// has been analyzed.
        /// </summary>
        private Dictionary<FarPointer16, bool> procedures;

        private List<Error> errors = new List<Error>();

        //  VECTOR(dasm_xref_t) entry_points; /* dasm_code_block_t code_blocks */
        /* however, it is not exactly a block; it is more like an entry point */
        //VECTOR(dasm_jump_table_t) jump_tables;

        public Disassembler(byte[] image, FarPointer16 baseAddress)
        {
            this.image = image;
            this.baseAddress = baseAddress;
            this.attr = new ByteAttributes[image.Length];
            this.entryPoints = new List<XRef>();
            this.procedures = new Dictionary<FarPointer16, bool>();
#if false
    VECTOR_CREATE(d->jump_tables, dasm_jump_table_t);
#endif
        }

        /// <summary>
        /// Gets the executable image being disassembled.
        /// </summary>
        public byte[] Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the attributes of each byte in the executable image.
        /// </summary>
        public ByteAttributes[] ByteAttributes
        {
            get { return attr; }
        }

        /// <summary>
        /// Gets the entry points of analyzed procedures.
        /// </summary>
        public FarPointer16[] Procedures
        {
            get
            {
                FarPointer16[] entries = new FarPointer16[procedures.Count];
                procedures.Keys.CopyTo(entries, 0);
                Array.Sort(entries);
                return entries;
            }
        }

        public IEnumerable<Error> Errors
        {
            get { return errors; }
        }

        /// <summary>
        /// Analyzes code starting from the given location. That location
        /// must be the entry point of a procedure, or otherwise the analysis
        /// may not work correctly.
        /// </summary>
        /// <param name="start">Entry point of a procedure.</param>
        /// <param name="recursive">Whether to analyze functions called by
        /// this procedure.</param>
        public void Analyze(FarPointer16 start, bool recursive)
        {
            // Create an entry point using the user-supplied starting address.
            List<FarPointer16> calls = new List<FarPointer16>();
            calls.Add(start);

            // Analyze each procedure in the list of procedures.
            for (int i = 0; i < calls.Count; i++)
            {
                FarPointer16 entry = calls[i];

                // If the procedure at this entry point has already been
                // analyzed, do nothing.
                if (procedures.ContainsKey(entry))
                    continue;

                // Analyze this procedure. Function calls made from this
                // procedure will be appended to the 'calls' list.
                AnalyzeProcedure(entry, calls);

                // Mark the procedure as processed.
                procedures[entry] = true;

                if (!recursive)
                    break;
            }

#if false
#if false
    fprintf(stderr, "\n-- Statistics after initial analysis --\n");
    dasm_stat(d);
#endif

            /* Analyze any jump tables encountered in the above analysis. Since we
     * may encounter more jump tables on the way, we do this recursively
     * until there are no more jump tables.
     */
            for (; i < VECTOR_SIZE(d->jump_tables); i++)
            {
                /* Analyze each entry in the jump table by assuming that it contains
                 * the address to a code block. Note that this is a rather
                 * opportunistic assumption -- it is fairly easy to construct a jump
                 * table that violates this logic. Nevertheless, for the moment we 
                 * will assume that the code is "well-formed".
                 */
                dasm_farptr_t insn_pos = VECTOR_AT(d->jump_tables, i).insn_pos;
                uint32_t table_offset = FARPTR_TO_OFFSET(VECTOR_AT(d->jump_tables, i).start);
                uint32_t entry_offset = table_offset;
                while (!(d->attr[entry_offset] & ATTR_PROCESSED) &&
                     !(d->attr[entry_offset + 1] & ATTR_PROCESSED))
                {
                    uint16_t target = (uint16_t)d->image[entry_offset] |
                        ((uint16_t)d->image[entry_offset + 1] << 8);

                    /* Mark this entry as data. */
                    d->attr[entry_offset] &= ~ATTR_TYPE;
                    d->attr[entry_offset] |= TYPE_DATA;
                    d->attr[entry_offset] |= ATTR_BOUNDARY;

                    d->attr[entry_offset + 1] &= ~ATTR_TYPE;
                    d->attr[entry_offset + 1] |= TYPE_DATA;
                    d->attr[entry_offset + 1] &= ~ATTR_BOUNDARY;

                    entry.target.seg = insn_pos.seg;
                    entry.target.off = target;
                    entry.source = insn_pos;
                    entry.type = XREF_INDIRECT_JUMP;

#if false
            fprintf(stderr, "Processing jump entry %d, target %04X:%04X\n",
                (entry_offset - table_offset) / 2, 
                entry.start.seg, entry.start.off);
#endif

                    analyze_code_block(d, entry);

                    /* Advance to the next jump entry. Each entry takes 2 bytes/ */
                    entry_offset += 2;
                }
            }

            /* Sort the XREFs built from the above analyses by target address. 
             * After this is done, the client can easily list the disassembled
             * instructions with xrefs sequentially in physical order.
             */
            VECTOR_QSORT(d->entry_points, compare_xrefs_by_target_and_source);
#endif
        }

        /// <summary>
        /// Analyzes a procedure starting from the given entry point.
        /// </summary>
        /// <param name="start">Address of procedure entry point.</param>
        /// <param name="calls">List of addresses called by this 
        /// procedure.</param>
        private void AnalyzeProcedure(FarPointer16 start, List<FarPointer16> calls)
        {
            // Create a dummy code block.
            XRef entry = new XRef
            {
                Target = start,
                Source = FarPointer16.Invalid,
                Type = XRefType.UserSpecified
            };
            entryPoints.Add(entry);

            // Process each code block in the queue until there are no more
            // left.
            for (int i = entryPoints.Count - 1; i < entryPoints.Count; i++)
            {
                // Legacy behavior: if the xref comes from a function call,
                // we push it to the queue but does not process it here.
                if (entryPoints[i].Type == XRefType.FunctionCall)
                {
                    calls.Add(entryPoints[i].Target);
                    continue;
                }

                // Decode the instruction at the given entry point.
                FarPointer16 pos = entryPoints[i].Target;
                //FarPointer16 from = entryPoints[i].Source;
#if false
        if (verbose)
        {
            printf("%04X:%04X  ; -- %s FROM %04X:%04X --\n", 
                pos.seg, pos.off, 
                dasm_xref_type_string(VECTOR_AT(d->entry_points, i).type),
                from.seg, from.off);
        }
#endif

                // Keep decoding instructions starting from this location
                // until we encounter end-of-input, analyzed code/data, or
                // any of the jump instructions: RET/IRET/JMP/HLT/CALL.
                while (true)
                {
                    Instruction insn;

                    // Decode an instruction at this location.
                    DecodeResult ret = TryDecodeInstruction(pos, out insn);
                    if (ret == DecodeResult.AlreadyAnalyzed)
                    {
                        //if (verbose)
                        //    printf("Already analyzed.\n");
                        break;
                    }
                    if (ret == DecodeResult.UnexpectedData)
                    {
                        //printf("Jump into data!\n");
                        errors.Add(new Error(pos, ret.ToString()));
                        break;
                    }
                    if (ret == DecodeResult.UnexpectedCode)
                    {
                        //fprintf(stderr, "%04X:%04X  %s\n", pos.seg, pos.off, 
                        //    "Jump into the middle of code!");
                        errors.Add(new Error(pos, ret.ToString()));
                        break;
                    }
                    if (ret == DecodeResult.BadInstruction)
                    {
                        //printf("Bad instruction!\n");
                        errors.Add(new Error(pos, ret.ToString()));
                        break;
                    }

#if false
            /* Debug only: display the instruction in assembly. */
            x86_format(&insn, text, X86_FMT_LOWER|X86_FMT_INTEL);
            if (verbose)
                printf("%04X:%04X  %s\n", pos.seg, pos.off, text);
#endif

                    // Analyze any flow-control instruction.
                    FlowResult ret2 = AnalyzeFlowInstruction(pos, insn);
                    if (ret2 == FlowResult.FinishBlock)
                    {
                        break;
                    }
                    if (ret2 == FlowResult.DynamicJump)
                    {
                        //fprintf(stderr, "%04X:%04X  %-32s ; Dynamic analysis required\n",
                        //    pos.seg, pos.off, text);
                        break;
                    }
                    if (ret2 == FlowResult.DynamicCall)
                    {
                        //fprintf(stderr, "%04X:%04X  %-32s ; Dynamic analysis required\n",
                        //    pos.seg, pos.off, text);
                        break;
                    }
                    if (ret2 == FlowResult.Failed)
                    {
                        DebugPrint("{0}  Flow analysis failed", pos);
                        break;
                    }

                    // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                    // if pos.off + count > 0xFFFF. This is probably not intended but
                    // technically allowed. So we allow for this for the moment.
                    pos += insn.EncodedLength;
                }

                //if (verbose)
                //  printf("\n");
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
        DecodeResult TryDecodeInstruction(FarPointer16 start, out Instruction instruction)
        {
            instruction = null;
            int b = start - baseAddress;

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
            instruction = X86Codec.Decoder.Decode(image, b, CpuMode.RealAddressMode);
            if (instruction == null)
                return DecodeResult.BadInstruction;

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

            return DecodeResult.OK;
        }

        enum FlowResult
        {
            Wrapped = -2,
            Failed = -1,
            Continue = 0,
            FinishBlock = 1,
            DynamicJump = 2,
            DynamicCall = 3,
        }

        /* Analyze an instruction decoded from offset _start_ for _count_ bytes.
         * TBD: address wrapping if IP is above 0xFFFF is not handled. It should be.
         */
        private FlowResult AnalyzeFlowInstruction(FarPointer16 start, Instruction insn)
        {
            Operation op = insn.Operation;

            // If this is an unconditional JMP instruction, push the jump
            // target to the queue and finish this block.
            if (op == Operation.JMP || op == Operation.JMPN)
            {
                if (insn.Operands[0] is RelativeOperand) // near jump to relative address
                {
                    RelativeOperand opr = (RelativeOperand)insn.Operands[0];
                    XRef xref = new XRef
                    {
                        Source = start,
                        Target = start + insn.EncodedLength + opr.Offset,
                        Type = XRefType.UnconditionalJump,
                    };
                    entryPoints.Add(xref);
                    return FlowResult.FinishBlock;
                }
                if (insn.Operands[0] is PointerOperand) // far jump to absolute address
                {
                    PointerOperand opr = (PointerOperand)insn.Operands[0];
                    XRef xref = new XRef
                    {
                        Source = start,
                        Target = new FarPointer16(opr.Segment, (UInt16)opr.Offset),
                        Type = XRefType.UnconditionalJump
                    };
                    entryPoints.Add(xref);
                    return FlowResult.FinishBlock;
                }

#if false
                /* Process near jump table. We recognize a jump table heuristically
                 * if the instruction is of the form:
                 *
                 *   jmpn word ptr cs:[bx+3782h] 
                 *
                 * where bx may be replaced by another register and 3782h must be
                 * the address immediately following this instruction. Also, the CS
                 * prefix is mandatory.
                 *
                 * Note that an ill-formed executable may create a jump table not
                 * conforming to the above rules, or create a non-jump table that
                 * conforms to the above rules. We are not ready to deal with that
                 * for now.
                 */
                if (insn->oprs[0].type == OPR_MEM &&
                    insn->oprs[0].size == OPR_16BIT &&
                    insn->oprs[0].val.mem.segment == R_CS &&
                    insn->oprs[0].val.mem.base != R_NONE &&
                    insn->oprs[0].val.mem.index == R_NONE &&
                    insn->oprs[0].val.mem.displacement == start.off + count)
                {
                    dasm_jump_table_t table;
                    table.insn_pos = start;
                    table.start = increment_farptr(start, count);
                    table.current = table.start;
                    VECTOR_PUSH(d->jump_tables, table);
                    return FLOW_FINISH_BLOCK;
                }
                return FLOW_DYNAMIC_JUMP;
#endif

                // Other jump targets we have no idea about.
                return FlowResult.DynamicJump;

            }

            // If this is a RET instruction, finish the current block.
            if (op == Operation.RETN || op == Operation.RETF)
            {
                return FlowResult.FinishBlock;
            }

            // If this is a CALL instruction, push the call target to the
            // queue and finish this block.
            //
            // Note: We need to know whether the subroutine being called
            // will ever return. For the moment we assume that it will.
            if (op == Operation.CALL || op == Operation.CALLF)
            {
                if (insn.Operands[0] is RelativeOperand) // near relative call
                {
                    RelativeOperand opr = (RelativeOperand)insn.Operands[0];
                    XRef xref = new XRef
                    {
                        Source = start,
                        Target = start + insn.EncodedLength + opr.Offset,
                        Type = XRefType.FunctionCall
                    };
                    entryPoints.Add(xref);
                    return FlowResult.Continue;
                }
                if (insn.Operands[0] is PointerOperand) // far absolute call
                {
                    PointerOperand opr = (PointerOperand)insn.Operands[0];
                    XRef xref = new XRef
                    {
                        Source = start,
                        Target = new FarPointer16(opr.Segment, (UInt16)opr.Offset),
                        Type = XRefType.FunctionCall
                    };
                    entryPoints.Add(xref);
                    return FlowResult.Continue;
                }
                return FlowResult.DynamicCall;
            }

            // If this is a Jcc/JCXZ instruction, push the jump target to the
            // queue, and follow the flow assuming no jump.
            //
            // Note: We assume that "no jump" is a reachable branch. If the
            // code is ill-formed such that the "no jump" branch will never
            // be executed, the analysis may not work correctly.
            switch (op)
            {
                case Operation.JO:
                case Operation.JNO:
                case Operation.JB:
                case Operation.JNB:
                case Operation.JE:
                case Operation.JNE:
                case Operation.JBE:
                case Operation.JNBE:
                case Operation.JS:
                case Operation.JNS:
                case Operation.JP:
                case Operation.JNP:
                case Operation.JL:
                case Operation.JNL:
                case Operation.JLE:
                case Operation.JNLE:
                case Operation.JCXZ:
                    if (insn.Operands[0] is RelativeOperand) // jump to relative position
                    {
                        RelativeOperand opr = (RelativeOperand)insn.Operands[0];
                        XRef xref = new XRef
                        {
                            Source = start,
                            Target = start + insn.EncodedLength + opr.Offset,
                            Type = XRefType.ConditionalJump,
                        };
                        entryPoints.Add(xref);
                        return FlowResult.Continue;
                    }

                    // A valid Jcc instruction must jump to relative address.
                    // If not, the instruction is malformed.
                    return FlowResult.Failed;
            }

            // This is not a flow-control instruction, so continue as usual.
            return FlowResult.Continue;
        }

        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
    }

    public class Error
    {
        public FarPointer16 Location;
        public string Message;

        public Error(FarPointer16 location, string message)
        {
            this.Location = location;
            this.Message = message;
        }
    }

    /* Represents a cross-referential link in the code and data. For a xref 
 * between code and code, it is equivalent to an edge in a Control Flow Graph.
 */
    struct XRef
    {
        /// <summary>
        /// Gets or sets the target address being referenced.
        /// </summary>
        public FarPointer16 Target { get; set; }

        /// <summary>
        /// Gets or sets the source address that refers to target.
        /// </summary>
        public FarPointer16 Source { get; set; }

        /// <summary>
        /// Gets or sets the type of cross-reference.
        /// </summary>
        public XRefType Type { get; set; }
    }

    /// <summary>
    /// Defines types of cross-references.
    /// </summary>
    enum XRefType
    {
        /// <summary>
        /// user specified entry point (e.g. program start)
        /// </summary>
        UserSpecified,

        /// <summary>
        /// A CALL instruction refers to this location.
        /// </summary>
        FunctionCall,

        /// <summary>
        /// A Jcc instruction refers to this location.
        /// </summary>
        ConditionalJump,

        /// <summary>
        /// A JUMP instruction refers to this location.
        /// </summary>
        UnconditionalJump,

        /// <summary>
        /// A JUMP instruction where the jump target address is given by
        /// a memory location (such as jump table).
        /// </summary>
        IndirectJump,

#if false
    XREF_RETURN_FROM_CALL      = 5,    
    XREF_RETURN_FROM_INTERRUPT = 6,    
#endif

#if false
    ENTRY_NEAR_JUMP_TABLE       = 7,    /* the word at this location appears to represent
                                         * a relative offset to a JUMP NEAR instruction.
                                         * The instruction is stored in _from_.
                                         */
    ENTRY_FAR_JUMP_TABLE        = 8,    /* the dword at this location appears to represent
                                         * an absolute address (seg:ptr) of a JUMP FAR
                                         * instruction.
                                         */
#endif
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

    public struct ByteAttributes
    {
        byte x;

        const byte TypeMask = 3;
        const byte BoundaryBit = 4;
        const byte BlockStartBit = 8;

        /// <summary>
        /// Tests whether the byte has been analyzed.
        /// </summary>
        public bool IsProcessed
        {
            get { return Type == ByteType.Code || Type == ByteType.Data; }
        }

        /// <summary>
        /// Gets or sets the type of the byte.
        /// </summary>
        public ByteType Type
        {
            get { return (ByteType)(x & TypeMask); }
            set
            {
                x = (byte)(x & ~TypeMask | (byte)value);
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether the byte is the first
        /// byte of an instruction or a data item.
        /// </summary>
        public bool IsBoundary
        {
            get { return (x & BoundaryBit) != 0; }
            set
            {
                if (value)
                    x |= BoundaryBit;
                else
                    x = (byte)(x & ~BoundaryBit);
            }
        }

        /// <summary>
        /// Gets or sets a flag that indicates whether the byte is the first
        /// byte of an instruction that starts a basic block.
        /// </summary>
        public bool IsBlockStart
        {
            get { return (x & BlockStartBit) != 0; }
            set
            {
                if (value)
                    x |= BlockStartBit;
                else
                    x = (byte)(x & ~BlockStartBit);
            }
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

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
        private ByteAttributes[] attr;
        private UInt16[] byteSegment;

        /// <summary>
        /// Maintains a queue of pending code entry points to analyze. At the
        /// beginning, there is only one entry point, which is the program
        /// entry point specified by the user. As we encounter branch 
        /// instructions (JMP, CALL, or Jcc) on the way, we push the target 
        /// addresses to the queue of entry points, so that they can be 
        /// analyzed later.
        /// </summary>
        private List<XRef> entryPoints;
        private Pointer baseAddress;

        /// <summary>
        /// Maintains a dictionary that maps the entry point address of a
        /// procedure to a boolean value that indicates whether the procedure
        /// has been analyzed.
        /// </summary>
        private Dictionary<Pointer, bool> procedures;

        private List<Error> errors = new List<Error>();

        //  VECTOR(dasm_xref_t) entry_points; /* dasm_code_block_t code_blocks */
        /* however, it is not exactly a block; it is more like an entry point */
        //VECTOR(dasm_jump_table_t) jump_tables;

        public Disassembler16(byte[] image, Pointer baseAddress)
        {
            this.image = image;
            this.baseAddress = baseAddress;
            this.attr = new ByteAttributes[image.Length];
            this.byteSegment = new ushort[image.Length]; // TBD
            this.entryPoints = new List<XRef>();
            this.procedures = new Dictionary<Pointer, bool>();
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
        public Pointer[] Procedures
        {
            get
            {
                Pointer[] entries = new Pointer[procedures.Count];
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
        public void Analyze(Pointer start, bool recursive)
        {
            // Create an entry point using the user-supplied starting address.
            List<Pointer> calls = new List<Pointer>();
            calls.Add(start);

            // Analyze each procedure in the list of procedures.
            for (int i = 0; i < calls.Count; i++)
            {
                Pointer entry = calls[i];

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
        private void AnalyzeProcedure(Pointer start, List<Pointer> calls)
        {
            // Create a dummy code block.
            XRef entry = new XRef
            {
                Target = start,
                Source = Pointer.Invalid,
                Type = XRefType.UserSpecified
            };
            entryPoints.Add(entry);

            // Process each code block in the queue until there are no more
            // left.
            for (int i = entryPoints.Count - 1; i < entryPoints.Count; i++)
            {
                XRef xref = entryPoints[i];
                Pointer target = xref.Target;

                // If we encounter a xref whose target we don't know, e.g.
                // in an instruction 
                //   jmp     word ptr [data_573] 
                // Skip this xref.
                if (target == Pointer.Invalid)
                {
                    Instruction insn = X86Codec.Decoder.Decode(image, xref.Source-baseAddress, xref.Source, CpuMode.RealAddressMode);
                    errors.Add(new Error(xref.Source, string.Format(
                        "Cannot determine target of {0} instruction.",
                        insn.Operation.ToString().ToUpperInvariant())));
                    continue;
                }

                //FarPointer16 from = entryPoints[i].Source;

                // Legacy behavior: if the xref comes from a function call,
                // we push it to the queue but does not process it here.
                if (xref.Type == XRefType.FunctionCall)
                {
                    calls.Add(target);
                    continue;
                }

                // If the target refers to a jump table entry, we delay its
                // processing until we have processed all other types of
                // cross-references. This reduces the chance that we process
                // past the end of the jump table.
                if (xref.Type == XRefType.NearJumpTableEntry)
                {
                    int iNonJumpTable = i + 1;
                    for (; iNonJumpTable < entryPoints.Count; iNonJumpTable++)
                    {
                        if (entryPoints[iNonJumpTable].Type != XRefType.NearJumpTableEntry)
                            break;
                    }
                    if (iNonJumpTable < entryPoints.Count)
                    {
                        XRef t = entryPoints[iNonJumpTable];
                        entryPoints[iNonJumpTable] = xref;
                        entryPoints[i] = t;
                        i--;
                        continue;
                    }

                    // Process this one.
                    int b = target - baseAddress;
                    if (!(attr[b].Type == ByteType.Unknown &&
                        attr[b + 1].Type == ByteType.Unknown))
                        continue;

                    // Mark the jump table entry as data.
                    attr[b].Type = ByteType.Data;
                    attr[b].IsBoundary = true;
                    attr[b + 1].Type = ByteType.Data;
                    attr[b + 1].IsBoundary = false;

                    // Read the jump offset.
                    ushort jumpOffset = BitConverter.ToUInt16(image, b);

                    // Add a xref from the jump table entry to the jump 
                    // target.
                    entryPoints.Add(new XRef
                    {
                        Source = target,
                        Target = new Pointer(target.Segment, jumpOffset),
                        Type = XRefType.NearJumpTableTarget
                    });

                    // Add a xref from the JMP instruction to the next jump
                    // table entry.
                    entryPoints.Add(new XRef
                    {
                        Source = xref.Source,
                        Target = xref.Target + 2,
                        Type = XRefType.NearJumpTableEntry
                    });
                    continue;
                }

                // Analyze this code block.
                AnalyzeCodeBlock(target, entryPoints);
            }
        }

        /// <summary>
        /// Analyzes a continuous block of code until end-of-input, analyzed
        /// code/data, or any of the following instructions: RET, IRET, JMP,
        /// HLT. Conditional jumps and calls are recorded but they do not
        /// terminate the block.
        /// </summary>
        private void AnalyzeCodeBlock(Pointer pos, List<XRef> xrefs)
        {
            while (true)
            {
                Instruction insn;

                // Decode an instruction at this location.
                DecodeResult ret = TryDecodeInstruction(pos, out insn);
                if (ret == DecodeResult.AlreadyAnalyzed)
                {
                    break;
                }
                if (ret == DecodeResult.UnexpectedData)
                {
                    errors.Add(new Error(pos, ret.ToString()));
                    break;
                }
                if (ret == DecodeResult.UnexpectedCode)
                {
                    errors.Add(new Error(pos, ret.ToString()));
                    break;
                }
                if (ret == DecodeResult.BadInstruction)
                {
                    errors.Add(new Error(pos, ret.ToString()));
                    break;
                }

                // Check if this instruction terminates the block.
                switch (insn.Operation)
                {
                    case Operation.RETN:
                    case Operation.RETF:
                    case Operation.RET:
                    case Operation.HLT:
                        return;
                }

                // Analyze BCJ (branch, jump, call) instructions. Such an
                // instruction will create a cross reference.
                XRef xref = AnalyzeFlowInstruction(pos, insn);
                if (xref != null)
                {
                    xrefs.Add(xref);
                    switch (xref.Type)
                    {
                        case XRefType.FunctionCall:
                            break;
                        case XRefType.ConditionalJump:
                            break;
                        case XRefType.UnconditionalJump:
                            return;
                        case XRefType.NearJumpTableEntry:
                            return;
                        default:
                            throw new ArgumentException("Unexpected.");
                    }
                }

                // Advance the byte pointer. Note: the IP may wrap around 0xFFFF 
                // if pos.off + count > 0xFFFF. This is probably not intended but
                // technically allowed. So we allow for this for the moment.
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
        DecodeResult TryDecodeInstruction(Pointer start, out Instruction instruction)
        {
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
            instruction = X86Codec.Decoder.Decode(image, b,start, CpuMode.RealAddressMode);
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
        private XRef AnalyzeFlowInstruction(Pointer start, Instruction insn)
        {
            Operation op = insn.Operation;

            // If this is an unconditional JMP instruction, return a xref
            // from this instruction to the jump target.
            if (op == Operation.JMP || op == Operation.JMPN)
            {
                if (insn.Operands[0] is RelativeOperand) // near jump to relative address
                {
                    RelativeOperand opr = (RelativeOperand)insn.Operands[0];
                    return new XRef
                    {
                        Source = start,
                        Target = start + insn.EncodedLength + opr.Offset,
                        Type = XRefType.UnconditionalJump,
                    };
                }

                if (insn.Operands[0] is PointerOperand) // far jump to absolute address
                {
                    PointerOperand opr = (PointerOperand)insn.Operands[0];
                    return new XRef
                    {
                        Source = start,
                        Target = new Pointer(opr.Segment, (UInt16)opr.Offset),
                        Type = XRefType.UnconditionalJump
                    };
                }

#if true
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
                if (insn.Operands[0] is MemoryOperand)
                {
                    MemoryOperand opr = (MemoryOperand)insn.Operands[0];
                    if (op == Operation.JMPN &&
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
#endif

                // Other jump targets we have no idea about.
                return new XRef
                {
                    Source = start,
                    Target = Pointer.Invalid,
                    Type = XRefType.UnconditionalJump
                };
            }

            // If this is a CALL instruction, return a cross reference from
            // this instruction to the entry point of the procedure being 
            // called.
            //
            // Note: We need to know whether the subroutine being called
            // will ever return. For the moment we assume that it will.
            if (op == Operation.CALL || op == Operation.CALLF)
            {
                if (insn.Operands[0] is RelativeOperand) // near relative call
                {
                    RelativeOperand opr = (RelativeOperand)insn.Operands[0];
                    return new XRef
                    {
                        Source = start,
                        Target = start + insn.EncodedLength + opr.Offset,
                        Type = XRefType.FunctionCall
                    };
                    //return FlowResult.Continue;
                }
                else if (insn.Operands[0] is PointerOperand) // far absolute call
                {
                    PointerOperand opr = (PointerOperand)insn.Operands[0];
                    return new XRef
                    {
                        Source = start,
                        Target = new Pointer(opr.Segment, (UInt16)opr.Offset),
                        Type = XRefType.FunctionCall
                    };
                    // return FlowResult.Continue;
                }
                else // Unknown CALL target.
                {
                    return new XRef
                    {
                        Source = start,
                        Target = Pointer.Invalid,
                        Type = XRefType.FunctionCall
                    };
                }
            }

            // If this is a Jcc/JCXZ instruction, return a cross-reference
            // from this instruction to the jump target.
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
                        return new XRef
                        {
                            Source = start,
                            Target = start + insn.EncodedLength + opr.Offset,
                            Type = XRefType.ConditionalJump
                        };
                        //return FlowResult.Continue;
                    }
                    else // unknown Jcc target
                    {
                        return new XRef
                        {
                            Source = start,
                            Target = Pointer.Invalid,
                            Type = XRefType.ConditionalJump
                        };
                    }
            }

            // This is not a BCJ instruction, so no xref will be returned.
            return null;
        }

        private static void DebugPrint(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, args));
        }
    }

    public class Error
    {
        public Pointer Location;
        public string Message;

        public Error(Pointer location, string message)
        {
            this.Location = location;
            this.Message = message;
        }
    }

    /* Represents a cross-referential link in the code and data. For a xref 
 * between code and code, it is equivalent to an edge in a Control Flow Graph.
 */
    class XRef
    {
        /// <summary>
        /// Gets or sets the target address being referenced.
        /// </summary>
        public Pointer Target { get; set; }

        /// <summary>
        /// Gets or sets the source address that refers to target.
        /// </summary>
        public Pointer Source { get; set; }

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
        /// a memory location (such as jump table). ??????
        /// </summary>
        //IndirectJump,

#if false
    XREF_RETURN_FROM_CALL      = 5,    
    XREF_RETURN_FROM_INTERRUPT = 6,    
#endif

        /// <summary>
        /// The word at the Target location appears to contain the relative
        /// offset to a JMPN instruction that refers to a jump-table. The
        /// location of the JMPN instruction is stored in Source.
        /// </summary>
        NearJumpTableEntry,

        NearJumpTableTarget,

#if false
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

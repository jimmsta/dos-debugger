using System;
using System.Text;

namespace X86Codec
{
    public class Decoder
    {
        /// <summary>
        /// Decodes an instruction.
        /// </summary>
        /// <param name="code">Code buffer.</param>
        /// <param name="startIndex">Location of first byte of instruction.
        /// </param>
        /// <param name="location">CS:IP of the instruction to decode.</param>
        /// <param name="cpuMode">CPU operating mode.</param>
        /// <returns>The decoded instruction.</returns>
        /// <exception cref="InvalidInstructionException">If decoding fails.
        /// </exception>
        public static Instruction Decode(
            byte[] code, 
            int startIndex, 
            Pointer location,
            CpuMode cpuMode)
        {
            if (cpuMode != CpuMode.RealAddressMode)
                throw new NotSupportedException();

            DecoderContext context = new DecoderContext();
            context.AddressSize = CpuSize.Use16Bit;
            context.OperandSize = CpuSize.Use16Bit;

            X86Codec.Decoder decoder = new X86Codec.Decoder();
            return decoder.Decode(code, startIndex, location, context);
        }

        /// <summary>
        /// Decodes an instruction.
        /// </summary>
        /// <param name="code">X86 binary code.</param>
        /// <param name="startIndex">Index of the first byte to start
        /// decoding.</param>
        /// <param name="context">Decoding context.</param>
        /// <returns>The decoded instruction.</returns>
        /// <exception cref="InvalidInstructionException">If decoding failed.
        /// </exception>
        public Instruction Decode(
            byte[] code, 
            int startIndex, 
            Pointer location, 
            DecoderContext context)
        {
            Instruction instruction = new Instruction();

            // Create an instruction reader.
            InstructionReader reader = new InstructionReader(code, startIndex, 100);

            // Reset the context.
            context.AddressSize = CpuSize.Use16Bit;
            context.OperandSize = CpuSize.Use16Bit;
            context.SegmentOverride = Register.None;

            // Decode prefixes.
            Prefixes prefix = DecodeLegacyPrefixes(reader);
            if ((prefix & Prefixes.OperandSizeOverride) != 0)
            {
                context.OperandSize = CpuSize.Use32Bit;
            }
            if ((prefix & Prefixes.AddressSizeOverride) != 0)
            {
                context.AddressSize = CpuSize.Use32Bit;
            }
            if ((prefix & Prefixes.Group2) != 0)
            {
                Register seg = Register.None;
                switch (prefix & Prefixes.Group2)
                {
                    case Prefixes.ES: seg = Register.ES; break;
                    case Prefixes.CS: seg = Register.CS; break;
                    case Prefixes.SS: seg = Register.SS; break;
                    case Prefixes.DS: seg = Register.DS; break;
                    case Prefixes.FS: seg = Register.FS; break;
                    case Prefixes.GS: seg = Register.GS; break;
                }
                context.SegmentOverride = seg;
            }
            instruction.Prefix = prefix;

            // Decode the opcode to retrieve opcode specification.
            Op spec = DecodeOpcode(reader);
            instruction.Operation = spec.Operation;

            // Decode operands.
            instruction.Operands = new Operand[spec.Operands.Length];
            for (int i = 0; i < spec.Operands.Length; i++)
            {
                instruction.Operands[i] =
                    DecodeOperand(spec.Operands[i], reader, context);
            }

            // Update the encoded length of the instruction.
            instruction.EncodedLength = reader.Position;
            instruction.Location = location;
            return instruction;
        }

        /// <summary>
        /// Decodes legacy prefixes of an instruction. There are four groups
        /// of legacy prefixes; at most one prefix from each group may be
        /// present. The encoded prefixes may take zero to four bytes.
        /// </summary>
        /// <exception cref="InvalidInstructionException">
        /// More than one prefix from the same group is present.
        /// </exception>
        /// <returns>The legacy prefixes, which may be None if no prefix is
        /// present.</returns>
        private Prefixes DecodeLegacyPrefixes(InstructionReader reader)
        {
            Prefixes prefix = Prefixes.None;
            while (true)
            {
                Prefixes pfx = Prefixes.None;
                Prefixes grp = Prefixes.None;
                byte c = reader.PeekByte();
                switch (c)
                {
                    case 0xF0: pfx = Prefixes.LOCK; grp = Prefixes.Group1; break;
                    case 0xF2: pfx = Prefixes.REPNZ; grp = Prefixes.Group1; break;
                    case 0xF3: pfx = Prefixes.REP; grp = Prefixes.Group1; break;

                    case 0x2E: pfx = Prefixes.CS; grp = Prefixes.Group2; break;
                    case 0x36: pfx = Prefixes.SS; grp = Prefixes.Group2; break;
                    case 0x3E: pfx = Prefixes.DS; grp = Prefixes.Group2; break;
                    case 0x26: pfx = Prefixes.ES; grp = Prefixes.Group2; break;
                    case 0x64: pfx = Prefixes.FS; grp = Prefixes.Group2; break;
                    case 0x65: pfx = Prefixes.GS; grp = Prefixes.Group2; break;

                    case 0x66:
                        pfx = Prefixes.OperandSizeOverride;
                        grp = Prefixes.Group3;
                        break;

                    case 0x67:
                        pfx = Prefixes.AddressSizeOverride;
                        grp = Prefixes.Group4;
                        break;
                }
                if (pfx == Prefixes.None)
                {
                    break;
                }
                if ((prefix & grp) != Prefixes.None)
                {
                    throw new InvalidInstructionException(string.Format(
                        "The instruction contains multiple prefixes from {0}: {1} and {2}.",
                        grp, prefix & grp, pfx));
                }

                // Consume 1 byte.
                reader.ConsumePrefixByte();
                prefix |= pfx;
            }
            return prefix;
        }

        private Op DecodeOpcode(InstructionReader reader)
        {
            // Process the first byte of the opcode.
            byte c = reader.PeekByte();
            reader.ConsumeOpcodeByte();

            // Find an opcode entry for this opcode, and return it if it's
            // well-defined.
            Op spec = OneByteOpcodeMap[c];

            // Check opcode extensions.
            if (spec.IsExtension)
            {
                int reg = reader.GetModRM().REG;
                int mod = reader.GetModRM().MOD;
                int rm = reader.GetModRM().RM;
                Op x = new Op();
                switch (spec.Extension)
                {
                    case OpcodeExtension.Ext1:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.ADD); break;
                            case 1: x = new Op(Operation.OR); break;
                            case 2: x = new Op(Operation.ADC); break;
                            case 3: x = new Op(Operation.SBB); break;
                            case 4: x = new Op(Operation.AND); break;
                            case 5: x = new Op(Operation.SUB); break;
                            case 6: x = new Op(Operation.XOR); break;
                            case 7: x = new Op(Operation.CMP); break;
                        }
                        break;

                    case OpcodeExtension.Ext1A:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.POP, O.Ev); break;
                        }
                        break;

                    case OpcodeExtension.Ext2:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.ROL); break;
                            case 1: x = new Op(Operation.ROR); break;
                            case 2: x = new Op(Operation.RCL); break;
                            case 3: x = new Op(Operation.RCR); break;
                            case 4: x = new Op(Operation.SHL); break;
                            case 5: x = new Op(Operation.SHR); break;
                            case 6: break;
                            case 7: x = new Op(Operation.SAR); break;
                        }
                        break;

                    case OpcodeExtension.Ext3:
                        if (c == 0xF6)
                        {
                            switch (reg)
                            {
                                case 0: x = new Op(Operation.TEST, O.Eb, O.Ib); break;
                                case 1: break;
                                case 2: x = new Op(Operation.NOT, O.Eb); break;
                                case 3: x = new Op(Operation.NEG, O.Eb); break;
                                case 4: x = new Op(Operation.MUL, O.Eb, O.AL); break;
                                case 5: x = new Op(Operation.IMUL, O.Eb, O.AL); break;
                                case 6: x = new Op(Operation.DIV, O.Eb, O.AL); break;
                                case 7: x = new Op(Operation.IDIV, O.Eb, O.AL); break;
                            }
                        }
                        else if (c == 0xF7)
                        {
                            switch (reg)
                            {
                                case 0: x = new Op(Operation.TEST, O.Ev, O.Iz); break;
                                case 1: break;
                                case 2: x = new Op(Operation.NOT, O.Ev); break;
                                case 3: x = new Op(Operation.NEG, O.Ev); break;
                                case 4: x = new Op(Operation.MUL, O.Ev, O.rAX); break;
                                case 5: x = new Op(Operation.IMUL, O.Ev, O.rAX); break;
                                case 6: x = new Op(Operation.DIV, O.Ev, O.rAX); break;
                                case 7: x = new Op(Operation.IDIV, O.Ev, O.rAX); break;
                            }
                        }
                        break;

                    case OpcodeExtension.Ext4:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.INC, O.Eb); break;
                            case 1: x = new Op(Operation.DEC, O.Eb); break;
                        }
                        break;

                    case OpcodeExtension.Ext5:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.INC, O.Ev); break;
                            case 1: x = new Op(Operation.DEC, O.Ev); break;
                            case 2: x = new Op(Operation.CALL, O.Ev); break;
                            case 3: x = new Op(Operation.CALLF, O.Ep); break;
                            case 4: x = new Op(Operation.JMP, O.Ev); break;
                            case 5: x = new Op(Operation.JMPF, O.Mp); break;
                            case 6: x = new Op(Operation.PUSH, O.Ev); break;
                            case 7: break;
                        }
                        break;

                    case OpcodeExtension.Ext6:
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.SLDT, O.Rv, O.Mw); break;
                            case 1: x = new Op(Operation.STR, O.Rv, O.Mw); break;
                            case 2: x = new Op(Operation.LLDT, O.Ew); break;
                            case 3: x = new Op(Operation.LTR, O.Ew); break;
                            case 4: x = new Op(Operation.VERR, O.Ew); break;
                            case 5: x = new Op(Operation.VERW, O.Ew); break;
                            case 6: break;
                            case 7: break;
                        }
                        break;

                    case OpcodeExtension.Ext11:
                        if (c == 0xC6)
                        {
                            switch (reg)
                            {
                                case 0: x = new Op(Operation.MOV, O.Eb, O.Ib); break;
                                case 7:
                                    if (mod == 3 && rm == 0)
                                        x = new Op(Operation.XABORT, O.Ib);
                                    break;
                            }
                        }
                        else if (c == 0xC7)
                        {
                            switch (reg)
                            {
                                case 0: x = new Op(Operation.MOV, O.Ev, O.Iz); break;
                                case 7:
                                    if (mod == 3 && rm == 0)
                                        x = new Op(Operation.XBEGIN, O.Jz);
                                    break;
                            }
                        }
                        break;

                    case OpcodeExtension.Fpu:
                        return DecodeFpuOpcode(reader, c);

                }
                spec = x.MergeOperandsFrom(spec);
            }

            if (spec.Operation == Operation.None)
                throw new InvalidInstructionException("Invalid opcode.");
            return spec;
        }

        private Op DecodeFpuOpcode(InstructionReader reader, byte firstByte)
        {
            ModRM modrm = reader.GetModRM();
            int mod = modrm.MOD;
            int reg = modrm.REG;
            int rm = modrm.RM;
            Op x = Op.Empty;

            switch (firstByte)
            {
                case 0xD8:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FADD, O.ST0, O.T80fp); break;
                            case 1: x = new Op(Operation.FMUL, O.ST0, O.T80fp); break;
                            case 2: x = new Op(Operation.FCOM, O.ST0, O.T80fp); break;
                            case 3: x = new Op(Operation.FCOMP, O.ST0, O.T80fp); break;
                            case 4: x = new Op(Operation.FSUB, O.ST0, O.T80fp); break;
                            case 5: x = new Op(Operation.FSUBR, O.ST0, O.T80fp); break;
                            case 6: x = new Op(Operation.FDIV, O.ST0, O.T80fp); break;
                            case 7: x = new Op(Operation.FDIVR, O.ST0, O.T80fp); break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FADD, O.ST0, O.M32fp); break;
                            case 1: x = new Op(Operation.FMUL, O.ST0, O.M32fp); break;
                            case 2: x = new Op(Operation.FCOM, O.ST0, O.M32fp); break;
                            case 3: x = new Op(Operation.FCOMP, O.ST0, O.M32fp); break;
                            case 4: x = new Op(Operation.FSUB, O.ST0, O.M32fp); break;
                            case 5: x = new Op(Operation.FSUBR, O.ST0, O.M32fp); break;
                            case 6: x = new Op(Operation.FDIV, O.ST0, O.M32fp); break;
                            case 7: x = new Op(Operation.FDIVR, O.ST0, O.M32fp); break;
                        }
                    }
                    break;

                case 0xD9:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FLD, O.ST0, O.T80fp); break;
                            case 1: x = new Op(Operation.FXCH, O.ST0, O.T80fp); break;
                            case 2:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.FNOP); break;
                                }
                                break;
                            case 3: break;
                            case 4:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.FCHS); break;
                                    case 1: x = new Op(Operation.FABS); break;
                                    case 4: x = new Op(Operation.FTST); break;
                                    case 5: x = new Op(Operation.FXAM); break;
                                }
                                break;
                            case 5:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.FLD1); break;
                                    case 1: x = new Op(Operation.FLDL2T); break;
                                    case 2: x = new Op(Operation.FLDL2E); break;
                                    case 3: x = new Op(Operation.FLDPI); break;
                                    case 4: x = new Op(Operation.FLDLG2); break;
                                    case 5: x = new Op(Operation.FLDLN2); break;
                                    case 6: x = new Op(Operation.FLDZ); break;
                                }
                                break;
                            case 6:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.F2XM1); break;
                                    case 1: x = new Op(Operation.FYL2X); break;
                                    case 2: x = new Op(Operation.FPTAN); break;
                                    case 3: x = new Op(Operation.FPATAN); break;
                                    case 4: x = new Op(Operation.FXTRACT); break;
                                    case 5: x = new Op(Operation.FPREM1); break;
                                    case 6: x = new Op(Operation.FDECSTP); break;
                                    case 7: x = new Op(Operation.FINCSTP); break;
                                }
                                break;
                            case 7:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.FPREM); break;
                                    case 1: x = new Op(Operation.FYL2XP1); break;
                                    case 2: x = new Op(Operation.FSQRT); break;
                                    case 3: x = new Op(Operation.FSINCOS); break;
                                    case 4: x = new Op(Operation.FRNDINT); break;
                                    case 5: x = new Op(Operation.FSCALE); break;
                                    case 6: x = new Op(Operation.FSIN); break;
                                    case 7: x = new Op(Operation.FCOS); break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FLD, O.ST0, O.M32fp); break;
                            case 1: break;
                            case 2: x = new Op(Operation.FST, O.ST0, O.M32fp); break;
                            case 3: x = new Op(Operation.FSTP, O.ST0, O.M32fp); break;
                            case 4: x = new Op(Operation.FLDENV, O.M14or28b); break;
                            case 5: x = new Op(Operation.FLDCW, O.Mw); break;
                            case 6: x = new Op(Operation.FSTENV, O.M14or28b); break;
                            case 7: x = new Op(Operation.FSTCW, O.Mw); break;
                        }
                    }
                    break;

                case 0xDA:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FCMOVB, O.ST0, O.T80fp); break;
                            case 1: x = new Op(Operation.FCMOVE, O.ST0, O.T80fp); break;
                            case 2: x = new Op(Operation.FCMOVBE, O.ST0, O.T80fp); break;
                            case 3: x = new Op(Operation.FCMOVU, O.ST0, O.T80fp); break;
                            case 6:
                                switch (rm)
                                {
                                    case 1: x = new Op(Operation.FUCOMPP); break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FIADD, O.ST0, O.Md); break;
                            case 1: x = new Op(Operation.FIMUL, O.ST0, O.Md); break;
                            case 2: x = new Op(Operation.FICOM, O.ST0, O.Md); break;
                            case 3: x = new Op(Operation.FICOMP, O.ST0, O.Md); break;
                            case 4: x = new Op(Operation.FISUB, O.ST0, O.Md); break;
                            case 5: x = new Op(Operation.FISUBR, O.ST0, O.Md); break;
                            case 6: x = new Op(Operation.FIDIV, O.ST0, O.Md); break;
                            case 7: x = new Op(Operation.FIDIVR, O.ST0, O.Md); break;
                        }
                    }
                    break;

                case 0xDB:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FCMOVNB, O.ST0, O.T80fp); break;
                            case 1: x = new Op(Operation.FCMOVNE, O.ST0, O.T80fp); break;
                            case 2: x = new Op(Operation.FCMOVNBE, O.ST0, O.T80fp); break;
                            case 3: x = new Op(Operation.FCMOVNU, O.ST0, O.T80fp); break;
                            case 4:
                                switch (rm)
                                {
                                    case 2: x = new Op(Operation.FCLEX); break;
                                    case 3: x = new Op(Operation.FINIT); break;
                                }
                                break;
                            case 5: x = new Op(Operation.FUCOMI, O.ST0, O.T80fp); break;
                            case 6: x = new Op(Operation.FCOMI, O.ST0, O.T80fp); break;
                            case 7: break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FILD, O.Md); break;
                            case 1: x = new Op(Operation.FISTTP, O.Md); break;
                            case 2: x = new Op(Operation.FIST, O.Md); break;
                            case 3: x = new Op(Operation.FISTP, O.Md); break;
                            case 4: break;
                            case 5: x = new Op(Operation.FLD, O.M80fp); break;
                            case 6: break;
                            case 7: x = new Op(Operation.FSTP, O.M80fp); break;
                        }
                    }
                    break;

                case 0xDC:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FADD,O.T80fp, O.ST0); break;
                            case 1: x = new Op(Operation.FMUL, O.T80fp, O.ST0); break;
                            case 2: break;
                            case 3: break;
                            case 4: x = new Op(Operation.FSUBR, O.T80fp, O.ST0); break;
                            case 5: x = new Op(Operation.FSUB, O.T80fp, O.ST0); break;
                            case 6: x = new Op(Operation.FDIVR, O.T80fp, O.ST0); break;
                            case 7: x = new Op(Operation.FDIV, O.T80fp, O.ST0); break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FADD, O.ST0, O.M64fp); break;
                            case 1: x = new Op(Operation.FMUL, O.ST0, O.M64fp); break;
                            case 2: x = new Op(Operation.FCOM, O.ST0, O.M64fp); break;
                            case 3: x = new Op(Operation.FCOMP, O.ST0, O.M64fp); break;
                            case 4: x = new Op(Operation.FSUB, O.ST0, O.M64fp); break;
                            case 5: x = new Op(Operation.FSUBR, O.ST0, O.M64fp); break;
                            case 6: x = new Op(Operation.FDIV, O.ST0, O.M64fp); break;
                            case 7: x = new Op(Operation.FDIVR, O.ST0, O.M64fp); break;
                        }
                    }
                    break;

                case 0xDD:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FFREE, O.T80fp); break;
                            case 1: break;
                            case 2: x = new Op(Operation.FST, O.T80fp); break;
                            case 3: x = new Op(Operation.FSTP, O.T80fp); break;
                            case 4: x = new Op(Operation.FUCOM, O.T80fp, O.ST0); break;
                            case 5: x = new Op(Operation.FUCOMP, O.T80fp, O.ST0); break;
                            case 6: break;
                            case 7: break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FLD, O.M64fp); break;
                            case 1: x = new Op(Operation.FISTTP, O.Mq); break;
                            case 2: x = new Op(Operation.FST, O.M64fp); break;
                            case 3: x = new Op(Operation.FSTP, O.M64fp); break;
                            case 4: x = new Op(Operation.FRSTOR, O.Mb); break; // TODO: should be 98/108 bytes
                            case 5: break;
                            case 6: x = new Op(Operation.FSAVE, O.Mb); break; // TODO: should be 98/108 bytes
                            case 7: x = new Op(Operation.FSTSW, O.Mw); break;
                        }
                    }
                    break;

                case 0xDE:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FADDP, O.T80fp, O.ST0); break;
                            case 1: x = new Op(Operation.FMULP, O.T80fp, O.ST0); break;
                            case 2: break;
                            case 3:
                                switch (rm)
                                {
                                    case 1: x = new Op(Operation.FCOMPP); break;
                                }
                                break;
                            case 4: x = new Op(Operation.FSUBRP, O.T80fp, O.ST0); break;
                            case 5: x = new Op(Operation.FSUBP, O.T80fp, O.ST0); break;
                            case 6: x = new Op(Operation.FDIVRP, O.T80fp, O.ST0); break;
                            case 7: x = new Op(Operation.FDIVP, O.T80fp, O.ST0); break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FIADD, O.ST0, O.Mw); break;
                            case 1: x = new Op(Operation.FIMUL, O.ST0, O.Mw); break;
                            case 2: x = new Op(Operation.FICOM, O.ST0, O.Mw); break;
                            case 3: x = new Op(Operation.FICOMP, O.ST0, O.Mw); break;
                            case 4: x = new Op(Operation.FISUB, O.ST0, O.Mw); break;
                            case 5: x = new Op(Operation.FISUBR, O.ST0, O.Mw); break;
                            case 6: x = new Op(Operation.FIDIV, O.ST0, O.Mw); break;
                            case 7: x = new Op(Operation.FIDIVR, O.ST0, O.Mw); break;
                        }
                    }
                    break;

                case 0xDF:
                    if (mod == 3)
                    {
                        switch (reg)
                        {
                            case 0: break;
                            case 1: break;
                            case 2: break;
                            case 3: break;
                            case 4:
                                switch (rm)
                                {
                                    case 0: x = new Op(Operation.FSTSW, O.AX); break;
                                }
                                break;
                            case 5: x = new Op(Operation.FUCOMIP, O.ST0, O.T80fp); break;
                            case 6: x = new Op(Operation.FCOMIP, O.ST0, O.T80fp); break;
                            case 7: break;
                        }
                    }
                    else
                    {
                        switch (reg)
                        {
                            case 0: x = new Op(Operation.FILD, O.Mw); break;
                            case 1: x = new Op(Operation.FISTTP, O.Mw); break;
                            case 2: x = new Op(Operation.FIST, O.Mw); break;
                            case 3: x = new Op(Operation.FISTP, O.Mw); break;
                            case 4: x = new Op(Operation.FBLD, O.M80fp); break;
                            case 5: x = new Op(Operation.FILD, O.Mq); break;
                            case 6: x = new Op(Operation.FBSTP, O.M80fp); break;
                            case 7: x = new Op(Operation.FISTP, O.Mq); break;
                        }
                    }
                    break;
            }
            if (x.IsEmpty)
            {
                throw new InvalidInstructionException(string.Format(
                    "Invalid opcode: {0:X2} {1:X2}.",
                    firstByte, modrm.Value));
            }
            return x;
        }

        /// <summary>
        /// Defines an entry in an opcode map.
        /// </summary>
        /// <remarks>
        /// Each opcode entry specifies the encoding of an instruction with a
        /// particular set of operands. For performance reason, each entry is
        /// stored in a 64-bit integer, in the following format:
        /// 
        /// Byte 0-1: a 16-bit signed integer, where
        ///    - If this value is positive, it is a member of Operation enum;
        ///    - If this value is zero, it designates an invalid instruction.
        ///    - If this value is negative, it specifies an opcode extension.
        /// Byte 2-5: Each byte is a member of O enum. There can be at most
        ///           4 operands.
        /// Byte 6-7: Reserved; always zero.
        /// </remarks>
        struct Op
        {
            long value;

            public bool IsEmpty
            {
                get { return value == 0; }
            }

            public bool IsExtension
            {
                get { return (short)this.Operation < 0; }
            }

            public Operation Operation
            {
                get { return (Operation)(value & 0xffffL); }
            }

            public OpcodeExtension Extension
            {
                get { return (OpcodeExtension)(-(short)this.Operation); }
            }

            public O[] Operands
            {
                get
                {
                    int n = 0;
                    while ((value & (0xffL << ((2 + n) * 8))) != 0L)
                        n++;

                    O[] operands = new O[n];
                    for (int i = 0; i < n; i++)
                    {
                        operands[i] = (O)(byte)(value >> ((2 + i) * 8));
                    }
                    return operands;
                }
            }

            public Op(Operation operation, params O[] operands)
                : this()
            {
                if (operands.Length > 4)
                    throw new ArgumentException("There can be at most four operands.");

                value = (long)(ushort)operation;
                for (int i = 0; i < operands.Length; i++)
                {
                    value |= (long)(byte)operands[i] << ((2 + i) * 8);
                }
            }

            public Op(OpcodeExtension extension, params O[] operands)
                : this((Operation)(ushort)(-(int)extension), operands)
            {
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.Operation.ToString());
                O[] operands = this.Operands;
                for (int i = 0; i < operands.Length; i++)
                {
                    if (i > 0)
                        sb.Append(',');
                    sb.Append(' ');
                    sb.Append(operands[i].ToString());
                }
                return sb.ToString();
            }

            public Op MergeOperandsFrom(Op other)
            {
                Op op = new Op();
                op.value = this.value | (other.value & ~0xffffL);
                return op;
            }

            public static readonly Op Empty = new Op();
        }

        /// <summary>
        /// Decodes a register operand from the REG field of the ModR/M byte.
        /// Since the REG field only contains the register number, additional
        /// parameters are used to determine the register's type and size.
        /// </summary>
        /// <param name="reader">Instruction reader.</param>
        /// <param name="registerType">Type of the register to return.</param>
        /// <param name="operandSize">Size of the register to return.</param>
        /// <param name="context">Decoding context.</param>
        /// <returns>The decoded register operand.</returns>
        private static RegisterOperand DecodeRegisterOperand(
            InstructionReader reader,
            RegisterType registerType,
            CpuSize operandSize,
            DecoderContext context)
        {
            if (context.CpuMode == CpuMode.X64Mode)
                throw new NotSupportedException();
            if (operandSize == CpuSize.Default)
                throw new ArgumentException("operandSize is not specified.");

            int reg = reader.GetModRM().REG;
            if (registerType == RegisterType.General &&
                operandSize == CpuSize.Use8Bit &&
                reg >= 4)
            {
                return new RegisterOperand(
                    RegisterType.General, 
                    reg - 4, 
                    CpuSize.Use8Bit,
                    RegisterOffset.HighByte);
            }
            return new RegisterOperand(registerType, reg, operandSize);
        }

        // Note: we need to take into account OperandSizeOverride and
        // AddressSizeOverride!!!!!!

        /// <summary>
        /// Decodes a memory operand encoded by the ModRM byte, SIB byte, and
        /// Displacement, or a register operand if MOD=3 and registerType is
        /// specified.
        /// </summary>
        /// <param name="reader">Instruction reader.</param>
        /// <param name="registerType">Type of the register to return if the
        /// Mod field of the ModR/M byte is 3. If this parameter is set to
        /// RegisterType.None, an exception is thrown if Mod=3.</param>
        /// <param name="operandSize">Size of the returned operand.</param>
        /// <param name="context">Decoding context.</param>
        /// <returns>The decoded memory or register operand.</returns>
        /// <exception cref="InvalidInstructionException">If registerType is
        /// set to None but the ModR/M byte encodes a register.</exception>
        static Operand DecodeMemoryOperand(
            InstructionReader reader,
            RegisterType registerType,
            CpuSize operandSize,
            DecoderContext context)
        {
            if (context.CpuMode == CpuMode.X64Mode)
                throw new NotSupportedException();
            if (operandSize == CpuSize.Default)
                throw new ArgumentException("operandSize is not specified.");
            if (context.AddressSize != CpuSize.Use16Bit)
                throw new NotSupportedException("32-bit addressing mode is not supported.");

            ModRM modrm = reader.GetModRM();
            int rm = modrm.RM;
            int mod = modrm.MOD;

            // Decode a register if MOD = (11).
            if (mod == 3)
            {
                // If the instruction expects a memory operand, throw an exception.
                if (registerType == RegisterType.None)
                {
                    throw new InvalidInstructionException(
                        "The instruction expects a memory operand, but the ModR/M byte encodes a register.");
                }

                // Treat AH-DH specially.
                if (registerType == RegisterType.General &&
                    operandSize == CpuSize.Use8Bit &&
                    rm >= 4)
                {
                    return new RegisterOperand(
                        RegisterType.General, 
                        rm - 4, 
                        CpuSize.Use8Bit,
                        RegisterOffset.HighByte);
                }
                else
                {
                    return new RegisterOperand(registerType, rm, operandSize);
                }
            }

            // Take into account segment override prefix if present.
            Register segment = context.SegmentOverride;

            // Special treatment for MOD = (00) and RM = (110).
            // This encodes a 16-bit sign-extended displacement.
            if (mod == 0 && rm == 6)
            {
                int displacement = reader.ReadImmediate(CpuSize.Use16Bit);
                return new MemoryOperand
                {
                    Size = operandSize,
                    Segment = segment,
                    Displacement = displacement
                };
            }

            /* Decode an indirect memory address XX[+YY][+disp]. */
            MemoryOperand mem = new MemoryOperand();
            mem.Size = operandSize;
            mem.Segment = segment;
            switch (rm)
            {
                case 0: /* [BX+SI] */
                    mem.Base = Register.BX;
                    mem.Index = Register.SI;
                    break;
                case 1: /* [BX+DI] */
                    mem.Base = Register.BX;
                    mem.Index = Register.DI;
                    break;
                case 2: /* [BP+SI] */
                    mem.Base = Register.BP;
                    mem.Index = Register.SI;
                    break;
                case 3: /* [BP+DI] */
                    mem.Base = Register.BP;
                    mem.Index = Register.DI;
                    break;
                case 4: /* [SI] */
                    mem.Base = Register.SI;
                    break;
                case 5: /* [DI] */
                    mem.Base = Register.DI;
                    break;
                case 6: /* [BP] */
                    mem.Base = Register.BP;
                    break;
                case 7: /* [BX] */
                    mem.Base = Register.BX;
                    break;
            }
            if (mod == 1) /* disp8, sign-extended */
            {
                mem.Displacement = reader.ReadImmediate(CpuSize.Use8Bit);
            }
            else if (mod == 2) /* disp16, sign-extended */
            {
                mem.Displacement = reader.ReadImmediate(CpuSize.Use16Bit);
            }
            return mem;
        }

        static ImmediateOperand DecodeImmediateOperand(InstructionReader reader, CpuSize size)
        {
            return new ImmediateOperand(reader.ReadImmediate(size), size);
        }

        static RelativeOperand DecodeRelativeOperand(InstructionReader reader, CpuSize size)
        {
            return new RelativeOperand(reader.ReadImmediate(size));
        }

        static Operand DecodeOperand(O spec,
            InstructionReader reader, DecoderContext context)
        {
            int number;
            CpuSize size;

            switch (spec)
            {
                case O.Imm0:
                case O.Imm1:
                case O.Imm2:
                case O.Imm3:
                    return new ImmediateOperand(spec - O.Imm0, CpuSize.Use8Bit);

                case O.ES:
                case O.CS:
                case O.SS:
                case O.DS:
                    return new RegisterOperand(
                        RegisterType.Segment,
                        spec - O.ES,
                        CpuSize.Use16Bit);

                case O.AL:
                case O.CL:
                case O.DL:
                case O.BL:
                    return new RegisterOperand(
                        RegisterType.General,
                        spec - O.AL,
                        CpuSize.Use8Bit);

                case O.AH:
                case O.CH:
                case O.DH:
                case O.BH:
                    return new RegisterOperand(
                        RegisterType.General,
                        spec - O.AH,
                        CpuSize.Use8Bit,
                        RegisterOffset.HighByte);

                case O.AX:
                case O.CX:
                case O.DX:
                case O.BX:
                case O.SP:
                case O.BP:
                case O.SI:
                case O.DI:
                    return new RegisterOperand(
                        RegisterType.General,
                        spec - O.AX,
                        CpuSize.Use16Bit);

                case O.eAX:
                case O.eCX:
                case O.eDX:
                case O.eBX:
                case O.eSP:
                case O.eBP:
                case O.eSI:
                case O.eDI:
                    number = spec - O.eAX;
                    size = (context.OperandSize == CpuSize.Use16Bit) ?
                        CpuSize.Use16Bit : CpuSize.Use32Bit;
                    return new RegisterOperand(RegisterType.General, number, size);

                case O.rAX:
                case O.rCX:
                case O.rDX:
                case O.rBX:
                case O.rSP:
                case O.rBP:
                case O.rSI:
                case O.rDI:
                    number = spec - O.rAX;
                    return new RegisterOperand(RegisterType.General, number, context.OperandSize);

                case O.ST0:
                    return new RegisterOperand(Register.ST0);

                case O.Ap: // off:seg encoded in the instruction
                    if (context.OperandSize != CpuSize.Use16Bit)
                    {
                        throw new NotSupportedException();
                    }
                    if (true)
                    {
                        ushort off = (ushort)reader.ReadImmediate(CpuSize.Use16Bit);
                        ushort seg = (ushort)reader.ReadImmediate(CpuSize.Use16Bit);
                        return new PointerOperand(seg, off);
                    }

                case O.Eb: // r/m; 8-bit
                    return DecodeMemoryOperand(reader, RegisterType.General, CpuSize.Use8Bit, context);

                case O.Ep: // r/m; contains far ptr
                    if (context.OperandSize != CpuSize.Use16Bit)
                        throw new NotSupportedException();
                    // TBD: operand size? address size?
                    return DecodeMemoryOperand(reader, RegisterType.General, CpuSize.Use16Bit, context);

                case O.Ev: // r/m; 16, 32, or 64 bit
                    return DecodeMemoryOperand(reader, RegisterType.General, context.OperandSize, context);

                case O.Ew: // r/m; 16-bit
                    return DecodeMemoryOperand(reader, RegisterType.General, CpuSize.Use16Bit, context);

                case O.Fv: // FLAGS/EFLAGS/RFLAGS
                    return new RegisterOperand(Register.FLAGS, context.OperandSize);

                case O.Gb: // general-purpose register; byte
                    return DecodeRegisterOperand(reader, RegisterType.General, CpuSize.Use8Bit, context);

                case O.Gv: // general-purpose register; 16, 32, or 64 bit
                    return DecodeRegisterOperand(reader, RegisterType.General, context.OperandSize, context);

                case O.Gw: // general-purpose register; 16-bit
                    return DecodeRegisterOperand(reader, RegisterType.General, CpuSize.Use16Bit, context);

                case O.Gz: // general-purpose register; 16 or 32 bit
                    return DecodeRegisterOperand(
                        reader,
                        RegisterType.General,
                        context.OperandSize == CpuSize.Use16Bit ? CpuSize.Use16Bit : CpuSize.Use32Bit,
                        context);

                case O.Ib: // immediate; byte
                    return DecodeImmediateOperand(reader, CpuSize.Use8Bit);

                case O.Iw: // immediate; 16 bit
                    return DecodeImmediateOperand(reader, CpuSize.Use16Bit);

                case O.Iv: // immediate, 16, 32, or 64 bit
                    return DecodeImmediateOperand(reader, context.OperandSize);

                case O.Iz: // immediate, 16 or 32 bit
                    return DecodeImmediateOperand(reader,
                        (context.OperandSize == CpuSize.Use16Bit) ?
                        CpuSize.Use16Bit : CpuSize.Use32Bit);

                case O.Jb: // immediate encodes relative offset; byte
                    return DecodeRelativeOperand(reader, CpuSize.Use8Bit);

                case O.Jz: // immediate encodes relative offset; 16 or 32 bit
                    return DecodeRelativeOperand(reader,
                        (context.OperandSize == CpuSize.Use16Bit) ?
                        CpuSize.Use16Bit : CpuSize.Use32Bit);

                case O.Mp: // r/m must refer to memory; contains far pointer
                    // seg:ptr of 32, 48, or 80 bits.
                    if (context.OperandSize != CpuSize.Use16Bit)
                        throw new NotSupportedException();
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use16Bit, context);

                case O.Mb: // r/m refers to memory; byte
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use8Bit, context);

                case O.Mw: // r/m refers to memory; word
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use16Bit, context);

                case O.Mq:
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use64Bit, context);

                case O.M14or28b: // r/m refers to memory; 14 or 28 bytes
                    // TODO: we actually need to check the CPU operation mode,
                    // not the operand-size attribute.
                    return DecodeMemoryOperand(
                        reader,
                        RegisterType.None,
                        (context.OperandSize == CpuSize.Use16Bit) ? 
                            CpuSize.Use14Bytes : CpuSize.Use28Bytes,
                        context);

                case O.M32fp: // r/m refers to memory; single-precision float
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use32Bit, context);

                case O.M64fp: // r/m refers to memory; double-precision float
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use64Bit, context);

                case O.M80fp: // r/m refers to memory; extended-precision float
                    return DecodeMemoryOperand(reader, RegisterType.None, CpuSize.Use80Bit, context);

                case O.Ob: // absolute address (w/o segment); byte
                    // TODO: check 64-bit mode behavior
                    return new MemoryOperand
                    {
                        Size = CpuSize.Use8Bit,
                        Displacement = reader.ReadImmediate(context.AddressSize),
                        // Segment = Register.DS,
                    };

                case O.Ov: // absolute address (w/o segment); 16, 32, or 64 bit
                    // TODO: check 64-bit mode behavior
                    return new MemoryOperand
                    {
                        Size = context.OperandSize,
                        Displacement = reader.ReadImmediate(context.AddressSize),
                        // Segment = Register.DS,
                    };

                case O.Sw: // REG(modrm) selects segment register
                    return DecodeRegisterOperand(reader, RegisterType.Segment, CpuSize.Use16Bit, context);

                case O.T80fp: // r/m selects x87 FPU register ST0-ST7
                    return new RegisterOperand(RegisterType.Fpu, reader.GetModRM().RM, CpuSize.Use80Bit);

                case O.Xb: // memory addressed by DS:rSI; byte
                    return new MemoryOperand
                    {
                        Size = CpuSize.Use8Bit,
                        Segment = Register.DS,
                        Base = RegisterOperand.Resize(Register.SI, context.AddressSize)
                    };

                case O.Xv: // memory addressed by DS:rSI; 16, 32, or 64 bit
                    return new MemoryOperand
                    {
                        Size = context.OperandSize,
                        Segment = Register.DS,
                        Base = RegisterOperand.Resize(Register.SI, context.AddressSize)
                    };

                case O.Yb: // memory addressed by ES:rDI; byte
                    return new MemoryOperand
                    {
                        Size = CpuSize.Use8Bit,
                        Segment = Register.ES,
                        Base = RegisterOperand.Resize(Register.DI, context.AddressSize)
                    };

                case O.Yv: // memory addressed by ES:rDI; 16, 32, or 64 bit
                    return new MemoryOperand
                    {
                        Size = context.OperandSize,
                        Segment = Register.ES,
                        Base = RegisterOperand.Resize(Register.DI, context.AddressSize)
                    };

                case O._XLAT:
                    return new MemoryOperand
                    {
                        Size = CpuSize.Use8Bit,
                        Segment = context.SegmentOverride,
                        Base = (context.AddressSize == CpuSize.Use16Bit) ?
                            Register.BX : Register.EBX
                    };

                default:
                    throw new NotImplementedException(
                        "DecodeOperand() is not implemented for operand type " + spec.ToString());
            }
        }

        /*
     * Specifies the encoding of an operand. Most operands are encoded in the form
     * "Zz", where "Z" specifies the addressing method and "z" specifies the data
     * type. These encodings are represented by a value using 16 bits. 
     *
     * The rest operands are encoded with special values. These encodings are
     * represented with a value above 0x10000.
     *
     * The special value 0 means the operand is not used.
     *
     * See Intel Reference, Volume 2, Appendix A.2 for an explanation of the 
     * abbreviations for addressing method and data type.
     */
        enum O
        {
            None = 0,

            /// <summary>
            /// Direct address: the instruction has no ModR/M byte; the 
            /// address of the operand is encoded in the instruction. No base
            /// register, index register, or scaling factor can be applied.
            /// 
            /// The operand size is 32-bit, 48-bit, or 80-bit pointer,
            /// depending on operand-size attribute.
            /// </summary>
            Ap,
            
            /// <summary>
            /// Specifies an operand that is either a general-purpose register
            /// or a memory address, encoded by ModR/M + SIB + displacement.
            /// The size of the operand is byte.
            /// </summary>            
            Eb, 
            
            Ep,

            /// <summary>
            /// Specifies an operand that is either a general-purpose register
            /// or a memory address, encoded by ModR/M + SIB + displacement.
            /// The size of the operand is the native CPU operand size (16,
            /// 32, or 64 bit).
            /// </summary>            
            Ev, 
            
            Ew,
            Fv,

            /// <summary>
            /// Specifies a general-purpose register operand specified by the
            /// REG field of the ModR/M byte. The size of the operand is byte.
            /// </summary>
            Gb,

            /// <summary>
            /// Specifies a general-purpose register operand specified by the
            /// REG field of the ModR/M byte. The size of the operand is the
            /// native CPU operand size (16, 32, or 64 bit).
            /// </summary>
            Gv,

            /// <summary>
            /// Specifies a general-purpose register operand specified by the
            /// REG field of the ModR/M byte. The size of the operand is word.
            /// </summary>
            Gw,

            /// <summary>
            /// Specifies a general-purpose register operand specified by the
            /// REG field of the ModR/M byte. The size of the operand is word
            /// for 16-bit operand-size or dword for 32 or 64-bit operand 
            /// size.
            /// </summary>
            Gz,

            Ib, Iv, Iw, Iz,
            Jb, Jz,
            Ma, Mb, Md, Mp, Mw, Mq,

            /// <summary>
            /// The ModR/M byte refers to a memory location containing 14 
            /// bytes or 28 bytes, depending on operation mode of the CPU.
            /// </summary>
            M14or28b,

            /// <summary>
            /// The ModR/M byte refers to a memory location containing 32-bit
            /// single-precision floating number.
            /// </summary>
            M32fp,

            /// <summary>
            /// The ModR/M byte refers to a memory location containing 64-bit
            /// double-precision floating number.
            /// </summary>
            M64fp,

            /// <summary>
            /// The ModR/M byte refers to a memory location containing 80-bit
            /// extended-precision floating number.
            /// </summary>
            M80fp,

            /// <summary>
            /// The instruction has no ModR/M byte. The offset of the operand
            /// is coded as a word or double word (depending on address size
            /// attribute) in the instruction. No base register, index 
            /// register, or scaling factor can be applied.
            /// </summary>
            Ob,

            /// <summary>
            /// The instruction has no ModR/M byte. The offset of the operand
            /// is coded as a word or double word (depending on address size
            /// attribute) in the instruction. No base register, index 
            /// register, or scaling factor can be applied.
            /// </summary>
            Ov,

            Rv,
            Sw,

            /// <summary>
            /// The R/M field of the ModR/M byte selects an x87 FPU data
            /// register ST(0)-ST(7).
            /// </summary>
            T80fp,

            Xb, Xv, Xz,
            Yb, Yv, Yz,

            /* Immediate */
            Imm0, Imm1, Imm2, Imm3, Imm4, Imm5, Imm6, Imm7,

            /* Segment registers. */
            ES, CS, SS, DS,

            /* Byte registers. */
            AL, CL, DL, BL, AH, CH, DH, BH,

            /* 16-bit generic registers */
            AX, CX, DX, BX, SP, BP, SI, DI,

            /* XX in 16-bit mode, EXX in 32- or 64-bit mode */
            eAX, eCX, eDX, eBX, eSP, eBP, eSI, eDI,

            /* XX in 16-bit mode, EXX in 32-bit mode, RXX in 64-bit mode */
            rAX, rCX, rDX, rBX, rSP, rBP, rSI, rDI,

            /* x87 FPU registers */
            ST0, // ST1, ST2, ST3, ST4, ST5, ST6, ST7,

            /// <summary>
            /// Special operand for XLAT instruction: byte ptr [eBX]
            /// </summary>
            _XLAT,
        }

        /// <summary>
        /// Opcode map for one-byte opcodes.
        /// See Table A-2 in Intel Reference, Volume 2, Appendix A.
        /// </summary>
        private static readonly Op[] OneByteOpcodeMap = new Op[]
        {
            /* 00 */ new Op(Operation.ADD, O.Eb, O.Gb),
            /* 01 */ new Op(Operation.ADD, O.Ev, O.Gv),
            /* 02 */ new Op(Operation.ADD, O.Gb, O.Eb),
            /* 03 */ new Op(Operation.ADD, O.Gv, O.Ev),
            /* 04 */ new Op(Operation.ADD, O.AL, O.Ib),
            /* 05 */ new Op(Operation.ADD, O.rAX, O.Iz),
            /* 06 */ new Op(Operation.PUSH, O.ES), /* i64 */
            /* 07 */ new Op(Operation.POP, O.ES),  /* i64 */
            /* 08 */ new Op(Operation.OR, O.Eb, O.Gb),
            /* 09 */ new Op(Operation.OR, O.Ev, O.Gv),
            /* 0A */ new Op(Operation.OR, O.Gb, O.Eb),
            /* 0B */ new Op(Operation.OR, O.Gv, O.Ev),
            /* 0C */ new Op(Operation.OR, O.AL, O.Ib),
            /* 0D */ new Op(Operation.OR, O.rAX, O.Iz),
            /* 0E */ new Op(Operation.PUSH, O.CS), /* i64 */
            /* 0F */ Op.Empty,      /* 2-byte escape */

            /* 10 */ new Op(Operation.ADC, O.Eb, O.Gb),
            /* 11 */ new Op(Operation.ADC, O.Ev, O.Gv),
            /* 12 */ new Op(Operation.ADC, O.Gb, O.Eb),
            /* 13 */ new Op(Operation.ADC, O.Gv, O.Ev),
            /* 14 */ new Op(Operation.ADC, O.AL, O.Ib),
            /* 15 */ new Op(Operation.ADC, O.rAX, O.Iz),
            /* 16 */ new Op(Operation.PUSH, O.SS), /* i64 */
            /* 17 */ new Op(Operation.POP, O.SS), /* i64 */
            /* 18 */ new Op(Operation.SBB, O.Eb, O.Gb),
            /* 19 */ new Op(Operation.SBB, O.Ev, O.Gv),
            /* 1A */ new Op(Operation.SBB, O.Gb, O.Eb),
            /* 1B */ new Op(Operation.SBB, O.Gv, O.Ev),
            /* 1C */ new Op(Operation.SBB, O.AL, O.Ib),
            /* 1D */ new Op(Operation.SBB, O.rAX, O.Iz),
            /* 1E */ new Op(Operation.PUSH, O.DS), /* i64 */
            /* 1F */ new Op(Operation.POP, O.DS), /* i64 */

            /* 20 */ new Op(Operation.AND, O.Eb, O.Gb),
            /* 21 */ new Op(Operation.AND, O.Ev, O.Gv),
            /* 22 */ new Op(Operation.AND, O.Gb, O.Eb),
            /* 23 */ new Op(Operation.AND, O.Gv, O.Ev),
            /* 24 */ new Op(Operation.AND, O.AL, O.Ib),
            /* 25 */ new Op(Operation.AND, O.rAX, O.Iz),
            /* 26 */ Op.Empty, /* SEG=ES (prefix) */
            /* 27 */ new Op(Operation.DAA), /* i64 */
            /* 28 */ new Op(Operation.SUB, O.Eb, O.Gb),
            /* 29 */ new Op(Operation.SUB, O.Ev, O.Gv),
            /* 2A */ new Op(Operation.SUB, O.Gb, O.Eb),
            /* 2B */ new Op(Operation.SUB, O.Gv, O.Ev),
            /* 2C */ new Op(Operation.SUB, O.AL, O.Ib),
            /* 2D */ new Op(Operation.SUB, O.rAX, O.Iz),
            /* 2E */ Op.Empty, /* SEG=CS (prefix) */
            /* 2F */ new Op(Operation.DAS), /* i64 */

            /* 30 */ new Op(Operation.XOR, O.Eb, O.Gb),
            /* 31 */ new Op(Operation.XOR, O.Ev, O.Gv),
            /* 32 */ new Op(Operation.XOR, O.Gb, O.Eb),
            /* 33 */ new Op(Operation.XOR, O.Gv, O.Ev),
            /* 34 */ new Op(Operation.XOR, O.AL, O.Ib),
            /* 35 */ new Op(Operation.XOR, O.rAX, O.Iz),
            /* 36 */ Op.Empty, /* SEG=SS (prefix) */
            /* 37 */ new Op(Operation.AAA), /* i64 */
            /* 38 */ new Op(Operation.CMP, O.Eb, O.Gb),
            /* 39 */ new Op(Operation.CMP, O.Ev, O.Gv),
            /* 3A */ new Op(Operation.CMP, O.Gb, O.Eb),
            /* 3B */ new Op(Operation.CMP, O.Gv, O.Ev),
            /* 3C */ new Op(Operation.CMP, O.AL, O.Ib),
            /* 3D */ new Op(Operation.CMP, O.rAX, O.Iz),
            /* 3E */ Op.Empty, /* SEG=DS (prefix) */
            /* 3F */ new Op(Operation.AAS), /* i64 */

            /* 40 */ new Op(Operation.INC, O.eAX), /* i64, O.REX */
            /* 41 */ new Op(Operation.INC, O.eCX), /* i64, O.REX.B */
            /* 42 */ new Op(Operation.INC, O.eDX), /* i64, O.REX.X */
            /* 43 */ new Op(Operation.INC, O.eBX), /* i64, O.REX.XB */
            /* 44 */ new Op(Operation.INC, O.eSP), /* i64, O.REX.R */
            /* 45 */ new Op(Operation.INC, O.eBP), /* i64, O.REX.RB */
            /* 46 */ new Op(Operation.INC, O.eSI), /* i64, O.REX.RX */
            /* 47 */ new Op(Operation.INC, O.eDI), /* i64, O.REX.RXB */
            /* 48 */ new Op(Operation.DEC, O.eAX), /* i64, O.REX.W */
            /* 49 */ new Op(Operation.DEC, O.eCX), /* i64, O.REX.WB */
            /* 4A */ new Op(Operation.DEC, O.eDX), /* i64, O.REX.WX */
            /* 4B */ new Op(Operation.DEC, O.eBX), /* i64, O.REX.WXB */
            /* 4C */ new Op(Operation.DEC, O.eSP), /* i64, O.REX.WR */
            /* 4D */ new Op(Operation.DEC, O.eBP), /* i64, O.REX.WRB */
            /* 4E */ new Op(Operation.DEC, O.eSI), /* i64, O.REX.WRX */
            /* 4F */ new Op(Operation.DEC, O.eDI), /* i64, O.REX.WRXB */

            /* 50 */ new Op(Operation.PUSH, O.rAX), /* d64 */
            /* 51 */ new Op(Operation.PUSH, O.rCX), /* d64 */
            /* 52 */ new Op(Operation.PUSH, O.rDX), /* d64 */
            /* 53 */ new Op(Operation.PUSH, O.rBX), /* d64 */
            /* 54 */ new Op(Operation.PUSH, O.rSP), /* d64 */
            /* 55 */ new Op(Operation.PUSH, O.rBP), /* d64 */
            /* 56 */ new Op(Operation.PUSH, O.rSI), /* d64 */
            /* 57 */ new Op(Operation.PUSH, O.rDI), /* d64 */
            /* 58 */ new Op(Operation.POP, O.rAX), /* d64 */
            /* 59 */ new Op(Operation.POP, O.rCX), /* d64 */
            /* 5A */ new Op(Operation.POP, O.rDX), /* d64 */
            /* 5B */ new Op(Operation.POP, O.rBX), /* d64 */
            /* 5C */ new Op(Operation.POP, O.rSP), /* d64 */
            /* 5D */ new Op(Operation.POP, O.rBP), /* d64 */
            /* 5E */ new Op(Operation.POP, O.rSI), /* d64 */
            /* 5F */ new Op(Operation.POP, O.rDI), /* d64 */
            
            /* 60 */ new Op(Operation.PUSHA), /* i64 */
            /* 61 */ new Op(Operation.POPA), /* i64 */
            /* 62 */ new Op(Operation.BOUND, O.Gv, O.Ma), /* i64 */
            /* 63 */ new Op(Operation.ARPL, O.Ew, O.Gw), /* i64, O.MOVSXD (o64) */
            /* 64 */ Op.Empty, /* SEG=FS (prefix) */
            /* 65 */ Op.Empty, /* SEG=GS (prefix) */
            /* 66 */ Op.Empty, /* operand size (prefix) */
            /* 67 */ Op.Empty, /* address size (prefix) */
            /* 68 */ new Op(Operation.PUSH, O.Iz), /* d64 */
            /* 69 */ new Op(Operation.IMUL, O.Gv, O.Ev, O.Iz),
            /* 6A */ new Op(Operation.PUSH, O.Ib), /* d64 */
            /* 6B */ new Op(Operation.IMUL, O.Gv, O.Ev, O.Ib),
            /* 6C */ new Op(Operation.INS, O.Yb, O.DX),
            /* 6D */ new Op(Operation.INS, O.Yz, O.DX),
            /* 6E */ new Op(Operation.OUTS, O.DX, O.Xb),
            /* 6F */ new Op(Operation.OUTS, O.DX, O.Xz),


            /* f64 - The operand size is forced to a 64-bit operand size
             * when in 64-bit mode, O.regardless of size prefix. 
             */
            /* 70 */ new Op(Operation.JO, O.Jb),
            /* 71 */ new Op(Operation.JNO, O.Jb),
            /* 72 */ new Op(Operation.JB, O.Jb),
            /* 73 */ new Op(Operation.JAE, O.Jb),
            /* 74 */ new Op(Operation.JE, O.Jb),
            /* 75 */ new Op(Operation.JNE, O.Jb),
            /* 76 */ new Op(Operation.JBE, O.Jb),
            /* 77 */ new Op(Operation.JA, O.Jb),
            /* 78 */ new Op(Operation.JS, O.Jb),
            /* 79 */ new Op(Operation.JNS, O.Jb),
            /* 7A */ new Op(Operation.JP, O.Jb),
            /* 7B */ new Op(Operation.JNP, O.Jb),
            /* 7C */ new Op(Operation.JL, O.Jb),
            /* 7D */ new Op(Operation.JGE, O.Jb),
            /* 7E */ new Op(Operation.JLE, O.Jb),
            /* 7F */ new Op(Operation.JG, O.Jb),

            /* 80 */ new Op(OpcodeExtension.Ext1, O.Eb, O.Ib),
            /* 81 */ new Op(OpcodeExtension.Ext1, O.Ev, O.Iz),
            /* 82 */ new Op(OpcodeExtension.Ext1, O.Eb, O.Ib), /* i64 ??? TBD */
            /* 83 */ new Op(OpcodeExtension.Ext1, O.Ev, O.Ib),
            /* 84 */ new Op(Operation.TEST, O.Eb, O.Gb),
            /* 85 */ new Op(Operation.TEST, O.Ev, O.Gv),
            /* 86 */ new Op(Operation.XCHG, O.Eb, O.Gb),
            /* 87 */ new Op(Operation.XCHG, O.Ev, O.Gv),
            /* 88 */ new Op(Operation.MOV, O.Eb, O.Gb),
            /* 89 */ new Op(Operation.MOV, O.Ev, O.Gv),
            /* 8A */ new Op(Operation.MOV, O.Gb, O.Eb),
            /* 8B */ new Op(Operation.MOV, O.Gv, O.Ev),
            /* 8C */ new Op(Operation.MOV, O.Ev, O.Sw),
            /* 8D */ new Op(Operation.LEA, O.Gv, O.Mp), /* ??? missing TBD */
            /* 8E */ new Op(Operation.MOV, O.Sw, O.Ew),
            /* 8F */ new Op(OpcodeExtension.Ext1A), /* POP(d64) Ev */

            /* 90 */ new Op(Operation.NOP), /* PAUSE (F3), O.XCHG r8, O.rAX */
            /* 91 */ new Op(Operation.XCHG, O.rCX, O.rAX),
            /* 92 */ new Op(Operation.XCHG, O.rDX, O.rAX),
            /* 93 */ new Op(Operation.XCHG, O.rBX, O.rAX),
            /* 94 */ new Op(Operation.XCHG, O.rSP, O.rAX),
            /* 95 */ new Op(Operation.XCHG, O.rBP, O.rAX),
            /* 96 */ new Op(Operation.XCHG, O.rSI, O.rAX),
            /* 97 */ new Op(Operation.XCHG, O.rDI, O.rAX),
            /* 98 */ new Op(Operation.CBW), /* CWDE/CDQE */
            /* 99 */ new Op(Operation.CWD), /* CDQ/CQO */
            /* 9A */ new Op(Operation.CALLF, O.Ap), /* i64 */
            /* 9B */ new Op(Operation.FWAIT), /* WAIT */
            /* 9C */ new Op(Operation.PUSHF, O.Fv), /* PUSHF/D/Q(d64) */
            /* 9D */ new Op(Operation.POPF, O.Fv), /* POPF/D/Q(d64) */
            /* 9E */ new Op(Operation.SAHF),
            /* 9F */ new Op(Operation.LAHF),

            /* A0 */ new Op(Operation.MOV, O.AL, O.Ob),
            /* A1 */ new Op(Operation.MOV, O.rAX, O.Ov),
            /* A2 */ new Op(Operation.MOV, O.Ob, O.AL),
            /* A3 */ new Op(Operation.MOV, O.Ov, O.rAX),
            /* A4 */ new Op(Operation.MOVS, O.Yb, O.Xb), /* MOVS/B */
            /* A5 */ new Op(Operation.MOVS, O.Yv, O.Xv), /* MOVS/W/D/Q */
            /* A6 */ new Op(Operation.CMPS, O.Xb, O.Yb), /* CMPS/B */
            /* A7 */ new Op(Operation.CMPS, O.Xv, O.Yv), /* CMPS/W/D */
            /* A8 */ new Op(Operation.TEST, O.AL, O.Ib),
            /* A9 */ new Op(Operation.TEST, O.rAX, O.Iz),
            /* AA */ new Op(Operation.STOS, O.Yb, O.AL), /* STOS/B */
            /* AB */ new Op(Operation.STOS, O.Yv, O.rAX), /* STOS/W/D/Q */
            /* AC */ new Op(Operation.LODS, O.AL, O.Xb), /* LODS/B */
            /* AD */ new Op(Operation.LODS, O.rAX, O.Xv), /* LODS/W/D/Q */
            /* AE */ new Op(Operation.SCAS, O.AL, O.Yb), /* SCAS/B */ /* TBD */
            /* AF */ new Op(Operation.SCAS, O.rAX, O.Xv), /* SCAS/W/D/Q */ /* TBD */

            /* B0 */ new Op(Operation.MOV, O.AL, O.Ib),
            /* B1 */ new Op(Operation.MOV, O.CL, O.Ib),
            /* B2 */ new Op(Operation.MOV, O.DL, O.Ib),
            /* B3 */ new Op(Operation.MOV, O.BL, O.Ib),
            /* B4 */ new Op(Operation.MOV, O.AH, O.Ib),
            /* B5 */ new Op(Operation.MOV, O.CH, O.Ib),
            /* B6 */ new Op(Operation.MOV, O.DH, O.Ib),
            /* B7 */ new Op(Operation.MOV, O.BH, O.Ib),
            /* B8 */ new Op(Operation.MOV, O.rAX, O.Iv),
            /* B9 */ new Op(Operation.MOV, O.rCX, O.Iv),
            /* BA */ new Op(Operation.MOV, O.rDX, O.Iv),
            /* BB */ new Op(Operation.MOV, O.rBX, O.Iv),
            /* BC */ new Op(Operation.MOV, O.rSP, O.Iv),
            /* BD */ new Op(Operation.MOV, O.rBP, O.Iv),
            /* BE */ new Op(Operation.MOV, O.rSI, O.Iv),
            /* BF */ new Op(Operation.MOV, O.rDI, O.Iv),

            /* C0 */ new Op(OpcodeExtension.Ext2,  O.Eb, O.Ib),
            /* C1 */ new Op(OpcodeExtension.Ext2,  O.Ev, O.Ib),
            /* C2 */ new Op(Operation.RET, O.Iw), /* f64 */
            /* C3 */ new Op(Operation.RET),       /* f64 */
            /* C4 */ new Op(Operation.LES, O.Gz, O.Mp), /* i64; VEX+2byte */
            /* C5 */ new Op(Operation.LDS, O.Gz, O.Mp), /* i64; VEX+1byte */
            /* C6 */ new Op(OpcodeExtension.Ext11, O.Eb, O.Ib),
            /* C7 */ new Op(OpcodeExtension.Ext11, O.Ev, O.Iz),
            /* C8 */ new Op(Operation.ENTER, O.Iw, O.Ib),
            /* C9 */ new Op(Operation.LEAVE), /* d64 */
            /* CA */ new Op(Operation.RETF, O.Iw),
            /* CB */ new Op(Operation.RETF),
            /* CC */ new Op(Operation.INT, O.Imm3),
            /* CD */ new Op(Operation.INT, O.Ib),
            /* CE */ new Op(Operation.INTO), /* i64 */
            /* CF */ new Op(Operation.IRET), /* IRET/D/Q */

            /* D0 */ new Op(OpcodeExtension.Ext2, O.Eb, O.Imm1),
            /* D1 */ new Op(OpcodeExtension.Ext2, O.Ev, O.Imm1),
            /* D2 */ new Op(OpcodeExtension.Ext2, O.Eb, O.CL),
            /* D3 */ new Op(OpcodeExtension.Ext2, O.Ev, O.CL),
            /* D4 */ new Op(Operation.AAM, O.Ib), /* i64 */
            /* D5 */ new Op(Operation.AAD, O.Ib), /* i64 */
            /* D6 */ Op.Empty,
            /* D7 */ new Op(Operation.XLAT, O._XLAT), /* a.k.a XLATB */
            /* D8 */ new Op(OpcodeExtension.Fpu),
            /* D9 */ new Op(OpcodeExtension.Fpu),
            /* DA */ new Op(OpcodeExtension.Fpu),
            /* DB */ new Op(OpcodeExtension.Fpu),
            /* DC */ new Op(OpcodeExtension.Fpu),
            /* DD */ new Op(OpcodeExtension.Fpu),
            /* DE */ new Op(OpcodeExtension.Fpu),
            /* DF */ new Op(OpcodeExtension.Fpu),

            /* E0 */ new Op(Operation.LOOPNZ, O.Jb), /* f64 */
            /* E1 */ new Op(Operation.LOOPZ, O.Jb), /* f64 */
            /* E2 */ new Op(Operation.LOOP, O.Jb), /* f64 */
            /* E3 */ new Op(Operation.JCXZ, O.Jb), /* f64; JrCXZ */
            /* E4 */ new Op(Operation.IN, O.AL, O.Ib),
            /* E5 */ new Op(Operation.IN, O.eAX, O.Ib),
            /* E6 */ new Op(Operation.OUT, O.Ib, O.AL),
            /* E7 */ new Op(Operation.OUT, O.Ib, O.eAX),
            /* E8 */ new Op(Operation.CALL, O.Jz), /* f64 */
            /* E9 */ new Op(Operation.JMP, O.Jz), /* near, O.f64 */
            /* EA */ new Op(Operation.JMPF, O.Ap), /* far, O.i64 */
            /* EB */ new Op(Operation.JMP, O.Jb), /* short, O.f64 */
            /* EC */ new Op(Operation.IN, O.AL, O.DX),
            /* ED */ new Op(Operation.IN, O.eAX, O.DX),
            /* EE */ new Op(Operation.OUT, O.DX, O.AL),
            /* EF */ new Op(Operation.OUT, O.DX, O.eAX),

            /* F0 */ Op.Empty, /* LOCK (prefix) */
            /* F1 */ Op.Empty,
            /* F2 */ Op.Empty, /* REPNE (prefix) */
            /* F3 */ Op.Empty, /* REPE (prefix) */
            /* F4 */ new Op(Operation.HLT),
            /* F5 */ new Op(Operation.CMC),
            /* F6 */ new Op(OpcodeExtension.Ext3, O.Eb),
            /* F7 */ new Op(OpcodeExtension.Ext3, O.Ev),
            /* F8 */ new Op(Operation.CLC),
            /* F9 */ new Op(Operation.STC),
            /* FA */ new Op(Operation.CLI),
            /* FB */ new Op(Operation.STI),
            /* FC */ new Op(Operation.CLD),
            /* FD */ new Op(Operation.STD),
            /* FE */ new Op(OpcodeExtension.Ext4), /* INC/DEC */
            /* FF */ new Op(OpcodeExtension.Ext5)  /* INC/DEC */
        };

        enum OpcodeExtension
        {
            None = 0,
            Ext1,
            Ext1A,
            Ext2,
            Ext3,
            Ext4,
            Ext5,
            Ext6,
            Ext7,
            Ext8,
            Ext11,

            // See table B.17 (page 2163) for instruction format.
            Fpu,
        }
    }

#if false
/**
 * Enumerated values for the type of a value accessible by an instruction.
 * The value may be stored in a register, in a memory location, or as an
 * immediate.
 */
enum x86_value_type
{
    VT_NONE = 0,       /* invalid */
    VT_I8   = 1,       /* signed byte */
    VT_U8   = 2,
    VT_I16  = 3,
    VT_U16  = 4,
    VT_I32  = 5,
    VT_U32  = 6,
    VT_F32  = 0x10,
    VT_F64  = 0x20
};

/**
 * Represents a value that is accessible by an instruction. The value may be 
 * stored in a register, in a memory location, or as an immediate.
 */
typedef struct asm_value_t
{
    enum asm_value_type type;
    union 
    {
        int8_t   i8;   /* signed byte */
        uint8_t  u8;   /* unsigned byte */
        int16_t  i16;  /* signed word */
        uint16_t u16;  /* unsigned word */
        int32_t  i32;  /* signed dword */
        uint32_t u32;  /* unsigned dword */
        float    f32;  /* single-precision */
        double   f64;  /* double-precision */
    } v;
} asm_value_t;

struct x86_operand_t
{
    enum x86_operand_type type; /* reg/imm/mem */
    asm_value_t val;

} x86_operand_t;
#endif

    

    public class DecoderContext
    {
        public CpuMode CpuMode { get; set; }

        /// <summary>
        /// Gets or sets the addressing size of the instruction.
        /// </summary>
        public CpuSize AddressSize { get; set; }

        /// <summary>
        /// Gets or sets the operand size of the instruction.
        /// </summary>
        /// <remarks>
        /// The operand-size attribute determines whether the processor is
        /// performing 16-bit, 32-bit or 64-bit operations. Within the 
        /// constraints of the current operand-size attribute, an instruction
        /// may use the operand-size bit (w) to indicate operations on 8-bit
        /// operands or full-size operands specified with the operand-size 
        /// attribute.
        ///
        /// The operand-size attribute is determined by the cpu's operation
        /// mode and any operand-size override prefix present in an 
        /// instruction.
        /// </remarks>
        public CpuSize OperandSize { get; set; }

        /// <summary>
        /// Gets or sets the segment register to use for the instruction.
        /// </summary>
        public Register SegmentOverride{get;set;}
    }
}

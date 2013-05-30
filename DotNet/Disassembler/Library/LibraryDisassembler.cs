using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Implements a specialized disassembler to analyze object library.
    /// An object library contains additional symbol information, which is
    /// helpful for binary analysis.
    /// </summary>
    public class LibraryDisassembler : DisassemblerBase
    {
        readonly ObjectLibrary library;

        public LibraryDisassembler(ObjectLibrary library)
            : base(library.Image)
        {
            this.library = library;
        }

        public override Assembly Assembly
        {
            get { return library; }
        }

        protected override void GenerateProcedures()
        {
            // Enumerate the defined names, and assign names to the procedures.
            foreach (ObjectModule module in library.Modules)
            {
                foreach (DefinedSymbol symbol in module.DefinedNames)
                {
                    if (symbol.BaseSegment != null)
                    {
                        Address address = new Address(symbol.BaseSegment.Id, (int)symbol.Offset);
                        if (image.IsAddressValid(address))
                        {
                            ByteAttribute b = image[address];
                            if (b.Type == ByteType.Code && b.IsLeadByte)
                            {
                                Procedure proc = Procedures.Find(address);
                                if (proc == null)
                                {
                                    proc = CreateProcedure(address);
                                    Procedures.Add(proc);
                                }
                                proc.Name = symbol.Name;
                            }
                        }
                    }
                }
            }
        }

        protected override Instruction DecodeInstruction(Address address)
        {
            Instruction instruction = base.DecodeInstruction(address);
            if (instruction == null)
                return instruction;

            // Find the first fixup that covers the instruction. If no
            // fix-up covers the instruction, find the closest fix-up
            // that comes after.
            FixupCollection fixups = library.Image.GetSegment(address.Segment).Segment.Fixups;
            int fixupIndex = fixups.BinarySearch(address.Offset);
            if (fixupIndex < 0)
                fixupIndex = ~fixupIndex;

            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (fixupIndex >= fixups.Count) // no more fixups
                    break;

                Fixup fixup = fixups[fixupIndex];
                if (fixup.StartIndex >= address.Offset + instruction.EncodedLength) // past end
                    break;

                Operand operand = instruction.Operands[i];
                if (operand.FixableLocation.Length > 0)
                {
                    int start = address.Offset + operand.FixableLocation.StartOffset;
                    int end = start + operand.FixableLocation.Length;

                    if (fixup.StartIndex >= end)
                        continue;

                    if (fixup.StartIndex != start || fixup.EndIndex != end)
                    {
                        // throw new BrokenFixupException(fixup);
                        if (IsFloatingPointEmulatorFixup(fixup))
                        {
                            AddError(new Address(address.Segment, fixup.StartIndex),
                                ErrorCode.FixupDiscarded,
                                "Floating point emulator fix-up discarded: {0}", fixup);
                        }
                        else
                        {
                            AddError(new Address(address.Segment, fixup.StartIndex),
                                ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                        }
                        continue;
                    }

                    instruction.Operands[i].Tag = fixup.Target;
                    ++fixupIndex;
                }
            }

            if (fixupIndex < fixups.Count)
            {
                Fixup fixup = fixups[fixupIndex];
                if (fixup.StartIndex < address.Offset + instruction.EncodedLength)
                {
                    if (IsFloatingPointEmulatorFixup(fixup))
                    {
                        AddError(new Address(address.Segment, fixup.StartIndex),
                            ErrorCode.FixupDiscarded,
                            "Floating point emulator fix-up discarded: {0}", fixup);
                    }
                    else
                    {
                        AddError(new Address(address.Segment, fixup.StartIndex),
                            ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                    }
                }
            }
            return instruction;
        }

        private bool IsFloatingPointEmulatorFixup(Fixup fixup)
        {
            ExternalSymbol symbol = fixup.Target.Referent as ExternalSymbol;
            if (symbol == null)
                return false;

            switch (symbol.Name)
            {
                case "FIARQQ":
                case "FICRQQ":
                case "FIDRQQ":
                case "FIERQQ":
                case "FISRQQ":
                case "FIWRQQ":
                case "FJARQQ":
                case "FJCRQQ":
                case "FJSRQQ":
                    return true;
                default:
                    return false;
            }
        }

        private Address ResolveSymbolicTarget(SymbolicTarget symbolicTarget)
        {
            Address referentAddress = symbolicTarget.Referent.Resolve();
            if (referentAddress == Address.Invalid)
            {
                //AddError(start, ErrorCode.UnresolvedTarget,
                //    "Cannot resolve target: {0}.", symbolicTarget);
                return Address.Invalid;
            }
            Address symbolicAddress = referentAddress + (int)symbolicTarget.Displacement;
            return symbolicAddress;
        }

        protected override Address ResolveFlowInstructionTarget(RelativeOperand operand)
        {
            SymbolicTarget symbolicTarget = operand.Tag as SymbolicTarget;
            if (symbolicTarget != null)
            {
                Address symbolicAddress = ResolveSymbolicTarget(symbolicTarget);
                if (symbolicAddress != Address.Invalid)
                {
                    Address target = symbolicAddress + operand.Offset.Value;
                    return new Address(target.Segment, (UInt16)target.Offset);
                }
                return Address.Invalid;
            }
            return base.ResolveFlowInstructionTarget(operand);
        }

        protected override Address ResolveFlowInstructionTarget(PointerOperand operand)
        {
            SymbolicTarget symbolicTarget = operand.Tag as SymbolicTarget;
            if (symbolicTarget != null)
            {
                Address symbolicAddress = ResolveSymbolicTarget(symbolicTarget);
                return symbolicAddress;
            }
            return base.ResolveFlowInstructionTarget(operand);
        }

        public override void Analyze()
        {
            foreach (ObjectModule module in library.Modules)
            {
                foreach (DefinedSymbol symbol in module.DefinedNames)
                {
                    if (symbol.BaseSegment == null)
                        continue;
                    if (!symbol.BaseSegment.Class.EndsWith("CODE"))
                        continue;

                    // TODO: do not disassemble if the symbol is obviously
                    // a data item.
                    int iFixup = symbol.BaseSegment.Fixups.BinarySearch((int)symbol.Offset);
                    if (iFixup >= 0 && symbol.BaseSegment.Fixups[iFixup].StartIndex
                        == (int)symbol.Offset) // likely a data item
                    {
                        continue;
                    }

                    Address entryPoint = new Address(
                        symbol.BaseSegment.Id, (int)symbol.Offset);
                    GenerateBasicBlocks(entryPoint);
                }
            }

            GenerateControlFlowGraph();
            GenerateProcedures();
            AddBasicBlocksToProcedures();
        }

#if false
        public static void Disassemble(ObjectLibrary library, Address entryPoint)
        {
            LibraryDisassembler dasm = new LibraryDisassembler(library);
            dasm.Analyze(entryPoint);
        }
#endif
    }
}

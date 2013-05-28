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
        {
            this.library = library;
        }

        public override Assembly Assembly
        {
            get { return library; }
        }

        protected override ImageChunk ResolveSegment(int segmentId)
        {
            return library.GetSegment(segmentId).Image;
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
                        if (symbol.BaseSegment.Image.Bounds.Contains((int)symbol.Offset))
                        {
                            ImageByte b = symbol.BaseSegment.Image[(int)symbol.Offset];
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

        protected override Instruction DecodeInstruction(ImageChunk image, Address address)
        {
            Instruction instruction = base.DecodeInstruction(image, address);
            if (instruction == null)
                return instruction;

            // Find the first fixup that covers the instruction. If no
            // fix-up covers the instruction, find the closest fix-up
            // that comes after.
            int fixupIndex = image.Fixups.BinarySearch(address.Offset);
            if (fixupIndex < 0)
                fixupIndex = ~fixupIndex;

            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (fixupIndex >= image.Fixups.Count) // no more fixups
                    break;

                Fixup fixup = image.Fixups[fixupIndex];
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
                        AddError(new Address(address.Segment, fixup.StartIndex),
                            ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                        continue;
                    }

                    instruction.Operands[i].Tag = fixup.Target;
                    ++fixupIndex;
                }
            }

            if (fixupIndex < image.Fixups.Count)
            {
                Fixup fixup = image.Fixups[fixupIndex];
                if (fixup.StartIndex < address.Offset + instruction.EncodedLength)
                {
                    AddError(new Address(address.Segment, fixup.StartIndex),
                        ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                }
            }
            return instruction;
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

        public void AnalyzeAll()
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

                    Address entryPoint = new Address(
                        symbol.BaseSegment.Id, (int)symbol.Offset);
                    GenerateBasicBlocks(entryPoint);
                }
            }

            GenerateControlFlowGraph();
            GenerateProcedures();
            AddBasicBlocksToProcedures();
        }

        public static void Disassemble(ObjectLibrary library, Address entryPoint)
        {
            LibraryDisassembler dasm = new LibraryDisassembler(library);
            dasm.Analyze(entryPoint);
        }
    }
}

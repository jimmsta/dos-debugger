using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Represents a target (typically a jump target) that is a symbol that
    /// must be resolved at link-time, or an address with a known label.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// mov ax, seg CODE    ; the second operand is the frame number of the
    ///                     ; segment named "CODE"
    /// mov ax, seg DGROUP  ; the second operand is the frame number of the
    ///                     ; group named "DGROUP"
    /// call strcpy         ; the symbol is the entry point offset of an
    ///                     ; subroutine
    /// callf malloc        ; the symbol is the entry point seg:off of a
    ///                     ; far proc
    /// jmp [bx+_Table]     ; the symbol is the offset of a jump table
    /// </remarks>
    public class SymbolicTarget
    {
        /// <summary>
        /// Gets or sets the referent of the target. The target is specified
        /// by referent + displacement.
        /// </summary>
        public IAddressable Referent { get; set; }

        /// <summary>
        /// Gets or sets the displacement of the target relative to the
        /// referent.
        /// </summary>
        public UInt32 Displacement { get; set; }

        public override string ToString()
        {
            return string.Format("{0}+{1}", Referent.Label, Displacement);
        }

#if false
        public override string ToString()
        {
            switch (Method)
            {
                case FixupTargetMethod.Absolute:
                    return string.Format("{0:X4}:{1:X4}", IndexOrFrame, Displacement);
                case FixupTargetMethod.SegmentPlusDisplacement:
                    return string.Format("SEG({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.GroupPlusDisplacement:
                    return string.Format("GRP({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.ExternalPlusDisplacement:
                    return string.Format("EXT({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.SegmentWithoutDisplacement:
                    return string.Format("SEG({0})", IndexOrFrame);
                case FixupTargetMethod.GroupWithoutDisplacement:
                    return string.Format("GRP({0})", IndexOrFrame);
                case FixupTargetMethod.ExternalWithoutDisplacement:
                    return string.Format("EXT({0})", IndexOrFrame);
                default:
                    return "(invalid)";
            }
        }
#endif
    }

    /// <summary>
    /// Represents a relative operand (used in branch/call/jump instructions)
    /// where the target is the offset of a symbol (typicaly defined in the
    /// same segment as the instruction).
    /// </summary>
    public class SymbolicRelativeOperand : Operand
    {
        public SymbolicTarget Target { get; private set; }
        
        public SymbolicRelativeOperand(SymbolicTarget target)
        {
            this.Target = target;
        }

        public override string ToString()
        {
            // We should take an argument to specify whether to return
            // html.
#if true
            return string.Format("<a href=\"somewhere\">{0}</a>", Target.ToString());
#else
            return Target.TargetName;
#endif
        }
    }

    public class SymbolicImmediateOperand { }
    public class SymbolicMemoryOperand { }
    public class SymbolicPointerOperand { }

#if false
    /// <summary>
    /// Represents an instruction with some fields replaced with a symbol,
    /// e.g. 
    /// CALL _strcpy
    /// MOV DX, seg DGROUP
    /// JMP [BX + off Table]
    /// </summary>
    public class SymbolicInstruction
    {
        public Instruction instruction { get; private set; }
        public string[] operandText;

        public SymbolicInstruction(Instruction instruction)
        {
            this.instruction = instruction;
            this.operandText = new string[instruction.Operands.Length];
        }

        /// <summary>
        /// Converts the instruction to a string in Intel syntax.
        /// </summary>
        /// <returns>The formatted instruction.</returns>
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            // Write address.
            //s.Append(instruction.Location.ToString());
            //s.Append("  ");

            // Format group 1 (LOCK/REPZ/REPNZ) prefix.
            if ((instruction.Prefix & Prefixes.Group1) != 0)
            {
                s.Append((instruction.Prefix & Prefixes.Group1).ToString());
                s.Append(' ');
            }

            // Format mnemonic.
            s.Append(instruction.Operation.ToString());

            // Format operands.
            for (int i = 0; i < instruction.Operands.Length; i++)
            {
                if (i > 0)
                {
                    s.Append(',');
                }
                s.Append(' ');
                s.Append(FormatOperand(i));
            }
            return s.ToString().ToLowerInvariant();
        }

        private string FormatOperand(int i)
        {
            var operand = instruction.Operands[i];
            if (operandText[i] != null)
                return operandText[i];

            return operand.ToString();
        }
    }
#endif
}

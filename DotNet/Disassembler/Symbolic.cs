using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;
using Disassembler.Omf;

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
        public SymbolicTargetType TargetType { get; set; }
        public string TargetName { get; set; }
        public UInt16 Displacement { get; set; }

        public SymbolicTarget(FixupDefinition fixup, ObjectModule module)
        {
            if (module == null)
                throw new ArgumentNullException("module");

            switch (fixup.Target.Method)
            {
                case FixupTargetMethod.ExternalPlusDisplacement:
                case FixupTargetMethod.ExternalWithoutDisplacement:
                    CreateFromExternal(fixup, module);
                    break;
            }
        }

        private void CreateFromExternal(FixupDefinition fixup, ObjectModule module)
        {
            var extIndex = fixup.Target.IndexOrFrame;
            var extName = module.ExternalNames[extIndex - 1];
            var disp = fixup.Target.Displacement;

            this.TargetType = SymbolicTargetType.ExternalName;
            this.TargetName = extName.Name;
            this.Displacement = (UInt16)disp;
        }

        public override string ToString()
        {
            return TargetName;
        }
    }

    public enum SymbolicTargetType
    {
        None,
        ExternalName,
        Segment,
        Group
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
            return string.Format("<a href=\"somewhere\">{0}</a>", Target.TargetName);
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

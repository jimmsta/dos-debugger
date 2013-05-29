using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    /// <summary>
    /// Specifies the features of a block of code (e.g. instruction, basic
    /// block, procedure, or program), which can be used to provide a hint
    /// for its behavior.
    /// </summary>
    /// <remarks>
    /// For example, if a procedure contains an "INT 21h" instruction, then
    /// this procedure likely interacts with the OS directly; it then follows
    /// that this procedure is more likely a library function than a user
    /// function.
    /// </remarks>
    [Flags]
    public enum CodeFeatures
    {
        None = 0,
        HasInterrupt = 1,
        HasFpu = 2,
        HasRETN = 4,
        HasRETF = 8,
        HasIRET = 0x10,
    }

    public static class CodeFeaturesHelper // may rename to CodeAnalyzer, or
                                          // make partial class Disassembler
    {
        public static CodeFeatures GetFeatures(Instruction instruction)
        {
            CodeFeatures features = CodeFeatures.None;
            switch (instruction.Operation)
            {
                case Operation.INT:
                case Operation.INTO:
                    features |= CodeFeatures.HasInterrupt;
                    break;
                case Operation.RET:
                    features |= CodeFeatures.HasRETN;
                    break;
                case Operation.RETF:
                    features |= CodeFeatures.HasRETF;
                    break;
                case Operation.IRET:
                    features |= CodeFeatures.HasIRET;
                    break;
                case Operation.FCLEX:
                    features |= CodeFeatures.HasFpu;
                    break;
            }
            return features;
        }

        public static CodeFeatures GetFeatures(IEnumerable<Instruction> instructions)
        {
            CodeFeatures features= CodeFeatures.None;
            foreach (Instruction instruction in instructions)
            {
                features |= GetFeatures(instruction);
            }
            return features;
        }
    }
}

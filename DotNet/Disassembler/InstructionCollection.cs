using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
#if false
    /// <summary>
    /// Contains the analysis results and related information about a binary
    /// image. The information include:
    /// 
    /// - byte attributes (code/data/padding)
    /// - instructions
    /// - basic blocks
    /// - procedures
    /// - analysis errors
    /// 
    /// The disassembler should ideally support generating results
    /// incrementally.
    /// 
    /// Note that segmentation information is supplied by the underlying
    /// BinaryImage object directly and is not treated as analysis results,
    /// though in reality the segmentation of an executable is indeed guessed
    /// from the analysis.
    /// </summary>
#endif

    /// <summary>
    /// Maintains a collection of instructions and provides methods to
    /// quickly retrieve the instruction starting at a given address.
    /// </summary>
    public class InstructionCollection
    {
        readonly Dictionary<Address, Instruction> instructions =
            new Dictionary<Address, Instruction>();

        readonly BinaryImage image;

        public InstructionCollection(BinaryImage image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this.image = image;
        }

        public void Add(Address address, Instruction instruction)
        {
            if (!image.IsAddressValid(address))
                throw new ArgumentOutOfRangeException("address");
            if (instruction == null)
                throw new ArgumentNullException("instruction");

            this.instructions.Add(address, instruction);
        }

        public Instruction Find(Address address)
        {
            return instructions[address];
        }
    }
}

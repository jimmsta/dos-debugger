using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
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
    public class AnalysisResults // may rename to AnalyzedImage
    {
        //ByteAttribute[] attrs;
        readonly BinaryImage image;
        readonly InstructionCollection instructions;
        readonly XRefCollection crossReferences = new XRefCollection();
        readonly BasicBlockCollection basicBlocks = new BasicBlockCollection();
        readonly ProcedureCollection procedures = new ProcedureCollection();
        readonly ErrorCollection errors = new ErrorCollection();

        public AnalysisResults(BinaryImage image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            this.image = image;
            this.instructions = new InstructionCollection(image);
        }

        public BinaryImage Image
        {
            get { return image; }
        }

        public InstructionCollection Instructions
        {
            get { return instructions; }
        }

        public XRefCollection CrossReferences
        {
            get { return crossReferences; }
        }

        public BasicBlockCollection BasicBlocks
        {
            get { return basicBlocks; }
        }

        public ProcedureCollection Procedures
        {
            get { return procedures; }
        }

        public ErrorCollection Errors
        {
            get { return errors; }
        }
    }

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

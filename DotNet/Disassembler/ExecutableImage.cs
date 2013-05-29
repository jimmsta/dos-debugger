using System;
using System.Collections.Generic;
using System.Text;
using X86Codec;

namespace Disassembler
{
    public class ExecutableImage : BinaryImage
    {
        readonly byte[] bytes;
        readonly ByteAttribute[] attrs;
        readonly Dictionary<Address, Instruction> instructions;

        public ExecutableImage(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            this.bytes = bytes;
            this.attrs = new ByteAttribute[bytes.Length];
            this.instructions = new Dictionary<Address, Instruction>();
        }

        public int Length
        {
            get { return bytes.Length; }
        }

        private static int ToLinearAddress(Address address)
        {
            return address.Segment * 16 + address.Offset;
        }

        public override bool IsAddressValid(Address address)
        {
            int index = ToLinearAddress(address);
            return (index >= 0) && (index < bytes.Length);
        }

        protected override ByteAttribute GetByteAttribute(Address address)
        {
            return attrs[ToLinearAddress(address)];
        }

        protected override void SetByteAttribute(Address address, ByteAttribute attr)
        {
            attrs[ToLinearAddress(address)] = attr;
        }

        public byte[] Data
        {
            get { return bytes; }
        }

        public override ArraySegment<byte> GetBytes(Address address, int count)
        {
            // TODO: we need to maintain the segments here.

            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, count);
        }

        public override ArraySegment<byte> GetBytes(Address address)
        {
            int index = ToLinearAddress(address);
            return new ArraySegment<byte>(bytes, index, bytes.Length - index);
        }

        public override Instruction GetInstruction(Address address)
        {
            return instructions[address];
        }

        public override void SetInstruction(Address address, Instruction instruction)
        {
            instructions[address] = instruction;
        }
    }
}

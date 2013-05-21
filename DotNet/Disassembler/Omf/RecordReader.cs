using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Disassembler.Omf
{
    /// <summary>
    /// Provides methods to read the fields of an OMF record.
    /// </summary>
    internal class RecordReader
    {
        public RecordNumber RecordNumber { get; private set; }
        public byte[] Data { get; private set; }
        public byte Checksum { get; private set; }
        public int Position { get; private set; }

        private int index; // currentIndex

        public RecordReader(BinaryReader reader)
        {
            this.Position = (int)reader.BaseStream.Position;
            this.RecordNumber = (RecordNumber)reader.ReadByte();

            int recordLength = reader.ReadUInt16();
            if (recordLength == 0)
                throw new InvalidDataException("RecordLength must be greater than zero.");

            this.Data = reader.ReadBytes(recordLength - 1);
            if (Data.Length != recordLength - 1)
                throw new EndOfStreamException("Cannot read enough bytes.");

            this.Checksum = reader.ReadByte();
        }

        public bool IsEOF
        {
            get { return index == Data.Length; }
        }

        public byte PeekByte()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();
            return Data[index];
        }

        public byte ReadByte()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();
            return Data[index++];
        }

        public byte[] ReadToEnd()
        {
            byte[] remaining = new byte[Data.Length - index];
            Array.Copy(Data, index, remaining, 0, remaining.Length);
            index = Data.Length;
            return remaining;
        }

        public string ReadToEndAsString()
        {
            string s = Encoding.ASCII.GetString(Data, index, Data.Length - index);
            index = Data.Length;
            return s;
        }

        public UInt16 ReadUInt16()
        {
            if (index + 2 > Data.Length)
                throw new InvalidDataException();
            byte b1 = Data[index++];
            byte b2 = Data[index++];
            return (UInt16)(b1 | (b2 << 8));
        }

        public UInt16 ReadUInt24()
        {
            if (index + 3 > Data.Length)
                throw new InvalidDataException();
            byte b1 = Data[index++];
            byte b2 = Data[index++];
            byte b3 = Data[index++];
            return (UInt16)(b1 | (b2 << 8) | (b3 << 16));
        }

        public UInt32 ReadUInt32()
        {
            if (index + 4 > Data.Length)
                throw new InvalidDataException();
            byte b1 = Data[index++];
            byte b2 = Data[index++];
            byte b3 = Data[index++];
            byte b4 = Data[index++];
            return (UInt32)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
        }

        /// <summary>
        /// Reads UInt16 if the record number is even, or UInt32 if the
        /// record number is odd.
        /// </summary>
        /// <returns></returns>
        public UInt32 ReadUInt16Or32()
        {
            if (((int)RecordNumber & 1) == 0)
                return ReadUInt16();
            else
                return ReadUInt32();
        }

        /// <summary>
        /// Reads a string encoded as an 8-bit unsigned 'count' followed by
        /// 'count' bytes of string data.
        /// </summary>
        public string ReadPrefixedString()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();

            byte len = Data[index++];
            if (len == 0)
                return "";

            if (index + len > Data.Length)
                throw new InvalidDataException();

            string s = Encoding.ASCII.GetString(Data, index, len);
            index += len;
            return s;
        }

        /// <summary>
        /// Reads an index in the range [0, 0x7FFF], encoded by 1 or 2 bytes.
        /// </summary>
        public UInt16 ReadIndex()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();

            byte b1 = Data[index++];
            if ((b1 & 0x80) == 0)
            {
                return b1;
            }
            else
            {
                if (index >= Data.Length)
                    throw new InvalidDataException();

                byte b2 = Data[index++];
                return (UInt16)(((b1 & 0x7F) << 8) | b2);
            }
        }
    }
}

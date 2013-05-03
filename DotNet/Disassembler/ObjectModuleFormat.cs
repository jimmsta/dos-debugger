using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace Disassembler.Omf
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OmfLoader
    {
        private OmfRecord[] records;

        public OmfRecord[] Records
        {
            //get { return new ReadOnlyCollection<OmfRecord>(records); }
            get { return records; }
        }

        public OmfLoader(string fileName)
        {
            List<OmfRecord> records = new List<OmfRecord>();

            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Read records.
                while (stream.Position < stream.Length)
                {
                    OmfRecord record = OmfRecord.ReadRecord(reader);
                    records.Add(record);
                    if (record.RecordNumber == OmfRecordNumber.LibraryEnd)
                        break;
                }
                this.records = records.ToArray();

                // Read dictionary.
            }
        }
    }

    public enum OmfRecordNumber : byte
    {
        /// <summary>Translator Header Record</summary>
        THEADR = 0x80,

        /// <summary>Library Module Header Record</summary>
        LHEADR = 0x82,

        /// <summary>Comment Record (with extensions)</summary>
        COMENT = 0x88,

        /// <summary>Module End Record (16-bit)</summary>
        MODEND16 = 0x8A,

        /// <summary>Module End Record (32-bit)</summary>
        MODEND32 = 0x8B,

        /// <summary>External Names Definition Record</summary>
        EXTDEF = 0x8C,

        /// <summary>Public Names Definition Record (16-bit)</summary>
        PUBDEF16 = 0x90,

        /// <summary>Public Names Definition Record (32-bit)</summary>
        PUBDEF32 = 0x91,

        /// <summary>Line Numbers Record (16-bit)</summary>
        LINNUM16 = 0x94,

        /// <summary>Line Numbers Record (32-bit)</summary>
        LINNUM32 = 0x95,

        /// <summary>List of Names Record</summary>
        LNAMES = 0x96,

        /// <summary>Segment Definition Record (16-bit)</summary>
        SEGDEF16 = 0x98,

        /// <summary>Segment Definition Record (32-bit)</summary>
        SEGDEF32 = 0x99,

        /// <summary>Group Definition Record</summary>
        GRPDEF = 0x9A,

        /// <summary>Fixup Record (16-bit)</summary>
        FIXUPP16 = 0x9C,

        /// <summary>Fixup Record (32-bit)</summary>
        FIXUPP32 = 0x9D,

        /// <summary>Logical Enumerated Data Record (16-bit)</summary>
        LEDATA16 = 0xA0,

        /// <summary>Logical Enumerated Data Record (32-bit)</summary>
        LEDATA32 = 0xA1,

        /// <summary>Logical Iterated Data Record (16-bit)</summary>
        LIDATA16 = 0xA2,

        /// <summary>Logical Iterated Data Record (32-bit)</summary>
        LIDATA32 = 0xA3,

        /// <summary>Communal Names Definition Record</summary>
        COMDEF = 0xB0,

        /// <summary>Backpatch Record (16-bit)</summary>
        BAKPAT16 = 0xB2,

        /// <summary>Backpatch Record (32-bit)</summary>
        BAKPAT32 = 0xB3,

        /// <summary>Local External Names Definition Record</summary>
        LEXTDEF = 0xB4,

        /// <summary>Local Public Names Definition Record (16-bit)</summary>
        LPUBDEF16 = 0xB6,

        /// <summary>Local Public Names Definition Record (32-bit)</summary>
        LPUBDEF32 = 0xB7,

        /// <summary>Local Communal Names Definition Record</summary>
        LCOMDEF = 0xB8,

        /// <summary>COMDAT External Names Definition Record</summary>
        CEXTDEF = 0xBC,

        /// <summary>Initialized Communal Data Record (16-bit)</summary>
        COMDAT16 = 0xC2,

        /// <summary>Initialized Communal Data Record (32-bit)</summary>
        COMDAT32 = 0xC3,

        /// <summary>Symbol Line Numbers Record (16-bit)</summary>
        LINSYM16 = 0xC4,

        /// <summary>Symbol Line Numbers Record (32-bit)</summary>
        LINSYM32 = 0xC5,

        /// <summary>Alias Definition Record</summary>
        ALIAS = 0xC6,

        /// <summary>Named Backpatch Record (16-bit)</summary>
        NBKPAT16 = 0xC8,

        /// <summary>Named Backpatch Record (32-bit)</summary>
        NBKPAT32 = 0xC9,

        /// <summary>Local Logical Names Definition Record</summary>
        LLNAMES = 0xCA,

        /// <summary>OMF Version Number Record</summary>
        VERNUM = 0xCC,

        /// <summary>Vendor-specific OMF Extension Record</summary>
        VENDEXT = 0xCE,

        /// <summary>Library Header Record</summary>
        LibraryHeader = 0xF0,

        /// <summary>Library End Record</summary>
        LibraryEnd = 0xF1,
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OmfRecord
    {
        public OmfRecordNumber RecordNumber { get; private set; }
        public int RecordLength { get; private set; }
        public byte[] Data { get; private set; }
        public byte Checksum { get; private set; }
        public int Position { get; private set; }

        private int index; // currentIndex

        public OmfRecord(OmfRecordNumber recordNumber, BinaryReader reader)
        {
            this.Position = (int)reader.BaseStream.Position - 1;
            this.RecordNumber = recordNumber;
            this.RecordLength = reader.ReadUInt16();
            if (this.RecordLength == 0)
                throw new InvalidDataException("RecordLength must be greater than zero.");

            this.Data = reader.ReadBytes(this.RecordLength - 1);
            if (Data.Length != this.RecordLength - 1)
                throw new EndOfStreamException("Cannot read enough bytes.");

            this.Checksum = reader.ReadByte();
        }

        /// <summary>
        /// Reads a string encoded as an 8-bit unsigned 'count' followed by
        /// 'count' bytes of string data.
        /// </summary>
        protected string ReadString()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();

            byte len = Data[index++];
            if (len == 0)
                return "";

            if (index + len > Data.Length)
                throw new InvalidDataException();

            string name = Encoding.ASCII.GetString(Data, index, len);
            index += len;
            return name;
        }

        /// <summary>
        /// Reads an index in the range [0, 0x7FFF], encoded by 1 or 2 bytes.
        /// </summary>
        /// <remarks>
        /// An index is used to reference an item in an ordered collection.
        /// The following ordered collections are defined:
        /// The ordered collections are:
        /// 
        /// Names => LNAMES records and names within each.
        /// Logical Segments => SEGDEF records
        /// Groups => GRPDEF records
        /// Symbols => COMDEF, LCOMDEF, EXTDEF, LEXTDEF, and CEXTDEF records
        ///            and symbols within each.
        /// </remarks>
        /// <returns></returns>
        protected int ReadIndex()
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
                return ((b1 & 0x7F) << 8) | b2;
            }
        }

        public static OmfRecord ReadRecord(BinaryReader reader)
        {
            OmfRecordNumber recordNumber = (OmfRecordNumber)reader.ReadByte();
            switch (recordNumber)
            {
                case OmfRecordNumber.THEADR:
                    return new TranslatorHeaderRecord(reader);
                case OmfRecordNumber.MODEND16:
                    // This is the last record of an object module. Since
                    // a LIB file consists of multiple object modules aligned
                    // on 16-byte boundaries, we need to consume the padding
                    // bytes if any.
                    {
                        OmfRecord r = new OmfRecord(recordNumber, reader);
                        int mod = (int)(reader.BaseStream.Position % 16);
                        if (mod != 0)
                        {
                            reader.ReadBytes(16 - mod);
                        }
                        return r;
                    }
                default:
                    return new OmfRecord(recordNumber, reader);
            }
        }

        public override string ToString()
        {
            return this.RecordNumber.ToString();
        }
    }

    /// <summary>
    /// Contains the name of the object module.
    /// </summary>
    public class TranslatorHeaderRecord : OmfRecord
    {
        public string Name { get; set; }

        public TranslatorHeaderRecord(BinaryReader reader)
            : base(OmfRecordNumber.THEADR, reader)
        {
            this.Name = ReadString();
        }
    }
}

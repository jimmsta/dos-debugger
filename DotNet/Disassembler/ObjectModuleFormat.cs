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
                OmfContext context = new OmfContext { Reader = reader };

                // Read records.
                while (stream.Position < stream.Length)
                {
                    OmfRecord record = OmfRecord.ReadRecord(context);
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
        MODEND = 0x8A,

        /// <summary>Module End Record (32-bit)</summary>
        MODEND32 = 0x8B,

        /// <summary>External Names Definition Record</summary>
        EXTDEF = 0x8C,

        /// <summary>Public Names Definition Record (16-bit)</summary>
        PUBDEF = 0x90,

        /// <summary>Public Names Definition Record (32-bit)</summary>
        PUBDEF32 = 0x91,

        /// <summary>Line Numbers Record (16-bit)</summary>
        LINNUM = 0x94,

        /// <summary>Line Numbers Record (32-bit)</summary>
        LINNUM32 = 0x95,

        /// <summary>List of Names Record</summary>
        LNAMES = 0x96,

        /// <summary>Segment Definition Record (16-bit)</summary>
        SEGDEF = 0x98,

        /// <summary>Segment Definition Record (32-bit)</summary>
        SEGDEF32 = 0x99,

        /// <summary>Group Definition Record</summary>
        GRPDEF = 0x9A,

        /// <summary>Fixup Record (16-bit)</summary>
        FIXUPP = 0x9C,

        /// <summary>Fixup Record (32-bit)</summary>
        FIXUPP32 = 0x9D,

        /// <summary>Logical Enumerated Data Record (16-bit)</summary>
        LEDATA = 0xA0,

        /// <summary>Logical Enumerated Data Record (32-bit)</summary>
        LEDATA32 = 0xA1,

        /// <summary>Logical Iterated Data Record (16-bit)</summary>
        LIDATA = 0xA2,

        /// <summary>Logical Iterated Data Record (32-bit)</summary>
        LIDATA32 = 0xA3,

        /// <summary>Communal Names Definition Record</summary>
        COMDEF = 0xB0,

        /// <summary>Backpatch Record (16-bit)</summary>
        BAKPAT = 0xB2,

        /// <summary>Backpatch Record (32-bit)</summary>
        BAKPAT32 = 0xB3,

        /// <summary>Local External Names Definition Record</summary>
        LEXTDEF = 0xB4,

        /// <summary>Local Public Names Definition Record (16-bit)</summary>
        LPUBDEF = 0xB6,

        /// <summary>Local Public Names Definition Record (32-bit)</summary>
        LPUBDEF32 = 0xB7,

        /// <summary>Local Communal Names Definition Record</summary>
        LCOMDEF = 0xB8,

        /// <summary>COMDAT External Names Definition Record</summary>
        CEXTDEF = 0xBC,

        /// <summary>Initialized Communal Data Record (16-bit)</summary>
        COMDAT = 0xC2,

        /// <summary>Initialized Communal Data Record (32-bit)</summary>
        COMDAT32 = 0xC3,

        /// <summary>Symbol Line Numbers Record (16-bit)</summary>
        LINSYM = 0xC4,

        /// <summary>Symbol Line Numbers Record (32-bit)</summary>
        LINSYM32 = 0xC5,

        /// <summary>Alias Definition Record</summary>
        ALIAS = 0xC6,

        /// <summary>Named Backpatch Record (16-bit)</summary>
        NBKPAT = 0xC8,

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
        [Browsable(false)]
        public OmfRecordNumber RecordNumber { get; private set; }

        [Browsable(false)]
        public int RecordLength { get; private set; }

        [Browsable(false)]
        public byte[] Data { get; private set; }

        [Browsable(false)]
        public byte Checksum { get; private set; }

        [Browsable(false)]
        [Description("Offset of the record relative to the beginning of the file.")]
        public int Position { get; private set; }

        private int index; // currentIndex

        internal OmfRecord(OmfContext context)
        {
            BinaryReader reader = context.Reader;

            this.Position = (int)reader.BaseStream.Position - 1;
            this.RecordNumber = context.RecordNumber;
            this.RecordLength = reader.ReadUInt16();
            if (this.RecordLength == 0)
                throw new InvalidDataException("RecordLength must be greater than zero.");

            this.Data = reader.ReadBytes(this.RecordLength - 1);
            if (Data.Length != this.RecordLength - 1)
                throw new EndOfStreamException("Cannot read enough bytes.");

            this.Checksum = reader.ReadByte();
        }

        protected bool IsEOF
        {
            get { return index == Data.Length; }
        }

        /// <summary>
        /// Reads a string encoded as an 8-bit unsigned 'count' followed by
        /// 'count' bytes of string data.
        /// </summary>
        protected string ReadPrefixedString()
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

        protected byte ReadByte()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();
            return Data[index++];
        }

        protected UInt16 ReadUInt16()
        {
            if (index + 2 > Data.Length)
                throw new InvalidDataException();
            byte b1 = Data[index++];
            byte b2 = Data[index++];
            return (UInt16)(b1 | (b2 << 8));
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

        internal static OmfRecord ReadRecord(OmfContext context)
        {
            BinaryReader reader = context.Reader;
            OmfRecordNumber recordNumber = (OmfRecordNumber)reader.ReadByte();
            context.RecordNumber = recordNumber;
            switch (recordNumber)
            {
                case OmfRecordNumber.COMENT:
                    return new CommentRecord(context);
                case OmfRecordNumber.LHEADR:
                    return new LibraryModuleHeaderRecord(context);
                case OmfRecordNumber.LNAMES:
                    return new ListOfNamesRecord(context);
                case OmfRecordNumber.MODEND:
                    // This is the last record of an object module. Since
                    // a LIB file consists of multiple object modules aligned
                    // on 16-byte boundaries, we need to consume the padding
                    // bytes if any.
                    {
                        OmfRecord r = new ModuleEndRecord(context);
                        int mod = (int)(reader.BaseStream.Position % 16);
                        if (mod != 0)
                        {
                            reader.ReadBytes(16 - mod);
                        }
                        return r;
                    }
                case OmfRecordNumber.SEGDEF:
                    return new SegmentDefinitionRecord(context);
                case OmfRecordNumber.THEADR:
                    return new TranslatorHeaderRecord(context);
                default:
                    return new OmfRecord(context);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} @ {1:X}", this.RecordNumber, this.Position);
        }
    }

    internal class OmfContext
    {
        public BinaryReader Reader;
        public List<string> Names = new List<string>();
        public OmfRecordNumber RecordNumber;
    }

    /// <summary>
    /// Contains the name of the object module.
    /// </summary>
    public class TranslatorHeaderRecord : OmfRecord
    {
        public string Name { get; private set; }

        internal TranslatorHeaderRecord(OmfContext context)
            : base(context)
        {
            this.Name = ReadPrefixedString();
        }
    }

    /// <summary>
    /// Contains the name of a module within a library file.
    /// </summary>
    public class LibraryModuleHeaderRecord : OmfRecord
    {
        public string Name { get; private set; }

        internal LibraryModuleHeaderRecord(OmfContext context)
            : base(context)
        {
            this.Name = ReadPrefixedString();
        }
    }

    public class CommentRecord : OmfRecord
    {
        public bool IsPreserved { get; private set; }
        public bool IsHidden { get; private set; }

        internal CommentRecord(OmfContext context)
            : base(context)
        {
            byte commentType = ReadByte();
            this.IsPreserved = (commentType & 0x80) != 0;
            this.IsHidden = (commentType & 0x40) != 0;

            byte commentClass = ReadByte();
            // TODO: complete the subtypes...
        }
    }

    /// <summary>
    /// Denotes the end of an object module.
    /// </summary>
    public class ModuleEndRecord : OmfRecord
    {
        public bool IsMainModule { get; private set; }
        public bool HasStartAddress { get; private set; }
        public bool IsStartAddressRelocatable { get; private set; }

        internal ModuleEndRecord(OmfContext context)
            : base(context)
        {
            byte type = ReadByte();
            this.IsMainModule = (type & 0x80) != 0;
            this.HasStartAddress = (type & 0x40) != 0;
            this.IsStartAddressRelocatable = (type & 0x01) != 0;
        }
    }

    /// <summary>
    /// Defines a list of names that can be referenced by subsequent records
    /// in the object module.
    /// </summary>
    public class ListOfNamesRecord : OmfRecord
    {
        public string[] Names { get; private set; }

        internal ListOfNamesRecord(OmfContext context)
            : base(context)
        {
            List<string> names = new List<string>();
            while (!IsEOF)
            {
                names.Add(ReadPrefixedString());
            }
            this.Names = names.ToArray();
            context.Names.AddRange(Names);
        }
    }

    public class SegmentDefinitionRecord : OmfRecord
    {
        public SegmentAlignment Alignment { get; private set; }
        public SegmentCombination Combination { get; private set; }
        public UInt16 SegmentAddress { get; private set; }
        public byte SegmentOffset { get; private set; }

        public int Length { get; private set; }
        public string Name { get; private set; }
        public string Class { get; private set; }

        [Browsable(false)]
        public int SegmentNameIndex { get; private set; }

        [Browsable(false)]
        public int ClassNameIndex { get; private set; }

        [Browsable(false)]
        public int OverlayNameIndex { get; private set; }

        private bool IsBig; // highest bit of SegmentLength
        private bool Use32;

        internal SegmentDefinitionRecord(OmfContext context)
            : base(context)
        {
            byte acbp = ReadByte();
            this.Alignment = (SegmentAlignment)(acbp >> 5);
            this.Combination = (SegmentCombination)((acbp >> 2) & 7);
            this.IsBig = (acbp & 0x02) != 0;
            this.Use32 = (acbp & 0x01) != 0;

            if (this.Alignment == SegmentAlignment.Absolute)
            {
                this.SegmentAddress = ReadUInt16();
                this.SegmentOffset = ReadByte();
            }

            this.Length = ReadUInt16();
            if (IsBig)
                this.Length += 0x10000;

            this.SegmentNameIndex = ReadIndex();
            this.ClassNameIndex = ReadIndex();
            this.OverlayNameIndex = ReadIndex();

            if (this.SegmentNameIndex > context.Names.Count ||
                this.ClassNameIndex > context.Names.Count ||
                this.OverlayNameIndex > context.Names.Count)
                throw new InvalidDataException();

            if (SegmentNameIndex > 0)
                this.Name = context.Names[SegmentNameIndex - 1];
            if (ClassNameIndex > 0)
                this.Class = context.Names[ClassNameIndex - 1];
        }
    }

    public enum SegmentAlignment : byte
    {
        Absolute = 0,
        ByteAligned = 1,
        WordAligned = 2,
        ParagraphAligned = 3,
        PageAligned = 4,
        DWordAligned = 5,
    }

    public enum SegmentCombination : byte
    {
        /// <summary>Do not combine with any other program segment.</summary>
        Private = 0,

        /// <summary>
        /// Combine by appending at an offset that meets the alignment
        /// requirement.
        /// </summary>
        Public = 2,

        /// <summary>Same as Public.</summary>
        Public2 = 4,

        /// <summary>Combine by appending at a byte-aligned offset.</summary>
        Stack = 5,

        /// <summary>Combine by overlay using maximum size.</summary>
        Common = 6,

        /// <summary>Same as Public.</summary>
        Public3 = 7,
    }

}

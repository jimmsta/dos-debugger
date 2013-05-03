using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Disassembler.Omf
{
    public enum RecordNumber : byte
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

        public byte ReadByte()
        {
            if (index >= Data.Length)
                throw new InvalidDataException();
            return Data[index++];
        }

        public UInt16 ReadUInt16()
        {
            if (index + 2 > Data.Length)
                throw new InvalidDataException();
            byte b1 = Data[index++];
            byte b2 = Data[index++];
            return (UInt16)(b1 | (b2 << 8));
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

    internal class RecordContext
    {
        public readonly List<string> Names = new List<string>();
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class Record
    {
        [Browsable(false)]
        public RecordNumber RecordNumber { get; private set; }

        /// <summary>
        /// Gets the position of the record expressed as a byte offset to the
        /// beginning of the OBJ or LIB file.
        /// </summary>
        [Browsable(false)]
        public int Position { get; private set; }

        internal Record(RecordReader reader, RecordContext context)
        {
            this.Position = reader.Position;
            this.RecordNumber = reader.RecordNumber;
        }

        internal static Record ReadRecord(BinaryReader binaryReader, RecordContext context)
        {
            RecordReader reader = new RecordReader(binaryReader);
            Record r;
            switch (reader.RecordNumber)
            {
                case RecordNumber.LibraryHeader:
                    r = new LibraryHeaderRecord(reader, context);
                    break;
                case RecordNumber.COMENT:
                    r = new CommentRecord(reader, context);
                    break;
                case RecordNumber.EXTDEF:
                    r = new ExternalNamesDefinitionRecord(reader, context);
                    break;
                case RecordNumber.GRPDEF:
                    r = new GroupDefinitionRecord(reader, context);
                    break;
                case RecordNumber.LHEADR:
                    r = new LibraryModuleHeaderRecord(reader, context);
                    break;
                case RecordNumber.LNAMES:
                    r = new ListOfNamesRecord(reader, context);
                    break;
                case RecordNumber.MODEND:
                    r = new ModuleEndRecord(reader, context);
                    break;
                case RecordNumber.PUBDEF:
                    r = new PublicNameDefinitionRecord(reader, context);
                    break;
                case RecordNumber.SEGDEF:
                    r = new SegmentDefinitionRecord(reader, context);
                    break;
                case RecordNumber.THEADR:
                    r = new TranslatorHeaderRecord(reader, context);
                    break;
                default:
                    r = new UnknownRecord(reader, context);
                    break;
            }

            // TODO: check all bytes are consumed.
            return r;
        }

        public override string ToString()
        {
            return string.Format("{0} @ {1:X}", RecordNumber, Position);
        }
    }

    public class UnknownRecord : Record
    {
        public byte[] Data { get; private set; }

        internal UnknownRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Data = reader.Data;
        }

        public override string ToString()
        {
            return string.Format("? {0} @ {1:X}", RecordNumber, Position);
        }
    }

    public class LibraryHeaderRecord : Record
    {
        public int PageSize { get; private set; }

        internal LibraryHeaderRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.PageSize = reader.Data.Length + 4;

            // Record data consists of 7 bytes of dictionary information
            // (which we ignore), followed by padding bytes to make the next
            // record (which should be THEADR) aligned on page boundary.
        }
    }

    /// <summary>
    /// Contains the name of the object module.
    /// </summary>
    public class TranslatorHeaderRecord : Record
    {
        public string Name { get; private set; }

        internal TranslatorHeaderRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Name = reader.ReadPrefixedString();
        }
    }

    /// <summary>
    /// Contains the name of a module within a library file.
    /// </summary>
    public class LibraryModuleHeaderRecord : Record
    {
        public string Name { get; private set; }

        internal LibraryModuleHeaderRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Name = reader.ReadPrefixedString();
        }
    }

    public class CommentRecord : Record
    {
        public bool IsPreserved { get; private set; }
        public bool IsHidden { get; private set; }

        internal CommentRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            byte commentType = reader.ReadByte();
            this.IsPreserved = (commentType & 0x80) != 0;
            this.IsHidden = (commentType & 0x40) != 0;

            byte commentClass = reader.ReadByte();
            // TODO: complete the subtypes...
        }
    }

    /// <summary>
    /// Denotes the end of an object module.
    /// </summary>
    public class ModuleEndRecord : Record
    {
        public bool IsMainModule { get; private set; }
        public bool HasStartAddress { get; private set; }
        public bool IsStartAddressRelocatable { get; private set; }

        internal ModuleEndRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            byte type = reader.ReadByte();
            this.IsMainModule = (type & 0x80) != 0;
            this.HasStartAddress = (type & 0x40) != 0;
            this.IsStartAddressRelocatable = (type & 0x01) != 0;

            // TODO: other fields...
        }
    }

    /// <summary>
    /// Contains a list of unresolved symbols.
    /// </summary>
    public class ExternalNamesDefinitionRecord : Record
    {
        public SymbolEntry[] Symbols { get; private set; }

        internal ExternalNamesDefinitionRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            List<SymbolEntry> symbols = new List<SymbolEntry>();
            while (!reader.IsEOF)
            {
                string name = reader.ReadPrefixedString();
                int typeIndex = reader.ReadIndex();
                SymbolEntry symbol = new SymbolEntry(name, typeIndex);
                symbols.Add(symbol);
            }
            this.Symbols = symbols.ToArray();
        }
    }

    // TODO: move to outer namespace
    public class SymbolEntry
    {
        public string Name { get; private set; }
        public int TypeIndex { get; private set; }

        public SymbolEntry(string name, int typeIndex)
        {
            this.Name = name;
            this.TypeIndex = TypeIndex;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, TypeIndex);
        }
    }

    /// <summary>
    /// Defines public symbols in this object module. The symbols are also
    /// available for export if so indicated in an EXPDEF comment record.
    /// </summary>
    public class PublicNameDefinitionRecord : Record
    {
        public int BaseGroupIndex { get; private set; }
        public int BaseSegmentIndex { get; private set; }
        public UInt16 BaseSegmentAddress { get; private set; }
        public PublicSymbolEntry[] Symbols { get; private set; }

        internal PublicNameDefinitionRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.BaseGroupIndex = reader.ReadIndex();
            this.BaseSegmentIndex = reader.ReadIndex();
            if (BaseSegmentIndex == 0)
                this.BaseSegmentAddress = reader.ReadUInt16();

            List<PublicSymbolEntry> symbols = new List<PublicSymbolEntry>();
            while (!reader.IsEOF)
            {
                string name = reader.ReadPrefixedString();
                int offset = reader.ReadUInt16();
                int typeIndex = reader.ReadIndex();
                PublicSymbolEntry symbol = new PublicSymbolEntry(name, offset, typeIndex);
                symbols.Add(symbol);
            }
            this.Symbols = symbols.ToArray();
        }
    }

    public class PublicSymbolEntry
    {
        public string Name { get; private set; }
        public int TypeIndex { get; private set; }
        public int Offset { get; private set; }

        public PublicSymbolEntry(string name, int offset, int typeIndex)
        {
            this.Name = name;
            this.Offset = offset;
            this.TypeIndex = TypeIndex;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, TypeIndex);
        }
    }

    /// <summary>
    /// Defines a list of names that can be referenced by subsequent records
    /// in the object module.
    /// </summary>
    public class ListOfNamesRecord : Record
    {
        public string[] Names { get; private set; }

        internal ListOfNamesRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            List<string> names = new List<string>();
            while (!reader.IsEOF)
            {
                names.Add(reader.ReadPrefixedString());
            }
            this.Names = names.ToArray();
            context.Names.AddRange(Names);
        }
    }

    public class SegmentDefinitionRecord : Record
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

        internal SegmentDefinitionRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            byte acbp = reader.ReadByte();
            this.Alignment = (SegmentAlignment)(acbp >> 5);
            this.Combination = (SegmentCombination)((acbp >> 2) & 7);
            this.IsBig = (acbp & 0x02) != 0;
            this.Use32 = (acbp & 0x01) != 0;

            if (this.Alignment == SegmentAlignment.Absolute)
            {
                this.SegmentAddress = reader.ReadUInt16();
                this.SegmentOffset = reader.ReadByte();
            }

            this.Length = reader.ReadUInt16();
            if (IsBig)
                this.Length += 0x10000;

            this.SegmentNameIndex = reader.ReadIndex();
            this.ClassNameIndex = reader.ReadIndex();
            this.OverlayNameIndex = reader.ReadIndex();

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

    public class GroupDefinitionRecord : Record
    {
        [Browsable(false)]
        public int GroupNameIndex { get; private set; }
        public string GroupName { get; private set; }
        public int[] SegmentIndices { get; private set; }

        internal GroupDefinitionRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            GroupNameIndex = reader.ReadIndex();
            if (GroupNameIndex > context.Names.Count)
                throw new InvalidDataException();

            if (GroupNameIndex > 0)
                this.GroupName = context.Names[GroupNameIndex - 1];

            List<int> indices = new List<int>();
            while (!reader.IsEOF)
            {
                reader.ReadByte(); // 'type' ignored
                int index = reader.ReadIndex();
                indices.Add(index);
            }
            this.SegmentIndices = indices.ToArray();
        }
    }
}

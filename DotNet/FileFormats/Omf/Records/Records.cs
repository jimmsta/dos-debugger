using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using Util.Data;

namespace FileFormats.Omf.Records
{
    public enum RecordNumber : byte
    {
        None = 0,

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

        /// <summary>Local External Names Definition Record (32-bit)</summary>
        LEXTDEF = 0xB4,

        /// <summary>Local External Names Definition Record (32-bit)</summary>
        LEXTDEF32 = 0xB5,

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
            return ReadRecord(binaryReader, context, RecordNumber.None);
        }

        internal static Record ReadRecord(
            BinaryReader binaryReader, 
            RecordContext context,
            RecordNumber expectedRecord)
        {
            RecordReader reader = new RecordReader(binaryReader);
            if (expectedRecord != RecordNumber.None &&
                reader.RecordNumber != expectedRecord)
            {
                throw new InvalidDataException(string.Format(
                    "Expecting record {0}, but got record {1}.",
                    expectedRecord, reader.RecordNumber));
            }

            Record r;
            switch (reader.RecordNumber)
            {
                case RecordNumber.LibraryHeader:
                    r = new LibraryHeaderRecord(reader, context);
                    break;
                case RecordNumber.LibraryEnd:
                    r = new LibraryEndRecord(reader, context);
                    break;
                case RecordNumber.ALIAS:
                    r = new ALIASRecord(reader, context);
                    break;
                case RecordNumber.CEXTDEF:
                    r = new CEXTDEFRecord(reader, context);
                    break;
                case RecordNumber.COMDAT:
                case RecordNumber.COMDAT32:
                    r = new COMDATRecord(reader, context);
                    break;
                case RecordNumber.COMDEF:
                    r = new COMDEFRecord(reader, context);
                    break;
                case RecordNumber.COMENT:
                    r = new CommentRecord(reader, context);
                    break;
                case RecordNumber.EXTDEF:
                    r = new EXTDEFRecord(reader, context);
                    break;
                case RecordNumber.FIXUPP:
                case RecordNumber.FIXUPP32:
                    r = new FixupRecord(reader, context);
                    break;
                case RecordNumber.GRPDEF:
                    r = new GRPDEFRecord(reader, context);
                    break;
                case RecordNumber.LCOMDEF:
                    r = new LCOMDEFRecord(reader, context);
                    break;
                case RecordNumber.LEDATA:
                case RecordNumber.LEDATA32:
                    r = new LEDATARecord(reader, context);
                    break;
                case RecordNumber.LEXTDEF:
                case RecordNumber.LEXTDEF32:
                    r = new LEXTDEFRecord(reader, context);
                    break;
                case RecordNumber.LHEADR:
                    r = new LHEADRRecord(reader, context);
                    break;
                case RecordNumber.LIDATA:
                case RecordNumber.LIDATA32:
                    r = new LIDATARecord(reader, context);
                    break;
                case RecordNumber.LNAMES:
                    r = new ListOfNamesRecord(reader, context);
                    break;
                case RecordNumber.LPUBDEF:
                case RecordNumber.LPUBDEF32:
                    r = new LPUBDEFRecord(reader, context);
                    break;
                case RecordNumber.MODEND:
                    r = new MODENDRecord(reader, context);
                    break;
                case RecordNumber.PUBDEF:
                case RecordNumber.PUBDEF32:
                    r = new PUBDEFRecord(reader, context);
                    break;
                case RecordNumber.SEGDEF:
                case RecordNumber.SEGDEF32:
                    r = new SEGDEFRecord(reader, context);
                    break;
                case RecordNumber.THEADR:
                    r = new THEADRRecord(reader, context);
                    break;
                default:
                    r = new UnknownRecord(reader, context);
                    break;
            }

            // TODO: check all bytes are consumed.
            // ...

            // Update RecordContext.LastRecord. This is necessary so that
            // a FIXUPP record knows which record to fix up.
            if (context != null)
            {
                context.LastRecord = r;
            }

            return r;
        }

        public override string ToString()
        {
            return string.Format("{0} @ {1:X}", RecordNumber, Position);
        }
    }

    /// <summary>
    /// Represents a record with an unknown record number.
    /// </summary>
    class UnknownRecord : Record
    {
        public byte[] Data { get; private set; }

        public UnknownRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Data = reader.Data;
        }

        public override string ToString()
        {
            return string.Format("? {0} @ {1:X}", RecordNumber, Position);
        }
    }

    class LibraryHeaderRecord : Record
    {
        /// <summary>
        /// Gets the size, in bytes, of a page in the library file. Each
        /// object module in the library file must be aligned on page
        /// boundary.
        /// </summary>
        public int PageSize { get; private set; }

        public LibraryHeaderRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.PageSize = reader.Data.Length + 4;

            // Record data consists of 7 bytes of dictionary information
            // (which we ignore), followed by padding bytes to make the next
            // record (which should be THEADR) aligned on page boundary.
        }
    }

    class LibraryEndRecord : Record
    {
        public LibraryEndRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            // Record data serves as padding to align the dictionary that
            // follows at 512-byte boundary.
        }
    }

    /// <summary>
    /// Translator Header Record -- contains the name of the source file that
    /// defines this object module. Mutliple THEADR records may be present in
    /// an object module (e.g. if the object is compiled from multiple source
    /// files).
    /// </summary>
    class THEADRRecord : Record
    {
        public string Name { get; private set; }

        public THEADRRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Name = reader.ReadPrefixedString();
            context.SourceName = Name;
        }
    }

    /// <summary>
    /// Library Module Header Record -- contains the name of a module within
    /// a library file. This record is not used by MS LINK.
    /// </summary>
    class LHEADRRecord : Record
    {
        public string Name { get; private set; }

        public LHEADRRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Name = reader.ReadPrefixedString();
        }
    }

    /// <summary>
    /// Module End Record -- denotes the end of an object module.
    /// </summary>
    class MODENDRecord : Record
    {
        public bool IsMainModule { get; private set; }
        public bool IsStartAddressPresent { get; private set; }
        public bool IsStartAddressRelocatable { get; private set; }

        public MODENDRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            byte type = reader.ReadByte();
            this.IsMainModule = (type & 0x80) != 0;
            this.IsStartAddressPresent = (type & 0x40) != 0;
            this.IsStartAddressRelocatable = (type & 0x01) != 0;

            // TODO: read the start address field...
        }
    }

    #region External Name Related Records

    /// <summary>
    /// External Names Definition Record -- contains a list of symbolic
    /// external references, i.e. references to symbols defined in other
    /// object modules.
    /// </summary>
    /// <remarks>
    /// EXTDEF names are ordered by occurrence jointly with the COMDEF and
    /// LEXTDEF records, and referenced by an index in FIXUPP records.
    /// 
    /// The linker resolves external references by matching the symbols
    /// declared in EXTDEF records with symbols declared in PUBDEF records.
    /// </remarks>
    class EXTDEFRecord : Record
    {
        public ExternalNameDefinition[] Definitions { get; private set; }

        public EXTDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            int startIndex = context.ExternalNames.Count;
            while (!reader.IsEOF)
            {
                ExternalNameDefinition def = new ExternalNameDefinition();
                def.Name = reader.ReadPrefixedString();
                def.TypeIndex = reader.ReadIndex();
                def.DefinedBy = reader.RecordNumber;
                context.ExternalNames.Add(def);
            }
            int endIndex = context.ExternalNames.Count;
            this.Definitions = context.ExternalNames.Slice(
                startIndex, endIndex - startIndex);
        }
    }

    /// <summary>
    /// Local External Names Definition Record -- identical to EXTDEF records
    /// except that the names defined in LEXTDEF records are visible only in
    /// the module where they are defined.
    /// </summary>
    /// <remarks>
    /// LEXTDEF records are associated with LPUBDEF and LCOMDEF records and
    /// ordered with EXTDEF and COMDEF records by occurrence, so that they
    /// may be referenced by an external name index for fixups.
    /// </remarks>
    class LEXTDEFRecord : EXTDEFRecord
    {
        public LEXTDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
        }
    }

    /// <summary>
    /// COMDAT External Names Definition Record -- serves the same purpose as
    /// the EXTDEF record, except that the name referenced are defined in
    /// COMDAT records.
    /// </summary>
    /// <remarks>
    /// A CEXTDEF can precede the COMDAT to which it will be resolved. In this
    /// case, the location of the COMDAT is not known at the time the CEXTDEF
    /// is seen.
    /// 
    /// This record is produced when a FIXUPP record refers to a COMDAT
    /// symbol.
    /// </remarks>
    class CEXTDEFRecord : Record
    {
        public ExternalNameDefinition[] Definitions { get; private set; }

        public CEXTDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            int startIndex = context.ExternalNames.Count;
            while (!reader.IsEOF)
            {
                UInt16 nameIndex = reader.ReadIndex();
                if (nameIndex == 0 || nameIndex > context.Names.Count)
                {
                    throw new InvalidDataException("LogicalNameIndex is out of range.");
                }
                UInt16 typeIndex = reader.ReadIndex();

                ExternalNameDefinition def = new ExternalNameDefinition();

                def.Name = context.Names[nameIndex - 1];
                def.TypeIndex = typeIndex;
                def.DefinedBy = reader.RecordNumber;
                context.ExternalNames.Add(def);
            }
            int endIndex = context.ExternalNames.Count;
            this.Definitions = context.ExternalNames.Slice(
                startIndex, endIndex - startIndex);
        }
    }

    #endregion

    /// <summary>
    /// Public Names Definition Record -- defines public symbols in this
    /// object module. The symbols are also available for export if so
    /// indicated in an EXPDEF comment record.
    /// </summary>
    /// <remarks>
    /// All defined functions and initialized global variables generate
    /// PUBDEF records in most compilers.
    /// </remarks>
    class PUBDEFRecord : Record
    {
        public SegmentDefinition BaseSegment { get; private set; }
        public GroupDefinition BaseGroup { get; private set; }
        public UInt16 BaseFrame { get; private set; }
        public PublicNameDefinition[] Definitions { get; private set; }

        public PUBDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            int baseGroupIndex = reader.ReadIndex();
            if (baseGroupIndex > context.Groups.Count)
                throw new InvalidDataException("GroupIndex is out of range.");
            if (baseGroupIndex > 0)
                this.BaseGroup = context.Groups[baseGroupIndex - 1];

            int baseSegmentIndex = reader.ReadIndex();
            if (baseSegmentIndex > context.Segments.Count)
                throw new InvalidDataException("SegmentIndex is out of range.");
            if (baseSegmentIndex == 0)
                this.BaseFrame = reader.ReadUInt16();
            else
                this.BaseSegment = context.Segments[baseSegmentIndex - 1];

            int startIndex = context.PublicNames.Count;
            while (!reader.IsEOF)
            {
                PublicNameDefinition def = new PublicNameDefinition();
                def.DefinedBy = reader.RecordNumber;
                def.BaseGroup = BaseGroup;
                def.BaseSegment = BaseSegment;
                def.BaseFrame = BaseFrame;
                def.Name = reader.ReadPrefixedString();
                def.Offset = (int)reader.ReadUInt16Or32();
                def.TypeIndex = reader.ReadIndex();
                context.PublicNames.Add(def);
            }
            int endIndex = context.PublicNames.Count;
            this.Definitions = context.PublicNames.Slice(
                startIndex, endIndex - startIndex);
        }
    }

    class LPUBDEFRecord : PUBDEFRecord
    {
        public LPUBDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
        }
    }

    /// <summary>
    /// Communal Names Definition Record -- declares a list of communal
    /// variables (uninitialized static data or data that may match
    /// initialized static data in another compilation unit).
    /// </summary>
    class COMDEFRecord : Record
    {
        public ExternalNameDefinition[] Definitions { get; private set; }

        public COMDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            int startIndex = context.ExternalNames.Count;
            while (!reader.IsEOF)
            {
                CommunalNameDefinition def = new CommunalNameDefinition();
                def.DefinedBy = reader.RecordNumber;
                def.Name = reader.ReadPrefixedString();
                def.TypeIndex = reader.ReadIndex();
                def.DataType = reader.ReadByte();
                def.ElementCount = ReadEncodedInteger(reader);
                if (def.DataType == 0x61) // FAR data: count, elemsize
                    def.ElementSize = ReadEncodedInteger(reader);
                else
                    def.ElementSize = 1;
                context.ExternalNames.Add(def);
            }
            int endIndex = context.ExternalNames.Count;
            this.Definitions = context.ExternalNames.Slice(
                startIndex, endIndex - startIndex);
        }

        private static UInt32 ReadEncodedInteger(RecordReader reader)
        {
            byte b = reader.ReadByte();
            if (b == 0x81)
                return reader.ReadUInt16();
            else if (b == 0x84)
                return reader.ReadUInt24();
            else if (b == 0x88)
                return reader.ReadUInt32();
            else
                return b;
        }
    }

    class LCOMDEFRecord : COMDEFRecord
    {
        public LCOMDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
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

    class GRPDEFRecord : Record
    {
        public GroupDefinition Definition { get; private set; }

        public GRPDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.Definition = new GroupDefinition();

            UInt16 groupNameIndex = reader.ReadIndex();
            if (groupNameIndex == 0 || groupNameIndex > context.Names.Count)
            {
                throw new InvalidDataException("GroupNameIndex is out of range.");
            }
            this.Definition.Name = context.Names[groupNameIndex - 1];

            while (!reader.IsEOF)
            {
                reader.ReadByte(); // 'type' ignored
                UInt16 segmentIndex = reader.ReadIndex();
                if (segmentIndex == 0 || segmentIndex > context.Segments.Count)
                {
                    throw new InvalidDataException("SegmentIndex is out of range.");
                }
                this.Definition.Segments.Add(context.Segments[segmentIndex - 1]);
            }

            context.Groups.Add(Definition);
        }
    }

    /// <summary>
    /// Contains contiguous binary data to be copied into the program's
    /// executable binary image.
    /// </summary>
    class LEDATARecord : Record
    {
        public SegmentDefinition Segment { get; private set; }
        public UInt32 DataOffset { get; private set; }
        public byte[] Data { get; private set; }

        public LEDATARecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            UInt16 segmentIndex = reader.ReadIndex();
            if (segmentIndex == 0 || segmentIndex > context.Segments.Count)
            {
                throw new InvalidDataException("SegmentIndex is out of range.");
            }
            this.Segment = context.Segments[segmentIndex - 1];

            this.DataOffset = reader.ReadUInt16Or32();
            this.Data = reader.ReadToEnd(); // TBD: this can be optimized to
                                            // reduce extra data copy

            // Fill the segment's data.
            if (Data.Length + DataOffset > Segment.Length)
                throw new InvalidDataException("The LEDATA overflows the segment.");

            Array.Copy(Data, 0, Segment.Data, DataOffset, Data.Length);
        }
    }

    /// <summary>
    /// Contains contiguous binary data to be copied into the program's
    /// executable binary image. The data is stored as a repeating pattern.
    /// </summary>
    class LIDATARecord : Record
    {
        public UInt16 SegmentIndex { get; private set; }
        public UInt32 DataOffset { get; private set; }
        public byte[] Data { get; private set; }

        public LIDATARecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            this.SegmentIndex = reader.ReadIndex();
            if (SegmentIndex == 0 || SegmentIndex > context.Segments.Count)
                throw new InvalidDataException("SegmentIndex is out of range.");

            this.DataOffset = reader.ReadUInt16Or32();
            this.Data = reader.ReadToEnd();

            // TODO: parse LIDATA (recursive; a bit messy)
        }
    }

    class COMDATRecord : Record
    {
        public COMDATRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            // TODO: parse contents.
        }
    }

    class ALIASRecord : Record
    {
        public AliasDefinition[] Definitions { get; private set; }

        public ALIASRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            int startIndex = context.Aliases.Count;
            while (!reader.IsEOF)
            {
                AliasDefinition def = new AliasDefinition();
                def.AliasName = reader.ReadPrefixedString();
                def.SubstituteName = reader.ReadPrefixedString();
                context.Aliases.Add(def);
            }
            int endIndex = context.Aliases.Count;
            this.Definitions = context.Aliases.Slice(startIndex, endIndex - startIndex);
        }
    }
}

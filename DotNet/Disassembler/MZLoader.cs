using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.ComponentModel;
using System.Globalization;

namespace Disassembler
{
    public struct FarPointer16 : IComparable<FarPointer16>
    {
        private UInt16 segment;
        private UInt16 offset;

        public FarPointer16(UInt16 segment, UInt16 offset)
            : this()
        {
            this.segment = segment;
            this.offset = offset;
        }

        public UInt16 Segment
        {
            get { return segment; }
            set { segment = value; }
        }

        public UInt16 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int EffectiveAddress
        {
            get { return segment * 16 + offset; }
        }

        public override string ToString()
        {
            return string.Format("{0:X4}:{1:X4}", segment, offset);
        }

        public static FarPointer16 Parse(string s)
        {
            FarPointer16 ptr;
            if (!TryParse(s, out ptr))
                throw new ArgumentException("s");
            return ptr;
        }

        public static bool TryParse(string s, out FarPointer16 pointer)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            pointer = new FarPointer16();

            if (s.Length != 9)
                return false;
            if (s[4] != ':')
                return false;

            if (!UInt16.TryParse(
                    s.Substring(0, 4),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.segment))
                return false;

            if (!UInt16.TryParse(
                    s.Substring(5, 4),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out pointer.offset))
                return false;

            return true;
        }

        public static FarPointer16 operator +(FarPointer16 p, int increment)
        {
            return new FarPointer16(p.segment, (ushort)(p.offset + increment));
        }

        public static int operator -(FarPointer16 a, FarPointer16 b)
        {
            return a.EffectiveAddress - b.EffectiveAddress;
        }

        public static readonly FarPointer16 Invalid = new FarPointer16(0xFFFF, 0xFFFF);

        public int CompareTo(FarPointer16 other)
        {
            return this.EffectiveAddress - other.EffectiveAddress;
        }
    }

    /// <summary>
    /// Represents a loaded DOS MZ executable file (.EXE).
    /// </summary>
    public class MZFile
    {
        private MZHeader header;
        private FarPointer16[] relocationTable;
        private byte[] image;

        /* Opens a DOS MZ executable file. */
        public MZFile(string fileName)
        {
            using (FileStream stream = new FileStream(fileName,
                FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Read file header.
                header = new MZHeader();
                header.Signature = reader.ReadUInt16();
                header.LastPageSize = reader.ReadUInt16();
                header.PageCount = reader.ReadUInt16();
                header.RelocCount = reader.ReadUInt16();
                header.HeaderSize = reader.ReadUInt16();
                header.MinAlloc = reader.ReadUInt16();
                header.MaxAlloc = reader.ReadUInt16();
                header.InitialSS = reader.ReadUInt16();
                header.InitialSP = reader.ReadUInt16();
                header.Checksum = reader.ReadUInt16();
                header.InitialIP = reader.ReadUInt16();
                header.InitialCS = reader.ReadUInt16();
                header.RelocOff = reader.ReadUInt16();
                header.Overlay = reader.ReadUInt16();

                // Verify signature. Both 'MZ' and 'ZM' are allowed.
                if (!(header.Signature == 0x5A4D || header.Signature == 0x4D5A))
                    throw new InvalidDataException("Signature mismatch.");

                // Calculate the stated size of the executable.
                if (header.PageCount <= 0)
                    throw new InvalidDataException("The PageCount field must be positive.");
                int fileSize = header.PageCount * 512 -
                    (header.LastPageSize > 0 ? 512 - header.LastPageSize : 0);

                // Make sure the stated file size is within the actual file size.
                if (fileSize > stream.Length)
                    throw new InvalidDataException("The stated file size is larger than the actual file size.");

                // Validate the header size.
                int headerSize = header.HeaderSize * 16;
                if (headerSize < 28 || headerSize > fileSize)
                    throw new InvalidDataException("The stated header size is invalid.");

                // Make sure the relocation table is within the header.
                if (header.RelocOff < 28 ||
                    header.RelocOff + header.RelocCount * 4 > headerSize)
                {
                    throw new InvalidDataException("The relocation table location is invalid.");
                }

                // Load relocation table.
                relocationTable = new FarPointer16[header.RelocCount];
                stream.Seek(header.RelocOff, SeekOrigin.Begin);
                for (int i = 0; i < header.RelocCount; i++)
                {
                    UInt16 off = reader.ReadUInt16();
                    UInt16 seg = reader.ReadUInt16();
                    relocationTable[i] = new FarPointer16(seg, off);
                }

                // Load the whole image into memory.
                int imageSize = fileSize - headerSize;
                stream.Seek(headerSize, SeekOrigin.Begin);
                image = new byte[imageSize];
                stream.Read(image, 0, image.Length);
            }
        }

        /// <summary>
        /// Relocates the image to start from the given segment.
        /// </summary>
        /// <param name="segment">The segment to relocate to.</param>
        public void Relocate(UInt16 segment)
        {
            header.InitialCS += segment;
            header.InitialSS += segment;
            for (int i = 0; i < relocationTable.Length; i++)
            {
                int address = relocationTable[i].EffectiveAddress;
                if (!(address >= 0 && address + 2 <= image.Length))
                    throw new InvalidDataException("The relocation entry is out-of-range.");

                UInt16 current = BitConverter.ToUInt16(image, address);
                current += segment;
                image[address] = (byte)(current & 0xff);
                image[address + 1] = (byte)(current >> 8);
            }
            baseAddress.Segment = segment;
        }

        FarPointer16 baseAddress;

        public FarPointer16 BaseAddress
        {
            get { return baseAddress; }
            set
            {
                if (value.Offset != 0)
                    throw new ArgumentException("value must have zero offset.");
                Relocate(baseAddress.Segment);
            }
        }

        /// <summary>
        /// Gets the executable image.
        /// </summary>
        [Browsable(false)]
        public byte[] Image
        {
            get { return image; }
        }

        /// <summary>
        /// Gets the number of bytes in the executable image.
        /// </summary>
        public int ImageSize
        {
            get { return image.Length; }
        }

        /// <summary>
        /// Gets a collection of relocation entries. Each relocation entry is
        /// a far pointer relative to the beginning of the executable image,
        /// which points to a 16-bit word that contains a segment address.
        /// The module loader should add the actual segment to the word at
        /// these locations.
        /// </summary>
        public FarPointer16[] RelocationTable
        {
            get { return relocationTable; }
        }

        /// <summary>
        /// Gets the address of the first instruction to execute. This address
        /// is relative to the beginning of the executable image.
        /// </summary>
        public FarPointer16 EntryPoint
        {
            get { return new FarPointer16(header.InitialCS, header.InitialIP); }
        }

        /// <summary>
        /// Gets the address of the top of the stack. This address is relative
        /// to the beginning of the executable image.
        /// </summary>
        public FarPointer16 StackTop
        {
            get { return new FarPointer16(header.InitialSS, header.InitialSP); }
        }

        /// <summary>
        /// Gets a copy of the file header.
        /// </summary>
        public MZHeader Header
        {
            get { return header; }
        }
    }

    /// <summary>
    /// Represents the file header in a DOS MZ executable.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct MZHeader
    {
        [Description("File format signature; should be MZ or ZM.")]
        public UInt16 Signature { get; set; }

        [Description("Number of bytes in the last page; 0 if the last page is full.")]
        public UInt16 LastPageSize { get; set; }

        [Description("Number of 512-byte pages in the file, including the last page.")]
        public UInt16 PageCount { get; set; }

        [Description("Number of relocation entries; may be 0.")]
        public UInt16 RelocCount { get; set; }

        [Description("Number of 16-byte paragraphs in the header. The executable image starts right after this.")]
        public UInt16 HeaderSize { get; set; }

        [Description("Minimum memory required, in 16-byte paragraphs.")]
        public UInt16 MinAlloc { get; set; }

        [Description("Maximum memory required, in 16-byte paragraphs; usually 0xFFFF.")]
        public UInt16 MaxAlloc { get; set; }

        [Description("Initial initial value of SS; this value must be relocated.")]
        public UInt16 InitialSS { get; set; }

        [Description("Initial value of SP.")]
        public UInt16 InitialSP { get; set; }

        [Description("Checksum of the executable file; usually not used.")]
        public UInt16 Checksum { get; set; }

        [Description("Initial value of IP.")]
        public UInt16 InitialIP { get; set; }

        [Description("Initial value of CS; this value must be relocated.")]
        public UInt16 InitialCS { get; set; }

        [Description("Offset (in bytes) of the relocation table relative to the beginning of the file.")]
        public UInt16 RelocOff { get; set; }

        [Description("Overlay number; usually 0.")]
        public UInt16 Overlay;
    }
}

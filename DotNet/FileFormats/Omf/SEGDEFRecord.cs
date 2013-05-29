using System;
using System.IO;

namespace FileFormats.Omf.Records
{
    class SEGDEFRecord : Record
    {
        public SegmentDefinition Definition { get; private set; }

        public SEGDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            SegmentDefinition def = new SegmentDefinition();

            // Read the record.
            byte acbp = reader.ReadByte();
            def.Alignment = GetAlignment(acbp);
            def.Combination = GetCombination(acbp);
            def.IsUse32 = GetUse32(acbp);

            if (def.Alignment == SegmentAlignment.Absolute)
            {
                def.Frame = reader.ReadUInt16();
                def.Offset = reader.ReadByte();
            }

            UInt32 storedLength=reader.ReadUInt16Or32();
            def.Length = GetLength(acbp, storedLength, reader.RecordNumber);

            UInt16 segmentNameIndex = reader.ReadIndex();
            if (segmentNameIndex > context.Names.Count)
                throw new InvalidDataException("SegmentNameIndex is out of range.");
            if (segmentNameIndex > 0)
                def.SegmentName = context.Names[segmentNameIndex - 1];

            UInt16 classNameIndex = reader.ReadIndex();
            if (classNameIndex > context.Names.Count)
                throw new InvalidDataException("ClassNameIndex is out of range.");
            if (classNameIndex > 0)
                def.ClassName = context.Names[classNameIndex - 1];

            UInt16 overlayNameIndex = reader.ReadIndex();
            if (overlayNameIndex > context.Names.Count)
                throw new InvalidDataException("OverlayNameIndex is out of range.");
            if (overlayNameIndex > 0)
                def.OverlayName = context.Names[overlayNameIndex - 1];

            def.Data = new byte[def.Length];
            //def.Fixups = new List<FixupDefinition>();

            this.Definition = def;
            context.Segments.Add(def);
        }

        private static bool GetUse32(byte acbp)
        {
            return (acbp & 0x01) != 0;
        }

        private static SegmentAlignment GetAlignment(byte acbp)
        {
            int alignment = acbp >> 5;
            switch (alignment)
            {
                case 0: return SegmentAlignment.Absolute;
                case 1: return SegmentAlignment.Byte;
                case 2: return SegmentAlignment.Word;
                case 3: return SegmentAlignment.Paragraph;
                case 4: return SegmentAlignment.Page;
                case 5: return SegmentAlignment.DWord;
                default:
                    throw new InvalidDataException("Unsupported segment alignment: " + alignment);
            }
        }

        private static SegmentCombination GetCombination(byte acbp)
        {
            int combination = (acbp >> 2) & 7;
            switch (combination)
            {
                case 0: return SegmentCombination.Private;
                case 2:
                case 4:
                case 7: return SegmentCombination.Public;
                case 5: return SegmentCombination.Stack;
                case 6: return SegmentCombination.Common;
                default:
                    throw new InvalidDataException("Unsupported segment combination: " + combination);
            }
        }

        private static long GetLength(byte acbp, UInt32 storedLength, RecordNumber recordNumber)
        {
            bool isBig = (acbp & 0x02) != 0;
            if (isBig)
            {
                if (recordNumber == RecordNumber.SEGDEF32)
                    return 0x100000000L;
                else
                    return 0x10000;
            }
            else
            {
                return storedLength;
            }
        }
    }
}

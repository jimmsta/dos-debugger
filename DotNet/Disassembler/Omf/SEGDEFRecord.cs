using System;
using System.Collections.Generic;
using System.IO;

namespace Disassembler2.Omf
{
    class SEGDEFRecord : Record
    {
        public SegmentDefinition Definition { get; private set; }

        public SEGDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            SegmentDefinition def = new SegmentDefinition();

            // Read the record.
            def.DefinedBy = reader.RecordNumber;
            def.ACBP = reader.ReadByte();
            if (def.Alignment == Alignment.None) // absolute segment
            {
                def.Frame = reader.ReadUInt16();
                def.Offset = reader.ReadByte();
            }
            def.Length = reader.ReadUInt16Or32();

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

            this.Definition = def;
            context.Segments.Add(def);
        }

#if false
        
            // Convert the record.
            LogicalSegment segment = new LogicalSegment();
            if (def.IsUse32)
                throw new NotSupportedException("Use32 is not supported.");

            segment.Alignment = def.Alignment;
            segment.Combination = def.Combination;

            if (def.Alignment == Alignment.None) // absolute segment
            {
                segment.AbsoluteFrame = def.Frame; // ignore Offset
            }

            if (def.SegmentNameIndex == 0 ||
                def.SegmentNameIndex > context.Names.Count)
            {
                throw new InvalidDataException("SegmentNameIndex out of range.");
            }
            segment.Name = context.Names[def.SegmentNameIndex - 1];

            if (def.ClassNameIndex == 0 ||
                def.ClassNameIndex > context.Names.Count)
            {
                throw new InvalidDataException("ClassNameIndex out of range.");
            }
            segment.Class = context.Names[def.ClassNameIndex - 1];

            long length = def.RealLength;
            if (length > Int32.MaxValue)
            {
                throw new InvalidDataException("Segment larger than 2GB is not supported.");
            }
            segment.Image = new ImageChunk((int)length,
                context.Module.Name + "." + segment.Name);
#endif
    }

    internal class SegmentDefinition
    {
        public RecordNumber DefinedBy;

        public byte ACBP { get; set; }
        public UInt16 Frame { get; set; }
        public byte Offset { get; set; }
        public UInt32 Length { get; set; }

        public string SegmentName;
        public string ClassName;
        public string OverlayName;

        public byte[] Data;
        public List<FixupDefinition> Fixups;

        public Alignment Alignment
        {
            get
            {
                int alignment = ACBP >> 5;
                switch (alignment)
                {
                    case 0: return Alignment.None;
                    case 1: return Alignment.Byte;
                    case 2: return Alignment.Word;
                    case 3: return Alignment.Paragraph;
                    case 4: return Alignment.Page;
                    case 5: return Alignment.DWord;
                    default:
                        throw new InvalidDataException("Unsupported segment alignment: " + alignment);
                }
            }
        }

        public SegmentCombination Combination
        {
            get
            {
                int combination = (ACBP >> 2) & 7;
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
        }

        public bool IsBig
        {
            get { return (ACBP & 0x02) != 0; }
        }

        public bool IsUse32
        {
            get { return (ACBP & 0x01) != 0; }
        }

        public long RealLength
        {
            get
            {
                if (IsBig)
                {
                    if (DefinedBy == RecordNumber.SEGDEF32)
                        return 0x100000000L;
                    else
                        return 0x10000;
                }
                else
                {
                    return Length;
                }
            }
        }
    }
}

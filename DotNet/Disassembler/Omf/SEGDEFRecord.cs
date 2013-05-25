using System;
using System.IO;

namespace Disassembler2.Omf
{
    internal class SEGDEFRecord : Record
    {
        public LogicalSegment Segment { get; private set; }

        internal SEGDEFRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            // Read the record.
            SegmentDefinition def = new SegmentDefinition();
            def.IsSEGDEF32 = (reader.RecordNumber == Omf.RecordNumber.SEGDEF32);
            def.ACBP = reader.ReadByte();
            if (def.Alignment == Alignment.None) // absolute segment
            {
                def.Frame = reader.ReadUInt16();
                def.Offset = reader.ReadByte();
            }
            def.Length = reader.ReadUInt16Or32();
            def.SegmentNameIndex = reader.ReadIndex();
            def.ClassNameIndex = reader.ReadIndex();
            def.OverlayNameIndex = reader.ReadIndex();

            // Convert the record.
            LogicalSegment segment = new LogicalSegment();
            if (def.IsUse32)
                throw new NotSupportedException("Use32 is not supported.");

            segment.Alignment =  def.Alignment;
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

            this.Segment = segment;
            context.Module.Segments.Add(segment);
        }
    }

    internal class SegmentDefinition
    {
        public bool IsSEGDEF32 { get; set; }
        public byte ACBP { get; set; }
        public UInt16 Frame { get; set; }
        public byte Offset { get; set; }
        public UInt32 Length { get; set; }
        public UInt16 SegmentNameIndex { get; set; }
        public UInt16 ClassNameIndex { get; set; }
        public UInt16 OverlayNameIndex { get; set; }

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
                    if (IsSEGDEF32)
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

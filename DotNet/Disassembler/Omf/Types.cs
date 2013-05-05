using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler.Omf
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SegmentDefinition
    {
        public string Name { get; internal set; }
        public string Class { get; internal set; }
        public string Overlay { get; internal set; }
        public UInt16 FrameNumber { get; internal set; }
        public SegmentAlignment Alignment { get; internal set; }
        public SegmentCombination Combination { get; internal set; }
        public long Length { get; internal set; }
        public bool IsUse32 { get; internal set; }

        public override string ToString()
        {
            return Name;
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


    /// <summary>
    /// Defines a symbolic name.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NameDefinition
    {
        public string Name { get; internal set; }
        public UInt16 TypeIndex { get; internal set; }

        public override string ToString()
        {
            if (TypeIndex == 0)
                return Name;
            else
                return string.Format("{0}:{1}", Name, TypeIndex);
        }
    }

    /// <summary>
    /// Contains information about an external symbolic name defined by one
    /// of the following records:
    /// EXTDEF  -- ExternalNamesDefinitionRecord
    /// LEXTDEF -- LocalExternalNamesDefinitionRecord
    /// COMDEF  -- CommunalNamesDefinitionRecord
    /// CEXTDEF -- COMDATExternalNamesDefinitionRecord
    /// </summary>
    public class ExternalNameDefinition : NameDefinition
    {
    }

    public class CommunalNameDefinition : ExternalNameDefinition
    {
        public byte DataType { get; internal set; }
        public UInt32 ElementCount { get; internal set; }
        public UInt32 ElementSize { get; internal set; }
    }

    /// <summary>
    /// Contains information about a public (exported) symbol.
    /// </summary>
    public class PublicNameDefinition : NameDefinition
    {
        /// <summary>
        /// Gets the LSEG (logical segment) in which this symbol is defined.
        /// If BaseSegment is not null, the Offset field is relative to the
        /// beginning of BaseSegment. If BaseSegment is null, the Offset field
        /// is relative to the physical frame indicated by FrameNumber.
        /// </summary>
        public SegmentDefinition BaseSegment { get; internal set; }

        /// <summary>
        /// Gets the group associated with this symbol. Such association is
        /// used to resolve the FRAME in a FIXUPP -- if BaseGroup is not null,
        /// then the group's frame is used as the FRAME of the fixup.
        /// </summary>
        public GroupDefinition BaseGroup { get; internal set; }

        /// <summary>
        /// Gets the frame number of the address of this symbol. This is only
        /// relevant if BaseSegment is null, which indicates that the symbol
        /// refers to an absolute SEG:OFF address.
        /// </summary>
        public UInt16 BaseFrame { get; internal set; }

        /// <summary>
        /// Gets the offset of the symbol relative to the start of the LSEG
        /// (logical segment) in which it is defined.
        /// </summary>
        public UInt32 Offset { get; internal set; }

        public override string ToString()
        {
            if (BaseSegment == null)
            {
                return string.Format("{0} @ {1:X4}:{2:X4}", Name, BaseFrame, Offset);
            }
            else
            {
                return string.Format("{0} @ {1}+{2:X}h", Name, BaseSegment.Name, Offset);
            }
        }
    }
}

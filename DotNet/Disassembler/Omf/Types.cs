using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler.Omf
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GroupDefinition
    {
        /// <summary>
        /// Gets the name of the group. Groups from different object modules
        /// are combined if their names are identical.
        /// </summary>
        [Browsable(true)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the logical segments contained in this group.
        /// </summary>
        [Browsable(true)]
        public LogicalSegment[] Segments { get; internal set; }

        public override string ToString()
        {
            return Name;
        }
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
    /// CEXTDEF -- COMDATExternalNamesDefinitionRecord
    /// One of these records must be defined so that a FIXUPP record can
    /// refer to the symbol's address.
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
        /// 
        /// Note: when BaseSegment is null, the public name is typically used
        /// to represent a constant.
        /// </summary>
        [Browsable(true)]
        public LogicalSegment BaseSegment { get; internal set; }

        /// <summary>
        /// Gets the group associated with this symbol. Such association is
        /// used to resolve the FRAME in a FIXUPP -- if BaseGroup is not null,
        /// then the group's frame is used as the FRAME of the fixup.
        /// </summary>
        [Browsable(true)]
        public GroupDefinition BaseGroup { get; internal set; }

        /// <summary>
        /// Gets the frame number of the address of this symbol. This is only
        /// relevant if BaseSegment is null, which indicates that the symbol
        /// refers to an absolute SEG:OFF address.
        /// </summary>
        [Browsable(true)]
        public UInt16 BaseFrame { get; internal set; }

        /// <summary>
        /// Gets the offset of the symbol relative to the start of the LSEG
        /// (logical segment) in which it is defined.
        /// </summary>
        [Browsable(true)]
        public UInt32 Offset { get; internal set; }

        /// <summary>
        /// Gets a flag indicating whether this symbol is local, i.e. is only
        /// visible within the object module where it is defined.
        /// </summary>
        [Browsable(true)]
        public bool IsLocal { get; internal set; }

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

    public enum MemoryModel
    {
        Unknown = 0,
        Tiny,
        Small,
        Medium,
        Compact,
        Large,
        Huge
    }
}

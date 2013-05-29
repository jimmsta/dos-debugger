using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler
{
    /// <summary>
    /// Represents a logical segment in an object module.
    /// </summary>
    /// <remarks>
    /// A logical segment is defined by a SEGDEF record.
    /// 
    /// Multiple logical segments are often combined to form a
    /// CombinedSegment.
    /// </remarks>
    /// <example>
    /// Examples: fopen._TEXT, crt0._DATA, etc.
    /// </example>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class LogicalSegment : Segment // create new class LibrarySegment
                                          // to handle book-keeping of Segment
    {
        readonly int id;
        readonly Alignment alignment;
        readonly SegmentCombination combination;
        readonly UInt16 absoluteFrame;
        readonly string segmentName;
        readonly string fullName;
        readonly string className;
        readonly byte[] data;
        readonly FixupCollection fixups = new FixupCollection();

        internal LogicalSegment(
            int id,
            Disassembler2.Omf.SegmentDefinition def,
            Dictionary<object, object> objectMap,
            ObjectModule module)
        {
            if (def.IsUse32)
                throw new NotSupportedException("Use32 is not supported.");

            this.id = id;
            this.alignment = def.Alignment;
            this.combination = def.Combination;
            this.absoluteFrame = def.Frame; // ignore Offset
            this.segmentName = def.SegmentName;
            this.fullName = module.Name + "." + def.SegmentName;
            this.className = def.ClassName; // ignore OverlayName
            this.data = def.Data;
        }

        /// <summary>
        /// Gets the segment's name, such as "_TEXT". A segment's name
        /// together with its class name uniquely identifies the segment.
        /// </summary>
        public string Name
        {
            get { return segmentName; }
        }

        // TODO: make Segment an interface, and explicitly implement
        // its Name property.
        public string FullName
        {
            get { return fullName; }
        }

        /// <summary>
        /// Gets the segment's class, such as "CODE". A segment's name
        /// together with its class name uniquely identifies the segment.
        /// </summary>
        public string Class
        {
            get { return className; }
        }

        /// <summary>
        /// Gets or sets the alignment requirement of the logical segment.
        /// </summary>
        public Alignment Alignment
        {
            get { return alignment; }
        }

        /// <summary>
        /// Gets or sets how to combine two segments of the same name and
        /// class.
        /// </summary>
        public SegmentCombination Combination
        {
            get { return combination; }
        }

        /// <summary>
        /// Gets the frame number of an absolute segment. This is only
        /// relevant if Alignment is Absolute.
        /// </summary>
        public UInt16 AbsoluteFrame
        {
            get { return absoluteFrame; }
        }

        /// <summary>
        /// Gets the length (in bytes) of the logical segment. This length
        /// does not include COMDAT records. If COMDAT records are present,
        /// their size should be added to this length.
        /// </summary>
        public int Length
        {
            get { return Data.Length; }
        }

        /// <summary>
        /// Gets the bytes in this logical segment.
        /// </summary>
        public byte[] Data
        {
            get { return data; }
        }

        public FixupCollection Fixups
        {
            get { return fixups; }
        }

        protected override string GetLabel()
        {
            return Name;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Class);
        }
    }


}

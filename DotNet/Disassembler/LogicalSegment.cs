using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Disassembler.Omf;

namespace Disassembler
{
    /// <summary>
    /// Represents a logical segment of code or data, such as one defined in
    /// an object module by the SEGDEF record. A logical segment does not
    /// define its physical layout in a binary image.
    /// 
    /// Multiple logical segments are often combined to form a
    /// CombinedSegment.
    /// </summary>
    /// <example>
    /// Examples: fopen._TEXT, crt0._DATA, etc.
    /// </example>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class LogicalSegment : IAddressable
    {
        /// <summary>
        /// Gets the segment's name, such as "_TEXT". A segment's name
        /// together with its class name uniquely identifies the segment.
        /// </summary>
        [Category("Identity")]
        [Browsable(true)]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the segment's class, such as "CODE". A segment's name
        /// together with its class name uniquely identifies the segment.
        /// </summary>
        [Category("Identity")]
        [Browsable(true)]
        public string Class { get; internal set; }

        /// <summary>
        /// Ignored by MS LINK.
        /// </summary>
        [Category("Identity")]
        [Browsable(false)]
        public string OverlayName { get; internal set; }

        /// <summary>
        /// Gets or sets the alignment requirement of the logical segment.
        /// </summary>
        [Browsable(true)]
        public Alignment Alignment { get; internal set; }

        /// <summary>
        /// Gets or sets how to combine two segments of the same name and
        /// class.
        /// </summary>
        [Browsable(true)]
        public SegmentCombination Combination { get; internal set; }

        /// <summary>
        /// Gets the start address of an absolute segment. This value is only
        /// relevant if Alignment is Absolute.
        /// </summary>
        [Browsable(true)]
        public Pointer StartAddress { get; internal set; }

        /// <summary>
        /// Gets the length (in bytes) of the logical segment. This length
        /// does not include COMDAT records. If COMDAT records are present,
        /// their size should be added to this length.
        /// </summary>
        [Browsable(true)]
        public long Length
        {
            get { return Image.Length; }
        }

#if false
        [Browsable(false)]
        public bool IsUse32 { get; internal set; }
#endif

#if false
        /// <summary>
        /// Gets the bytes in this logical segment.
        /// </summary>
        public byte[] Data { get; internal set; }

        // datafixups[i] corresponds to data[i]
        // it is 1+index of the fix up that covers this byte. 0=none
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public UInt16[] DataFixups { get; internal set; }

        internal readonly List<FixupDefinition> fixups = new List<FixupDefinition>();

        public FixupDefinition[] Fixups
        {
            get { return fixups.ToArray(); }
        }
#endif

        public ImageChunk Image { get; internal set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Class);
        }

        /// <summary>
        /// Gets a label that identifies the logical segment, such as
        /// "fopen._TEXT" or "crt0._DATA".
        /// </summary>
        string IAddressable.Label
        {
            get { throw new NotImplementedException(); }
        }
    }

    public enum Alignment: short
    {
        /// <summary>The alignment is irrelevant or default.</summary>
        None = 0,

        /// <summary>Align on byte boundary.</summary>
        Byte = 1,

        /// <summary>Align on word boundary.</summary>
        Word = 2,

        /// <summary>Align on dword boundary.</summary>
        DWord = 4,
        
        /// <summary>Align on qword boundary.</summary>
        QWord = 8,
        
        /// <summary>Align on paragraph (16-byte) boundary.</summary>
        Paragraph = 16,

        /// <summary>Align on page (256-byte) boundary.</summary>
        Page = 256,
    }

    /// <summary>
    /// Specifies how the linker should combine two segments with the same
    /// name and class.
    /// </summary>
    public enum SegmentCombination : byte
    {
        /// <summary>
        /// Do not combine the segment with segments from other modules, even
        /// if they have the same segment name and class name.
        /// </summary>
        Private,

        /// <summary>
        /// Concatenates all segments having the same name (and class name)
        /// to form a single, contiguous segment (subject to alignment
        /// requirements). This may leave a gap in between two segments.
        /// </summary>
        Public,

        /// <summary>
        /// Combine by overlapping the two segments. They either have the same
        /// start address or the same end address. The combine segment size is
        /// the larger size of the two.
        /// </summary>
        Common,

        /// <summary>
        /// Concatenates all segments having the same name (and class name)
        /// and causes the operating system to set SS:00 to the bottom and
        /// SS:SP to the top of the resulting segment.
        /// </summary>
        Stack,
    }
}

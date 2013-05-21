using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler
{
    /// <summary>
    /// Contains information about a fix-up to be applied to a given range of
    /// bytes in a binary image.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Fixup
    {
        private int startIndex;

        public Fixup(int startIndex)
        {
            this.startIndex = startIndex;
        }

        /// <summary>
        /// Gets or sets the location to fix up.
        /// </summary>
        public Range<int> Location
        {
            get
            {
                return new Range<int>(
                    startIndex, 
                    startIndex + GetLengthFromLocationType(LocationType));
            }
        }

        /// <summary>
        /// Gets or sets the type of data to fix in that location.
        /// </summary>
        public FixupLocationType LocationType { get; internal set; }

        /// <summary>
        /// Gets or sets the fix-up mode.
        /// </summary>
        public FixupMode Mode { get; internal set; }

        /// <summary>
        /// Gets or sets the fix-up target.
        /// </summary>
        public FixupTarget Target { get; internal set; }

        /// <summary>
        /// Gets or sets the target frame relative to which to apply the
        /// fix up.
        /// </summary>
        public FixupFrame Frame { get; internal set; }

        /// <summary>
        /// Gets the number of bytes to fix up. This is inferred from the
        /// LocationType property.
        /// </summary>
        private static int GetLengthFromLocationType(FixupLocationType type)
        {
            switch (type)
            {
                case FixupLocationType.LowByte:
                    return 1;
                case FixupLocationType.Offset:
                case FixupLocationType.Base:
                    return 2;
                case FixupLocationType.Pointer:
                    return 4;
                default:
                    return 0;
            }
        }
    }

    public enum FixupMode : byte
    {
        SelfRelative = 0,
        SegmentRelative = 1
    }

    /// <summary>
    /// Specifies the type of data to fix up in that location.
    /// </summary>
    public enum FixupLocationType : byte
    {
        /// <summary>The fixup location type is unknown.</summary>
        Unknown,

        /// <summary>
        /// 8-bit displacement or low byte of 16-bit offset.
        /// </summary>
        LowByte,

        /// <summary>16-bit offset.</summary>
        Offset,

        /// <summary>16-bit base.</summary>
        Base,

        /// <summary>32-bit pointer (16-bit base:16-bit offset).</summary>
        Pointer,
    }

    public struct FixupTarget
    {
        /// <summary>
        /// Gets or sets the referent of the target. The target is specified
        /// by referent + displacement.
        /// </summary>
        public IAddressable Referent { get; set; }

        /// <summary>
        /// Gets or sets the displacement of the target relative to the
        /// referent.
        /// </summary>
        public UInt32 Displacement { get; internal set; }

#if false
        public override string ToString()
        {
            switch (Method)
            {
                case FixupTargetMethod.Absolute:
                    return string.Format("{0:X4}:{1:X4}", IndexOrFrame, Displacement);
                case FixupTargetMethod.SegmentPlusDisplacement:
                    return string.Format("SEG({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.GroupPlusDisplacement:
                    return string.Format("GRP({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.ExternalPlusDisplacement:
                    return string.Format("EXT({0})+{1:X}H", IndexOrFrame, Displacement);
                case FixupTargetMethod.SegmentWithoutDisplacement:
                    return string.Format("SEG({0})", IndexOrFrame);
                case FixupTargetMethod.GroupWithoutDisplacement:
                    return string.Format("GRP({0})", IndexOrFrame);
                case FixupTargetMethod.ExternalWithoutDisplacement:
                    return string.Format("EXT({0})", IndexOrFrame);
                default:
                    return "(invalid)";
            }
        }
#endif
    }

    public struct FixupFrame
    {
        public FixupFrameMethod Method { get; internal set; }

        /// <summary>
        /// Gets or sets the INDEX of the SEG/GRP/EXT item that is used as
        /// the referent to find the frame. This is used only if Method is
        /// one of 0, 1, or 2. If Method is 3, then this field contains an
        /// absolute frame number. If Method is 4-7, this field is not used.
        /// </summary>
        public UInt16 IndexOrFrame { get; internal set; }

        public override string ToString()
        {
            switch (Method)
            {
                case FixupFrameMethod.SegmentIndex:
                    return string.Format("SEG({0})", IndexOrFrame);
                case FixupFrameMethod.GroupIndex:
                    return string.Format("GRP({0})", IndexOrFrame);
                case FixupFrameMethod.ExternalIndex:
                    return string.Format("EXT({0})", IndexOrFrame);
                case FixupFrameMethod.ExplicitFrame:
                    return string.Format("{0:X4}", IndexOrFrame);
                case FixupFrameMethod.UseLocation:
                    return "LOCATION";
                case FixupFrameMethod.UseTarget:
                    return "TARGET";
                default:
                    return "(invalid)";
            }
        }
    }

    public enum FixupFrameMethod : byte
    {
        /// <summary>
        /// The FRAME is the canonical frame of the LSEG (logical segment)
        /// identified by Index.
        /// </summary>
        SegmentIndex = 0,

        /// <summary>
        /// The FRAME is the canonical frame of the group identified by Index.
        /// </summary>
        GroupIndex = 1,

        /// <summary>
        /// The FRAME is determined according to the External Name's PUBDEF
        /// record. There are three cases:
        /// a) If there is an associated group with the symbol, the canonical
        ///    frame of that group is used; otherwise,
        /// b) If the symbol is defined relative to some LSEG, the canonical
        ///    frame of the LSEG is used.
        /// c) If the symbol is defined at an absolute address, the frame of
        ///    this absolute address is used.
        /// </summary>
        ExternalIndex = 2,

        /// <summary>
        /// The FRAME is specified explicitly by a number. This method is not
        /// supported by any linker.
        /// </summary>
        ExplicitFrame = 3,

        /// <summary>
        /// The FRAME is the canonic FRAME of the LSEG containing LOCATION.
        /// If the location is defined by an absolute address, the frame 
        /// component of that address is used.
        /// </summary>
        UseLocation = 4,

        /// <summary>
        /// The FRAME is determined by the TARGET's segment, group, or
        /// external index.
        /// </summary>
        UseTarget = 5,
    }
}

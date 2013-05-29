using System;
using System.Collections.Generic;
using System.Text;

namespace FileFormats.Omf
{
    public class FixupDefinition
    {
        /// <summary>
        /// Gets or sets the offset (relative to the beginning of the segment)
        /// to fix up.
        /// </summary>
        public UInt16 DataOffset { get; internal set; } // indicates where to fix up

        public FixupLocation Location { get; internal set; } // indicates what to fix up

        public FixupMode Mode { get; internal set; }
        public FixupTarget Target { get; internal set; }
        public FixupFrame Frame { get; internal set; }

        //public int StartIndex { get { return DataOffset; } }
        //public int EndIndex { get { return StartIndex + Length; } }

#if false
        /// <summary>
        /// Gets the number of bytes to fix up. This is inferred from the
        /// Location property.
        /// </summary>
        public int Length
        {
            get
            {
                switch (Location)
                {
                    case FixupLocation.LowByte:
                    case FixupLocation.HighByte:
                        return 1;
                    case FixupLocation.Offset:
                    case FixupLocation.Base:
                    case FixupLocation.LoaderResolvedOffset:
                        return 2;
                    case FixupLocation.Pointer:
                    case FixupLocation.Offset32:
                    case FixupLocation.LoaderResolvedOffset32:
                        return 4;
                    case FixupLocation.Pointer32:
                        return 6;
                    default:
                        return 0;
                }
            }
        }
#endif
    }

    /// <summary>
    /// Specifies the type of data to fix up in that location.
    /// </summary>
    public enum FixupLocation : byte
    {
        /// <summary>
        /// 8-bit displacement or low byte of 16-bit offset.
        /// </summary>
        LowByte = 0,

        /// <summary>16-bit offset.</summary>
        Offset = 1,

        /// <summary>16-bit base.</summary>
        Base = 2,

        /// <summary>32-bit pointer (16-bit base:16-bit offset).</summary>
        Pointer = 3,

        /// <summary>
        /// High byte of 16-bit offset. Not supported by MS LINK.
        /// </summary>
        HighByte = 4,

        /// <summary>
        /// 16-bit loader-resolved offset, treated as Location=1.
        /// </summary>
        LoaderResolvedOffset = 5,

        /// <summary>32-bit offset.</summary>
        Offset32 = 9,

        /// <summary>48-bit pointer (16-bit base:32-bit offset).</summary>
        Pointer32 = 11,

        /// <summary>
        /// 32-bit loader-resolved offset, treated as Location=9.
        /// </summary>
        LoaderResolvedOffset32 = 13,
    }

    public enum FixupMode : byte
    {
        SelfRelative = 0,
        SegmentRelative = 1
    }

    public struct FixupTarget
    {
        /// <summary>
        /// Gets or sets the REFERENT of TARGET. This must be one of the
        /// following:
        /// UInt16 -- stores the frame number of an absolute frame.
        /// SegmentDefinition -- specifies a segment (SEG).
        /// GroupDefinition -- specifies a group (GRP).
        /// ExternalNameDefinition -- specifies an external name (EXT).
        /// </summary>
        public object Referent { get; set; }

        /// <summary>
        /// Gets or sets the displacement of TARGET relative to REFERENT.
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

    /// <summary>
    /// Specifies the format in which a fixup TARGET is stored in a FIXUPP
    /// record.
    /// </summary>
    public enum FixupTargetMethod : byte
    {
        /// <summary>
        /// T0: INDEX(SEGDEF),DISP -- The TARGET is the DISP'th byte in the
        /// LSEG (logical segment) identified by the INDEX.
        /// </summary>
        SegmentPlusDisplacement = 0,

        /// <summary>
        /// T1: INDEX(GRPDEF),DISP -- The TARGET is the DISP'th byte following
        /// the first byte in the group identified by the INDEX.
        /// </summary>
        GroupPlusDisplacement = 1,

        /// <summary>
        /// T2: INDEX(EXTDEF),DISP -- The TARGET is the DISP'th byte following
        /// the byte whose address is (eventuall) given by the External Name
        /// identified by the INDEX.
        /// </summary>
        ExternalPlusDisplacement = 2,

        /// <summary>
        /// (Not supported by Microsoft)
        /// T3: FRAME,DISP -- The TARGET is the DISP'th byte in FRAME, i.e.
        /// the address of TARGET is [FRAME*16+DISP].
        /// </summary>
        Absolute = 3,

        /// <summary>
        /// T4: INDEX(SEGDEF),0 -- The TARGET is the first byte in the LSEG
        /// (logical segment) identified by the INDEX.
        /// </summary>
        SegmentWithoutDisplacement = 4,

        /// <summary>
        /// T5: INDEX(GRPDEF),0 -- The TARGET is the first byte in the group
        /// identified by the INDEX.
        /// </summary>
        GroupWithoutDisplacement = 5,

        /// <summary>
        /// T6: INDEX(EXTDEF),0 -- The TARGET is the byte whose address is
        /// (eventually given by) the External Name identified by the INDEX.
        /// </summary>
        ExternalWithoutDisplacement = 6,
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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Disassembler.Omf
{
    /// <summary>
    /// Contains information that allows the linker to resolve (fix up) and
    /// eventually relocate references between object modules. FIXUPP records
    /// describe the LOCATION of each address value to be fixed up, the TARGET
    /// address to which the fixup refers, and the FRAME relative to which the
    /// address computation is performed.
    /// </summary>
    public class FixupRecord : Record
    {
        internal ThreadDefinition[] Threads { get; private set; }
        internal FixupDefinition[] Fixups { get; private set; }

        internal FixupRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            List<ThreadDefinition> threads = new List<ThreadDefinition>();
            List<FixupDefinition> fixups = new List<FixupDefinition>();
            while (!reader.IsEOF)
            {
                byte b = reader.PeekByte();
                if ((b & 0x80) == 0)
                {
                    ThreadDefinition thread = ParseThreadSubrecord(reader);
                    threads.Add(thread);
                    if (thread.Kind == FixupThreadKind.Target)
                        context.TargetThreads[thread.ThreadNumber] = thread;
                    else
                        context.FrameThreads[thread.ThreadNumber] = thread;
                }
                else
                {
                    FixupDefinition fixup = ParseFixupSubrecord(reader, context);
                    fixups.Add(fixup);

                    if (context.LastRecord is LEDATARecord)
                    {
                        var r = (LEDATARecord)context.LastRecord;
                        Fixup f = ConvertFixupDefinition(fixup, r, context);
                        r.Segment.Image.Fixups.Add(f);
                    }
                    else if (context.LastRecord is LogicalIteratedDataRecord)
                    {
                    }
                    else if (context.LastRecord is COMDATRecord)
                    {
                    }
                    else
                    {
                        throw new InvalidDataException("FIXUPP record must follow LEDATA, LIDATA, or COMDAT record.");
                    }
                }
            }

            this.Threads = threads.ToArray();
            this.Fixups = fixups.ToArray();
        }

        private ThreadDefinition ParseThreadSubrecord(RecordReader reader)
        {
            ThreadDefinition thread = new ThreadDefinition();

            byte b = reader.ReadByte();
            thread.Kind = ((b & 0x40) == 0) ? FixupThreadKind.Target : FixupThreadKind.Frame;
            thread.Method = (byte)((b >> 2) & 3);
            thread.ThreadNumber = (byte)(b & 3);

            if (thread.Method <= 2) // TBD: should be 3 for intel
                thread.IndexOrFrame = reader.ReadIndex();

            thread.IsDefined = true;
            return thread;
        }

        private FixupDefinition ParseFixupSubrecord(RecordReader reader, RecordContext context)
        {
            FixupDefinition fixup = new FixupDefinition();

            byte b1 = reader.ReadByte();
            byte b2 = reader.ReadByte();
            UInt16 w = (UInt16)((b1 << 8) | b2); // big endian

            fixup.Mode = (w & 0x4000) != 0 ? FixupMode.SegmentRelative : FixupMode.SelfRelative;
            fixup.Location = (FixupLocation)((w >> 10) & 0x0F);
            fixup.DataOffset = (UInt16)(w & 0x03FF);

            byte b = reader.ReadByte();
            bool useFrameThread = (b & 0x80) != 0;
            if (useFrameThread)
            {
                int frameNumber = (b >> 4) & 0x3;
                ThreadDefinition thread = context.FrameThreads[frameNumber];
                if (!thread.IsDefined)
                    throw new InvalidDataException("Frame thread " + frameNumber + " is not defined.");

                FixupFrame spec = new FixupFrame();
                spec.Method = (FixupFrameMethod)thread.Method;
                spec.IndexOrFrame = thread.IndexOrFrame;
                fixup.Frame = spec;
            }
            else
            {
                FixupFrame spec = new FixupFrame();
                spec.Method = (FixupFrameMethod)((b >> 4) & 7);
                if ((int)spec.Method <= 3)
                {
                    spec.IndexOrFrame = reader.ReadIndex();
                }
                fixup.Frame = spec;
            }

            bool useTargetThread = (b & 0x08) != 0;
            if (useTargetThread)
            {
                bool hasTargetDisplacement = (b & 0x04) != 0;
                int targetNumber = b & 3;
                ThreadDefinition thread = context.TargetThreads[targetNumber];
                if (!thread.IsDefined)
                    throw new InvalidDataException("Target thread " + targetNumber + " is not defined.");

                FixupTarget spec = new FixupTarget();
                spec.Method = (FixupTargetMethod)((int)thread.Method & 3);
                if (hasTargetDisplacement)
                    spec.Method = (FixupTargetMethod)((int)spec.Method | 4);
                spec.IndexOrFrame = thread.IndexOrFrame;
                if ((int)spec.Method <= 3)
                {
                    spec.Displacement = reader.ReadUInt16Or32();
                }
                fixup.Target = spec;
            }
            else
            {
                FixupTarget spec = new FixupTarget();
                spec.Method = (FixupTargetMethod)(b & 7);
                spec.IndexOrFrame = reader.ReadIndex();
                if ((int)spec.Method <= 3)
                {
                    spec.Displacement = reader.ReadUInt16Or32();
                }
                fixup.Target = spec;
            }
            return fixup;
        }

        private Fixup ConvertFixupDefinition(
            FixupDefinition fixup, LEDATARecord r, RecordContext context)
        {
            Fixup f = new Fixup();
            f.StartIndex = fixup.DataOffset + (int)r.DataOffset;
            switch (fixup.Location)
            {
                case FixupLocation.LowByte:
                    f.LocationType = FixupLocationType.LowByte;
                    break;
                case FixupLocation.Offset:
                case FixupLocation.LoaderResolvedOffset:
                    f.LocationType = FixupLocationType.Offset;
                    break;
                case FixupLocation.Base:
                    f.LocationType = FixupLocationType.Base;
                    break;
                case FixupLocation.Pointer:
                    f.LocationType = FixupLocationType.Pointer;
                    break;
                default:
                    throw new InvalidDataException("The fixup location is not supported.");
            }
            f.Mode = (fixup.Mode == FixupMode.SelfRelative) ?
                Disassembler.FixupMode.SelfRelative :
                Disassembler.FixupMode.SegmentRelative;

            IAddressable referent;
            switch (fixup.Target.Method)
            {
                case FixupTargetMethod.SegmentPlusDisplacement:
                case FixupTargetMethod.SegmentWithoutDisplacement:
                    referent = context.Module.Segments[fixup.Target.IndexOrFrame - 1];
                    break;
                case FixupTargetMethod.GroupPlusDisplacement:
                case FixupTargetMethod.GroupWithoutDisplacement:
                    referent = context.Module.Groups[fixup.Target.IndexOrFrame - 1];
                    break;
                case FixupTargetMethod.ExternalPlusDisplacement:
                case FixupTargetMethod.ExternalWithoutDisplacement:
                    referent = context.Module.ExternalNames[fixup.Target.IndexOrFrame - 1];
                    break;
                case FixupTargetMethod.Absolute:
                    referent = new PhysicalAddress(fixup.Target.IndexOrFrame, 0);
                    break;
                default:
                    throw new InvalidDataException("Unsupported fixup method.");
            }
            f.Target = new SymbolicTarget
            {
                Referent = referent,
                Displacement = fixup.Target.Displacement
            };
            //f.Frame = null;
            return f;
        }
    }

    /// <summary>
    /// A THREAD definition works like "preset" for FIXUPP records. Instead
    /// of explicitly specifying how to do the fix-up in the FIXUPP record,
    /// it could instead refer to a previously defined THREAD and use the
    /// fix-up settings defined in the THREAD.
    /// 
    /// There are four TARGET threads (numbered 0-3) and four FRAME threads
    /// (numbered 0-3). So at any time, a maximum of 8 threads are available.
    /// If a thread with the same number is defined again, it overwrites the
    /// previous definition.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal struct ThreadDefinition
    {
        public bool IsDefined { get; internal set; } // whether this entry is defined
        public byte ThreadNumber { get; internal set; } // 0 - 3
        public FixupThreadKind Kind { get; internal set; }

        public byte Method { get; internal set; } // target method or frame method
        public UInt16 IndexOrFrame { get; internal set; }
    }

    public enum FixupThreadKind : byte
    {
        Target = 0,
        Frame = 1
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class FixupDefinition
    {
        public UInt16 DataOffset { get; internal set; } // indicates where to fix up
        public FixupLocation Location { get; internal set; } // indicates what to fix up
        public FixupMode Mode { get; internal set; }
        public FixupTarget Target { get; internal set; }
        public FixupFrame Frame { get; internal set; }

        public int StartIndex { get { return DataOffset; } }
        public int EndIndex { get { return StartIndex + Length; } }

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
    }

    public enum FixupMode : byte
    {
        SelfRelative = 0,
        SegmentRelative = 1
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

    public struct FixupTarget
    {
        public FixupTargetMethod Method { get; internal set; }

        /// <summary>
        /// Gets or sets the INDEX of the SEG/GRP/EXT item that is used as
        /// the referent to find the target. If Method is Absolute, this
        /// contains the frame number.
        /// </summary>
        public UInt16 IndexOrFrame { get; internal set; }
        public UInt32 Displacement { get; internal set; }

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
    }

    /// <summary>
    /// Specifies how to determine the TARGET of a fixup.
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

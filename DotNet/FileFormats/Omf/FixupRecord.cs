using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace FileFormats.Omf.Records
{
    /// <summary>
    /// Contains information that allows the linker to resolve (fix up) and
    /// eventually relocate references between object modules. FIXUPP records
    /// describe the LOCATION of each address value to be fixed up, the TARGET
    /// address to which the fixup refers, and the FRAME relative to which the
    /// address computation is performed.
    /// </summary>
    class FixupRecord : Record
    {
        public FixupThreadDefinition[] Threads { get; private set; }
        public FixupDefinition[] Fixups { get; private set; }

        public FixupRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            List<FixupThreadDefinition> threads = new List<FixupThreadDefinition>();
            List<FixupDefinition> fixups = new List<FixupDefinition>();
            while (!reader.IsEOF)
            {
                byte b = reader.PeekByte();
                if ((b & 0x80) == 0)
                {
                    FixupThreadDefinition thread = ParseThreadSubrecord(reader);
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
                        fixup.DataOffset += (ushort)r.DataOffset;
                        r.Segment.Fixups.Add(fixup);
                    }
                    else if (context.LastRecord is LIDATARecord)
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

        private FixupThreadDefinition ParseThreadSubrecord(RecordReader reader)
        {
            FixupThreadDefinition thread = new FixupThreadDefinition();

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
                FixupThreadDefinition thread = context.FrameThreads[frameNumber];
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
                FixupThreadDefinition thread = context.TargetThreads[targetNumber];
                if (!thread.IsDefined)
                    throw new InvalidDataException("Target thread " + targetNumber + " is not defined.");

                FixupTargetMethod method = (FixupTargetMethod)((int)thread.Method & 3);
                if (hasTargetDisplacement)
                    method |= (FixupTargetMethod)4;

                FixupTarget spec = new FixupTarget();
                spec.Referent = ResolveFixupReferent(context, method, thread.IndexOrFrame);
                if ((int)method <= 3)
                {
                    spec.Displacement = reader.ReadUInt16Or32();
                }
                fixup.Target = spec;
            }
            else
            {
                FixupTargetMethod method = (FixupTargetMethod)(b & 7);
                UInt16 indexOrFrame = reader.ReadIndex();

                FixupTarget spec = new FixupTarget();
                spec.Referent = ResolveFixupReferent(context, method, indexOrFrame);
                if ((int)method <= 3)
                {
                    spec.Displacement = reader.ReadUInt16Or32();
                }
                fixup.Target = spec;
            }
            return fixup;
        }

        private static object ResolveFixupReferent(
            RecordContext context, FixupTargetMethod method, UInt16 indexOrFrame)
        {
            switch (method)
            {
                case FixupTargetMethod.Absolute:
                    return indexOrFrame;
                case FixupTargetMethod.SegmentPlusDisplacement:
                case FixupTargetMethod.SegmentWithoutDisplacement:
                    return context.Segments[indexOrFrame - 1];
                case FixupTargetMethod.GroupPlusDisplacement:
                case FixupTargetMethod.GroupWithoutDisplacement:
                    return context.Groups[indexOrFrame - 1];
                case FixupTargetMethod.ExternalPlusDisplacement:
                case FixupTargetMethod.ExternalWithoutDisplacement:
                    return context.ExternalNames[indexOrFrame - 1];
                default:
                    throw new InvalidDataException("Invalid fixup target method: " + method);
            }
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
    struct FixupThreadDefinition
    {
        public bool IsDefined { get; internal set; } // whether this entry is defined
        public byte ThreadNumber { get; internal set; } // 0 - 3
        public FixupThreadKind Kind { get; internal set; }

        public byte Method { get; internal set; } // target method or frame method
        public UInt16 IndexOrFrame { get; internal set; }
    }

    enum FixupThreadKind : byte
    {
        Target = 0,
        Frame = 1
    }
}

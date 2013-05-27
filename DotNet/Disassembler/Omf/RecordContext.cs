using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2.Omf
{
    /// <summary>
    /// Contains context information to assist reading and writing records.
    /// </summary>
    class RecordContext
    {
        // Populated by THEADR records.
        public string ObjectName;
        
        // Populated by COMENT/LIBMOD subrecords.
        public string SourceName;

        // Populated by LNAMES records.
        public readonly List<string> Names = new List<string>();

        // Populated by SEGDEF records.
        public readonly List<SegmentDefinition> Segments =
            new List<SegmentDefinition>();

        // Populated by GRPDEF records.
        public readonly List<GroupDefinition> Groups =
            new List<GroupDefinition>();

        // Populated by EXTDEF, LEXTDEF, CEXTDEF, COMDEF, and LCOMDEF records.
        public readonly List<ExternalNameDefinition> ExternalNames =
            new List<ExternalNameDefinition>();

        // Populated by PUBDEF and LPUBDEF records.
        public readonly List<PublicNameDefinition> PublicNames =
            new List<PublicNameDefinition>();

#if false
        // Populated by COMDEF and LCOMDEF records.
        public readonly List<CommunalNameDefinition> CommunalNames =
            new List<CommunalNameDefinition>();
#endif

        // Populated by ALIAS records.
        public readonly List<AliasDefinition> Aliases =
            new List<AliasDefinition>();

        // FRAME threads.
        public readonly FixupThreadDefinition[] FrameThreads = new FixupThreadDefinition[4];

        // TARGET threads.
        public readonly FixupThreadDefinition[] TargetThreads = new FixupThreadDefinition[4];

        // Contains the last record.
        public Record LastRecord = null;
    }

    class GroupDefinition
    {
        public string Name;
        public readonly List<SegmentDefinition> Segments = 
            new List<SegmentDefinition>();
    }

    class NameDefinition
    {
        public string Name;
        public UInt16 TypeIndex;
        public RecordNumber DefinedBy;
    }

    class ExternalNameDefinition : NameDefinition
    {
    }

    class PublicNameDefinition : NameDefinition
    {
        public GroupDefinition BaseGroup;
        public SegmentDefinition BaseSegment;
        public UInt16 BaseFrame;
        public int Offset;
    }

    class CommunalNameDefinition : ExternalNameDefinition
    {
        public byte DataType;
        public UInt32 ElementCount;
        public UInt32 ElementSize;
    }

    class AliasDefinition
    {
        public string AliasName;
        public string SubstituteName;
    }
}

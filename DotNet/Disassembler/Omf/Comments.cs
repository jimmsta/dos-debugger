using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler.Omf
{
    public class CommentRecord : Record
    {
        public bool IsPreserved { get; private set; }
        public bool IsHidden { get; private set; }
        public Comment Comment { get; private set; }

        internal CommentRecord(RecordReader reader, RecordContext context)
            : base(reader, context)
        {
            byte commentType = reader.ReadByte();
            this.IsPreserved = (commentType & 0x80) != 0;
            this.IsHidden = (commentType & 0x40) != 0;

            byte commentClass = reader.ReadByte();
            switch (commentClass)
            {
                case 0:
                    Comment = new TextComment(reader, "Translator");
                    break;
                case 1:
                    Comment = new TextComment(reader, "Copyright");
                    break;
                case 0x81:
                case 0x9F:
                    Comment = new TextComment(reader, "DefaultLibrarySearchName");
                    break;
                case 0x9C: // not supported
                    break;
                case 0x9D:
                    Comment = new MemoryModelComment(reader);
                    break;
                case 0x9E:
                    Comment = new DOSSEGComment(reader);
                    break;
                case 0xA0: // OMF Extensions
                    Comment = ParseOmfExtensions(reader);
                    break;
                case 0xA1: // debug symbol type, such as CV; ignored
                    break;
                case 0xA2:
                    Comment = new LinkPassSeparatorComment(reader);
                    break;
                case 0xA3:
                    Comment = new LIBMODComment(reader, context);
                    break;
                case 0xA4: // EXESTR
                    break;
                case 0xA6: // INCERR
                    break;
                case 0xA7: // NOPAD
                    break;
                case 0xA8: // WKEXT
                    Comment = new WKEXTComment(reader);
                    break;
                case 0xA9: // LZEXT
                    break;
                case 0xDA:
                    Comment = new TextComment(reader, "Comment");
                    break;
                case 0xDB:
                    Comment = new TextComment(reader, "Compiler");
                    break;
                case 0xDC:
                    Comment = new TextComment(reader, "Date");
                    break;
                case 0xDD:
                    Comment = new TextComment(reader, "Timestamp");
                    break;
                case 0xDF:
                    Comment = new TextComment(reader, "User");
                    break;
                case 0xE9: // borland
                    Comment = new TextComment(reader, "Dependencies");
                    break;
                case 0xFF: // MS QuickC
                    Comment = new TextComment(reader, "CommandLine");
                    break;
            }
            if (Comment == null)
            {
                Comment = new UnknownComment(reader, commentClass);
            }
        }

        /// <summary>
        /// The presence of any unrecognized subtype causes the linker to
        /// generate a fatal error.
        /// </summary>
        private static Comment ParseOmfExtensions(RecordReader reader)
        {
            byte subtype = reader.ReadByte();
            return null;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class Comment
    {
    }

    public class UnknownComment : Comment
    {
        public byte CommentClass { get; private set; }
        public byte[] Data { get; private set; }

        internal UnknownComment(RecordReader reader, byte commentClass)
        {
            this.CommentClass = commentClass;
            this.Data = reader.ReadToEnd();
        }

        public override string ToString()
        {
            return string.Format("Class 0x{0:X2}", CommentClass);
        }
    }

    public class TextComment : Comment
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        internal TextComment(RecordReader reader,string key)
        {
            this.Key = key;
            this.Value = reader.ReadToEndAsString();
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", Key, Value);
        }
    }

    public class MemoryModelComment : Comment
    {
        public int InstructionSet { get; private set; } // e.g. 8086, 80286
        public bool Optimized { get; private set; }
        public MemoryModel MemoryModel { get; private set; }

        internal MemoryModelComment(RecordReader reader)
        {
            while (!reader.IsEOF)
            {
                byte b = reader.ReadByte();
                switch ((char)b)
                {
                    case '0': InstructionSet = 8086; break;
                    case '1': InstructionSet = 80186; break;
                    case '2': InstructionSet = 80286; break;
                    case '3': InstructionSet = 80386; break;

                    case 'O': Optimized = true; break;

                    case 's': MemoryModel = MemoryModel.Small; break;
                    case 'm': MemoryModel = MemoryModel.Medium; break;
                    case 'c': MemoryModel = MemoryModel.Compact; break;
                    case 'l': MemoryModel = MemoryModel.Large; break;
                    case 'h': MemoryModel = MemoryModel.Huge; break;

                    case 'A': InstructionSet = 68000; break;
                    case 'B': InstructionSet = 68010; break;
                    case 'C': InstructionSet = 68020; break;
                    case 'D': InstructionSet = 68030; break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (InstructionSet != 0)
            {
                sb.Append(InstructionSet);
            }
            if (MemoryModel != MemoryModel.Unknown)
            {
                if (sb.Length > 0)
                    sb.Append(',');
                sb.Append(MemoryModel.ToString());
            }
            if (Optimized)
            {
                if (sb.Length > 0)
                    sb.Append(',');
                sb.Append("Optimized");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a DOSSEG comment. See Remarks for details.
    /// </summary>
    /// <remarks>
    /// The DOSSEG option forces segments to be ordered as follows:
    /// 1. All segments with a class name ending in CODE.
    /// 2. All other segments outside DGROUP.
    /// 3. DGROUP segments in the following order:
    ///    a. Any segments of class BEGDATA.
    ///    b. Any segments not of class BEGDATA, BSS, or STACK.
    ///    c. Segments of class BSS.
    ///    d. Segments of class STACK.
    ///
    /// In addition, the DOSSEG option defines the following two labels:
    /// __edata = DGROUP : BSS
    /// __end = DGROUP : STACK.
    ///
    /// The DOSSEG option also inserts 16 null bytes at the beginning of
    /// the _TEXT segment (if this segment is defined); unassigned pointers
    /// point to this area. This behavior of the option is overridden by the
    /// /NONULLS option when both are used; use /NONULLS to override the
    /// DOSSEG comment record commonly found in standard Microsoft libraries.
    /// </remarks>
    public class DOSSEGComment : Comment
    {
        internal DOSSEGComment(RecordReader reader)
        {
        }

        public override string ToString()
        {
            return "DOSSEG";
        }
    }

    public class LinkPassSeparatorComment : Comment
    {
        public bool IsPresent { get; private set; }

        internal LinkPassSeparatorComment(RecordReader reader)
        {
            byte subtype = reader.ReadByte();
            IsPresent = (subtype == 1);
        }
    }

    /// <summary>
    /// Specifies the name of an object module within a library, allowing the
    /// librarian to preserve the source filename in the THEADR record and
    /// still identify the module names that make up the library.
    /// </summary>
    public class LIBMODComment : Comment
    {
        public string ModuleName { get; private set; }

        internal LIBMODComment(RecordReader reader, RecordContext context)
        {
            this.ModuleName = reader.ReadPrefixedString();
            context.Module.ObjectName = ModuleName;
        }

        public override string ToString()
        {
            return "ModuleName=" + ModuleName;
        }
    }

    public class WKEXTComment : Comment
    {
        public WeakExternalDefinition[] Definitions { get; private set; }

        internal WKEXTComment(RecordReader reader)
        {
            var defs = new List<WeakExternalDefinition>();
            while (!reader.IsEOF)
            {
                var def = new WeakExternalDefinition();
                def.WeakExternalIndex = reader.ReadIndex();
                def.DefaultResolutionIndex = reader.ReadIndex();
                defs.Add(def);
            }
            this.Definitions = defs.ToArray();
            // TODO: add this information into the Context
        }
    }

    public struct WeakExternalDefinition
    {
        public UInt16 WeakExternalIndex { get; internal set; }
        public UInt16 DefaultResolutionIndex { get; internal set; }

        public override string ToString()
        {
            return string.Format("{0} => {1}",
                WeakExternalIndex,
                DefaultResolutionIndex);
        }
    }
}

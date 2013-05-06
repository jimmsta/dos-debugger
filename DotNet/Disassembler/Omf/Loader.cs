using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace Disassembler.Omf
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OmfLoader
    {
        private ObjectLibrary library;

        public ObjectLibrary Library
        {
            //get { return new ReadOnlyCollection<OmfRecord>(records); }
            get { return library; }
        }

        public OmfLoader(string fileName)
        {
            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                this.library = LoadLibrary(reader);
            }
        }

        /// <summary>
        /// Returns null if LibraryEnd record is encountered before
        /// MODEND or MODEND32 record is encountered.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectModule LoadObject(BinaryReader reader)
        {
            ObjectModule module = new ObjectModule();
            List<Omf.Record> records = new List<Omf.Record>();
            Omf.RecordContext context = new Omf.RecordContext(module);
            module.Context = context;

            while (true)
            {
                Omf.Record record = Omf.Record.ReadRecord(reader, context);
                records.Add(record);

                if (record.RecordNumber == Omf.RecordNumber.MODEND ||
                    record.RecordNumber == Omf.RecordNumber.MODEND32)
                {
                    break;
                }
                if (record.RecordNumber == Omf.RecordNumber.LibraryEnd)
                {
                    return null;
                }
            }
            module.Records = records.ToArray();

            // Convert the records to the information we need.
#if false
            foreach (Omf.Record r in records)
            {
                if (r.RecordNumber == Omf.RecordNumber.THEADR)
                {
                    //module.Name = ((Omf.TranslatorHeaderRecord)r).Name;
                    break;
                }
            }
#endif

            return module;
        }

        public static ObjectLibrary LoadLibrary(BinaryReader reader)
        {
            List<ObjectModule> modules = new List<ObjectModule>();

            Omf.LibraryHeaderRecord r = (Omf.LibraryHeaderRecord)
                Omf.Record.ReadRecord(reader, null, Omf.RecordNumber.LibraryHeader);
            int pageSize = r.PageSize;

            while (true)
            {
                ObjectModule module = LoadObject(reader);
                if (module == null) // LibraryEndRecord encountered
                {
                    break;
                }
                modules.Add(module);

                // Since a LIB file consists of multiple object modules
                // aligned on page boundary, we need to consume the padding
                // bytes if present.
                int mod = (int)(reader.BaseStream.Position % pageSize);
                if (mod != 0)
                {
                    reader.ReadBytes(pageSize - mod);
                }
            }

            // The dictionary follows, but we ignore it.
            return new ObjectLibrary { Modules = modules.ToArray() };
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectModule
    {
        internal Omf.RecordContext Context { get; set; }

        internal readonly List<LogicalSegment> segments
            = new List<LogicalSegment>();

        internal readonly List<GroupDefinition> groups
            = new List<GroupDefinition>();

        internal readonly List<PublicNameDefinition> publicNames
            = new List<PublicNameDefinition>();

        public Record[] Records { get; internal set; }

        /// <summary>
        /// Gets the name of the object module in the library. This name is
        /// defined by the LIBMOD subrecord of COMENT.
        /// </summary>
        public string ObjectName { get; internal set; }

        /// <summary>
        /// Gets the source file name of the object module. This name is
        /// defined in the THEADR record.
        /// </summary>
        public string SourceName { get; internal set; }

        /// <summary>
        /// Gets a list of logical segments defined in this module. A logical
        /// segment is defined by SEGDEF records.
        /// </summary>
        public LogicalSegment[] Segments
        {
            get { return segments.ToArray(); }
        }

        /// <summary>
        /// Gets a list of groups defined in this module. A group is defined
        /// by GRPDEF records.
        /// </summary>
        public GroupDefinition[] Groups
        {
            get { return groups.ToArray(); }
        }

        public ExternalNameDefinition[] ExternalNames
        {
            get { return Context.ExternalNames.ToArray(); }
        }

        public PublicNameDefinition[] PublicNames
        {
            get { return publicNames.ToArray(); }
        }

        [Browsable(false)]
        public ExternalNameDefinition[] LocalExternalNames
        {
            get { return Context.LocalExternalNames.ToArray(); }
        }

        [Browsable(false)]
        public Omf.PublicNameDefinition[] LocalPublicNames
        {
            get { return Context.LocalPublicNames.ToArray(); }
        }

        public override string ToString()
        {
            if (this.ObjectName == null)
                return this.SourceName;
            else
                return string.Format("{0} ({1})", this.ObjectName, this.SourceName);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectLibrary
    {
        public ObjectModule[] Modules { get; internal set; }

        public Dictionary<string, List<ObjectModule>> DuplicateSymbols
            = new Dictionary<string, List<ObjectModule>>();

        public Dictionary<string, List<ObjectModule>> UnresolvedSymbols
            = new Dictionary<string, List<ObjectModule>>();

        

        public void BuildDependencyGraph()
        {
            // First, we need to build a map of each public name.
            var nameDefs = new Dictionary<string, ObjectModule>();
            foreach (var module in Modules)
            {
                foreach (var name in module.PublicNames)
                {
                    if (nameDefs.ContainsKey(name.Name))
                    {
                        var prevDef = nameDefs[name.Name];
                    }
                    nameDefs[name.Name] = module;
                }
            }

            // Create a dummy node for "unresolved external symbols".
            // ...

            // Next, we create an edge for each external symbol reference.
            foreach (var module in Modules)
            {
                foreach (var name in module.ExternalNames)
                {
                    ObjectModule defModule;
                    if (nameDefs.TryGetValue(name.Name, out defModule))
                    {
                        // ...
                    }
                    else // unresolved external symbol
                    {
                        // ...
                        List<ObjectModule> list;
                        if (!UnresolvedSymbols.TryGetValue(name.Name, out list))
                        {
                            list = new List<ObjectModule>();
                            UnresolvedSymbols.Add(name.Name, list);
                        }
                        list.Add(module);
                    }
                }
            }
        }
    }
}

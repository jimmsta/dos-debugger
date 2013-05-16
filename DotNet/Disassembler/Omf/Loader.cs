using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Disassembler.Omf
{
    public static class OmfLoader
    {
        /// <summary>
        /// Returns null if LibraryEnd record is encountered before
        /// MODEND or MODEND32 record is encountered.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectModule LoadObject(BinaryReader reader)
        {
            ObjectModule module = new ObjectModule();
            List<Record> records = new List<Record>();
            RecordContext context = new RecordContext(module);

            while (true)
            {
                Record record = Record.ReadRecord(reader, context);
                records.Add(record);

                if (record.RecordNumber == RecordNumber.MODEND ||
                    record.RecordNumber == RecordNumber.MODEND32)
                {
                    break;
                }
                if (record.RecordNumber == RecordNumber.LibraryEnd)
                {
                    return null;
                }
            }
            module.Records = records.ToArray();
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

        public static ObjectLibrary LoadLibrary(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return LoadLibrary(reader);
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Browsable(true)]
    public class ObjectModule
    {
        readonly List<LogicalSegment> segments = new List<LogicalSegment>();
        readonly List<SegmentGroup> groups = new List<SegmentGroup>();
        readonly List<DefinedSymbol> definedNames = new List<DefinedSymbol>();
        readonly List<ExternalSymbol> externalNames = new List<ExternalSymbol>();
        readonly List<SymbolAlias> aliases = new List<SymbolAlias>();

        public Record[] Records { get; internal set; }

        /// <summary>
        /// Gets the name of the object module in the library. This name is
        /// defined by the LIBMOD subrecord of COMENT.
        /// </summary>
        [Browsable(true)]
        public string ObjectName { get; internal set; }

        /// <summary>
        /// Gets the source file name of the object module. This name is
        /// defined in the THEADR record.
        /// </summary>
        [Browsable(true)]
        public string SourceName { get; internal set; }

        /// <summary>
        /// Gets a list of logical segments defined in this module. A logical
        /// segment is defined by SEGDEF records.
        /// </summary>
        public List<LogicalSegment> Segments
        {
            get { return segments; }
        }

        /// <summary>
        /// Gets a list of groups defined in this module. A group is defined
        /// by GRPDEF records.
        /// </summary>
        public List<SegmentGroup> Groups
        {
            get { return groups; }
        }

        /// <summary>
        /// Gets a list of external names used in this module, including:
        /// EXTDEF  -- refers to public names in other modules
        /// LEXTDEF -- refers to a local name defined in this module
        /// CEXTDEF -- refers to a COMDAT name defined in another module 
        ///            (by COMDEF) or in this module (by LCOMDEF)
        /// </summary>
        public List<ExternalSymbol> ExternalNames
        {
            get { return externalNames; }
        }

        //[TypeConverter(typeof(CollectionConverter))]
        [Browsable(true)]
        public List<DefinedSymbol> DefinedNames
        {
            get { return definedNames; }
        }

        /// <summary>
        /// Gets a list of symbol aliases defined in this object module.
        /// </summary>
        [Browsable(true)]
        public List<SymbolAlias> Aliases
        {
            get { return aliases; }
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
        //[TypeConverter(typeof(ExpandableObjectConverter))]
        //[TypeConverter(typeof(ArrayConverter))]
        //[TypeConverter(typeof(CollectionConverter))]
        //[TypeConverter(typeof(ExpandableCollectionConverter))]
        [Browsable(true)]
        public ObjectModule[] Modules { get; internal set; }

        [Browsable(true)]
        public ListWrapper ModuleList
        {
            get { return new ListWrapper { Collection = Modules }; }
        }

        public Dictionary<string, List<ObjectModule>> DuplicateSymbols
            = new Dictionary<string, List<ObjectModule>>();

        public Dictionary<string, List<ObjectModule>> UnresolvedSymbols
            = new Dictionary<string, List<ObjectModule>>();

        public void ResolveAllSymbols()
        {
            // First, we need to build a map of each public name.
            var nameDefs = new Dictionary<string, ObjectModule>();
            foreach (var module in Modules)
            {
                foreach (var name in module.DefinedNames)
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

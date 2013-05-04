using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace Disassembler
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class OmfLoader
    {
        private ObjectModule[] modules;

        public ObjectModule[] Modules
        {
            //get { return new ReadOnlyCollection<OmfRecord>(records); }
            get { return modules; }
        }

        public OmfLoader(string fileName)
        {
            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                this.modules = LoadLibrary(reader);
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
            Omf.RecordContext context = new Omf.RecordContext();

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
            foreach (Omf.Record r in records)
            {
                if (r.RecordNumber == Omf.RecordNumber.THEADR)
                {
                    module.Name = ((Omf.TranslatorHeaderRecord)r).Name;
                    break;
                }
            }

            return module;
        }

        public static ObjectModule[] LoadLibrary(BinaryReader reader)
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
            return modules.ToArray();
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectModule
    {
        public string Name { get; internal set; }
        public Omf.Record[] Records { get; internal set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}

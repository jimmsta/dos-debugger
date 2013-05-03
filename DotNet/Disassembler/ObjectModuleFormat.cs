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
        private Omf.Record[] records;

        public Omf.Record[] Records
        {
            //get { return new ReadOnlyCollection<OmfRecord>(records); }
            get { return records; }
        }

        public OmfLoader(string fileName)
        {
            List<Omf.Record> records = new List<Omf.Record>();

            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Omf.RecordContext context = new Omf.RecordContext();

                // Read records.
                int pageSize = 1;
                while (stream.Position < stream.Length)
                {
                    Omf.Record record = Omf.Record.ReadRecord(reader, context);
                    records.Add(record);

                    if (record.RecordNumber == Omf.RecordNumber.LibraryHeader)
                    {
                        pageSize = ((Omf.LibraryHeaderRecord)record).PageSize;
                    }
                    else if (record.RecordNumber == Omf.RecordNumber.MODEND
                          || record.RecordNumber == Omf.RecordNumber.MODEND32)
                    {
                        // This is the last record of an object module. Since
                        // a LIB file consists of multiple object modules
                        // aligned on page boundary, we need to consume the
                        // padding bytes if present.
                        int mod = (int)(stream.Position % pageSize);
                        if (mod != 0)
                        {
                            reader.ReadBytes(pageSize - mod);
                        }
                    }
                    else if (record.RecordNumber == Omf.RecordNumber.LibraryEnd)
                    {
                        break;
                    }
                }
                this.records = records.ToArray();

                // The dictionary follows, but we ignore it.
            }
        }

        public static ObjectModule LoadObject(Stream stream)
        {
            throw new NotImplementedException();
        }

        public static ObjectModule[] LoadLibrary(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectModule
    {
    }
}

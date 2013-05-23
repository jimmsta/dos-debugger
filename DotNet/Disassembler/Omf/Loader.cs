using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Disassembler2.Omf
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
            ObjectLibrary library = new ObjectLibrary();

            LibraryHeaderRecord r = (LibraryHeaderRecord)
                Record.ReadRecord(reader, null, RecordNumber.LibraryHeader);
            int pageSize = r.PageSize;

            while (true)
            {
                ObjectModule module = LoadObject(reader);
                if (module == null) // LibraryEndRecord encountered
                {
                    break;
                }
                library.Modules.Add(module);

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
            return library;
        }

        public static ObjectLibrary LoadLibrary(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ObjectLibrary library = LoadLibrary(reader);
                library.AssignIdsToSegments();
                return library;
            }
        }
    }
}

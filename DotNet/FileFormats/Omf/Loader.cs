using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FileFormats.Omf.Records;

namespace FileFormats.Omf
{
    public static class OmfLoader
    {
#if false
        /// <summary>
        /// Loads a LIB file from disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ObjectLibrary LoadLibrary(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            using (Stream stream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ObjectLibrary library = LoadLibrary(reader);
                library.AssignIdsToSegments();
                library.FileName = fileName;
                return library;
            }
        }
#endif

#if false
        /// <summary>
        /// Loads a LIB file from a BinaryReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectLibrary LoadLibrary(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

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
#endif

        /// <summary>
        /// Loads an object module from binary reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>
        /// The loaded module if successful, or null if LibraryEndRecord is
        /// encountered before MODEND or MODEND32 record is encountered.
        /// </returns>
        public static RecordContext LoadObject(BinaryReader reader)
        {
            List<Record> records = new List<Record>();
            RecordContext context = new RecordContext();

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
            context.Records = records.ToArray();
            return context;
        }
    }
}

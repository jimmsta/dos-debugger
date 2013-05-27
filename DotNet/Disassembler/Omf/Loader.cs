using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Disassembler2.Omf
{
    public static class OmfLoader
    {
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

                // Convert OMF record to our format.
                switch (record.RecordNumber)
                {
                    case RecordNumber.THEADR:
                        module.SourceName = (record as THEADRRecord).Name;
                        break;
                    default:
                        //System.Diagnostics.Debug.WriteLine(string.Format(
                        //    "Record {0} is not interpreted.",
                        //    record.RecordNumber));
                        break;
                }
            }
            module.Records = records.ToArray();

            // TODO: add dictionary to map SegmentDefinition to Segment.
            Dictionary<GroupDefinition, SegmentGroup> groupMap;
            Dictionary<SegmentDefinition, LogicalSegment> segmentMap;
            Dictionary<ExternalNameDefinition, ExternalSymbol> externalMap;

            // Convert segment grouops.
            foreach (GroupDefinition def in context.Groups)
            {
                module.Groups.Add(new SegmentGroup
                {
                    Name = def.Name,
                    Segments = null // TBD
                });
            }

            // Convert external names.
            foreach (ExternalNameDefinition def in context.ExternalNames)
            {
                module.ExternalNames.Add(new ExternalSymbol
                {
                    Name = def.Name,
                    TypeIndex = def.TypeIndex,
                });
            }

            // Convert public names.
            foreach (PublicNameDefinition def in context.PublicNames)
            {
                module.DefinedNames.Add(new DefinedSymbol
                {
                    BaseGroup = null,
                    BaseSegment = null,
                    BaseFrame = def.BaseFrame,
                    Name = def.Name,
                    TypeIndex = def.TypeIndex,
                    Offset = (uint)def.Offset,
                    Scope = (def.DefinedBy == RecordNumber.LPUBDEF ||
                             def.DefinedBy == RecordNumber.LPUBDEF32) ?
                            SymbolScope.Private : SymbolScope.Public
                });
            }

            // Convert aliases.
            foreach (AliasDefinition def in context.Aliases)
            {
                module.Aliases.Add(new SymbolAlias
                {
                    AliasName = def.AliasName,
                    SubstituteName = def.SubstituteName
                });
            }

            return module;
        }

        private static Fixup ConvertFixupDefinition(
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
            f.Mode = fixup.Mode;

            IAddressReferent referent;
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
}

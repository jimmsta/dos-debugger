using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using FileFormats.Omf;

namespace Disassembler
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

            foreach (var context in FileFormats.Omf.OmfLoader.LoadLibrary(reader))
            {
                ObjectModule module = LoadObject(context);
                library.Modules.Add(module);
            }
            return library;
        }

        /// <summary>
        /// Returns null if LibraryEnd record is encountered before
        /// MODEND or MODEND32 record is encountered.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ObjectModule LoadObject(FileFormats.Omf.Records.RecordContext context)
        {
            ObjectModule module = new ObjectModule();

            Dictionary<object, object> objectMap =
                new Dictionary<object, object>();

            // Convert meta-data.
            module.Name = context.ObjectName;
            module.SourceName = context.SourceName;

            // Convert segments.
            foreach (SegmentDefinition def in context.Segments)
            {
                LogicalSegment segment = ConvertSegmentDefinition(def, objectMap, module);
                objectMap[def] = segment;
                module.Segments.Add(segment);
            }

            // Convert segment groups.
            foreach (GroupDefinition def in context.Groups)
            {
                SegmentGroup group = ConvertGroupDefinition(def, objectMap);
                module.Groups.Add(group);
                objectMap[def] = group;
            }

            // Convert external names.
            foreach (ExternalNameDefinition def in context.ExternalNames)
            {
                ExternalSymbol symbol = new ExternalSymbol
                {
                    Name = def.Name,
                    TypeIndex = def.TypeIndex,
                };
                module.ExternalNames.Add(symbol);
                objectMap[def] = symbol;
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

            // Convert public names.
            foreach (PublicNameDefinition def in context.PublicNames)
            {
                module.DefinedNames.Add(ConvertPublicNameDefinition(def, objectMap));
            }

            // Convert fixups.
            foreach (SegmentDefinition def in context.Segments)
            {
                LogicalSegment segment = (LogicalSegment)objectMap[def];
                foreach (FixupDefinition f in def.Fixups)
                {
                    segment.Fixups.Add(ConvertFixupDefinition(f, objectMap));
                }
            }

            return module;
        }

        private static LogicalSegment ConvertSegmentDefinition(
            SegmentDefinition def, Dictionary<object, object> objectMap,
            ObjectModule module)
        {
            // Convert the record.
            LogicalSegment segment = new LogicalSegment(def, objectMap, module);
            return segment;
        }

        private static Fixup ConvertFixupDefinition(
            FixupDefinition def, Dictionary<object, object> objectMap)
        {
            Fixup fixup = new Fixup();
            fixup.StartIndex = def.DataOffset;
            switch (def.Location)
            {
                case FixupLocation.LowByte:
                    fixup.LocationType = FixupLocationType.LowByte;
                    break;
                case FixupLocation.Offset:
                case FixupLocation.LoaderResolvedOffset:
                    fixup.LocationType = FixupLocationType.Offset;
                    break;
                case FixupLocation.Base:
                    fixup.LocationType = FixupLocationType.Base;
                    break;
                case FixupLocation.Pointer:
                    fixup.LocationType = FixupLocationType.Pointer;
                    break;
                default:
                    throw new InvalidDataException("The fixup location is not supported.");
            }
            fixup.Mode = def.Mode;

            IAddressReferent referent;
            if (def.Target.Referent is UInt16)
            {
                referent = new PhysicalAddress((UInt16)def.Target.Referent, 0);
            }
            else
            {
                referent = (IAddressReferent)objectMap[def.Target.Referent];
            }
            fixup.Target = new SymbolicTarget
            {
                Referent = (IAddressReferent)referent,
                Displacement = def.Target.Displacement
            };
            //f.Frame = null;
            return fixup;
        }

        private static SegmentGroup ConvertGroupDefinition(
            GroupDefinition def, Dictionary<object, object> objectMap)
        {
            SegmentGroup group = new SegmentGroup();
            group.Name = def.Name;
            group.Segments = new LogicalSegment[def.Segments.Count];
            for (int i = 0; i < group.Segments.Length; i++)
            {
                group.Segments[i] = (LogicalSegment)objectMap[def.Segments[i]];
            }
            return group;
        }

        private static DefinedSymbol ConvertPublicNameDefinition(
            FileFormats.Omf.PublicNameDefinition def, Dictionary<object, object> objectMap)
        {
            DefinedSymbol symbol = new DefinedSymbol();
            if (def.BaseGroup != null)
                symbol.BaseGroup = (SegmentGroup)objectMap[def.BaseGroup];
            if (def.BaseSegment != null)
                symbol.BaseSegment = (LogicalSegment)objectMap[def.BaseSegment];
            symbol.BaseFrame = def.BaseFrame;
            symbol.Name = def.Name;
            symbol.TypeIndex = def.TypeIndex;
            symbol.Offset = (uint)def.Offset;
            if (def.IsLocal)
                symbol.Scope = SymbolScope.Private;
            else
                symbol.Scope = SymbolScope.Public;
            return symbol;
        }
    }
}

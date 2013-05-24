using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler2
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectLibrary : Assembly
    {
#if false
        public ObjectLibrary(IEnumerable<ObjectModule> modules)
        {
            if (modules == null)
                throw new ArgumentNullException("modules");

            foreach (ObjectModule module in modules)
                base.Modules.Add(module);
        }
#endif

#if false
        /// <summary>
        /// Gets a list of object modules in this library.
        /// </summary>
        //[TypeConverter(typeof(ExpandableObjectConverter))]
        //[TypeConverter(typeof(ArrayConverter))]
        //[TypeConverter(typeof(CollectionConverter))]
        //[TypeConverter(typeof(ExpandableCollectionConverter))]
        [Browsable(true)]
        public ObjectModule[] Modules { get; internal set; }
#endif

        public readonly SortedDictionary<string, List<ObjectModule>> Symbols
            = new SortedDictionary<string, List<ObjectModule>>();

        public IEnumerable<string> GetUnresolvedSymbols()
        {
            foreach (var kv in Symbols)
            {
                if (kv.Value == null)
                    yield return kv.Key;
            }
        }

        public ObjectModule FindModule(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            foreach (ObjectModule module in Modules)
            {
                if (module.Name == name)
                    return module;
            }
            return null;
        }

        public void AssignIdsToSegments()
        {
            int id = 0;
            foreach (ObjectModule module in Modules)
            {
                foreach (LogicalSegment segment in module.Segments)
                {
                    ++id;
                    segment.Id = id;
                    base.AddSegment(id, segment.Image);
                }
            }
        }

        public void ResolveAllSymbols()
        {
            // First, build a map of each public name.
            foreach (ObjectModule module in Modules)
            {
                foreach (var name in module.DefinedNames)
                {
                    List<ObjectModule> definitionList;
                    if (!Symbols.TryGetValue(name.Name, out definitionList))
                    {
                        definitionList = new List<ObjectModule>(1);
                        Symbols.Add(name.Name, definitionList);
                    }
                    definitionList.Add(module);
                }
            }

            // Next, try to resolve each external symbol.
            // TODO: check aliases.
            foreach (ObjectModule module in Modules)
            {
                foreach (var name in module.ExternalNames)
                {
                    if (!Symbols.ContainsKey(name.Name)) // cannot resolve
                    {
                        Symbols.Add(name.Name, null); // indicate that it's not there
                    }
                }
            }
        }
    }
}

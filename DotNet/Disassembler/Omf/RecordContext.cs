using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler.Omf
{
    /// <summary>
    /// Contains context information to assist reading and writing records.
    /// </summary>
    internal class RecordContext
    {
        public readonly List<string> Names = new List<string>();

        // FRAME threads.
        public readonly ThreadDefinition[] FrameThreads = new ThreadDefinition[4];

        // TARGET threads.
        public readonly ThreadDefinition[] TargetThreads = new ThreadDefinition[4];

        // Contains the last record.
        public Record LastRecord = null;

        public ObjectModule Module { get; private set; }

        internal RecordContext(ObjectModule module)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            this.Module = module;
        }
    }
}

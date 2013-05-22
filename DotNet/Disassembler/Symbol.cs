using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler2
{
    /// <summary>
    /// Represents a symbol with name and optionally type information.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Symbol
    {
        [Browsable(true)]
        public string Name { get; internal set; }

        [Browsable(false)]
        public UInt16 TypeIndex { get; internal set; }

        [Browsable(true)]
        public SymbolScope Scope { get; internal set; }

        public override string ToString()
        {
            if (TypeIndex == 0)
                return Name;
            else
                return string.Format("{0}:{1}", Name, TypeIndex);
        }
    }

    public enum SymbolScope : byte
    {
        /// <summary>
        /// The symbol is visible globally.
        /// </summary>
        Public = 0,

        /// <summary>
        /// The symbol is visible only within the module where it is defined.
        /// </summary>
        Private = 1,
    }

    /// <summary>
    /// Represents an external symbol, i.e. one which must be resolved by a
    /// matching defined symbol.
    /// 
    /// An external symbol is defined by one of the following records:
    ///   EXTDEF  -- ExternalNamesDefinitionRecord
    ///   LEXTDEF -- LocalExternalNamesDefinitionRecord
    ///   CEXTDEF -- COMDATExternalNamesDefinitionRecord
    /// One of these records must be defined so that a FIXUPP record can
    /// refer to the symbol's address.
    /// </summary>
    public class ExternalSymbol : Symbol, IAddressReferent
    {
        public string Label
        {
            get { throw new NotImplementedException(); }
        }

        public ResolvedAddress Resolve()
        {
            throw new NotSupportedException();
        }
    }

    public class CommunalNameDefinition : ExternalSymbol
    {
        public byte DataType { get; internal set; }
        public UInt32 ElementCount { get; internal set; }
        public UInt32 ElementSize { get; internal set; }
    }

    /// <summary>
    /// Represents a defined symbol, i.e. one that can be used to resolve
    /// an external symbol reference.
    /// </summary>
    public class DefinedSymbol : Symbol
    {
        /// <summary>
        /// Gets the LSEG (logical segment) in which this symbol is defined.
        /// If BaseSegment is not null, the Offset field is relative to the
        /// beginning of BaseSegment. If BaseSegment is null, the Offset field
        /// is relative to the physical frame indicated by FrameNumber.
        /// 
        /// Note: when BaseSegment is null, the public name is typically used
        /// to represent a constant.
        /// </summary>
        [Browsable(true)]
        public LogicalSegment BaseSegment { get; internal set; }

        /// <summary>
        /// Gets the group associated with this symbol. Such association is
        /// used to resolve the FRAME in a FIXUPP -- if BaseGroup is not null,
        /// then the group's frame is used as the FRAME of the fixup.
        /// </summary>
        [Browsable(true)]
        public SegmentGroup BaseGroup { get; internal set; }

        /// <summary>
        /// Gets the frame number of the address of this symbol. This is only
        /// relevant if BaseSegment is null, which indicates that the symbol
        /// refers to an absolute SEG:OFF address.
        /// </summary>
        [Browsable(true)]
        public UInt16 BaseFrame { get; internal set; }

        /// <summary>
        /// Gets the offset of the symbol relative to the start of the logical
        /// segment in which it is defined.
        /// </summary>
        [Browsable(true)]
        public UInt32 Offset { get; internal set; }

        public override string ToString()
        {
            if (BaseSegment == null)
            {
                return string.Format("{0} @ {1:X4}:{2:X4}", Name, BaseFrame, Offset);
            }
            else
            {
                return string.Format("{0} @ {1}+{2:X}h", Name, BaseSegment.Name, Offset);
            }
        }
    }

    public class SymbolAlias : Symbol
    {
        [Browsable(true)]
        public string AliasName
        {
            get { return Name; }
            internal set { Name = value; }
        }

        [Browsable(true)]
        public string SubstituteName { get; internal set; }
    }
}

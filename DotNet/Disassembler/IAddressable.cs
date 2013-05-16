using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler
{
    /// <summary>
    /// Represents a logical object that can be used as an address reference.
    /// The address does not have to be physical. For example, a logical
    /// segment is addressible, but an immediate or EAX register is not.
    /// </summary>
    public interface IAddressable
    {
        /// <summary>
        /// Gets a string representation of the object's address. This address
        /// is not necessarily physical; for example, it can be something like
        /// "fopen._TEXT" or "_strcpy".
        /// </summary>
        string Label { get; }
    }

    public struct PhysicalAddress : IAddressable
    {
        ushort Segment;
        ushort Offset;

        string IAddressable.Label
        {
            get { return string.Format("X4:X4", Segment, Offset); }
        }
    }

    public struct LogicalAddress : IAddressable
    {
        IAddressable Base;
        ushort Offset;

        string IAddressable.Label
        {
            get { throw new NotImplementedException(); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SegmentGroup
    {
    }
}

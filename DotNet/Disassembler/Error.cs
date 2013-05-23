using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Disassembler2
{
    /// <summary>
    /// Contains information about an error encountered during disassembling.
    /// </summary>
    public class Error
    {
        readonly ErrorCode errorCode;
        readonly ErrorCategory category;
        readonly Address location;
        readonly string message;

        public Error(Address location, ErrorCode errorCode, string message)
        {
            this.category = ErrorCategory.Error;
            this.location = location;
            this.message = message;
            this.errorCode = errorCode;
        }

        public ErrorCode ErrorCode
        {
            get { return errorCode; }
        }

        public ErrorCategory Category
        {
            get { return category; }
        }

        public Address Location
        {
            get { return location; }
        }

        public string Message
        {
            get { return message; }
        }

#if false
        public Error(Address location, string message, ErrorCategory category)
        {
            this.Category = category;
            this.Location = location;
            this.Message = message;
        }

        public Error(Address location, string message)
            : this(location, message, ErrorCategory.Error)
        {
        }
#endif
        public static int CompareByLocation(Error x, Error y)
        {
            return x.Location.CompareTo(y.Location);
        }
    }

    [Flags]
    public enum ErrorCategory
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Message = 4,
    }

    public class ErrorCategoryAttribute : Attribute
    {
        readonly ErrorCategory category;

        public ErrorCategoryAttribute(ErrorCategory category)
        {
            this.category = category;
        }
    }

    public enum ErrorCode
    {
        [ErrorCategory(ErrorCategory.None)]
        OK = 0,

        GenericError,
        InvalidInstruction,
        BrokenFixup,

        /// <summary>
        /// Indicates that the same procedure (identified by its entry point
        /// address) was called both near and far. Since a near call must be
        /// returned with RETN and a far call with RETF, this error indicates
        /// a potential problem with the analysis.
        /// </summary>
        /// [Summary]
        /// [Description]
        /// [Example]
        /// [Solution]
        /// 
        InconsistentCall,

        /// <summary>
        /// Indicates that data was encountered when code was expected.
        /// </summary>
        RanIntoData,

        /// <summary>
        /// Indicates that we ran into the middle of an instruction.
        /// </summary>
        RanIntoCode,

        /// <summary>
        /// While analyzing a basic block, a decoded instruction would 
        /// overlap with existing bytes that are already analyzed as code
        /// or data.
        /// </summary>
        /// <remarks>
        /// Possible causes for this error include:
        /// - 
        /// - Other analysis errors.
        /// </remarks>
        [ErrorCategory(ErrorCategory.Error)]
        OverlappingInstruction,

        /// <summary>
        /// After executing an instruction, the CS:IP pointer would wrap
        /// around 0xFFFF.
        /// </summary>
        /// <remarks>
        /// While it is technically allowed (and occassionally useful) to let
        /// CS:IP wrap, this situation typically indicates an analysis error.
        /// </remarks>
        [ErrorCategory(ErrorCategory.Error)]
        AddressWrapped,

        /// <summary>
        /// The target of a branch/call/jump instruction cannot be determined
        /// through static analysis.
        /// </summary>
        /// <remarks>
        /// To resolve this problem, dynamic analysis can be employed.
        /// </remarks>
        [ErrorCategory(ErrorCategory.Message)]
        DynamicTarget,
    }

    public class ErrorCollection : List<Error>
    {
    }
}

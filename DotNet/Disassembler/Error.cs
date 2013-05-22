using System;
using System.Collections.Generic;
using System.Text;

namespace Disassembler2
{
    /// <summary>
    /// Contains information about an error encountered during disassembling.
    /// </summary>
    public class Error
    {
        public ErrorCode ErrorCode { get; private set; }
        public ErrorCategory Category { get; private set; }
        public LogicalAddress Location { get; private set; }
        public string Message { get; private set; }

        public Error(LogicalAddress location, ErrorCode errorCode, string message)
        {
            this.Category = ErrorCategory.Error;
            this.Location = location;
            this.Message = message;
            this.ErrorCode = errorCode;
        }

        public Error(LogicalAddress location, string message, ErrorCategory category)
        {
            this.Category = category;
            this.Location = location;
            this.Message = message;
        }

        public Error(LogicalAddress location, string message)
            : this(location, message, ErrorCategory.Error)
        {
        }

        public static int CompareByLocation(Error x, Error y)
        {
            return LogicalAddress.CompareByLexical(x.Location, y.Location);
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

    public enum ErrorCode
    {
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
        InconsistentCall,

        /// <summary>
        /// Indicates that data was encountered when code was expected.
        /// </summary>
        RanIntoData,

        /// <summary>
        /// Indicates that we ran into the middle of an instruction.
        /// </summary>
        RanIntoCode,
    }

    public class ErrorCollection : List<Error>
    {
    }
}

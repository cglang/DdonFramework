using System;

namespace Ddon.OpenProtocol.Models
{
    public class OpenProtocolException : Exception
    {
        public int FailedMid { get; }

        public string ErrorCode { get; }

        public OpenProtocolException(int failedMid, string errorCode)
            : base($"MID {failedMid:D4} failed with error code {errorCode}.")
        {
            FailedMid = failedMid;
            ErrorCode = errorCode;
        }

        public OpenProtocolException(int failedMid, string errorCode, string message)
            : base(message)
        {
            FailedMid = failedMid;
            ErrorCode = errorCode;
        }
    }
}

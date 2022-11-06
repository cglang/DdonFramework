using System;

namespace Ddon.ConvenientSocket.Exceptions
{
    public class DdonSocketRequestException : Exception
    {
        public DdonSocketRequestException(string message) : base(message)
        {
        }

        public DdonSocketRequestException(string info, string message, Exception exception) : base(message, exception)
        {
            Info = info;
        }

        public string? Info { get; }
    }
}

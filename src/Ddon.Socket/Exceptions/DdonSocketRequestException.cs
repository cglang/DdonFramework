using System;

namespace Ddon.ConvenientSocket.Exceptions
{
    public class DdonSocketRequestException : Exception
    {
        public DdonSocketRequestException() : base("请求超时") { }
    }
}

using System;

namespace Ddon.Identity.Exceptions
{
    /// <summary>
    /// 未认证
    /// </summary>
    public class UnauthenticationException : Exception
    {
        public UnauthenticationException(string message) : base(message)
        {

        }
    }
}

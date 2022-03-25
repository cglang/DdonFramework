using System;

namespace Ddon.Core.Exceptions
{
    /// <summary>
    /// 未授权异常
    /// </summary>
    public class UnauthorizedException : Exception
    {
        /// <summary>
        /// 未授权异常
        /// </summary>
        public UnauthorizedException()
        {

        }

        /// <summary>
        /// 未授权异常
        /// </summary>
        /// <param name="message">错误信息</param>
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}

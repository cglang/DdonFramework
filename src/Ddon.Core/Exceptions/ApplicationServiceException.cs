using System;

namespace Ddon.Core.Exceptions
{
    public class ApplicationServiceException : Exception
    {
        /// <summary>
        /// 应用程序异常
        /// </summary>
        public ApplicationServiceException()
        {

        }

        /// <summary>
        /// 应用程序异常
        /// </summary>
        /// <param name="message">错误信息</param>
        public ApplicationServiceException(string message) : base(message)
        {
        }
    }
}

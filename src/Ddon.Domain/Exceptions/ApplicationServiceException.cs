using System;

namespace Ddon.Domain.Exceptions
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

        /// <summary>
        /// 应用程序异常
        /// </summary>
        /// <param name="e">原始异常</param>
        /// <param name="message">错误信息</param>
        public ApplicationServiceException(Exception e, string message) : base(message, e)
        {
        }
    }
}

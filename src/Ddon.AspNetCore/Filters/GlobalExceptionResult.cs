namespace Ddon.AspNetCore.Filters
{
    /// <summary>
    /// 全局响应数据模型
    /// </summary>
    public class GlobalExceptionResult
    {
        public GlobalExceptionResult(string message)
        {
            Success = false;
            Message = message;
        }

        public GlobalExceptionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public GlobalExceptionResult(bool success = false, int code = 0, string? message = default)
        {
            Success = success;
            Message = message;
        }

        /// <summary>
        /// 成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string? Message { get; set; }
    }
}

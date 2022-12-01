namespace Ddon.AspNetCore.Filters
{
    /// <summary>
    /// 全局响应数据模型
    /// </summary>
    public class GlobalResult
    {
        public GlobalResult(bool success = false, int code = 0, string? message = default)
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

    /// <summary>
    /// 全局响应数据模型
    /// </summary>
    public class GlobalResult<T> : GlobalResult
    {
        public GlobalResult(bool success = false, int code = 0, string? message = default, T? data = default) : base(success, code, message)
        {
            Result = data;
        }

        public T? Result { get; set; }
    }
}

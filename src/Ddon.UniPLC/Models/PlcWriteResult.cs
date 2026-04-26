namespace Ddon.UniPLC.Models;

/// <summary>
/// PLC 写入结果
/// </summary>
public class PlcWriteResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 异常
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// 创建成功的结果
    /// </summary>
    public static PlcWriteResult Success() =>
        new() { IsSuccess = true };

    /// <summary>
    /// 创建失败的结果
    /// </summary>
    public static PlcWriteResult Failure(string message, Exception? ex = null) =>
        new() { IsSuccess = false, ErrorMessage = message, Exception = ex };
}

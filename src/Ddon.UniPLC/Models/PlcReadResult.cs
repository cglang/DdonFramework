namespace Ddon.UniPLC.Models;

/// <summary>
/// PLC 读取结果
/// </summary>
/// <typeparam name="T">返回值类型</typeparam>
public class PlcReadResult<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 返回值
    /// </summary>
    public T? Value { get; set; }

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
    public static PlcReadResult<T> Success(T value) =>
        new() { IsSuccess = true, Value = value };

    /// <summary>
    /// 创建失败的结果
    /// </summary>
    public static PlcReadResult<T> Failure(string message, Exception? ex = null) =>
        new() { IsSuccess = false, ErrorMessage = message, Exception = ex };
}

namespace Ddon.UniPLC.Models;

/// <summary>
/// PLC 批量读取结果
/// </summary>
public class PlcValueResult
{
    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 异常
    /// </summary>
    public Exception? Exception { get; set; }
}

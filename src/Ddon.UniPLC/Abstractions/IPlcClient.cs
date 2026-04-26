using Ddon.UniPLC.Models;

namespace Ddon.UniPLC.Abstractions;

/// <summary>
/// PLC 客户端基础接口
/// </summary>
public interface IPlcClient : IAsyncDisposable
{
    /// <summary>
    /// 客户端名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 建立连接
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 心跳检测 / Ping
    /// </summary>
    Task<bool> PingAsync();

    /// <summary>
    /// 读取字节数组
    /// </summary>
    Task<PlcReadResult<byte[]>> ReadBytesAsync(string address, int length);

    /// <summary>
    /// 写入字节数组
    /// </summary>
    Task<PlcWriteResult> WriteBytesAsync(string address, byte[] data);

    /// <summary>
    /// 泛型读取
    /// </summary>
    Task<T> ReadAsync<T>(string address);

    /// <summary>
    /// 泛型写入
    /// </summary>
    Task WriteAsync<T>(string address, T value);

    /// <summary>
    /// 批量读取
    /// </summary>
    Task<IReadOnlyList<PlcValueResult>> BatchReadAsync(params string[] addresses);
}

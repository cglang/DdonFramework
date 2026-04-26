using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Models;
using System.Collections.Concurrent;

namespace Ddon.UniPLC.Clients;

/// <summary>
/// 内存模拟 PLC 客户端（用于测试和离线开发）
/// </summary>
public class MemoryPlcClient : IPlcClient
{
    private readonly PlcOptions _options;
    private bool _isConnected;
    private readonly ConcurrentDictionary<string, byte[]> _memory;

    public string Name => _options.Name;
    public bool IsConnected => _isConnected;

    public MemoryPlcClient(PlcOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _memory = new ConcurrentDictionary<string, byte[]>();
        _isConnected = false;
    }

    public Task ConnectAsync()
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<bool> PingAsync()
    {
        return Task.FromResult(_isConnected);
    }

    public async Task<PlcReadResult<byte[]>> ReadBytesAsync(string address, int length)
    {
        if (!_isConnected)
            return PlcReadResult<byte[]>.Failure("Not connected");

        if (_memory.TryGetValue(address, out var data))
        {
            var result = new byte[length];
            Array.Copy(data, 0, result, 0, Math.Min(data.Length, length));
            return PlcReadResult<byte[]>.Success(result);
        }

        // 返回零初始化的字节数组
        return PlcReadResult<byte[]>.Success(new byte[length]);
    }

    public async Task<PlcWriteResult> WriteBytesAsync(string address, byte[] data)
    {
        if (!_isConnected)
            return PlcWriteResult.Failure("Not connected");

        _memory.AddOrUpdate(address, data, (_, _) => data);
        return PlcWriteResult.Success();
    }

    public async Task<T> ReadAsync<T>(string address)
    {
        var result = await ReadBytesAsync(address, 8);
        if (!result.IsSuccess || result.Value == null)
            throw new InvalidOperationException($"Failed to read from {address}");

        return ConvertBytes<T>(result.Value);
    }

    public async Task WriteAsync<T>(string address, T value)
    {
        var data = ConvertToBytes(value);
        var result = await WriteBytesAsync(address, data);
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to write to {address}");
    }

    public async Task<IReadOnlyList<PlcValueResult>> BatchReadAsync(params string[] addresses)
    {
        var results = new List<PlcValueResult>();
        foreach (var address in addresses)
        {
            try
            {
                var readResult = await ReadBytesAsync(address, 8);
                results.Add(new PlcValueResult
                {
                    Address = address,
                    IsSuccess = readResult.IsSuccess,
                    Value = readResult.Value,
                    ErrorMessage = readResult.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                results.Add(new PlcValueResult
                {
                    Address = address,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Exception = ex
                });
            }
        }
        return results;
    }

    public ValueTask DisposeAsync()
    {
        _isConnected = false;
        _memory.Clear();
        return ValueTask.CompletedTask;
    }

    private static T ConvertBytes<T>(byte[] data)
    {
        var type = typeof(T);
        if (type == typeof(bool))
            return (T)(object)(data[0] != 0);
        if (type == typeof(byte))
            return (T)(object)data[0];
        if (type == typeof(short))
            return (T)(object)BitConverter.ToInt16(data, 0);
        if (type == typeof(ushort))
            return (T)(object)BitConverter.ToUInt16(data, 0);
        if (type == typeof(int))
            return (T)(object)BitConverter.ToInt32(data, 0);
        if (type == typeof(uint))
            return (T)(object)BitConverter.ToUInt32(data, 0);
        if (type == typeof(long))
            return (T)(object)BitConverter.ToInt64(data, 0);
        if (type == typeof(ulong))
            return (T)(object)BitConverter.ToUInt64(data, 0);
        if (type == typeof(float))
            return (T)(object)BitConverter.ToSingle(data, 0);
        if (type == typeof(double))
            return (T)(object)BitConverter.ToDouble(data, 0);
        if (type == typeof(string))
            return (T)(object)System.Text.Encoding.UTF8.GetString(data).TrimEnd('\0');

        throw new NotSupportedException($"Type {type.Name} is not supported");
    }

    private static byte[] ConvertToBytes<T>(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var type = typeof(T);
        if (type == typeof(bool))
            return new[] { (byte)(((bool)(object)value) ? 1 : 0) };
        if (type == typeof(byte))
            return new[] { (byte)(object)value };
        if (type == typeof(short))
            return BitConverter.GetBytes((short)(object)value);
        if (type == typeof(ushort))
            return BitConverter.GetBytes((ushort)(object)value);
        if (type == typeof(int))
            return BitConverter.GetBytes((int)(object)value);
        if (type == typeof(uint))
            return BitConverter.GetBytes((uint)(object)value);
        if (type == typeof(long))
            return BitConverter.GetBytes((long)(object)value);
        if (type == typeof(ulong))
            return BitConverter.GetBytes((ulong)(object)value);
        if (type == typeof(float))
            return BitConverter.GetBytes((float)(object)value);
        if (type == typeof(double))
            return BitConverter.GetBytes((double)(object)value);
        if (type == typeof(string))
        {
            var str = (string)(object)value;
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        throw new NotSupportedException($"Type {type.Name} is not supported");
    }
}

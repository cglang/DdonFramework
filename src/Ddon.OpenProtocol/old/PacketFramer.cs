using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;

namespace OpenProtocol.Framing;

/// <summary>
/// Open Protocol TCP 拆包器
/// 使用 System.IO.Pipelines 实现零拷贝拆包
///
/// Open Protocol 帧格式：
///   [Length: 4 ASCII digits][MID: 4][Rev: 3][NoAck: 1][Station: 2][Spindle: 2][Data...][NUL]
///
/// Length 字段 = 整个帧的字节数（含 NUL 终止符）
/// </summary>
public sealed class PacketFramer
{
    private readonly PipeReader _reader;

    public PacketFramer(PipeReader reader)
    {
        _reader = reader;
    }

    /// <summary>
    /// 异步枚举完整帧。每次 yield 一个完整 Open Protocol 包（含 NUL）。
    /// </summary>
    public async IAsyncEnumerable<byte[]> ReadPacketsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            ReadResult result = await _reader.ReadAsync(ct);
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (TryReadPacket(ref buffer, out byte[]? packet))
            {
                yield return packet!;
            }

            // 告知 PipeReader 已消费到哪里
            _reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                yield break;
        }
    }

    /// <summary>
    /// 尝试从 buffer 中提取一个完整帧。
    ///
    /// 容错处理：
    ///   Open Protocol 帧以 NUL(\0) 结尾，部分控制器在帧尾额外发送 \0 或 \r\n。
    ///   遇到非数字字节（帧头损坏）时逐字节跳过自动同步，而不是抛异常崩溃 ReceiveLoop。
    /// </summary>
    private static bool TryReadPacket(
        ref ReadOnlySequence<byte> buffer,
        out byte[]? packet)
    {
        packet = null;

        // 跳过帧间垃圾字节（NUL / \r / \n 等）
        // Open Protocol 帧头第一个字节必须是 '0'~'9'
        while (buffer.Length > 0)
        {
            byte first = buffer.FirstSpan[0];
            if (first >= (byte)'0' && first <= (byte)'9')
                break;

            // 非数字 → 跳过该字节，继续同步
            buffer = buffer.Slice(1);
        }

        // 至少需要 4 字节来读 Length 字段
        if (buffer.Length < 4)
            return false;

        // 读取前 4 个 ASCII 字节
        Span<byte> lenBytes = stackalloc byte[4];
        buffer.Slice(0, 4).CopyTo(lenBytes);

        if (!TryParseLength(lenBytes, out int frameLength))
        {
            // 帧头损坏，跳过 1 字节后让上层再次调用
            buffer = buffer.Slice(1);
            return false;
        }

        if (frameLength <= 0 || frameLength > 65535)
        {
            // 长度超范围，跳过 1 字节重新同步
            buffer = buffer.Slice(1);
            return false;
        }

        // 缓冲区不足，等待更多数据
        if (buffer.Length < frameLength)
            return false;

        // 提取完整帧
        packet = buffer.Slice(0, frameLength).ToArray();
        buffer = buffer.Slice(frameLength);

        return true;
    }

    private static bool TryParseLength(
        ReadOnlySpan<byte> bytes,
        out int length)
    {
        length = 0;
        foreach (byte b in bytes)
        {
            if (b < '0' || b > '9')
                return false;

            length = length * 10 + (b - '0');
        }

        return true;
    }
}

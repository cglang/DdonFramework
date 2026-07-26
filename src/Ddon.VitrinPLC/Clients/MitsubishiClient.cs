using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.AddressParsers;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Clients
{
    /// <summary>
    /// 三菱 MC 协议（3E 帧，二进制模式）客户端。
    /// 支持 Q/iQ-R 系列 CPU 通过以太网模块通信。
    /// </summary>
    public sealed class MitsubishiClient : IPlcClient
    {
        private readonly MitsubishiOptions _options;
        private readonly ILogger<MitsubishiClient> _logger;
        private TcpClient _tcp;
        private NetworkStream _stream;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public string Name => _options.Name;
        public bool IsConnected => _tcp?.Connected ?? false;
        public IPlcAddressParser Parser { get; } = new MitsubishiAddressParser();

        public MitsubishiClient(MitsubishiOptions options, ILogger<MitsubishiClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("连接三菱 PLC: {Ip}:{Port}", _options.Ip, _options.Port);
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(_options.Ip, _options.Port, ct);
            _stream = _tcp.GetStream();
            _logger.LogInformation("三菱 MC 协议连接成功。");
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            _stream?.Close();
            _tcp?.Close();
            _logger.LogInformation("三菱 PLC 已断开。");
            await Task.CompletedTask;
        }

        public async Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var request = Build3EReadFrame(area, start, length / 2); // length in words
                await _stream.WriteAsync(request, ct);
                await _stream.FlushAsync(ct);
                return await ReceiveReadResponse(length, ct);
            }
            finally { _semaphore.Release(); }
        }

        public async Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var addr = Parser.Parse(address, PlcDataType.Int16);
                var request = Build3EWriteFrame(addr.Area, addr.ByteOffset / 2, data);
                await _stream.WriteAsync(request, ct);
                await _stream.FlushAsync(ct);
                await ReceiveWriteAck(ct);
            }
            finally { _semaphore.Release(); }
        }

        // ─────────────────────────────────────────────
        // MC 协议 3E 帧构造（二进制）
        // ─────────────────────────────────────────────
        private byte[] Build3EReadFrame(string area, int startWord, int wordCount)
        {
            var (devCode, isBit) = GetDeviceCode(area);
            // 副标题 + 网络/PC/IO/CH + 时钟等待 + 数据长度 + 命令
            return new byte[]
            {
                0x50, 0x00,                      // 副标题 (3E)
                0x00,                            // 网络编号
                0xFF,                            // PC 编号
                0xFF, 0x03,                      // 请求目标模块 IO
                0x00,                            // 多 CPU 编号
                0x0C, 0x00,                      // 数据长度 = 12
                0x0A, 0x00,                      // 等待时钟
                0x01, 0x04,                      // 命令: 0x0401 = 批量读
                isBit ? (byte)0x01 : (byte)0x00, // 子命令: 0=字, 1=位
                (byte)(startWord),               // 起始地址 (3 bytes LE)
                (byte)(startWord >> 8),
                (byte)(startWord >> 16),
                devCode,                         // 软元件代码
                (byte)(wordCount),               // 点数 (LE)
                (byte)(wordCount >> 8)
            };
        }

        private byte[] Build3EWriteFrame(string area, int startWord, byte[] data)
        {
            var (devCode, _) = GetDeviceCode(area);
            int wordCount = data.Length / 2;
            int dataLen = 10 + data.Length;

            var frame = new byte[7 + 2 + dataLen];
            // 副标题
            frame[0] = 0x50; frame[1] = 0x00;
            frame[2] = 0x00; frame[3] = 0xFF;
            frame[4] = 0xFF; frame[5] = 0x03; frame[6] = 0x00;
            frame[7] = (byte)(dataLen); frame[8] = (byte)(dataLen >> 8);
            frame[9] = 0x0A; frame[10] = 0x00;
            // 写命令 0x1401
            frame[11] = 0x01; frame[12] = 0x14;
            frame[13] = 0x00;
            frame[14] = (byte)startWord; frame[15] = (byte)(startWord >> 8); frame[16] = (byte)(startWord >> 16);
            frame[17] = devCode;
            frame[18] = (byte)wordCount; frame[19] = (byte)(wordCount >> 8);
            Buffer.BlockCopy(data, 0, frame, 20, data.Length);
            return frame;
        }

        private async Task<byte[]> ReceiveReadResponse(int byteCount, CancellationToken ct)
        {
            var header = new byte[11]; // 副标题(2) + 网络(1) + PC(1) + IO(2) + CH(1) + 数据长度(2) + 结束代码(2)
            await ReadExactAsync(header, ct);

            int endCode = (header[9] | (header[10] << 8));
            if (endCode != 0) throw new InvalidOperationException($"MC 协议读取错误: 0x{endCode:X4}");

            var data = new byte[byteCount];
            await ReadExactAsync(data, ct);
            return data;
        }

        private async Task ReceiveWriteAck(CancellationToken ct)
        {
            var ack = new byte[11];
            await ReadExactAsync(ack, ct);
            int endCode = (ack[9] | (ack[10] << 8));
            if (endCode != 0) throw new InvalidOperationException($"MC 协议写入错误: 0x{endCode:X4}");
        }

        private async Task ReadExactAsync(byte[] buf, CancellationToken ct)
        {
            int total = 0;
            while (total < buf.Length)
            {
                int read = await _stream.ReadAsync(buf.AsMemory(total, buf.Length - total), ct);
                if (read == 0) throw new InvalidOperationException("三菱 PLC 连接意外断开。");
                total += read;
            }
        }

        private static (byte code, bool isBit) GetDeviceCode(string area) =>
            area.ToUpperInvariant() switch
            {
                "D" => (0xA8, false),
                "W" => (0xB4, false),
                "R" => (0xAF, false),
                "M" => (0x90, true),
                "X" => (0x9C, true),
                "Y" => (0x9D, true),
                "B" => (0xA0, true),
                "SM" => (0x91, true),
                "SD" => (0xA9, false),
                _ => throw new NotSupportedException($"不支持的三菱软元件区域: {area}")
            };

        public void Dispose()
        {
            _stream?.Dispose();
            _tcp?.Dispose();
            _semaphore.Dispose();
        }
    }

    public sealed class MitsubishiOptions
    {
        public string Name { get; set; } = "Mitsubishi-Main";
        public string Ip { get; set; } = "192.168.1.20";
        public int Port { get; set; } = 5007;
    }
}

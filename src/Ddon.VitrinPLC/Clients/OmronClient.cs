using System;
using System.Net;
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
    /// 欧姆龙 FINS/TCP 协议客户端。
    /// 支持 CJ/CS 系列 CPU 通过内置以太网通信。
    /// </summary>
    public sealed class OmronClient : IPlcClient
    {
        private readonly OmronOptions _options;
        private readonly ILogger<OmronClient> _logger;
        private TcpClient    _tcp;
        private NetworkStream _stream;
        private byte _serviceId;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public string Name        => _options.Name;
        public bool   IsConnected => _tcp?.Connected ?? false;
        public IPlcAddressParser Parser { get; } = new OmronAddressParser();

        public OmronClient(OmronOptions options, ILogger<OmronClient> logger)
        {
            _options = options;
            _logger  = logger;
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("连接欧姆龙 PLC: {Ip}:{Port}", _options.Ip, _options.Port);
            _tcp    = new TcpClient();
            await _tcp.ConnectAsync(_options.Ip, _options.Port, ct);
            _stream = _tcp.GetStream();

            // FINS/TCP 握手
            await SendFinsHandshake(ct);
            await ReceiveFinsHandshakeAck(ct);
            _logger.LogInformation("欧姆龙 FINS/TCP 握手完成。");
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            _stream?.Close();
            _tcp?.Close();
            _logger.LogInformation("欧姆龙 PLC 已断开。");
            await Task.CompletedTask;
        }

        public async Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var (areaCode, bitMode) = GetMemoryArea(area);
                int wordCount  = (length + 1) / 2;
                var finsCmd    = BuildFinsRead(areaCode, start, wordCount);
                var frame      = WrapFins(finsCmd);
                await _stream.WriteAsync(frame, ct);
                await _stream.FlushAsync(ct);
                return await ReceiveFinsReadResponse(length, ct);
            }
            finally { _semaphore.Release(); }
        }

        public async Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var addr       = Parser.Parse(address, PlcDataType.Int16);
                var (code, _)  = GetMemoryArea(addr.Area);
                var finsCmd    = BuildFinsWrite(code, addr.ByteOffset / 2, data);
                var frame      = WrapFins(finsCmd);
                await _stream.WriteAsync(frame, ct);
                await _stream.FlushAsync(ct);
                await ReceiveFinsWriteAck(ct);
            }
            finally { _semaphore.Release(); }
        }

        // ─────────────────────────────────────────────
        // FINS 帧构造
        // ─────────────────────────────────────────────

        private async Task SendFinsHandshake(CancellationToken ct)
        {
            // FINS/TCP 节点地址握手
            var handshake = new byte[]
            {
                0x46,0x49,0x4E,0x53, // "FINS"
                0x00,0x00,0x00,0x0C, // 长度 = 12
                0x00,0x00,0x00,0x00, // 命令: 0=节点地址请求
                0x00,0x00,0x00,0x00, // 错误码
                0x00,0x00,0x00,0x00  // 客户端节点地址 (自动分配)
            };
            await _stream.WriteAsync(handshake, ct);
            await _stream.FlushAsync(ct);
        }

        private async Task ReceiveFinsHandshakeAck(CancellationToken ct)
        {
            var ack = new byte[24];
            await ReadExactAsync(ack, ct);
            _options.ClientNode = ack[19]; // 分配的客户端节点
            _options.ServerNode = ack[23]; // 服务器节点
            _logger.LogDebug("FINS 节点: client={Client}, server={Server}", _options.ClientNode, _options.ServerNode);
        }

        private byte[] BuildFinsRead(byte areaCode, int startWord, int wordCount)
        {
            return new byte[]
            {
                0x80,                      // ICF: command
                0x00,                      // RSV
                0x02,                      // GCT
                0x00,                      // DNA
                _options.ServerNode,       // DA1
                0x00,                      // DA2
                0x00,                      // SNA
                _options.ClientNode,       // SA1
                0x00,                      // SA2
                NextServiceId(),           // SID
                0x01, 0x01,                // MRC/SRC: Memory Area Read
                areaCode,                  // 内存区域代码
                (byte)(startWord >> 8),    // 起始字地址 (high)
                (byte)startWord,           // 起始字地址 (low)
                0x00,                      // bit
                (byte)(wordCount >> 8),    // 字数 (high)
                (byte)wordCount            // 字数 (low)
            };
        }

        private byte[] BuildFinsWrite(byte areaCode, int startWord, byte[] data)
        {
            int wordCount = data.Length / 2;
            var cmd       = new byte[12 + data.Length];
            cmd[0]=0x80; cmd[1]=0x00; cmd[2]=0x02; cmd[3]=0x00;
            cmd[4]=_options.ServerNode; cmd[5]=0x00;
            cmd[6]=0x00; cmd[7]=_options.ClientNode; cmd[8]=0x00;
            cmd[9]=NextServiceId();
            cmd[10]=0x01; cmd[11]=0x02;  // Memory Area Write
            cmd[12]=areaCode;
            cmd[13]=(byte)(startWord>>8); cmd[14]=(byte)startWord; cmd[15]=0x00;
            cmd[16]=(byte)(wordCount>>8); cmd[17]=(byte)wordCount;
            Buffer.BlockCopy(data, 0, cmd, 18, data.Length);
            return cmd;
        }

        private byte[] WrapFins(byte[] finsCmd)
        {
            // FINS/TCP 封装
            int dataLen = finsCmd.Length + 8;
            var frame   = new byte[dataLen];
            frame[0]=0x46; frame[1]=0x49; frame[2]=0x4E; frame[3]=0x53; // "FINS"
            frame[4]=(byte)((finsCmd.Length+4)>>24);
            frame[5]=(byte)((finsCmd.Length+4)>>16);
            frame[6]=(byte)((finsCmd.Length+4)>>8);
            frame[7]=(byte)(finsCmd.Length+4);
            frame[8]=0x00; frame[9]=0x00; frame[10]=0x00; frame[11]=0x02; // 命令=2
            frame[12]=0x00; frame[13]=0x00; frame[14]=0x00; frame[15]=0x00;
            Buffer.BlockCopy(finsCmd, 0, frame, 16, finsCmd.Length);
            return frame;
        }

        private async Task<byte[]> ReceiveFinsReadResponse(int byteCount, CancellationToken ct)
        {
            var header = new byte[30]; // FINS/TCP 头(16) + FINS 响应头(10) + 结束码(4)
            await ReadExactAsync(header, ct);
            int mres = header[28];
            int sres = header[29];
            if (mres != 0 || sres != 0)
                throw new InvalidOperationException($"FINS 读取错误: MRES=0x{mres:X2} SRES=0x{sres:X2}");

            var data = new byte[byteCount];
            await ReadExactAsync(data, ct);
            return data;
        }

        private async Task ReceiveFinsWriteAck(CancellationToken ct)
        {
            var ack  = new byte[30];
            await ReadExactAsync(ack, ct);
            int mres = ack[28];
            int sres = ack[29];
            if (mres != 0 || sres != 0)
                throw new InvalidOperationException($"FINS 写入错误: MRES=0x{mres:X2} SRES=0x{sres:X2}");
        }

        private async Task ReadExactAsync(byte[] buf, CancellationToken ct)
        {
            int total = 0;
            while (total < buf.Length)
            {
                int read = await _stream.ReadAsync(buf.AsMemory(total, buf.Length - total), ct);
                if (read == 0) throw new InvalidOperationException("欧姆龙 PLC 连接意外断开。");
                total += read;
            }
        }

        private byte NextServiceId() => ++_serviceId;

        private static (byte code, bool bit) GetMemoryArea(string area) =>
            area.ToUpperInvariant() switch
            {
                "D"   => (0x82, false),
                "W"   => (0x31, false),
                "H"   => (0x32, false),
                "CIO" => (0xB0, false),
                "DM"  => (0x82, false),
                "C"   => (0x89, false),
                "T"   => (0x89, false),
                _     => throw new NotSupportedException($"不支持的欧姆龙区域: {area}")
            };

        public void Dispose()
        {
            _stream?.Dispose();
            _tcp?.Dispose();
            _semaphore.Dispose();
        }
    }

    public sealed class OmronOptions
    {
        public string Name       { get; set; } = "Omron-Main";
        public string Ip         { get; set; } = "192.168.1.30";
        public int    Port       { get; set; } = 9600;
        internal byte ClientNode { get; set; }
        internal byte ServerNode { get; set; }
    }
}

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using S7.Net;

namespace Ddon.VitrinPLC.Clients
{
    /// <summary>
    /// 西门子 S7 协议客户端。
    ///
    /// 生产环境推荐使用 S7.Net 或 Sharp7 库封装。
    /// 本实现为完整可扩展的骨架，标注了各协议帧的位置。
    /// </summary>
    public sealed class SiemensClient : IPlcClient
    {
        private readonly SiemensOptions _options;
        private readonly ILogger<SiemensClient> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private S7.Net.Plc _plc;

        //private TcpClient _tcp;
        //private NetworkStream _stream;

        public string Name => _options.Name;
        public bool IsConnected => _plc?.IsConnected ?? false;

        public SiemensClient(SiemensOptions options, ILogger<SiemensClient> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task ConnectAsync(CancellationToken ct = default)
        {
            _plc = new S7.Net.Plc(S7.Net.CpuType.S71500, _options.Ip, _options.Port, (short)_options.Rack, (short)_options.Slot);

            if (!_plc.IsConnected)
                await _plc.OpenAsync();

            //_logger.LogInformation("连接西门子 PLC: {Ip}:{Port}", _options.Ip, _options.Port);
            //_tcp = new TcpClient();
            //await _tcp.ConnectAsync(_options.Ip, _options.Port, ct);
            //_stream = _tcp.GetStream();

            //// ── COTP 连接请求（ISO 8073）───────────────────
            //await SendCotpConnectRequest(ct);
            //await ReceiveCotpConnectConfirm(ct);

            //// ── S7 通信建立 ────────────────────────────────
            //await SendS7SetupCommunication(ct);
            //await ReceiveS7SetupAck(ct);

            _logger.LogInformation("西门子 PLC 已连接。");
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            _plc?.Close();
            //_stream?.Close();
            //_tcp?.Close();
            _logger.LogInformation("西门子 PLC 已断开。");
            await Task.CompletedTask;
        }

        public async Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                _logger.LogTrace("读取: {Area} offset={Start} len={Length}", area, start, length);

                // ── 解析 area（DB1 / M / I / Q 等）──────────
                var (areaCode, dbNumber) = ParseArea(area);

                return await _plc.ReadBytesAsync(DataType.DataBlock, dbNumber, start, length, ct);

                //// ── 构造 S7 Read 请求帧 ─────────────────────
                //var request = BuildReadRequest(areaCode, dbNumber, start, length);
                //await _stream.WriteAsync(request, ct);
                //await _stream.FlushAsync(ct);

                //// ── 读取 S7 响应 ────────────────────────────
                //return await ReadResponse(length, ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                _logger.LogTrace("写入: {Address} ({Bytes} bytes)", address, data.Length);

                var (area, dbNum, offset) = ParseWriteAddress(address);

                await _plc.WriteBytesAsync(DataType.DataBlock, dbNum, offset, data);

                //// ── 构造 S7 Write 请求帧 ────────────────────
                //var request = BuildWriteRequest(area, dbNum, offset, data);
                //await _stream.WriteAsync(request, ct);
                //await _stream.FlushAsync(ct);

                //await ReadWriteAck(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ══════════════════════════════════════════════════
        // 内部协议帧（实际项目中使用 Sharp7 或 S7.Net 库）
        // ══════════════════════════════════════════════════

        //private async Task SendCotpConnectRequest(CancellationToken ct)
        //{
        //    // TPKT + COTP CR (Connect Request)
        //    byte[] cotpCR = new byte[]
        //    {
        //        0x03, 0x00, 0x00, 0x16,   // TPKT: version=3, reserved=0, length=22
        //        0x11,                      // COTP: length=17
        //        0xe0,                      // COTP: CR PDU
        //        0x00, 0x00,                // dst ref
        //        0x00, 0x01,                // src ref
        //        0x00,                      // class
        //        0xC0, 0x01, 0x0A,          // tpdu-size param
        //        0xC1, 0x02, 0x01, 0x00,    // src-tsap
        //        0xC2, 0x02,                // dst-tsap
        //        (byte)(_options.Rack * 0x20 + _options.Slot),
        //        0x02
        //    };
        //    await _stream.WriteAsync(cotpCR, ct);
        //    await _stream.FlushAsync(ct);
        //}

        //private async Task ReceiveCotpConnectConfirm(CancellationToken ct)
        //{
        //    var buf = new byte[22];
        //    await ReadExactAsync(buf, ct);
        //    if (buf[5] != 0xD0) // CC = Connect Confirm
        //        throw new InvalidOperationException("COTP Connect Confirm 失败。");
        //}

        //private async Task SendS7SetupCommunication(CancellationToken ct)
        //{
        //    byte[] s7Setup = new byte[]
        //    {
        //        0x03,0x00,0x00,0x19,       // TPKT
        //        0x02,0xf0,0x80,            // COTP DT
        //        0x32,0x01,0x00,0x00,       // S7: protocol, job, reserved
        //        0x00,0x00,0x00,0x08,       // PDU ref, param length
        //        0x00,0x00,                 // data length
        //        0xf0,0x00,                 // setup communication function
        //        0x00,0x03,0x00,0x03,       // reserved ack, max jobs calling/called
        //        0x03,0xc0                  // PDU size = 960
        //    };
        //    await _stream.WriteAsync(s7Setup, ct);
        //    await _stream.FlushAsync(ct);
        //}

        //private async Task ReceiveS7SetupAck(CancellationToken ct)
        //{
        //    var buf = new byte[27];
        //    await ReadExactAsync(buf, ct);
        //    _logger.LogDebug("S7 通信建立完成，PDU 长度 = {Pdu}", (buf[25] << 8) | buf[26]);
        //}

        //private byte[] BuildReadRequest(byte areaCode, int dbNumber, int start, int length)
        //{
        //    // S7 PDU Read Request（简化版，仅字节读取）
        //    return new byte[]
        //    {
        //        0x03,0x00,0x00,0x1f,       // TPKT
        //        0x02,0xf0,0x80,            // COTP
        //        0x32,0x01,0x00,0x00,
        //        0x00,0x01,0x00,0x0e,0x00,0x00,
        //        0x04,0x01,                 // read request, 1 item
        //        0x12,0x0a,0x10,
        //        0x02,                      // byte access
        //        (byte)(length >> 8),(byte)length,
        //        (byte)(dbNumber >> 8),(byte)dbNumber,
        //        areaCode,
        //        (byte)((start * 8) >> 16),(byte)((start * 8) >> 8),(byte)(start * 8)
        //    };
        //}

        //private byte[] BuildWriteRequest(byte areaCode, int dbNum, int offset, byte[] data)
        //{
        //    int paramLen = 10;
        //    int dataLen = 4 + data.Length;
        //    int totalLen = 10 + paramLen + dataLen;

        //    var frame = new byte[totalLen];
        //    // TPKT
        //    frame[0] = 0x03; frame[1] = 0x00;
        //    frame[2] = (byte)(totalLen >> 8); frame[3] = (byte)totalLen;
        //    // COTP
        //    frame[4] = 0x02; frame[5] = 0xf0; frame[6] = 0x80;
        //    // S7 Header
        //    frame[7] = 0x32; frame[8] = 0x01; frame[9] = 0x00; frame[10] = 0x00;
        //    frame[11] = 0x00; frame[12] = 0x01;
        //    frame[13] = (byte)(paramLen >> 8); frame[14] = (byte)paramLen;
        //    frame[15] = (byte)(dataLen >> 8); frame[16] = (byte)dataLen;
        //    // Param
        //    frame[17] = 0x05; frame[18] = 0x01;
        //    frame[19] = 0x12; frame[20] = 0x0a; frame[21] = 0x10; frame[22] = 0x02;
        //    frame[23] = (byte)(data.Length >> 8); frame[24] = (byte)data.Length;
        //    frame[25] = (byte)(dbNum >> 8); frame[26] = (byte)dbNum;
        //    frame[27] = areaCode;
        //    int bitOffset = offset * 8;
        //    frame[28] = (byte)(bitOffset >> 16); frame[29] = (byte)(bitOffset >> 8); frame[30] = (byte)bitOffset;
        //    // Data
        //    frame[31] = 0x00; frame[32] = 0x04;
        //    frame[33] = (byte)((data.Length * 8) >> 8); frame[34] = (byte)(data.Length * 8);
        //    Buffer.BlockCopy(data, 0, frame, 35, data.Length);
        //    return frame;
        //}

        //private async Task<byte[]> ReadResponse(int expectedLength, CancellationToken ct)
        //{
        //    // 读 TPKT + S7 响应头（最少 25 字节）
        //    var header = new byte[25];
        //    await ReadExactAsync(header, ct);
        //    int dataStart = 25;
        //    // 实际数据在 header[21] 之后（简化）
        //    var result = new byte[expectedLength];
        //    await ReadExactAsync(result, ct);
        //    return result;
        //}

        //private async Task ReadWriteAck(CancellationToken ct)
        //{
        //    var ack = new byte[22];
        //    await ReadExactAsync(ack, ct);
        //    if (ack[21] != 0xFF)
        //        throw new InvalidOperationException($"写入 PLC 返回错误代码: 0x{ack[21]:X2}");
        //}

        //private async Task ReadExactAsync(byte[] buf, CancellationToken ct)
        //{
        //    int total = 0;
        //    while (total < buf.Length)
        //    {
        //        int read = await _stream.ReadAsync(buf.AsMemory(total, buf.Length - total), ct);
        //        if (read == 0) throw new InvalidOperationException("连接意外断开。");
        //        total += read;
        //    }
        //}

        private static (byte areaCode, int dbNumber) ParseArea(string area)
        {
            area = area.ToUpperInvariant();
            if (area.StartsWith("DB"))
                return (0x84, int.Parse(area[2..]));
            return area switch
            {
                "M" => (0x83, 0),
                "I" => (0x81, 0),
                "Q" => (0x82, 0),
                "T" => (0x1D, 0),
                "C" => (0x1C, 0),
                _ => throw new NotSupportedException($"不支持的 Siemens 区域: {area}")
            };
        }

        private static (byte areaCode, int dbNum, int offset) ParseWriteAddress(string address)
        {
            var addr = AddressParser.Parse(address, PlcDataType.Byte);
            var (code, db) = ParseArea(addr.Area);
            return (code, db, addr.ByteOffset);
        }

        public void Dispose()
        {
            _plc?.Close();
            //_stream?.Dispose();
            //_tcp?.Dispose();
            _semaphore.Dispose();
        }
    }

    public sealed class SiemensOptions
    {
        public string Name { get; set; } = "Siemens-Main";
        public string Ip { get; set; } = "192.168.1.10";
        public int Port { get; set; } = 102;
        public int Rack { get; set; } = 0;
        public int Slot { get; set; } = 1;
    }
}

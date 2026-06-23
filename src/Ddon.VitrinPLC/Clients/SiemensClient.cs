using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using S7.Net;

namespace Ddon.VitrinPLC.Clients
{
    /// <summary>
    /// 西门子 S7 协议客户端，基于 S7.Net 库实现。
    /// </summary>
    public sealed class SiemensClient : IPlcClient
    {
        private readonly SiemensOptions _options;
        private readonly ILogger<SiemensClient> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private S7.Net.Plc _plc;

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

            _logger.LogInformation("西门子 PLC 已连接。");
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            _plc?.Close();
            _logger.LogInformation("西门子 PLC 已断开。");
            await Task.CompletedTask;
        }

        public async Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                _logger.LogTrace("读取: {Area} offset={Start} len={Length}", area, start, length);

                var (_, dbNumber) = ParseArea(area);

                return await _plc.ReadBytesAsync(DataType.DataBlock, dbNumber, start, length, ct);
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

                var (_, dbNum, offset) = ParseWriteAddress(address);

                await _plc.WriteBytesAsync(DataType.DataBlock, dbNum, offset, data);
            }
            finally
            {
                _semaphore.Release();
            }
        }

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

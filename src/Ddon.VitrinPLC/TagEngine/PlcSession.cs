using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.TagEngine
{
    public sealed class PlcSession : IPlcSession
    {
        private readonly ITagRegistry _registry;
        private readonly IPlcMemoryMirror _mirror;
        private readonly IChangeNotifier _notifier;
        private readonly ILogger<PlcSession> _logger;
        private readonly IPlcClient _client;
        private readonly EndianFormat _endian;
        private readonly IPlcAddressParser _parser;

        public PlcSession(
            ITagRegistry registry,
            IPlcMemoryMirror mirror,
            IChangeNotifier notifier,
            ILogger<PlcSession> logger,
            IPlcClient client,
            EndianFormat endian,
            IPlcAddressParser parser)
        {
            _registry = registry;
            _mirror = mirror;
            _notifier = notifier;
            _logger = logger;
            this._client = client;
            this._endian = endian;
            _parser = parser;
        }

        public IPlcMemoryMirror Mirror => _mirror;
        public IReadOnlyList<TagDefinition> Tags => _registry.GetAll();

        public T Get<T>(string tagName)
        {
            var tag = _registry.Resolve(tagName);
            return _mirror.Read<T>(tag);
        }

        public async Task<WriteResult> SetAsync<T>(string tagName, T value, CancellationToken ct = default)
        {
            var tag = _registry.Resolve(tagName);
            _logger.LogDebug("SetAsync: {Tag} = {Value}", tagName, value);
            try
            {
                var addr = _parser.Parse(tag.Address, tag.Type);

                byte[] bytes;
                if (tag.Type == PlcDataType.Bool)
                {
                    bytes = PlcCodec.Encode(value, tag.Type, _endian, addr.ByteOffset, addr.BitIndex, tag.StringLength);
                    var region = _mirror.GetRegion(addr.RegionKey);
                    bytes[0] = SetBit(region.ReadByte(addr.ByteOffset), addr.BitIndex, Convert.ToBoolean(value));
                }
                else
                {
                    bytes = PlcCodec.Encode(value, tag.Type, _endian, addr.ByteOffset, addr.BitIndex, tag.StringLength);
                }

                _logger.LogDebug("写入 PLC: {Tag}={Value} @ {Address} ({Bytes} bytes)",
                    tag.Name, value, tag.Address, bytes.Length);

                await _client.WriteBytesAsync(tag.Address, bytes, ct);

                _logger.LogInformation("写入成功: {Tag}={Value} (等待扫描确认)", tag.Name, value);
                return WriteResult.Ok(tag.Name, value);
            }
            catch (OperationCanceledException)
            {
                return WriteResult.Fail(tag.Name, "操作已取消");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入失败: {Tag}", tag.Name);
                return WriteResult.Fail(tag.Name, ex.Message, ex);
            }

            static byte SetBit(byte value, int bitIndex, bool setToOne)
            {
                if (bitIndex < 0 || bitIndex > 7)
                    throw new ArgumentOutOfRangeException(nameof(bitIndex), "bitIndex must be 0~7");

                if (setToOne)
                {
                    // 置 1
                    return (byte)(value | 1 << bitIndex);
                }
                else
                {
                    // 置 0
                    return (byte)(value & ~(1 << bitIndex));
                }
            }
        }

        public IDisposable Subscribe<T>(string tagName, Action<T> handler)
        {
            _ = _registry.Resolve(tagName);
            return _notifier.Subscribe(tagName, handler);
        }

        public IDisposable Subscribe<T>(string tagName, Action<T, T> onChanged)
        {
            _ = _registry.Resolve(tagName);
            return _notifier.Subscribe(tagName, onChanged);
        }

        public void AddTag(TagDefinition tag)
        {
            _registry.Register(tag);
            var addr = _parser.Parse(tag.Address, tag.Type);
            try { _mirror.RegisterRegion(addr.RegionKey, addr.Area); }
            catch { }
        }

        public bool RemoveTag(string tagName)
        {
            return _registry.Unregister(tagName);
        }
    }
}

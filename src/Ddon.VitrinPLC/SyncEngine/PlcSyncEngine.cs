using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;

namespace Ddon.VitrinPLC.SyncEngine
{
    public sealed class PlcSyncEngine : IPlcSyncEngine, IAsyncDisposable
    {
        private readonly IPlcClient _client;
        private readonly IPlcMemoryMirror _mirror;
        private readonly ITagRegistry _registry;
        private readonly IChangeNotifier _notifier;
        private readonly int _scanInterval;
        private readonly IPlcAddressParser _parser;
        private readonly ILogger<PlcSyncEngine> _logger;

        private CancellationTokenSource _cts;
        private Task _runTask;

        public bool IsRunning => _runTask is { IsCompleted: false };
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;

        public PlcSyncEngine(
            IPlcClient client,
            IPlcMemoryMirror mirror,
            ITagRegistry registry,
            IChangeNotifier notifier,
            int scanInterval,
            ILogger<PlcSyncEngine> logger,
            IPlcAddressParser parser)
        {
            _client = client;
            _mirror = mirror;
            _registry = registry;
            _notifier = notifier;
            _scanInterval = scanInterval;
            _parser = parser;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            if (IsRunning) return;

            await _client.ConnectAsync(ct);
            _logger.LogInformation("PLC 已连接，启动同步引擎（周期 {Interval}ms）", _scanInterval);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _runTask = RunLoopAsync(_cts.Token);
        }

        public async Task StopAsync(CancellationToken ct = default)
        {
            if (!IsRunning) return;
            _logger.LogInformation("正在停止同步引擎...");
            await _cts.CancelAsync();
            try { await _runTask; }
            catch (OperationCanceledException) { }
            await _client.DisconnectAsync(ct);
            _logger.LogInformation("同步引擎已停止。");
        }

        private async Task RunLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await ScanOnceAsync(ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "扫描异常，下个周期重试。");
                }

                var elapsed = sw.Elapsed;
                var delay = TimeSpan.FromMilliseconds(_scanInterval) - elapsed;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, ct);
            }
        }

        public async Task ScanOnceAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();
            var changes = new List<TagChange>();
            var tags = _registry.GetAll();

            var regionGroups = GroupTagsByRegion(tags);

            foreach (var (regionKey, regionTags) in regionGroups)
            {
                int minOff = int.MaxValue, maxOff = 0;
                foreach (var tag in regionTags)
                {
                    var addr = _parser.Parse(tag.Address, tag.Type);
                    int size = PlcByteSize.Get(tag.Type, tag.StringLength);
                    minOff = Math.Min(minOff, addr.ByteOffset);
                    maxOff = Math.Max(maxOff, addr.ByteOffset + size);
                }

                int length = maxOff - minOff;
                var area = _parser.Parse(regionTags[0].Address, regionTags[0].Type).Area;

                byte[] rawSegment = await _client.ReadBytesAsync(area, minOff, length, ct);

                var newData = new BufferSlice(rawSegment, minOff);
                var oldData = _mirror.ApplySnapshot(regionKey, newData);

                foreach (var tag in regionTags)
                {
                    try
                    {
                        var addr = _parser.Parse(tag.Address, tag.Type);
                        object oldV = ReadFromData(oldData, addr, tag);
                        object newV = ReadFromData(newData, addr, tag);

                        if (!Equals(oldV, newV))
                        {
                            changes.Add(new TagChange { Tag = tag, OldValue = oldV, NewValue = newV });
                            _logger.LogTrace("变化: {Tag} {Old} → {New}", tag.Name, oldV, newV);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "变化检测失败: {Tag}", tag.Name);
                    }
                }
            }

            if (changes.Count > 0)
                _notifier.NotifyChanges(changes);

            ScanCompleted?.Invoke(this, new ScanCompletedEventArgs
            {
                Version = _mirror.Version,
                CompletedAt = DateTime.UtcNow,
                Elapsed = sw.Elapsed,
                Changes = changes
            });
        }

        private Dictionary<string, List<TagDefinition>> GroupTagsByRegion(
            IReadOnlyList<TagDefinition> tags)
        {
            var dict = new Dictionary<string, List<TagDefinition>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in tags)
            {
                var key = _parser.Parse(tag.Address, tag.Type).RegionKey;
                if (!dict.TryGetValue(key, out var list))
                    dict[key] = list = new List<TagDefinition>();
                list.Add(tag);
            }
            return dict;
        }

        private object ReadFromData(BufferSlice data, ParsedAddress addr, TagDefinition tag)
        {
            return tag.Type switch
            {
                PlcDataType.Bool => PlcCodec.Read<bool>(data, addr, endian: _mirror.Endian),
                PlcDataType.Byte => PlcCodec.Read<byte>(data, addr, endian: _mirror.Endian),
                PlcDataType.Int16 => PlcCodec.Read<short>(data, addr, endian: _mirror.Endian),
                PlcDataType.UInt16 => PlcCodec.Read<ushort>(data, addr, endian: _mirror.Endian),
                PlcDataType.Int32 => PlcCodec.Read<int>(data, addr, endian: _mirror.Endian),
                PlcDataType.UInt32 => PlcCodec.Read<uint>(data, addr, endian: _mirror.Endian),
                PlcDataType.Float => PlcCodec.Read<float>(data, addr, endian: _mirror.Endian),
                PlcDataType.Double => PlcCodec.Read<double>(data, addr, endian: _mirror.Endian),
                PlcDataType.String => PlcCodec.Read<string>(data, addr, tag.StringLength, _mirror.Endian),
                _ => throw new NotSupportedException()
            };
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}

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
    /// <summary>
    /// 核心同步引擎：周期扫描 PLC → 刷新镜像 → 检测变化 → 发布事件
    ///
    /// 流程（每周期）：
    ///   1. 按区域批量读取 PLC 原始字节（合并读取，减少请求次数）
    ///   2. 原子替换 MemoryMirror 中对应区域
    ///   3. 逐 Tag 比较新旧值，收集变化列表
    ///   4. 通过 IChangeNotifier 发布变化
    ///   5. 触发 ScanCompleted 事件
    /// </summary>
    public sealed class PlcSyncEngine : IPlcSyncEngine, IAsyncDisposable
    {
        private readonly IPlcClient _client;
        private readonly PlcMemoryMirror _mirror;
        private readonly ITagRegistry _registry;
        private readonly IChangeNotifier _notifier;
        private readonly int _scanInterval;
        private readonly IReadOnlyDictionary<string, int> _regionLengths;
        private readonly ILogger<PlcSyncEngine> _logger;

        private CancellationTokenSource _cts;
        private Task _runTask;

        public bool IsRunning => _runTask is { IsCompleted: false };
        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;

        public PlcSyncEngine(
            IPlcClient client,
            PlcMemoryMirror mirror,
            ITagRegistry registry,
            IChangeNotifier notifier,
            int scanInterval,
            ILogger<PlcSyncEngine> logger)
        {
            _client = client;
            _mirror = mirror;
            _registry = registry;
            _notifier = notifier;
            _scanInterval = scanInterval;
            _regionLengths = mirror.GetRegionInfo()
                .ToDictionary(x => x.Key, x => x.Value.Length, StringComparer.OrdinalIgnoreCase);
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
            catch (OperationCanceledException) { /* 正常退出 */ }
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

            // ── Step 1：按区域分组，合并批量读取 ──────────────
            var regionGroups = GroupTagsByRegion(tags);

            foreach (var (regionKey, regionTags) in regionGroups)
            {
                // 计算区域范围（min offset ~ max offset+size）
                int minOff = int.MaxValue, maxOff = 0;
                foreach (var tag in regionTags)
                {
                    var addr = AddressParser.Parse(tag.Address, tag.Type);
                    int size = AddressParser.GetByteSize(tag.Type, tag.StringLength);
                    minOff = Math.Min(minOff, addr.ByteOffset);
                    maxOff = Math.Max(maxOff, addr.ByteOffset + size);
                }

                int length = maxOff - minOff;
                var area = AddressParser.Parse(regionTags[0].Address, regionTags[0].Type).Area;

                // ── Step 2：批量读取原始字节 ─────────────────
                byte[] rawSegment = await _client.ReadBytesAsync(area, minOff, length, ct);

                // 将原始 PLC 数据填入全尺寸 buffer 的正确偏移位置
                int regionLength = GetRegisteredLength(regionKey);
                var newFullData = new byte[regionLength];
                Buffer.BlockCopy(rawSegment, 0, newFullData, minOff,
                    Math.Min(rawSegment.Length, regionLength - minOff));

                var oldFullData = _mirror.ApplySnapshot(regionKey, newFullData);

                // ── Step 3：变化检测（双方都在全尺寸 buffer 的同偏移位置读取）─
                foreach (var tag in regionTags)
                {
                    try
                    {
                        var addr = AddressParser.Parse(tag.Address, tag.Type);
                        object oldV = ReadFromBuffer(oldFullData, addr, tag);
                        object newV = ReadFromBuffer(newFullData, addr, tag);

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

            // ── Step 4/5：发布事件 ────────────────────────────
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

        // ── 辅助：按区域分组 ────────────────────────────────
        private static Dictionary<string, List<TagDefinition>> GroupTagsByRegion(
            IReadOnlyList<TagDefinition> tags)
        {
            var dict = new Dictionary<string, List<TagDefinition>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in tags)
            {
                var key = AddressParser.Parse(tag.Address, tag.Type).RegionKey;
                if (!dict.TryGetValue(key, out var list))
                    dict[key] = list = new List<TagDefinition>();
                list.Add(tag);
            }
            return dict;
        }

        private object ReadFromBuffer(byte[] buf, ParsedAddress addr, TagDefinition tag)
        {
            return tag.Type switch
            {
                PlcDataType.Bool => PlcCodec.Read<bool>(buf, addr, endian: _mirror.Endian),
                PlcDataType.Byte => PlcCodec.Read<byte>(buf, addr, endian: _mirror.Endian),
                PlcDataType.Int16 => PlcCodec.Read<short>(buf, addr, endian: _mirror.Endian),
                PlcDataType.UInt16 => PlcCodec.Read<ushort>(buf, addr, endian: _mirror.Endian),
                PlcDataType.Int32 => PlcCodec.Read<int>(buf, addr, endian: _mirror.Endian),
                PlcDataType.UInt32 => PlcCodec.Read<uint>(buf, addr, endian: _mirror.Endian),
                PlcDataType.Float => PlcCodec.Read<float>(buf, addr, endian: _mirror.Endian),
                PlcDataType.Double => PlcCodec.Read<double>(buf, addr, endian: _mirror.Endian),
                PlcDataType.String => PlcCodec.Read<string>(buf, addr, tag.StringLength, _mirror.Endian),
                _ => throw new NotSupportedException()
            };
        }

        private int GetRegisteredLength(string regionKey) =>
            _regionLengths.TryGetValue(regionKey, out var len) ? len : 4096;

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}

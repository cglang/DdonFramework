using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;
using Plc.Hosting;

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
        private readonly PlcMirrorOptions _options;
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
            PlcMirrorOptions options,
            ILogger<PlcSyncEngine> logger)
        {
            _client = client;
            _mirror = mirror;
            _registry = registry;
            _notifier = notifier;
            _options = options;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            if (IsRunning) return;

            await _client.ConnectAsync(ct);
            _logger.LogInformation("PLC 已连接，启动同步引擎（周期 {Interval}ms）", _options.ScanInterval);

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
                var delay = TimeSpan.FromMilliseconds(_options.ScanInterval) - elapsed;
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
                byte[] newData = await _client.ReadBytesAsync(area, minOff, length, ct);

                // 若区域长度不匹配需要对齐（填充到注册大小）
                // 简化实现：直接用读到的数据作为新 buffer
                var oldData = _mirror.ApplySnapshot(regionKey, PadOrTrim(newData, GetRegisteredLength(regionKey)));

                // ── Step 3：变化检测 ─────────────────────────
                foreach (var tag in regionTags)
                {
                    try
                    {
                        var addr = AddressParser.Parse(tag.Address, tag.Type);
                        object oldV = ReadFromBuffer(oldData, addr, tag);
                        object newV = ReadFromBuffer(newData, addr, tag);

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

        private static object ReadFromBuffer(byte[] buf, ParsedAddress addr, TagDefinition tag)
        {
            // 根据类型调用泛型 Codec（统一用 object 装箱）
            return tag.Type switch
            {
                PlcDataType.Bool => PlcCodec.Read<bool>(buf, addr),
                PlcDataType.Byte => PlcCodec.Read<byte>(buf, addr),
                PlcDataType.Int16 => PlcCodec.Read<short>(buf, addr),
                PlcDataType.UInt16 => PlcCodec.Read<ushort>(buf, addr),
                PlcDataType.Int32 => PlcCodec.Read<int>(buf, addr),
                PlcDataType.UInt32 => PlcCodec.Read<uint>(buf, addr),
                PlcDataType.Float => PlcCodec.Read<float>(buf, addr),
                PlcDataType.Double => PlcCodec.Read<double>(buf, addr),
                PlcDataType.String => PlcCodec.Read<string>(buf, addr, tag.StringLength),
                _ => throw new NotSupportedException()
            };
        }

        private byte[] PadOrTrim(byte[] src, int targetLen)
        {
            if (src.Length == targetLen) return src;
            var result = new byte[targetLen];
            Buffer.BlockCopy(src, 0, result, 0, Math.Min(src.Length, targetLen));
            return result;
        }

        private int GetRegisteredLength(string regionKey)
        {
            // 简化实现，实际应从 MemoryRegion 配置获取
            return _options.Regions.FirstOrDefault(x => x.Key == regionKey)?.Length ?? 4096;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}

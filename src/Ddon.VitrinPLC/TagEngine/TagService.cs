using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.TagEngine
{
    /// <summary>
    /// 业务层入口。
    /// Get<T>  → 只读内存镜像（极快，无 IO）
    /// SetAsync → 直接写 PLC，镜像不变，等待下次扫描确认
    /// Subscribe → 注册值变化回调
    /// </summary>
    public sealed class TagService : ITagService
    {
        private readonly ITagRegistry _registry;
        private readonly IPlcMemoryMirror _mirror;
        private readonly IWriteCommandService _writer;
        private readonly IChangeNotifier _notifier;
        private readonly ILogger<TagService> _logger;

        public TagService(
            ITagRegistry registry,
            IPlcMemoryMirror mirror,
            IWriteCommandService writer,
            IChangeNotifier notifier,
            ILogger<TagService> logger)
        {
            _registry = registry;
            _mirror = mirror;
            _writer = writer;
            _notifier = notifier;
            _logger = logger;
        }

        /// <summary>从镜像读取 Tag 值（同步，极低延迟）</summary>
        public T Get<T>(string tagName)
        {
            var tag = _registry.Resolve(tagName);
            return _mirror.Read<T>(tag);
        }

        /// <summary>将值直接写入 PLC，本地镜像不变（设计原则2/3）</summary>
        public async Task<WriteResult> SetAsync<T>(string tagName, T value, CancellationToken ct = default)
        {
            var tag = _registry.Resolve(tagName);
            _logger.LogDebug("SetAsync: {Tag} = {Value}", tagName, value);
            return await _writer.ExecuteAsync(tagName, value, ct);
        }

        /// <summary>订阅 Tag 值变化（扫描后触发）</summary>
        public IDisposable Subscribe<T>(string tagName, Action<T> handler)
        {
            _ = _registry.Resolve(tagName); // 校验 Tag 存在
            return _notifier.Subscribe(tagName, handler);
        }
    }
}

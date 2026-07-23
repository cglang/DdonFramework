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
        private readonly IWriteCommandService _writer;
        private readonly IChangeNotifier _notifier;
        private readonly ILogger<PlcSession> _logger;

        public PlcSession(
            ITagRegistry registry,
            IPlcMemoryMirror mirror,
            IWriteCommandService writer,
            IChangeNotifier notifier,
            ILogger<PlcSession> logger)
        {
            _registry = registry;
            _mirror = mirror;
            _writer = writer;
            _notifier = notifier;
            _logger = logger;
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
            return await _writer.ExecuteAsync(tagName, value, ct);
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
            var addr = AddressParser.Parse(tag.Address, tag.Type);
            try { _mirror.RegisterRegion(addr.RegionKey, addr.Area, 0, 4096); }
            catch { }
        }

        public bool RemoveTag(string tagName)
        {
            return _registry.Unregister(tagName);
        }
    }
}

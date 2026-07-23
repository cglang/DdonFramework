using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Abstractions;

namespace Ddon.VitrinPLC
{
    public sealed class PlcHub : IPlcHub
    {
        private readonly ConcurrentDictionary<string, IPlcSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IPlcSyncEngine> _engines = new(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceProvider _serviceProvider;

        internal PlcHub(
            Dictionary<string, IPlcSession> sessions,
            Dictionary<string, IPlcSyncEngine> engines,
            IServiceProvider serviceProvider)
        {
            foreach (var kvp in sessions)
                _sessions.TryAdd(kvp.Key, kvp.Value);
            foreach (var kvp in engines)
                _engines.TryAdd(kvp.Key, kvp.Value);
            _serviceProvider = serviceProvider;
        }

        public IPlcSession For(string plcName)
        {
            if (!_sessions.TryGetValue(plcName, out var svc))
                throw new KeyNotFoundException($"PLC '{plcName}' 未注册。");
            return svc;
        }

        public IEnumerable<string> Names => _sessions.Keys;

        public async Task AddPlcAsync(string name, IPlcClient client, Action<PlcHostOptions> configure, CancellationToken ct = default)
        {
            var options = new PlcHostOptions();
            configure(options);

            var group = PlcServiceFactory.Build(client, options, _serviceProvider);

            if (!_sessions.TryAdd(name, group.Session))
                throw new InvalidOperationException($"PLC '{name}' 已存在。");

            _engines.TryAdd(name, group.Engine);
            try
            {
                await group.Engine.StartAsync(ct);
            }
            catch
            {
                _engines.TryRemove(name, out _);
                _sessions.TryRemove(name, out _);
                throw;
            }
        }

        public async Task RemovePlcAsync(string name, CancellationToken ct = default)
        {
            if (!_engines.TryRemove(name, out var engine))
                throw new KeyNotFoundException($"PLC '{name}' 不存在。");

            await engine.StopAsync(ct);
            _sessions.TryRemove(name, out _);
        }

        internal async Task StartAllAsync(CancellationToken ct)
        {
            foreach (var engine in _engines.Values)
                await engine.StartAsync(ct);
        }

        internal async Task StopAllAsync(CancellationToken ct)
        {
            foreach (var engine in _engines.Values)
                await engine.StopAsync(ct);
        }
    }
}


using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.SyncEngine;

namespace Ddon.VitrinPLC
{
    /// <summary>
    /// <see cref="IPlcHub"/> 实现。持有所有 PLC 的 TagService 与 SyncEngine，
    /// 生命周期由 <see cref="VitrinPlcHostedService"/> 管理。
    /// </summary>
    public sealed class PlcHub : IPlcHub
    {
        private readonly IReadOnlyDictionary<string, ITagService> _services;
        private readonly IReadOnlyList<PlcSyncEngine> _engines;

        internal PlcHub(Dictionary<string, ITagService> services, List<PlcSyncEngine> engines)
        {
            _services = services;
            _engines = engines;
        }

        /// <inheritdoc/>
        public ITagService For(string plcName)
        {
            if (!_services.TryGetValue(plcName, out var svc))
                throw new KeyNotFoundException($"PLC '{plcName}' 未注册，请检查 AddVitrinPlc 配置。");
            return svc;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Names => _services.Keys;

        internal async Task StartAllAsync(CancellationToken ct)
        {
            foreach (var engine in _engines)
                await engine.StartAsync(ct);
        }

        internal async Task StopAllAsync(CancellationToken ct)
        {
            foreach (var engine in _engines)
                await engine.StopAsync(ct);
        }
    }
}

using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.SyncEngine;

namespace Ddon.VitrinPLC
{
    // ─────────────────────────────────────────────
    // IHostedService：生命周期管理
    // ─────────────────────────────────────────────
    public sealed class PlcMirrorHostedService : IHostedService, IAsyncDisposable
    {
        private readonly PlcSyncEngine _engine;
        private readonly ILogger<PlcMirrorHostedService> _logger;

        public PlcMirrorHostedService(PlcSyncEngine engine, ILogger<PlcMirrorHostedService> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            _logger.LogInformation("PlcMirror 后台服务启动。");
            await _engine.StartAsync(ct);
        }

        public async Task StopAsync(CancellationToken ct)
        {
            _logger.LogInformation("PlcMirror 后台服务停止。");
            await _engine.StopAsync(ct);
        }

        public async ValueTask DisposeAsync() => await _engine.DisposeAsync();
    }
}

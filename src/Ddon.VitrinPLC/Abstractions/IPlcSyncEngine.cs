using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // 同步引擎层
    // ─────────────────────────────────────────────
    public interface IPlcSyncEngine
    {
        bool IsRunning { get; }
        Task StartAsync(CancellationToken ct = default);
        Task StopAsync(CancellationToken ct = default);
        Task ScanOnceAsync(CancellationToken ct = default);

        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
    }
}

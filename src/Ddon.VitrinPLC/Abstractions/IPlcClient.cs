using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // 协议层：与 PLC 通信的最底层抽象
    // ─────────────────────────────────────────────
    public interface IPlcClient : IDisposable
    {
        string Name { get; }
        bool IsConnected { get; }
        Task ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync(CancellationToken ct = default);
        Task<byte[]> ReadBytesAsync(string area, int start, int length, CancellationToken ct = default);
        Task WriteBytesAsync(string address, byte[] data, CancellationToken ct = default);
    }
}

using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{

    // ─────────────────────────────────────────────
    // 写命令服务（可选扩展）
    // ─────────────────────────────────────────────
    public interface IWriteCommandService
    {
        Task<WriteResult> ExecuteAsync<T>(TagDefinition tag, T value, CancellationToken ct = default);
    }
}

using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // Tag API：业务层入口
    // ─────────────────────────────────────────────
    public interface ITagService
    {
        T Get<T>(string tagName);
        Task<WriteResult> SetAsync<T>(string tagName, T value, CancellationToken ct = default);
    }
}

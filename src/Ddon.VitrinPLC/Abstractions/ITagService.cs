using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    public interface ITagService
    {
        T Get<T>(string tagName);
        Task<WriteResult> SetAsync<T>(string tagName, T value, CancellationToken ct = default);
        IDisposable Subscribe<T>(string tagName, Action<T> handler);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolEndpoint
    {
        string Name { get; }

        bool IsConnected { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task<TResponse> SendAsync<TResponse>(
            Mid request,
            CancellationToken cancellationToken = default)
            where TResponse : Mid;

        Task<TResponse> SubscribeAsync<TResponse>(
            Mid request,
            CancellationToken cancellationToken = default)
            where TResponse : Mid;

        Task RegisterSubscriptionAsync(
            Mid request,
            CancellationToken cancellationToken = default);

        IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid;

        IDisposable Subscribe<TMid>(Action<TMid> handler) where TMid : Mid;

        IDisposable SubscribeAll(Func<Mid, Task> handler);
    }
}

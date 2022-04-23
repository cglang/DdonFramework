using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync(INotification @event, CancellationToken cancellationToken = default);

        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}

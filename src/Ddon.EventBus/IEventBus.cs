using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus.Abstractions
{
    public interface IEventBus
    {
        Task PublishAsync(IEventData eventData, CancellationToken cancellationToken = default);

        Task PublishAsync(IDistributedEventData eventData, CancellationToken cancellationToken = default);

        Task PublishAsync(IDomainEventData eventData, CancellationToken cancellationToken = default);

        Task<TResponse> PublishAsync<TResponse>(IDomainEventData<TResponse> eventData, CancellationToken cancellationToken = default);
    }
}

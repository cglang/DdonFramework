using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus.Contracts
{
    public interface IEventBus
    {
        Task PublishAsync(IEventData eventData, EventPublishOptions options = default, CancellationToken cancellationToken = default);

        Task PublishAsync(IDomainEventData eventData, EventPublishOptions options = default, CancellationToken cancellationToken = default);
    }
}

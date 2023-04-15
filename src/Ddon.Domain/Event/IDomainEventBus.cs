using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Domain.Event
{
    public interface IDomainEventBus
    {
        Task PublishAsync(IDomainEventData @event, CancellationToken cancellationToken = default);
    }
}

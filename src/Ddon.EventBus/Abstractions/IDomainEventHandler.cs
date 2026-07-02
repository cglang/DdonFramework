using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus.Contracts
{
    public interface IDomainEventHandler<in TDomainEvent>
        where TDomainEvent : IDomainEventData
    {
        Task HandleAsync(TDomainEvent eventData, CancellationToken cancellationToken = default);
    }
}

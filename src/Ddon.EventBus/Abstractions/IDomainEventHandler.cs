using MediatR;

namespace Ddon.EventBus.Contracts
{
    public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
        where TDomainEvent : IDomainEventData
    {
    }
}

using MediatR;

namespace Ddon.EventBus
{
    public interface IDomainEventHandler<in TDomainEvent> : IRequestHandler<TDomainEvent>
        where TDomainEvent : IDomainEventData
    {
    }
}

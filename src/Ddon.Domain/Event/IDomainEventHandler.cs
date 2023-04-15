using System.Threading.Tasks;
using MediatR;

namespace Ddon.Domain.Event
{
    public interface IDomainEventHandler<in TDomainEvent> : IRequestHandler<TDomainEvent>
        where TDomainEvent : IDomainEventData
    {
    }
}

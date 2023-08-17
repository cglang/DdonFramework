using MediatR;

namespace Ddon.EventBus
{
    public interface IDomainEventData : IRequest
    {
    }

    public interface IDomainEventData<T> : IRequest<T>
    {
    }
}

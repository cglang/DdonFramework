using System.Threading;
using System.Threading.Tasks;
using Ddon.EventBus.Abstractions;
using MediatR;

namespace Ddon.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IMediator _mediator;

        public InMemoryEventBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task PublishAsync(IDistributedEventData eventData, CancellationToken cancellationToken = default)
        {
            return _mediator.Publish(eventData, cancellationToken);
        }

        public Task PublishAsync(IEventData eventData, CancellationToken cancellationToken = default)
        {
            return _mediator.Publish(eventData, cancellationToken);
        }

        public Task PublishAsync(IDomainEventData eventData, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(eventData, cancellationToken);
        }


        public Task<TResponse> PublishAsync<TResponse>(IDomainEventData<TResponse> eventData, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(eventData, cancellationToken);
        }
    }
}

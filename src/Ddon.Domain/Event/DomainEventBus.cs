using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ddon.Domain.Event
{
    public class DomainEventBus : IDomainEventBus
    {
        private readonly IMediator _mediator;

        public DomainEventBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task PublishAsync(IDomainEventData @event, CancellationToken cancellationToken)
        {
            await _mediator.Send(@event, cancellationToken);
        }
    }
}

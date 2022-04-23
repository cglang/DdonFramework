using Ddon.EventBus.Abstractions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IMediator _mediator;

        public InMemoryEventBus(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            await _mediator.Publish(notification, cancellationToken);
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return await _mediator.Send(request, cancellationToken);
        }
    }
}

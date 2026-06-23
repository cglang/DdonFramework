using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.EventBus.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ddon.EventBus
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IEventBus> _logger;
        private readonly BackgroundEventProcessor _backgroundEventProcessor;

        public InMemoryEventBus(IMediator mediator, ILogger<IEventBus> logger, BackgroundEventProcessor backgroundEventProcessor)
        {
            _mediator = mediator;
            _logger = logger;
            _backgroundEventProcessor = backgroundEventProcessor;
        }

        public async Task PublishAsync(IEventData eventData, EventPublishOptions? options = default, CancellationToken cancellationToken = default)
        {
            if (options == default || options.Mode == Mode.Default)
            {
                await _mediator.Publish(eventData, cancellationToken);
            }
            else if (options.Mode == Mode.RunInBackground)
            {
                await _backgroundEventProcessor.EnqueueAsync(eventData, cancellationToken);
            }
            else if (options.Mode == Mode.RunInThread)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _mediator.Publish((INotification)eventData, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while publishing event in background: {EventData}", eventData);
                    }
                }, cancellationToken);
            }
        }

        public Task PublishAsync(IDomainEventData eventData, EventPublishOptions? options = default, CancellationToken cancellationToken = default)
        {
            return PublishAsync((IEventData)eventData, options, cancellationToken);
        }
    }
}

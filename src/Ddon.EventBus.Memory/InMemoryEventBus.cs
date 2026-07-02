using System.Threading;
using System.Threading.Tasks;
using Ddon.Common.EventBus;
using Ddon.EventBus.Contracts;

namespace Ddon.EventBus.Memory
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly GeneralEventBus _eventBus = GeneralEventBus.Default;

        public Task PublishAsync(IEventData eventData, EventPublishOptions options = default, CancellationToken cancellationToken = default)
        {
            return _eventBus.PublishAsync((dynamic)eventData);
        }

        public Task PublishAsync(IDomainEventData eventData, EventPublishOptions options = default, CancellationToken cancellationToken = default)
        {
            return _eventBus.PublishAsync((dynamic)eventData);
        }
    }
}

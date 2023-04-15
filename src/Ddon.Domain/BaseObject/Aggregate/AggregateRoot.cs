using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ddon.Domain.Event;

namespace Ddon.Domain.BaseObject.Aggregate
{
    public class AggregateRoot : IAggregateRoot
    {
        private readonly List<IDomainEventData> _domainEvents = new();

        [JsonIgnore]
        public IEnumerable<IDomainEventData> DomainEvents => _domainEvents;

        public void AddDomainEvent(IDomainEventData eventItem) => _domainEvents.Add(eventItem);

        public void RemoveDomainEvent(IDomainEventData eventItem) => _domainEvents.Remove(eventItem);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}

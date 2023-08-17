using System.Collections.Generic;
using Ddon.EventBus;

namespace Ddon.Domain.BaseObject.Aggregate
{
    public interface IAggregateRoot
    {        
        IEnumerable<IDomainEventData> DomainEvents { get; }

        void AddDomainEvent(IDomainEventData eventItem);

        void RemoveDomainEvent(IDomainEventData eventItem);

        void ClearDomainEvents();
    }
}

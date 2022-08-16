using MediatR;
using System.Collections.Generic;

namespace Ddon.Domain.Entities
{
    public interface IDomainEvents
    {
        IReadOnlyCollection<INotification> DomainEvents { get; }

        void AddDomainEvent(INotification eventItem);

        void RemoveDomainEvent(INotification eventItem);

        void ClearDomainEvents();
    }

    public interface IAggregateRoot : IDomainEvents
    {
    }
}

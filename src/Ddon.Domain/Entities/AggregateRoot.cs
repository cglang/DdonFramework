using MediatR;
using System;
using System.Collections.Generic;

namespace Ddon.Domain.Entities
{
    public abstract class AggregateRoot<TKey> : TenantEntity<TKey>, IAggregateRoot, IDomainEvents
        where TKey : IEquatable<TKey>
    {
        private readonly List<INotification> _domainEvents = new();

        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(INotification eventItem) => _domainEvents.Add(eventItem);

        public void RemoveDomainEvent(INotification eventItem) => _domainEvents.Remove(eventItem);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}

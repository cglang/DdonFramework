﻿using System.Linq;
using Ddon.Core.Use.Di;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Domain.Entities;
using Ddon.EventBus.Abstractions;

namespace Ddon.Repository.Uow
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>, IScopedDependency where TDbContext : DbContext
    {
        private readonly DbContext _dbContext;
        private readonly IEventBus _eventBus;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(TDbContext dbContext, IEventBus eventBus)
        {
            _dbContext = dbContext;
            _eventBus = eventBus;
        }

        public void Begin()
        {
            _transaction = _dbContext.Database.BeginTransaction();
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                await _transaction.CommitAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            var changeEntities = _dbContext.ChangeTracker.Entries().Select(e => e.Entity);
            foreach (var entity in changeEntities)
            {
                if (entity is not IDomainEvents domainEvents) continue;

                foreach (var domainEvent in domainEvents.DomainEvents)
                {
                    await _eventBus.PublishAsync(domainEvent, cancellationToken);
                }
            }
        }
    }
}

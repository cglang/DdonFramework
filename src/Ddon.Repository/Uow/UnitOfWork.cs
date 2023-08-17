using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Domain.BaseObject.Aggregate;
using Ddon.EventBus.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ddon.Repository.Uow
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : DbContext
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
            await SaveChangesAsync(cancellationToken);

            var changeEntities = _dbContext.ChangeTracker.Entries().Select(e => e.Entity);
            foreach (var entity in changeEntities)
            {
                if (entity is not IAggregateRoot domainEvents) continue;

                foreach (var domainEvent in domainEvents.DomainEvents)
                {
                    await _eventBus.PublishAsync(domainEvent, cancellationToken);
                }
            }

            if (_transaction is not null)
                await _transaction.CommitAsync(cancellationToken);
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

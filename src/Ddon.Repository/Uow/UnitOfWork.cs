using Ddon.Core.Use.Di;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repository.Uow
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>, ITransientDependency where TDbContext : DbContext
    {
        private readonly DbContext _dbContext;

        private IDbContextTransaction? _transaction;

        public UnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Begin()
        {
            _transaction = _dbContext.Database.BeginTransaction();
        }

        public async Task CompleteAsync(CancellationToken cancellationToken = default)
        {
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

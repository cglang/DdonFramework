using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Repository.EntityFrameworkCore.Uow
{
    public interface IUnitOfWork<TDbContext> where TDbContext : DbContext
    {
        void Begin();

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task CompleteAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}

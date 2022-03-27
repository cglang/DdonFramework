using Ddon.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ddon.Uow
{
    public interface IUnitOfWork : IScopedDependency
    {
        void Begin();

        void Commit();

        void Init(DatabaseFacade database);
    }
}
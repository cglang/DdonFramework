using Ddon.Core.Use.Di;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ddon.Repositiry.Uow
{
    public interface IUnitOfWork : IScopedDependency
    {
        void Begin();

        void Commit();

        void Init(DatabaseFacade database);
    }
}
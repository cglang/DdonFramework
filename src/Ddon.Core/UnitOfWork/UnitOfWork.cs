using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ddon.Uow
{
    public class UnitOfWork : IUnitOfWork
    {
        private DatabaseFacade? _database;

        private IDbContextTransaction? _begin;

        public void Begin()
        {
            _begin = _database?.BeginTransaction();
        }

        public void Commit()
        {
            _begin?.Commit();
        }

        public void Init(DatabaseFacade database)
        {
            _database = database;
        }
    }
}

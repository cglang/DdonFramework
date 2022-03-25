using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Ddon.Repositiry.Dapper
{
    public interface IDapperRepository
    {
        IDbConnection DbConnection { get; }

        IDbContextTransaction? DbTransaction { get; }
    }
}

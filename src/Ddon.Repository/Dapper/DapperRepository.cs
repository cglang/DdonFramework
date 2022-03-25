using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ddon.Repositiry.Dapper
{
    public class DapperRepository<TDbContext, TKey> : IDapperRepository
        where TDbContext : DbContext
        where TKey : IEquatable<TKey>
    {
        private readonly TDbContext _dbContext;

        public DapperRepository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IDbConnection DbConnection => _dbContext.Database.GetDbConnection();

        public IDbContextTransaction? DbTransaction => _dbContext.Database.CurrentTransaction;

        public async Task<T> QueryFirstAsync<T>(string sql, object? param = default)
        {
            return await DbConnection.QueryFirstAsync<T>(sql, param);
        }

        public async Task<T> QueryFirstAsync<T, TTenantKey>(QueryBase<TTenantKey> query)
        {
            return await QueryFirstAsync<T>(query.BuildCompleteSql(), query);
        }

        public async Task<long> CountAsync(string sql, object? param = default)
        {
            return await DbConnection.QueryFirstAsync<long>(sql, param);
        }

        public async Task<long> CountAsync<TTenantKey>(QueryBase<TTenantKey> query)
        {
            var sql = $"SELECT COUNT(*) {query.BuildFromSql()} {query.BuildWhereSql()}";
            return await CountAsync(sql, query);
        }
    }
}

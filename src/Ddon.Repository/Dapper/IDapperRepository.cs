using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Ddon.Domain.Entities;

namespace Ddon.Repository.Dapper;

public interface IDapperRepository
{
    IDbConnection DbConnection { get; }

    IEnumerable<T> Query<T>(string sql, object? param = default);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = default);

    T FirstOrDefault<T>(string sql, object? param = default);

    Task<T> FirstOrDefaultAsync<T>(string sql, object? param = default);


    // 以下需要表对应实体
        
    IEnumerable<T> Query<T>(object param);

    Task<IEnumerable<T>> QueryAsync<T>(object param);
        
    T FirstOrDefault<T>(object param) where T : IEntity;

    Task<T> FirstOrDefaultAsync<T>(object param) where T : IEntity;

    T FirstOrDefaultById<T, TKey>(TKey key)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>;

    Task<T> FirstOrDefaultByIdAsync<T, TKey>(TKey key)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>;

    IEnumerable<T> QueryByIds<T, TKey>(IEnumerable<TKey> keys)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>;

    Task<IEnumerable<T>> QueryByIdsAsync<T, TKey>(IEnumerable<TKey> keys)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>;

    // 使用查询对象
    IEnumerable<T> Query<T>(QueryBase query);

    Task<IEnumerable<T>> QueryAsync<T>(QueryBase query);

    T FirstOrDefault<T>(QueryBase query);

    Task<T> FirstOrDefaultAsync<T>(QueryBase query);

    PageResult<T> Query<T>(QueryPageBase query);

    Task<PageResult<T>> QueryAsync<T>(QueryPageBase query);
}

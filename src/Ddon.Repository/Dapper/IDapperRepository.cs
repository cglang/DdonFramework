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

    T? FirstOrDefault<T>(string sql, object? param = default);

    Task<T?> FirstOrDefaultAsync<T>(string sql, object? param = default);


    // 以下需要表对应实体

    IEnumerable<T> Query<T>(object param) where T : class;

    Task<IEnumerable<T>> QueryAsync<T>(object param) where T : class;

    T? FirstOrDefault<T>(object param) where T : class, IEntity;

    Task<T?> FirstOrDefaultAsync<T>(object param) where T : class, IEntity;

    T? FirstOrDefaultById<T, TKey>(TKey key)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>;

    Task<T?> FirstOrDefaultByIdAsync<T, TKey>(TKey key)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>;

    IEnumerable<T> QueryByIds<T, TKey>(TKey[] keys)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>;

    Task<IEnumerable<T>> QueryByIdsAsync<T, TKey>(TKey[] keys)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>;

    // 使用查询对象
    IEnumerable<T> Query<T>(QueryBase query) where T : class;

    Task<IEnumerable<T>> QueryAsync<T>(QueryBase query) where T : class;

    T? FirstOrDefault<T>(QueryBase query) where T : class;

    Task<T?> FirstOrDefaultAsync<T>(QueryBase query) where T : class;

    PageResult<T> Query<T>(QueryPageBase query) where T : class;

    Task<PageResult<T>> QueryAsync<T>(QueryPageBase query) where T : class;
}

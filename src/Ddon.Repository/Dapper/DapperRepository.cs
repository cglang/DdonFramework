using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Ddon.Domain.Entities;

namespace Ddon.Repository.Dapper;

public class DapperRepository : IDapperRepository
{
    public DapperRepository()
    {
        DbConnection = DbConnectionProvider.Connection;
    }

    public DapperRepository(IDbConnection dbConnection)
    {
        DbConnection = dbConnection;
    }

    public IDbConnection DbConnection { get; }

    public T FirstOrDefault<T>(string sql, object? param = null)
    {
        return DbConnection.QueryFirstOrDefault<T>(sql, param);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(object param)
    {
        throw new NotImplementedException();
    }

    public T FirstOrDefault<T>(object param) where T : IEntity
    {
        var para = BuildParams(param);
        var sql = $"{BuildSelectSql<T>()} WHERE {string.Join(" AND ", para.Value)}";
        return FirstOrDefault<T>(sql, param);
    }

    public T FirstOrDefault<T>(QueryBase query)
    {
        return FirstOrDefault<T>(query.BuildCompleteSql(), query);
    }

    public Task<T> FirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        return DbConnection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public IEnumerable<T> Query<T>(object param)
    {
        throw new NotImplementedException();
    }

    public Task<T> FirstOrDefaultAsync<T>(object param) where T : IEntity
    {
        var para = BuildParams(param);
        var sql = $"{BuildSelectSql<T>()} WHERE {string.Join(" AND ", para.Value)}";
        return FirstOrDefaultAsync<T>(sql, param);
    }

    public Task<T> FirstOrDefaultAsync<T>(QueryBase query)
    {
        return FirstOrDefaultAsync<T>(query.BuildCompleteSql(), query);
    }

    public T FirstOrDefaultById<T, TKey>(TKey key)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>();
        return FirstOrDefault<T>(sql);
    }

    public Task<T> FirstOrDefaultByIdAsync<T, TKey>(TKey key)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>();
        return FirstOrDefaultAsync<T>(sql);
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        return DbConnection.Query<T>(sql, param);
    }

    public IEnumerable<T> Query<T>(QueryBase query)
    {
        return Query<T>(query.BuildCompleteSql(), query);
    }

    public PageResult<T> Query<T>(QueryPageBase query)
    {
        var data = Query<T>(query.BuildCompleteSql(), query);
        var count = Count(query);

        return new PageResult<T>(data, count);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return DbConnection.QueryAsync<T>(sql, param);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(QueryBase query)
    {
        return QueryAsync<T>(query.BuildCompleteSql(), query);
    }

    public async Task<PageResult<T>> QueryAsync<T>(QueryPageBase query)
    {
        var data = await QueryAsync<T>(query.BuildCompleteSql(), query);
        var count = await CountAsync(query);

        return new PageResult<T>(data, count);
    }

    public IEnumerable<T> QueryByIds<T, TKey>(IEnumerable<TKey> keys)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>(true);
        return Query<T>(sql, new { Key = keys });
    }

    public Task<IEnumerable<T>> QueryByIdsAsync<T, TKey>(IEnumerable<TKey> keys)
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>(true);
        return QueryAsync<T>(sql, new { Key = keys });
    }

    private static string BuildWhereKeySqlByTable<T>(bool isIDs = false)
    {
        var tableName = GetTableName<T>();
        var pk = ReadOnlyQueryExtensions.GetPrimaryKeys<T>();
        var fields = ReadOnlyQueryExtensions.GetFields<T>();
        var alias = fields.Select(p => $"{p.Key} {p.Value}");
        return
            $"SELECT {string.Join(",", alias)} FROM {tableName}(NOLOCK) WHERE {pk.FirstOrDefault().Key} {(isIDs ? "IN" : "=")} @Key";
    }

    private static string BuildSelectSql<T>()
    {
        var tableName = GetTableName<T>();
        var fields = ReadOnlyQueryExtensions.GetFields<T>();
        var alias = fields.Select(p => $"{p.Key} {p.Value}");
        return $"SELECT {string.Join(",", alias)} FROM {tableName}(NOLOCK)";
    }

    private static string GetTableName<T>()
    {
        var att = typeof(T).GetCustomAttribute<TableAttribute>();
        if (att == null) throw new NotSupportedException("TableAttribute does not exist");
        return att.Name;
    }

    private static KeyValuePair<DynamicParameters, List<string>> BuildParams(object param)
    {
        var paramCount = 0;
        var valuePair = new KeyValuePair<DynamicParameters, List<string>>(new DynamicParameters(), new List<string>());

        var props = param.GetType().GetProperties();
        foreach (var prop in props)
        {
            valuePair.Value.Add($"{prop.Name}=@{prop.Name}");
            valuePair.Key.Add(prop.Name, prop.GetValue(param));
            paramCount++;
        }

        if (paramCount > 2000)
            throw new ArgumentException($"单次查询只允许{2000}个参数,请手动拆分List类型参数数量，分批查询");
        return valuePair;
    }


    private long Count(string sql, object? param = default)
    {
        return FirstOrDefault<long>(sql, param);
    }

    private long Count(QueryBase query)
    {
        var sql = $"SELECT COUNT(1) {query.BuildFromSql()} {query.BuildWhereSql()}";
        return Count(sql, query);
    }

    private Task<long> CountAsync(string sql, object? param = default)
    {
        return FirstOrDefaultAsync<long>(sql, param);
    }

    private Task<long> CountAsync(QueryBase query)
    {
        var sql = $"SELECT COUNT(1) {query.BuildFromSql()} {query.BuildWhereSql()}";
        return CountAsync(sql, query);
    }
}

public class ReadOnlyQueryExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> Fields;

    private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> PrimaryKeyFields;

    static ReadOnlyQueryExtensions()
    {
        Fields = new ConcurrentDictionary<Type, Dictionary<string, string>>();
        PrimaryKeyFields = new ConcurrentDictionary<Type, Dictionary<string, string>>();
    }

    public static Dictionary<string, string> GetFields<T>()
    {
        return Fields.GetOrAdd(typeof(T), _ =>
        {
            var props = typeof(T).GetProperties().Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);
            var result = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    continue;
                if (prop.GetCustomAttribute<ForeignKeyAttribute>() != null)
                    continue;
                if (prop.PropertyType.GetCustomAttribute<TableAttribute>() != null)
                    continue;

                var col = prop.GetCustomAttribute<ColumnAttribute>();
                result.Add(string.IsNullOrWhiteSpace(col?.Name) ? prop.Name : col.Name, prop.Name);
            }

            return result;
        });
    }


    public static Dictionary<string, string> GetPrimaryKeys<T>()
    {
        return PrimaryKeyFields.GetOrAdd(typeof(T), _ =>
        {
            var props = typeof(T).GetProperties().Where(t => t.GetCustomAttribute<KeyAttribute>() != null);
            var result = new Dictionary<string, string>();
            foreach (var prop in props)
            {
                var col = prop.GetCustomAttribute<ColumnAttribute>();
                result.Add(string.IsNullOrWhiteSpace(col?.Name) ? prop.Name : col.Name, prop.Name);
            }

            return result;
        });
    }
}

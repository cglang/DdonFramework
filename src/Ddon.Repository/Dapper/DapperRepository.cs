﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Ddon.Domain.Entities;
using Ddon.Repository.Dapper.Generator;

namespace Ddon.Repository.Dapper;

public class DapperRepository : IDapperRepository
{
    public DapperRepository()
    {
        DbConnection = DapperInitialization.Connection;
    }

    public IDbConnection DbConnection { get; }

    public T? FirstOrDefault<T>(string sql, object? param = null)
    {
        return DbConnection.QueryFirstOrDefault<T>(sql, param);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(object param) where T : class
    {
        throw new NotImplementedException();
    }

    public T? FirstOrDefault<T>(object param) where T : class, IEntity
    {
        var keyValuePair = BuildParams(param);
        var sql = $"{BuildSelectSql<T>()} WHERE {string.Join(" AND ", keyValuePair.Value)}";
        return FirstOrDefault<T>(sql, param);
    }

    public T? FirstOrDefault<T>(QueryBase query) where T : class
    {
        return FirstOrDefault<T>(query.BuildCompleteSql(), query);
    }

    public Task<T?> FirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        return DbConnection.QueryFirstOrDefaultAsync<T?>(sql, param);
    }

    public IEnumerable<T> Query<T>(object param) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<T?> FirstOrDefaultAsync<T>(object param) where T : class, IEntity
    {
        var keyValuePair = BuildParams(param);
        var sql = $"{BuildSelectSql<T>()} WHERE {string.Join(" AND ", keyValuePair.Value)}";
        return FirstOrDefaultAsync<T>(sql, param);
    }

    public Task<T?> FirstOrDefaultAsync<T>(QueryBase query) where T : class
    {
        return FirstOrDefaultAsync<T>(query.BuildCompleteSql(), query);
    }

    public T? FirstOrDefaultById<T, TKey>(TKey key)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>();
        return FirstOrDefault<T>(sql, new { Key = key });
    }

    public Task<T?> FirstOrDefaultByIdAsync<T, TKey>(TKey key)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>();
        return FirstOrDefaultAsync<T>(sql, new { Key = key });
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        return DbConnection.Query<T>(sql, param);
    }

    public IEnumerable<T> Query<T>(QueryBase query) where T : class
    {
        return Query<T>(query.BuildCompleteSql(), query);
    }

    public PageResult<T> Query<T>(QueryPageBase query) where T : class
    {
        var data = Query<T>(query.BuildCompleteSql(), query);
        var count = Count(query);

        return new PageResult<T>(data, count);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return DbConnection.QueryAsync<T>(sql, param);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(QueryBase query) where T : class
    {
        return QueryAsync<T>(query.BuildCompleteSql(), query);
    }

    public async Task<PageResult<T>> QueryAsync<T>(QueryPageBase query) where T : class
    {
        var data = await QueryAsync<T>(query.BuildCompleteSql(), query);
        var count = await CountAsync(query);

        return new PageResult<T>(data, count);
    }

    public IEnumerable<T> QueryByIds<T, TKey>(TKey[] keys)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>(true);
        return Query<T>(sql, new { Key = keys });
    }

    public Task<IEnumerable<T>> QueryByIdsAsync<T, TKey>(TKey[] keys)
        where T : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        var sql = BuildWhereKeySqlByTable<T>(true);
        return QueryAsync<T>(sql, new { Key = keys });
    }

    private static string BuildWhereKeySqlByTable<T>(bool isIDs = false) where T : class
    {
        var tableName = SqlGenerator<T>.TableName;
        var primaryKeys = SqlGenerator<T>.GetPrimaryKeys();
        var fields = SqlGenerator<T>.GetFields();
        var alias = fields.Select(p => $"{p.Key} {p.Value}");

        var sql = new StringBuilder();
        sql.Append($"SELECT {string.Join(",", alias)}").Append(Environment.NewLine);
        sql.Append($"FROM {tableName}").Append(Environment.NewLine);
        sql.Append($"WHERE {primaryKeys.FirstOrDefault().Key} {(isIDs ? "IN" : "=")} @Key");
        return sql.ToString();
    }

    private static string BuildSelectSql<T>() where T : class
    {
        var tableName = SqlGenerator<T>.TableName;
        var fields = SqlGenerator<T>.GetFields();
        var alias = fields.Select(p => $"{p.Key} {p.Value}");
        return $"SELECT {string.Join(",", alias)} FROM {tableName}";
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

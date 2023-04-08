using System;
using System.Data;
using Dapper;
using Ddon.Repository.Dapper.TypeHandler;

namespace Ddon.Repository.Dapper;

public class DapperInitialization
{
    private static IDbConnection? _connection = null;

    private static DatabaseType _databaseType;

    public static DatabaseType DatabaseType
    {
        get
        {
            if (_databaseType == default)
                throw new ArgumentOutOfRangeException(nameof(DatabaseType), "DatabaseType 未初始化");
            return _databaseType;
        }
        set => _databaseType = value;
    }

    public static void Init(IDbConnection connection, DatabaseType databaseType)
    {
        _connection = connection;
        DatabaseType = databaseType;

        SqlMapper.AddTypeHandler(new GuidTypeHandler());
        SqlMapper.AddTypeHandler(new NullableGuidTypeHandler());
    }

    public static IDbConnection Connection
    {
        get
        {
            if (_connection is null)
                throw new Exception($"IDbConnection 未初始化");

            return _connection;
        }
        private set
        {
            _connection = value;
        }
    }
}

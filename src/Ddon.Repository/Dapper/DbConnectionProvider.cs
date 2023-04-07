using System;
using System.Data;

namespace Ddon.Repository.Dapper;

public class DbConnectionProvider
{
    private static IDbConnection? _connection = null;

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

    public static void Init(IDbConnection connection)
    {
        Connection = connection;
    }
}

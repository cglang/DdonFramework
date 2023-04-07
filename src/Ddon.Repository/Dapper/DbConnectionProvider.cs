using System;
using System.Data;

namespace Ddon.Repository.Dapper
{
    public class DbConnectionProvider
    {
        private static IDbConnection? connection = null;

        public static IDbConnection Connection
        {
            get
            {
                if (connection is null)
                    throw new Exception($"IDbConnection 未初始化");

                return connection;
            }
            private set
            {
                connection = value;
            }
        }

        public static void Init(IDbConnection connection)
        {
            Connection = connection;
        }
    }
}

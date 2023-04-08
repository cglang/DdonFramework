using System.Text;

namespace Ddon.Repository.Dapper.SqlGenerator;

public sealed class SqlQueryWhere
{
    public SqlQueryWhere()
    {
        SqlBuilder = new StringBuilder();
    }

    public StringBuilder SqlBuilder { get; }

    public object? Param { get; private set; }
    
    public string GetWhereSql()
    {
        return SqlBuilder.ToString().Trim();
    }
    
    public void SetParam(object param)
    {
        Param = param;
    }
}

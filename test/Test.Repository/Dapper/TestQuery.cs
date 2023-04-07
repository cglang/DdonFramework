using System;
using Ddon.Repository.Dapper;

namespace Test.Repository.Dapper;

public class TestQuery : QueryBase
{
    public int Id { get; set; }

    public DateTime CreateTime { get; set; }

    protected override string SelectSql()
    {
        return "SELECT *";
    }

    protected override string FromSql()
    {
        return "FROM Test";
    }

    protected override void WhereSql()
    {
        AddWhere("Id=@Id");
        AddWhere("CreateTime=@CreateTime", AndOr.Or);
    }

    protected override void SortSql()
    {
        AddSort("Id");
        AddSort("CreateTime", SortType.Desc);
    }
}

public class TestPageQuery : QueryPageBase
{
    public int Id { get; set; }

    public DateTime CreateTime { get; set; }

    protected override string SelectSql()
    {
        return "SELECT *";
    }

    protected override string FromSql()
    {
        return "FROM Test";
    }

    protected override void WhereSql()
    {
        AddWhere("Id=@Id");
        AddWhere("CreateTime=@CreateTime", AndOr.Or);
    }

    protected override void SortSql()
    {
        AddSort("Id");
        AddSort("CreateTime", SortType.Desc);
    }
}

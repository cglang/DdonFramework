using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Repository.Dapper;

[TestClass]
public class QueryBaseTest
{
    private readonly TestQuery _query = new() { Id = 10, CreateTime = DateTime.Parse("2023-01-01") };

    private readonly TestPageQuery _pageQuery = new()
    {
        PageIndex = 1, PageCount = 10, Id = 10, CreateTime = DateTime.Parse("2023-01-01")
    };

    [TestMethod]
    public void BaseBuildSelectTest()
    {
        var sql = _query.BuildSelectSql().ToString();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("SELECT *").Append(Environment.NewLine);

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseBuildFromTest()
    {
        var sql = _query.BuildFromSql().ToString();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("FROM Test").Append(Environment.NewLine);

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseBuildWhereTest()
    {
        var sql = _query.BuildWhereSql().ToString();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("WHERE 1 = 1 And Id=@Id Or CreateTime=@CreateTime").Append(Environment.NewLine);

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseBuildOrderByTest()
    {
        var sql = _query.BuildOrderBy().ToString();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("ORDER BY Id Asc,CreateTime Desc");

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseQueryTest()
    {
        var sql = _query.BuildCompleteSql();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("SELECT *").Append(Environment.NewLine);
        sqlBuilder.Append("FROM Test").Append(Environment.NewLine);
        sqlBuilder.Append("WHERE 1 = 1 And Id=@Id Or CreateTime=@CreateTime").Append(Environment.NewLine);
        sqlBuilder.Append("ORDER BY Id Asc,CreateTime Desc");

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseBuildPageTest()
    {
        var sql = _pageQuery.BuildPageSql().ToString();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }

    [TestMethod]
    public void BaseQueryPageTest()
    {
        var sql = _pageQuery.BuildCompleteSql();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.Append("SELECT *").Append(Environment.NewLine);
        sqlBuilder.Append("FROM Test").Append(Environment.NewLine);
        sqlBuilder.Append("WHERE 1 = 1 And Id=@Id Or CreateTime=@CreateTime").Append(Environment.NewLine);
        sqlBuilder.Append("ORDER BY Id Asc,CreateTime Desc").Append(Environment.NewLine);
        sqlBuilder.Append("OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY");

        string resultsql = sqlBuilder.ToString();
        Assert.AreEqual(sql, resultsql);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ddon.Repository.Dapper.SqlGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Repository.Dapper;

[TestClass]
public class SqlGeneratorTest
{
    [TestMethod]
    public void InsertTest()
    {
        var sql = new SqlGenerator<TestTable>();

        var query = sql.AppendWherePredicateQuery(x =>
            x.Id == 1
            && x.CreateTime > DateTime.Now
            && x.Id != 1
            || x.Id != 1
            && (x.Id != 1 || x.Id != 1)
            && new List<int>() { 2 }.Contains(x.Id)
            && !new List<int>() { 2 }.Contains(x.Id));
        var sqltext = query.GetWhereSql();
        Console.WriteLine(sqltext);
    }
}

[Table("Test")]
class TestTable
{
    public int Id { get; set; }

    [Column("CreateDate")]
    public DateTime CreateTime { get; set; }
}

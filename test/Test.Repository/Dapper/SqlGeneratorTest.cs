using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Repository.Dapper;

[TestClass]
public class SqlGeneratorTest
{
    [TestMethod]
    public void InsertTest()
    {
    }
}

[Table("Test")]
class TestTable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime CreateTime { get; set; }
}

using System;
using Ddon.Core.Use.Di;
using Ddon.Domain.BaseObject;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository.Db
{
    public class TestRepository : IScopedDependency
    {
        public DbSet<TestEntity> Test { get; set; }
    }

    public class TestEntity : AuditEntity<Guid>
    {
        public string? Title { get; set; }
    }
}

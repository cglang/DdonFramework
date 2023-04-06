using System;
using Ddon.Core.Use.Di;
using Ddon.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository.Db
{
    public class TestRepository : IScopedDependency
    {
        public DbSet<TestEntity> Test { get; set; }
    }

    public class TestEntity : AuditedAggregateRoot<Guid>
    {
        public string? Title { get; set; }
    }
}

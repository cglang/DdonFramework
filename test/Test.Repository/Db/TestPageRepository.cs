using System;
using Ddon.Core.Use.Di;
using Ddon.Domain.Entities.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository.Db
{
    public class TestPageRepository : IScopedDependency
    {
        public DbSet<TestPageEntity> TestPage { get; set; }
    }

    public class TestPageEntity : AuditedAggregateRoot<Guid>
    {
        public string? Title { get; set; }
    }
}

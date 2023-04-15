using System;
using Ddon.Core.Use.Di;
using Ddon.Domain.BaseObject;
using Microsoft.EntityFrameworkCore;

namespace Test.Repository.Db
{
    public class TestPageRepository : IScopedDependency
    {
        public DbSet<TestPageEntity> TestPage { get; set; }
    }

    public class TestPageEntity : AuditEntity<Guid>
    {
        public string? Title { get; set; }
    }
}

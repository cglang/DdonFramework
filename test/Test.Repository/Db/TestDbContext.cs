using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Test.Repository.Db
{
    public class TestDbContext : BasicDbContext<TestDbContext, Guid>
    {
        public TestDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<TestDbContext> options) : base(lazyServiceProvider, options)
        {
        }

        [AllowNull]
        public DbSet<TestEntity> Tests { get; set; }
    }
}

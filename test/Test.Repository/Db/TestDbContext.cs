using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Test.Repository.Db
{
    public class TestDbContext : BasicDbContext<TestDbContext>
    {
        public TestDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<TestDbContext> options) : base(lazyServiceProvider, options)
        {
        }

        [AllowNull]
        public DbSet<TestEntity> Tests { get; set; }
        public DbSet<TestPageEntity> TestPages { get; set; }
    }
}

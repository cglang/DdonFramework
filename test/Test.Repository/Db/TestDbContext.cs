using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Test.Repository.Db
{
    public class TestDbContext : BasicDbContext<TestDbContext>
    {
        public TestDbContext(IServiceProvider serviceProvider, DbContextOptions<TestDbContext> options) : base(serviceProvider, options)
        {
        }

        [AllowNull]
        public DbSet<TestEntity> Tests { get; set; }
        public DbSet<TestPageEntity> TestPages { get; set; }
    }
}

using Ddon.Core.Use.Di;
using Ddon.Domain.Entities.Auditing;
using Ddon.Domain.Repositories;
using Ddon.Repositiry.EntityFrameworkCore;
using System;

namespace Test.Repository.Db
{
    public class TestPageRepository : EfCoreRepository<TestDbContext, TestPageEntity, Guid>, IRepository<TestPageEntity>, ITransientDependency
    {
        public TestPageRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }

    public class TestPageEntity : AuditedAggregateRoot<Guid>
    {
        public string? Title { get; set; }
    }
}

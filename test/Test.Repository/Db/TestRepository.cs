using Ddon.Core.Use.Di;
using Ddon.Domain.Repositories;
using Ddon.Repositiry.EntityFrameworkCore;
using System;

namespace Test.Repository.Db
{
    public class TestRepository : EfCoreRepository<TestDbContext, TestEntity, Guid>, IRepository<TestEntity>, ITransientDependency
    {
        public TestRepository(TestDbContext dbContext) : base(dbContext)
        {
        }
    }
}

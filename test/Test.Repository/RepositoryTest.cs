using Ddon.Domain.Dtos;
using Ddon.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Test.Repository.Db;

namespace Test.Repository
{
    [TestClass]
    public class RepositoryTest : TestBase<TestRepositoryModule>
    {
        [TestMethod]
        public async Task DbContextBasicTest()
        {
            var db = ServiceProvider.LazyGetService<TestDbContext>()!;

            var en = new TestEntity { Title = "测试" };
            await db.Tests.AddAsync(en);
            await db.SaveChangesAsync();

            var aaa = db.Tests.Count();
            Assert.IsTrue(aaa > 0);
        }

        [TestMethod]
        public async Task RepositoryTestAsync()
        {
            var repository = ServiceProvider.LazyGetService<TestRepository>();

            var en = new TestEntity { Title = "测试" };
            await repository.AddAsync(en, true);

            var count = await repository.GetCountAsync();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public async Task RepositoryTestQueryPageAsync()
        {
            var repository = ServiceProvider.LazyGetService<TestRepository>();

            var en = new TestEntity { Title = "测试" };
            await repository.AddAsync(en, true);
            await repository.AddAsync(en, true);
            await repository.AddAsync(en, true);
            await repository.AddAsync(en, true);
            await repository.AddAsync(en, true);

            Page page = new()
            {
                Index = 1,
                Size = 2
            };
            var pageResult = await repository.GetListAsync(page);
            Assert.IsNotNull(pageResult);

            var pageResult1 = repository.GetListAsync(page);
            var pageResult2 = repository.GetListAsync(page);
            var pageResult3 = repository.GetListAsync(page);
            var pageResult4 = repository.GetListAsync(page);

            await Task.WhenAll(pageResult1, pageResult2, pageResult3, pageResult4);

            var count = await repository.GetCountAsync();
            Assert.IsTrue(count > 0);
        }
    }
}

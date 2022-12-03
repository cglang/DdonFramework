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
            var testRepository = ServiceProvider.LazyGetService<TestRepository>();
            var pageRepository = ServiceProvider.LazyGetService<TestPageRepository>();

            var en = new TestEntity { Title = "测试" };
            await testRepository.AddAsync(en, true);
            await testRepository.AddAsync(en, true);
            await testRepository.AddAsync(en, true);
            await testRepository.AddAsync(en, true);
            await testRepository.AddAsync(en, true);

            var pg = new TestPageEntity { Title = "测试" };
            await pageRepository.AddAsync(pg, true);
            await pageRepository.AddAsync(pg, true);
            await pageRepository.AddAsync(pg, true);
            await pageRepository.AddAsync(pg, true);
            await pageRepository.AddAsync(pg, true);

            Page page = new()
            {
                Index = 1,
                Size = 2
            };
            var pageResult = await testRepository.GetListAsync(page);
            Assert.IsNotNull(pageResult);

            var testPageResult = await pageRepository.GetListAsync(page);
            Assert.IsNotNull(testPageResult);

            var pageResult1 = testRepository.GetListAsync(page);
            var pageResult2 = testRepository.GetListAsync(page);
            var pageResult3 = testRepository.GetListAsync(page);
            var pageResult4 = testRepository.GetListAsync(page);

            await Task.WhenAll(pageResult1, pageResult2, pageResult3, pageResult4);

            var count = await testRepository.GetCountAsync();
            Assert.IsTrue(count > 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ddon.Domain.Extensions.ValueObject;
using Ddon.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Repository.Db;

namespace Test.Repository
{
    [TestClass]
    public class RepositoryTest : TestBase<TestRepositoryModule>
    {
        [TestMethod]
        public async Task DbContextBasicTest()
        {
            var db = ServiceProvider.GetRequiredService<TestDbContext>()!;

            var en = new TestEntity { Title = "测试" };
            await db.Tests.AddAsync(en);
            await db.SaveChangesAsync();

            var aaa = db.Tests.Count();
            Assert.IsTrue(aaa > 0);
        }

        [TestMethod]
        public async Task RepositoryTestAsync()
        {
            var repository = ServiceProvider.GetRequiredService<TestRepository>();

            var en = new TestEntity { Title = "测试" };
            await repository.Test.AddAsync(en);

            var count = await repository.Test.CountAsync();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public async Task RepositoryTestQueryPageAsync()
        {
            var testRepository = ServiceProvider.GetRequiredService<TestRepository>();
            var pageRepository = ServiceProvider.GetRequiredService<TestPageRepository>();

            var en = new TestEntity { Title = "测试" };
            await testRepository.Test.AddAsync(en);
            await testRepository.Test.AddAsync(en);
            await testRepository.Test.AddAsync(en);
            await testRepository.Test.AddAsync(en);
            await testRepository.Test.AddAsync(en);

            var pg = new TestPageEntity { Title = "测试" };
            await pageRepository.TestPage.AddAsync(pg);
            await pageRepository.TestPage.AddAsync(pg);
            await pageRepository.TestPage.AddAsync(pg);
            await pageRepository.TestPage.AddAsync(pg);
            await pageRepository.TestPage.AddAsync(pg);

            Page page = new()
            {
                Index = 1,
                Size = 2
            };
            var pageResult = await testRepository.Test.QueryPageOrderByAsync(page);
            Assert.IsNotNull(pageResult);

            var testPageResult = await pageRepository.TestPage.QueryPageOrderByAsync(page);
            Assert.IsNotNull(testPageResult);

            var pageResult1 = testRepository.Test.QueryPageOrderByAsync(page);
            var pageResult2 = testRepository.Test.QueryPageOrderByAsync(page);
            var pageResult3 = testRepository.Test.QueryPageOrderByAsync(page);
            var pageResult4 = testRepository.Test.QueryPageOrderByAsync(page);

            await Task.WhenAll(pageResult1, pageResult2, pageResult3, pageResult4);

            var count = await testRepository.Test.CountAsync();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public async Task AddRangeTestAsync()
        {
            var repository = ServiceProvider.GetRequiredService<TestRepository>();

            var value = Guid.NewGuid().ToString();
            var value2 = Guid.NewGuid().ToString();
            var value3 = Guid.NewGuid().ToString();
            List<TestEntity> ls = new()
            {
                new TestEntity { Title = value },
                new TestEntity { Title = value2 },
                new TestEntity { Title = value3 },
            };
            await repository.Test.AddRangeAsync(ls);
            var re = await repository.Test.Where(x => ls.Select(l => l.Title).Contains(x.Title)).ToListAsync();
            Assert.AreEqual(ls.Count, re.Count);
        }
    }
}

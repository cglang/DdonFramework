﻿using Ddon.Domain.Dtos;
using Ddon.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            await repository.AddAsync(en, true);

            var count = await repository.GetCountAsync();
            Assert.IsTrue(count > 0);
        }

        [TestMethod]
        public async Task RepositoryTestQueryPageAsync()
        {
            var testRepository = ServiceProvider.GetRequiredService<TestRepository>();
            var pageRepository = ServiceProvider.GetRequiredService<TestPageRepository>();

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
            await repository.AddAsync(ls, true);
            var re = await repository.GetListAsync(x => ls.Select(l => l.Title).Contains(x.Title));
            Assert.AreEqual(ls.Count, re.Count);

            var c = repository.GetCountAsync();
        }
    }
}

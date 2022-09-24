using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Ddon.Test;
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
    }
}

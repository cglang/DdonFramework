using Ddon.Cache;
using Ddon.Test;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Cache
{
    [TestClass]
    public class CacheTest : TestBase<TestCacheMemoryModule>
    {
        readonly ICache _cache;

        public CacheTest()
        {
            _cache = ServiceProvider.GetRequiredService<ICache>();
        }

        [TestMethod]
        public void BaseTestMethod()
        {
            var cache = ServiceProvider.GetRequiredService<ICache>();
            Assert.IsNotNull(cache);
        }

        [TestMethod]
        public async Task TestCecheGetSet()
        {
            var key = Guid.NewGuid().ToString();
            var cacheValue = Guid.NewGuid().ToString();

            await _cache.SetAsync(key, cacheValue);
            var value = await _cache.GetAsync<string>(key);

            Assert.AreEqual(cacheValue, value);
        }
    }
}

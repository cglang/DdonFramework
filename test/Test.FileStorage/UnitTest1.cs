using Ddon.FileStorage.DataBase;
using Ddon.FileStorage.Service;
using Ddon.Test;

namespace Test.FileStorage
{
    [TestClass]
    public class UnitTest1 : TestBase<TestFileStorageModule>
    {
        private readonly IFileStorageService _fileStorage;
        private readonly DatabaseMigrate _databaseMigrate;
        public UnitTest1()
        {
            _fileStorage = ServiceProvider.LazyGetService<IFileStorageService>();
            _databaseMigrate= ServiceProvider.LazyGetService<DatabaseMigrate>();
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            Assert.IsNotNull(_fileStorage);
            await _databaseMigrate.MigrateAsync();
        }
    }
}
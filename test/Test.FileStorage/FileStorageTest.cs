using Ddon.FileStorage.DataBase;
using Ddon.FileStorage.Service;
using Ddon.Test;

namespace Test.FileStorage
{
    [TestClass]
    public class FileStorageTest : TestBase<TestFileStorageModule>
    {
        private readonly IFileStorageService _fileStorage;
        private readonly DatabaseMigrate _databaseMigrate;
        private readonly IFileStorageService _fileStorageService;

        public FileStorageTest()
        {
            _fileStorage = ServiceProvider.LazyGetService<IFileStorageService>();
            _databaseMigrate = ServiceProvider.LazyGetService<DatabaseMigrate>();
            _fileStorageService = ServiceProvider.LazyGetService<IFileStorageService>();
        }

        [TestMethod]
        public async Task TestMethod()
        {
            Assert.IsNotNull(_fileStorage);
            await _databaseMigrate.MigrateAsync();

            var fileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            var fileS = File.OpenRead(fileNames.First());
            await _fileStorageService.SaveFileAsync(fileS, fileS.Name);
        }
    }
}
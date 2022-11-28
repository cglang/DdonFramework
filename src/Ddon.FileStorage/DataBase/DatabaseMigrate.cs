using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ddon.FileStorage.DataBase
{
    public class DatabaseMigrate
    {
        private readonly FileStorageDbContext _dbContext;

        public DatabaseMigrate(FileStorageDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task MigrateAsync()
        {
            var datasource = FileStorageConfig.DatabaseSource;

            Directory.CreateDirectory(FileStorageConfig.FileStorageFullPath);
            if (!File.Exists(datasource)) File.WriteAllBytes(datasource, Array.Empty<byte>());

            await _dbContext.Database.MigrateAsync();
        }
    }
}

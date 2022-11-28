using Microsoft.EntityFrameworkCore;
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
            //_dbContext.Database
            var aaa = await _dbContext.Database.GetAppliedMigrationsAsync();
            await _dbContext.Database.MigrateAsync();

            //_dbContext.Database.mi
        }
    }
}

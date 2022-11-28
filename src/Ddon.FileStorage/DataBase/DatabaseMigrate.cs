using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task MigrateAsync() => await _dbContext.Database.MigrateAsync();
    }
}

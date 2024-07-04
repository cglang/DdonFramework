using Microsoft.EntityFrameworkCore;

namespace Ddon.FileStorage.DataBase
{
    internal class FileStorageRepository : IFileStorageRepository
    {
        public DbSet<FileEntity> File { get; }
        
        public FileStorageRepository(FileStorageDbContext dbContext)
        {
            File = dbContext.Files;
        }

    }
}

using Ddon.Repositiry.EntityFrameworkCore;

namespace Ddon.FileStorage.DataBase
{
    internal class FileStorageRepository : EfCoreRepository<FileStorageDbContext, FileEntity, string>, IFileStorageRepository
    {
        public FileStorageRepository(FileStorageDbContext dbContext) : base(dbContext)
        {
        }
    }
}

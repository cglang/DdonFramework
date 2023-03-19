using Microsoft.EntityFrameworkCore;

namespace Ddon.FileStorage.DataBase
{
    public interface IFileStorageRepository
    {
        public DbSet<FileEntity> File { get; }
    }
}

using System;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;

namespace Ddon.FileStorage.DataBase
{
    public class FileStorageDbContext : BasicDbContext<FileStorageDbContext>
    {
        public DbSet<FileEntity>? Files { get; set; }

        public FileStorageDbContext(IServiceProvider serviceProvider, DbContextOptions<FileStorageDbContext> options)
            : base(serviceProvider, options)
        {
        }
    }
}

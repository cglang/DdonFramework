using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.FileStorage.DataBase
{
    public class FileStorageDbContext : BasicDbContext<FileStorageDbContext, Guid>
    {
        public DbSet<FileEntity>? Files { get; set; }

        public FileStorageDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<FileStorageDbContext> options)
            : base(lazyServiceProvider, options)
        {
        }
    }
}

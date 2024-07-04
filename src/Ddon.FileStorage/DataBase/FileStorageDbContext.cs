using System;
using System.Diagnostics.CodeAnalysis;
using Ddon.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ddon.FileStorage.DataBase
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class FileStorageDbContext : BasicDbContext<FileStorageDbContext>
    {
        [AllowNull] public DbSet<FileEntity> Files { get; set; }

        public FileStorageDbContext(IServiceProvider serviceProvider, DbContextOptions<FileStorageDbContext> options)
            : base(serviceProvider, options)
        {
        }
    }
}

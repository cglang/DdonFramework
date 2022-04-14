using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ddon.UploadFile
{
    public class UploadFileDbContext : BasicDbContext<UploadFileDbContext, Guid>
    {
        public DbSet<FileEntity>? Files { get; set; }

        public UploadFileDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<UploadFileDbContext> options) : base(lazyServiceProvider, options)
        {
        }
    }
}

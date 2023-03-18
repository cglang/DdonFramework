using System;
using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;

namespace Ddon.UploadFile
{
    public class UploadFileDbContext : BasicDbContext<UploadFileDbContext>
    {
        public DbSet<FileEntity>? Files { get; set; }

        public UploadFileDbContext(IServiceProvider serviceProvider, DbContextOptions<UploadFileDbContext> options) : base(serviceProvider, options)
        {
        }
    }
}

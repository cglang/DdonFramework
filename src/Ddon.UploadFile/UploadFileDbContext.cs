using Ddon.Core.Services.LazyService;
using Ddon.Repositiry;
using Microsoft.EntityFrameworkCore;

namespace Ddon.UploadFile
{
    public class UploadFileDbContext : BasicDbContext<UploadFileDbContext>
    {
        public DbSet<FileEntity>? Files { get; set; }

        public UploadFileDbContext(ILazyServiceProvider lazyServiceProvider, DbContextOptions<UploadFileDbContext> options) : base(lazyServiceProvider, options)
        {
        }
    }
}

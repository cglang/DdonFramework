using Ddon.Core.Services.LazyService;
using Ddon.FileStorage.DataBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Gardener.HostService
{
    /// <summary>
    /// 
    /// </summary>
    public class FileStorageHostService : BackgroundService
    {
        private readonly ILazyServiceProvider _lazyServiceProvider;

        /// <summary></summary>
        public FileStorageHostService(ILazyServiceProvider lazyServiceProvider)
        {
            _lazyServiceProvider = lazyServiceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _lazyServiceProvider.ServiceProvider.CreateScope();
            var databaseMigrate = scope.ServiceProvider.GetRequiredService<DatabaseMigrate>();
            await databaseMigrate.MigrateAsync();
        }
    }
}

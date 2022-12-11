using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.FileStorage.DataBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gardener.HostService
{
    /// <summary>
    /// 
    /// </summary>
    public class FileStorageHostService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary></summary>
        public FileStorageHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var databaseMigrate = scope.ServiceProvider.GetRequiredService<DatabaseMigrate>();
            await databaseMigrate.MigrateAsync();
        }
    }
}

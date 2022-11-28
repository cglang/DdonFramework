using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.Core.Services.LazyService;
using Ddon.Core.Use;
using Ddon.FileStorage.DataBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gardener.HostService
{
    /// <summary>
    /// 
    /// </summary>
    public class FileStorageHostService : IHostedService
    {
        private readonly DatabaseMigrate _databaseMigrate;

        /// <summary></summary>
        public FileStorageHostService(DatabaseMigrate databaseMigrate)
        {
            _databaseMigrate = databaseMigrate;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _databaseMigrate.MigrateAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        ///// <summary>
        ///// StopAsync
        ///// </summary>
        //public override async Task StopAsync(CancellationToken cancellationToken) => await Task.CompletedTask;

        ///// <summary>
        ///// ExecuteAsync
        ///// </summary>
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    var ddonTimer = new DdonTimer(null, new TimeSpan(0, 1, 0))
        //    {
        //        Elapsed = async () =>
        //        {
        //            if (DateTime.UtcNow.Hour == 10 && DateTime.UtcNow.Minute == 0)
        //            {
        //                using var scope = _lazyServiceProvider.ServiceProvider.CreateScope();
        //                var fundService = scope.ServiceProvider.GetRequiredService<IFundService>();
        //                await fundService.TodayAnalyze();
        //            }
        //        }
        //    };
        //    ddonTimer.Start();

        //    await Task.CompletedTask;
        //}
    }
}

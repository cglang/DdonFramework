using Ddon.Core;
using Ddon.FileStorage.DataBase;
using Ddon.FileStorage.Service;
using Gardener.HostService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Ddon.FileStorage
{
    public class FileStorageModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);

            services.AddDbContext<FileStorageDbContext>(options =>
            {
                options.UseSqlite($"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage", "FileStorage.db")}");
            });

            services.AddHostedService<FileStorageHostService>();

            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<DatabaseMigrate>();
        }
    }
}

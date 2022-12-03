﻿using Ddon.Core;
using Ddon.FileStorage.DataBase;
using Ddon.FileStorage.Service;
using Gardener.HostService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.FileStorage
{
    public class FileStorageModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Load<CoreModule>(services, configuration);

            services.AddDbContext<FileStorageDbContext>(options =>
            {
                options.UseSqlite($"Data Source={FileStorageConfig.DatabaseSource}");
            });

            services.AddHostedService<FileStorageHostService>();

            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<IFileStorageRepository, FileStorageRepository>();
            services.AddTransient<DatabaseMigrate>();
        }
    }
}
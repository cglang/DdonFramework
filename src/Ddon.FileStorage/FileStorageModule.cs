using System;
using System.IO;
using Ddon.AspNetCore;
using Ddon.Core;
using Ddon.FileStorage.DataBase;
using Ddon.FileStorage.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Ddon.FileStorage
{
    public class FileStorageModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage"));

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

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();

            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(root), RequestPath = "/FileStorage"
            });
        }
    }
}

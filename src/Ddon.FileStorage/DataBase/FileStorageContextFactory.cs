﻿using Ddon.Core.Services.LazyService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Ddon.FileStorage.DataBase
{
    public class FileStorageContextFactory : IDesignTimeDbContextFactory<FileStorageDbContext>
    {
        public FileStorageDbContext CreateDbContext(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.Build();
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider().GetService<IServiceProvider>()!;
            var optionsBuilder = new DbContextOptionsBuilder<FileStorageDbContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage", "FileStorage.db")}");

            return new FileStorageDbContext(serviceProvider, optionsBuilder.Options);
        }
    }
}

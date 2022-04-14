using Ddon.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Ddon.UploadFile
{
    public class UploadFileModule : Module
    {
        public override void Load(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UploadFileDbContext>(options =>
                options.UseSqlite($"Data Source={AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}UploadFile.db"));

            services.AddTransient<IUploadFileService, UploadFileService>();
        }
    }
}

using System;
using Ddon.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ddon.AspNetCore
{
    public static class ApplicationInitializationContextExtensions
    {
        public static WebApplication GetApplicationBuilder(this ApplicationInitializationContext _)
        {
            return ObjectAccessor.WebApplication ?? throw new Exception();
        }

        public static IWebHostEnvironment GetEnvironment(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        }

        public static IWebHostEnvironment? GetEnvironmentOrNull(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetService<IWebHostEnvironment>();
        }

        public static IConfiguration GetConfiguration(this ApplicationInitializationContext context)
        {
            return context.ServiceProvider.GetRequiredService<IConfiguration>();
        }
    }
}

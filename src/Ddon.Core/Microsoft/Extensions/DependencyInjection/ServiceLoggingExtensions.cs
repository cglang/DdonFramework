using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using ILogger = Serilog.ILogger;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Logging 配置
    /// </summary>
    public static class ServiceLoggingExtensions
    {
        public static void AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggerBuilder =>
            {
                var loggerProvider = ServiceDescriptor.Singleton<ILoggerProvider>(provider => new SerilogLoggerProvider(CreateSeriLogLogger(provider), true));
                loggerBuilder.Services.Add(loggerProvider);
            });
        }


        private static ILogger CreateSeriLogLogger(IServiceProvider provider)
        {
            var loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();

            loggerConfiguration.WriteTo.File(
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log.log"),
                            fileSizeLimitBytes: 1_000_000,
                            rollOnFileSizeLimit: true,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1),
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Properties:j} {Message:lj}{NewLine}{Exception}")
                        .MinimumLevel.Verbose();

            return loggerConfiguration.CreateLogger();
        }
    }
}

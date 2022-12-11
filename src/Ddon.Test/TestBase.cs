using System;
using Ddon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ddon.Test
{
    public class TestBase<TModule> where TModule : Module, new()
    {
        protected IServiceProvider ServiceProvider { get; }

        protected TestBase()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.Build();

            var services = new ServiceCollection();

            services.LoadModule<CoreModule>(configuration);
            services.LoadModule<TModule>(configuration);

            ServiceProvider = services.BuildServiceProvider().GetService<IServiceProvider>()!;
        }
    }
}

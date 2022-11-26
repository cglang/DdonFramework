﻿using Ddon.Core;
using Ddon.Core.Services.LazyService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ddon.Test
{
    public class TestBase<TModule> where TModule : Module, new()
    {
        protected ILazyServiceProvider ServiceProvider { get; }

        protected TestBase()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.Build();

            var services = new ServiceCollection();

            services.LoadModule<TModule>(configuration);

            ServiceProvider = services.BuildServiceProvider().GetService<ILazyServiceProvider>()!;
        }
    }
}
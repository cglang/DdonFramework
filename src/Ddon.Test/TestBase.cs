using Ddon.Core;
using Ddon.Core.Services.LazyService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Test.Repository
{
    public class TestBase<TModule> where TModule : Module, new()
    {
        protected ILazyServiceProvider ServiceProvider { get; set; }

        public TestBase()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            IConfiguration configuration = configurationBuilder.Build();

            var services = new ServiceCollection();

            services.LoadModule<CoreModule>(configuration);
            services.LoadModule<TModule>(configuration);           

            ServiceProvider = services.BuildServiceProvider().GetService<ILazyServiceProvider>()!;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Ddon.Core
{
    public abstract class Module
    {
        private static readonly HashSet<string> LoadedModule = new();

        public abstract void Load(IServiceCollection services, IConfiguration configuration);

        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        protected static void Load<TModule>(IServiceCollection services, IConfiguration configuration) where TModule : Module, new()
        {
            if (LoadedModule.Contains(typeof(TModule).FullName ?? typeof(TModule).Name))
            {
                return;
            }

            LoadedModule.Add(typeof(TModule).FullName ?? typeof(TModule).Name);

            new TModule().Load(services, configuration);
        }
    }
}
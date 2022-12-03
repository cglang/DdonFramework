using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Ddon.Core
{
    public class ModuleCore
    {
        protected readonly HashSet<string> LoadedModule = new();

        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        protected void Load<TModule>(IServiceCollection services, IConfiguration configuration) where TModule : Module, new()
        {
            if (LoadedModule.Contains(typeof(TModule).FullName ?? typeof(TModule).Name))
            {
                return;
            }

            LoadedModule.Add(typeof(TModule).FullName ?? typeof(TModule).Name);

            new TModule().Load(services, configuration);
        }

        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        protected void Load<TModule, TOption>(IServiceCollection services, IConfiguration configuration, Action<TOption> optionBuilder)
            where TModule : Module<TOption>, new()
        {
            if (LoadedModule.Contains(typeof(TModule).FullName ?? typeof(TModule).Name))
            {
                return;
            }

            LoadedModule.Add(typeof(TModule).FullName ?? typeof(TModule).Name);

            new TModule().Load(services, configuration, optionBuilder);
        }
    }

    public abstract class Module : ModuleCore
    {
        public abstract void Load(IServiceCollection services, IConfiguration configuration);
    }

    public abstract class Module<TOption> : ModuleCore
    {
        public abstract void Load(IServiceCollection services, IConfiguration configuration, Action<TOption> optionBuilder);
    }
}
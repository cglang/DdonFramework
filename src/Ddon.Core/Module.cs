using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Ddon.Core
{
    public abstract class Module
    {
        protected readonly HashSet<string> LoadedModule = new();

        public abstract void Load(IServiceCollection services, IConfiguration configuration);

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
    }

    public abstract class Module<TOption> : Module
    {
        public abstract void Load(IServiceCollection services, IConfiguration configuration, Action<TOption> optionBuilder);

        public override void Load(IServiceCollection services, IConfiguration configuration) { }

        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        protected void Load<TModule>(IServiceCollection services, IConfiguration configuration, Action<TOption> optionBuilder) where TModule : Module<TOption>, new()
        {
            if (LoadedModule.Contains(typeof(TModule).FullName ?? typeof(TModule).Name))
            {
                return;
            }

            LoadedModule.Add(typeof(TModule).FullName ?? typeof(TModule).Name);

            var module = new TModule();
            module.Load(services, configuration, optionBuilder);
            module.Load(services, configuration);
        }
    }
}
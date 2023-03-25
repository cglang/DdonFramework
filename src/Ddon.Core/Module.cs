using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Ddon.Core
{
    public class ModuleInfo : IDisposable
    {
        internal readonly HashSet<string> LoadedModule;

        internal readonly HashSet<ModuleCore> Modules;

        private ModuleInfo()
        {
            LoadedModule = new HashSet<string>();
            Modules = new HashSet<ModuleCore>();
        }
        
        public static readonly ModuleInfo Instance = new Lazy<ModuleInfo>(() => new ModuleInfo()).Value;

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing) { }

            LoadedModule.Clear();
            Modules.Clear();
            
            _disposed = true;
        }
    }

    public abstract class ModuleCore
    {
        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        protected static void Load<TModule>(IServiceCollection services, IConfiguration configuration)
            where TModule : Module, new()
        {
            var module = new TModule();
            if (CacheModule(module))
            {
                module.Load(services, configuration);
            }
        }

        /// <summary>
        /// 加载指定 Module
        /// </summary>
        /// <typeparam name="TModule">Module</typeparam>
        /// <typeparam name="TOption">Option</typeparam>
        /// <param name="services">IServiceCollection 对象</param>
        /// <param name="configuration">IConfiguration 对象</param>
        /// <param name="optionBuilder"></param>
        protected static void Load<TModule, TOption>(
            IServiceCollection services,
            IConfiguration configuration,
            Action<TOption> optionBuilder)
            where TModule : Module<TOption>, new()
        {
            var module = new TModule();
            if (CacheModule(module))
            {
                module.Load(services, configuration, optionBuilder);
            }
        }

        public virtual void HttpMiddleware(IApplicationBuilder app, IHostEnvironment env) { }

        public static bool CacheModule(ModuleCore module)
        {
            var moduleType = module.GetType();
            if (ModuleInfo.Instance.LoadedModule.Contains(moduleType.FullName ?? moduleType.Name)) return false;

            ModuleInfo.Instance.LoadedModule.Add(moduleType.FullName ?? moduleType.Name);
            ModuleInfo.Instance.Modules.Add(module);
            return true;
        }
    }

    public abstract class Module : ModuleCore
    {
        public abstract void Load(IServiceCollection services, IConfiguration configuration);
    }

    public abstract class Module<TOption> : ModuleCore
    {
        public abstract void Load(IServiceCollection services, IConfiguration configuration,
            Action<TOption> optionBuilder);
    }
}

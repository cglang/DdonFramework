using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Core
{
    public class ModuleInfo : IDisposable
    {
        internal readonly HashSet<string> LoadedModuleCache;

        public readonly HashSet<ModuleCore> Modules;

        private ModuleInfo()
        {
            LoadedModuleCache = new HashSet<string>();
            Modules = new HashSet<ModuleCore>();
        }

        public static readonly ModuleInfo Instance = new Lazy<ModuleInfo>(() => new ModuleInfo()).Value;

        public void ClearCache()
        {
            Instance.LoadedModuleCache.Clear();
        }

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

            LoadedModuleCache.Clear();
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
            Action<TOption>? optionBuilder = default)
            where TModule : Module<TOption>, new()
        {
            var module = new TModule();
            if (CacheModule(module))
            {
                module.Load(services, configuration, optionBuilder);
            }
        }

        public virtual void OnApplicationInitialization(ApplicationInitializationContext context) { }

        public static bool CacheModule(ModuleCore module)
        {
            var moduleType = module.GetType();
            if (ModuleInfo.Instance.LoadedModuleCache.Contains(moduleType.FullName ?? moduleType.Name)) return false;

            ModuleInfo.Instance.LoadedModuleCache.Add(moduleType.FullName ?? moduleType.Name);
            ModuleInfo.Instance.Modules.Add(module);
            return true;
        }
    }

    public abstract class Module : ModuleCore
    {
        public abstract void Load(
            IServiceCollection services,
            IConfiguration configuration);
    }

    public abstract class Module<TOption> : ModuleCore
    {
        public abstract void Load(
            IServiceCollection services,
            IConfiguration configuration,
            Action<TOption>? optionAction);
    }

    public class ApplicationInitializationContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public ApplicationInitializationContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}

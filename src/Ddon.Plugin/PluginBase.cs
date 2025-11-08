using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Plugin
{
    public abstract class PluginBase : IPlugin
    {
        public Guid Id { get; }

        public string Name { get; }

        public string Description { get; }

        public abstract bool StartOnLoad { get; }

        public abstract TaskCreationOptions TaskCreationOptions { get; }

        protected PluginBase(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        protected PluginBase(string id, string name, string description)
        {
            Id = Guid.Parse(id);
            Name = name;
            Description = description;
        }

        public abstract Task BeforeExecuteAsync(CancellationToken cancellationToken = default);

        public abstract Task ExecuteAsync(CancellationToken cancellationToken = default);

        public abstract Task AfterExecuteAsync(CancellationToken cancellationToken = default);

        public abstract void Init(IServiceCollection services, IServiceProvider serviceProvider);
    }
}

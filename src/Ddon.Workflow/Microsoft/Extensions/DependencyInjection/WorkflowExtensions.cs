using Ddon.Workflow;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Persistence;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WorkflowExtensions
    {
        /// <summary>
        /// 添加工作流服务（核心，不含持久化）
        /// </summary>
        public static void AddWorkflow(this IServiceCollection services)
        {
            services.AddTransient<IWorkflowScheduler, WorkflowScheduler>();
            services.AddTransient<WorkflowBuilder>();
        }

        /// <summary>
        /// 添加带文件系统持久化的工作流服务（核心 + 持久化）
        /// </summary>
        public static void AddWorkflowWithPersistence(
            this IServiceCollection services,
            string storagePath = null,
            ServiceLifetime persistenceLifetime = ServiceLifetime.Singleton)
        {
            var descriptor = new ServiceDescriptor(
                typeof(IWorkflowPersistenceStrategy),
                provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<FileSystemPersistenceStrategy>>();
                    return new FileSystemPersistenceStrategy(storagePath, logger);
                },
                persistenceLifetime);

            services.Add(descriptor);
            services.AddSingleton<IWorkflowRecoveryService, WorkflowRecoveryService>();
            services.AddWorkflow();
        }
    }
}

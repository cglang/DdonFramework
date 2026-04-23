using System;
using Ddon.Workflow;
using Ddon.Workflow.Abstractions;
using Ddon.Workflow.Abstractions.Persistence;
using Ddon.Workflow.Persistence;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WorkflowExtensions
    {
        /// <summary>
        /// 添加基本的工作流服务（无持久化）
        /// </summary>
        public static void AddWorkflow(this IServiceCollection services)
        {
            services.AddTransient<IWorkflowScheduler, WorkflowScheduler>();
        }

        /// <summary>
        /// 添加带文件系统持久化功能的工作流服务
        /// </summary>
        public static void AddWorkflowWithPersistence(
            this IServiceCollection services,
            string storagePath = null,
            ServiceLifetime persistenceLifetime = ServiceLifetime.Singleton)
        {
            // 注册持久化策略
            var descriptor = new ServiceDescriptor(
                typeof(IWorkflowPersistenceStrategy),
                provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<FileSystemPersistenceStrategy>>();
                    return new FileSystemPersistenceStrategy(storagePath, logger);
                },
                persistenceLifetime);

            services.Add(descriptor);

            // 注册恢复服务
            services.AddSingleton<IWorkflowRecoveryService, WorkflowRecoveryService>();

            // 注册带持久化支持的调度器
            services.AddTransient<IWorkflowScheduler>(provider =>
            {
                var logger = provider.GetRequiredService<
                    ILogger<IWorkflowScheduler>>();
                var recoveryService = provider.GetService<IWorkflowRecoveryService>();
                var persistenceStrategy = provider.GetService<IWorkflowPersistenceStrategy>();

                return new WorkflowScheduler(logger, recoveryService, persistenceStrategy);
            });
        }

        /// <summary>
        /// 添加带自定义持久化策略的工作流服务
        /// </summary>
        public static void AddWorkflowWithCustomPersistence(
            this IServiceCollection services,
            IWorkflowPersistenceStrategy persistenceStrategy,
            ServiceLifetime schedulerLifetime = ServiceLifetime.Singleton)
        {
            services.Add(new ServiceDescriptor(
                typeof(IWorkflowPersistenceStrategy),
                _ => persistenceStrategy,
                ServiceLifetime.Singleton));

            services.AddSingleton<IWorkflowRecoveryService, WorkflowRecoveryService>();

            services.Add(new ServiceDescriptor(
                typeof(IWorkflowScheduler),
                provider =>
                {
                    var logger = provider.GetRequiredService<
                        ILogger<IWorkflowScheduler>>();
                    var recoveryService = provider.GetService<IWorkflowRecoveryService>();

                    return new WorkflowScheduler(logger, recoveryService, persistenceStrategy);
                },
                schedulerLifetime));
        }

        public static void AddWorkflowCommon(this IServiceCollection services)
        {
            services.AddTransient<WorkflowBuilder>();
        }
    }
}

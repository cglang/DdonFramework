using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.Plugin
{
    public interface IPlugin
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// 插件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 是否在加载时自动启动
        /// </summary>
        bool StartOnLoad { get; }

        /// <summary>
        /// 任务启动运行方式
        /// </summary>
        TaskCreationOptions TaskCreationOptions { get; }

        /// <summary>
        /// 插件逻辑执行前
        /// </summary>
        Task BeforeExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行插件逻辑
        /// </summary>
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 插件逻辑执行后
        /// </summary>
        Task AfterExecuteAsync(CancellationToken cancellationToken = default);
    }
}

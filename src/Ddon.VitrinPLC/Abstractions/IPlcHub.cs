using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddon.VitrinPLC.Abstractions
{
    /// <summary>
    /// 多 PLC 访问入口，通过名称索引对应的 <see cref="IPlcSession"/>。
    /// </summary>
    public interface IPlcHub
    {
        /// <summary>获取指定 PLC 的访问会话。</summary>
        IPlcSession For(string plcName);

        /// <summary>所有已注册的 PLC 名称。</summary>
        IEnumerable<string> Names { get; }

        /// <summary>运行时动态添加 PLC。</summary>
        Task AddPlcAsync(string name, IPlcClient client, Action<PlcHostOptions> configure, CancellationToken ct = default);

        /// <summary>运行时动态移除 PLC，会先停止同步引擎。</summary>
        Task RemovePlcAsync(string name, CancellationToken ct = default);
    }
}

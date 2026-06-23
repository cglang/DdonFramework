using System.Collections.Generic;

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
    }
}

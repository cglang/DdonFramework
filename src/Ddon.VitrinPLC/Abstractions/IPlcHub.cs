using System.Collections.Generic;

namespace Ddon.VitrinPLC.Abstractions
{
    /// <summary>
    /// 多 PLC 访问入口，通过名称索引对应的 <see cref="ITagService"/>。
    /// </summary>
    public interface IPlcHub
    {
        /// <summary>获取指定 PLC 的 Tag 服务。</summary>
        ITagService For(string plcName);

        /// <summary>所有已注册的 PLC 名称。</summary>
        IEnumerable<string> Names { get; }
    }
}

using System.Collections.Generic;
using Ddon.VitrinPLC.Models;

namespace Ddon.VitrinPLC.Abstractions
{
    // ─────────────────────────────────────────────
    // Tag 注册表
    // ─────────────────────────────────────────────
    public interface ITagRegistry
    {
        void Register(TagDefinition tag);
        TagDefinition Resolve(string tagName);
        IReadOnlyList<TagDefinition> GetAll();
    }
}

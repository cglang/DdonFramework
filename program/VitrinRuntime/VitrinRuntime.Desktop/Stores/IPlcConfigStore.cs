using VitrinRuntime.Desktop.Services;

namespace VitrinRuntime.Desktop.Stores;

public interface IPlcConfigStore
{
    // ── PLC ──────────────────────────────────────────

    PlcConfig? GetPlc(string name);

    List<PlcConfig> GetAllPlcs();

    void AddPlc(PlcConfig config);

    PlcConfig? RemovePlc(string name);

    void UpdatePlcConnection(string name, bool connected, string? error = null);

    /// <summary>更新 PLC 配置（支持改名，oldName 用于查找原记录）</summary>
    void UpdatePlc(string oldName, PlcConfig config);

    List<TagConfig> GetAllTagsForPlc(string plcName);

    // ── DB Group ─────────────────────────────────────

    DbGroup? GetGroup(string groupId);

    List<DbGroup> GetGroupsByPlc(string plcName);

    void AddGroup(DbGroup group);

    DbGroup? RemoveGroup(string groupId);

    bool RenameGroup(string groupId, string newName);

    // ── Tag ──────────────────────────────────────────

    TagConfig? GetTag(string tagId);

    List<TagConfig> GetTagsByGroup(string groupId);

    List<TagConfig> GetTagsByPlc(string plcName);

    void AddTag(TagConfig tag);

    TagConfig? RemoveTag(string tagId);

    void UpdateTag(TagConfig tag);
}

using System.Collections.Concurrent;

namespace VitrinRuntime.Services;

/// <summary>PLC 配置、DB分组、点位定义的内存存储</summary>
public sealed class MemoryPlcConfigStore : IPlcConfigStore
{
    private readonly ConcurrentDictionary<string, PlcConfig> _plcs = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DbGroup> _groups = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, TagConfig> _tags = new(StringComparer.Ordinal);

    public PlcConfig? GetPlc(string name) =>
        _plcs.TryGetValue(name, out var cfg) ? cfg : null;

    public List<PlcConfig> GetAllPlcs() => _plcs.Values.OrderBy(p => p.CreatedAt).ToList();

    public void AddPlc(PlcConfig config)
    {
        if (!_plcs.TryAdd(config.Name, config))
            throw new InvalidOperationException($"PLC '{config.Name}' 已存在。");
    }

    public PlcConfig? RemovePlc(string name)
    {
        _plcs.TryRemove(name, out var cfg);
        var groupKeys = _groups.Values
            .Where(g => g.PlcName.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(g => g.Id).ToList();
        foreach (var gid in groupKeys)
        {
            _groups.TryRemove(gid, out _);
            var tagKeys = _tags.Values.Where(t => t.GroupId == gid).Select(t => t.Id).ToList();
            foreach (var tid in tagKeys) _tags.TryRemove(tid, out _);
        }
        return cfg;
    }

    public void UpdatePlcConnection(string name, bool connected, string? error = null)
    {
        if (_plcs.TryGetValue(name, out var cfg))
        {
            cfg.IsConnected = connected;
            cfg.ErrorMessage = error;
            if (connected) cfg.LastConnectedAt = DateTime.UtcNow;
        }
    }

    public List<TagConfig> GetAllTagsForPlc(string plcName)
    {
        var groupIds = _groups.Values
            .Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
            .Select(g => g.Id).ToHashSet();
        return _tags.Values.Where(t => groupIds.Contains(t.GroupId)).ToList();
    }

    public DbGroup? GetGroup(string groupId) =>
        _groups.TryGetValue(groupId, out var g) ? g : null;

    public List<DbGroup> GetGroupsByPlc(string plcName) =>
        _groups.Values.Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(g => g.CreatedAt).ToList();

    public void AddGroup(DbGroup group) => _groups.TryAdd(group.Id, group);

    public DbGroup? RemoveGroup(string groupId)
    {
        _groups.TryRemove(groupId, out var g);
        var tagKeys = _tags.Values.Where(t => t.GroupId == groupId).Select(t => t.Id).ToList();
        foreach (var tid in tagKeys) _tags.TryRemove(tid, out _);
        return g;
    }

    public bool RenameGroup(string groupId, string newName)
    {
        if (_groups.TryGetValue(groupId, out var g)) { g.Name = newName; return true; }
        return false;
    }

    public TagConfig? GetTag(string tagId) =>
        _tags.TryGetValue(tagId, out var t) ? t : null;

    public List<TagConfig> GetTagsByGroup(string groupId) =>
        _tags.Values.Where(t => t.GroupId == groupId).OrderBy(t => t.CreatedAt).ToList();

    public List<TagConfig> GetTagsByPlc(string plcName)
    {
        var groupIds = _groups.Values
            .Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
            .Select(g => g.Id).ToHashSet();
        return _tags.Values.Where(t => groupIds.Contains(t.GroupId)).OrderBy(t => t.CreatedAt).ToList();
    }

    public void AddTag(TagConfig tag) => _tags.TryAdd(tag.Id, tag);

    public TagConfig? RemoveTag(string tagId)
    {
        _tags.TryRemove(tagId, out var t);
        return t;
    }

    public void UpdateTag(TagConfig tag)
    {
        _tags[tag.Id] = tag;
    }
}

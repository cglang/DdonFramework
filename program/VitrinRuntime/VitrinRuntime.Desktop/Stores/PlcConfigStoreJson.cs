using System.Text.Encodings.Web;
using System.Text.Json;

namespace VitrinRuntime.Services;

/// <summary>PLC 配置的 JSON 文件持久化存储，后续可替换为数据库实现</summary>
public sealed class JsonPlcConfigStore : IPlcConfigStore
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private List<PlcConfig> _plcs = [];
    private List<DbGroup> _groups = [];
    private List<TagConfig> _tags = [];

    public JsonPlcConfigStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "plc-config.json");
        LoadFromFile();
    }

    // ── 内部持久化 ────────────────────────────────────

    private void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath)) return;

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<PlcStoreData>(json);
            if (data is null) return;

            lock (_lock)
            {
                _plcs = data.Plcs;
                _groups = data.Groups;
                _tags = data.Tags;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载 PLC 配置文件失败: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            PlcStoreData data;
            lock (_lock)
            {
                data = new PlcStoreData
                {
                    Plcs = new List<PlcConfig>(_plcs),
                    Groups = new List<DbGroup>(_groups),
                    Tags = new List<TagConfig>(_tags)
                };
            }

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存 PLC 配置文件失败: {ex.Message}");
        }
    }

    // ── PLC ──────────────────────────────────────────

    public PlcConfig? GetPlc(string name)
    {
        lock (_lock)
        {
            return _plcs.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public List<PlcConfig> GetAllPlcs()
    {
        lock (_lock)
        {
            return [.. _plcs.OrderBy(p => p.CreatedAt)];
        }
    }

    public void AddPlc(PlcConfig config)
    {
        lock (_lock)
        {
            if (_plcs.Any(p => p.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"PLC '{config.Name}' 已存在。");
            _plcs.Add(config);
        }
        SaveToFile();
    }

    public PlcConfig? RemovePlc(string name)
    {
        PlcConfig? removed;
        lock (_lock)
        {
            var idx = _plcs.FindIndex(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return null;

            removed = _plcs[idx];
            _plcs.RemoveAt(idx);

            // 同时移除该 PLC 下的所有分组和点位
            var groupIds = _groups
                .Where(g => g.PlcName.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(g => g.Id)
                .ToHashSet();
            _groups.RemoveAll(g => groupIds.Contains(g.Id));
            _tags.RemoveAll(t => groupIds.Contains(t.GroupId));
        }
        SaveToFile();
        return removed;
    }

    public void UpdatePlcConnection(string name, bool connected, string? error = null)
    {
        lock (_lock)
        {
            var plc = _plcs.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (plc is null) return;

            plc.IsConnected = connected;
            plc.ErrorMessage = error;
            if (connected) plc.LastConnectedAt = DateTime.UtcNow;
        }
        SaveToFile();
    }

    public List<TagConfig> GetAllTagsForPlc(string plcName)
    {
        lock (_lock)
        {
            var groupIds = _groups
                .Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
                .Select(g => g.Id)
                .ToHashSet();
            return _tags.Where(t => groupIds.Contains(t.GroupId)).ToList();
        }
    }

    // ── DB Group ─────────────────────────────────────

    public DbGroup? GetGroup(string groupId)
    {
        lock (_lock)
        {
            return _groups.FirstOrDefault(g => g.Id == groupId);
        }
    }

    public List<DbGroup> GetGroupsByPlc(string plcName)
    {
        lock (_lock)
        {
            return [.. _groups
                .Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(g => g.CreatedAt)];
        }
    }

    public void AddGroup(DbGroup group)
    {
        lock (_lock)
        {
            _groups.Add(group);
        }
        SaveToFile();
    }

    public DbGroup? RemoveGroup(string groupId)
    {
        DbGroup? removed;
        lock (_lock)
        {
            var idx = _groups.FindIndex(g => g.Id == groupId);
            if (idx < 0) return null;

            removed = _groups[idx];
            _groups.RemoveAt(idx);

            // 同时移除分组下的所有点位
            _tags.RemoveAll(t => t.GroupId == groupId);
        }
        SaveToFile();
        return removed;
    }

    public bool RenameGroup(string groupId, string newName)
    {
        lock (_lock)
        {
            var group = _groups.FirstOrDefault(g => g.Id == groupId);
            if (group is null) return false;
            group.Name = newName;
        }
        SaveToFile();
        return true;
    }

    // ── Tag ──────────────────────────────────────────

    public TagConfig? GetTag(string tagId)
    {
        lock (_lock)
        {
            return _tags.FirstOrDefault(t => t.Id == tagId);
        }
    }

    public List<TagConfig> GetTagsByGroup(string groupId)
    {
        lock (_lock)
        {
            return [.. _tags.Where(t => t.GroupId == groupId).OrderBy(t => t.CreatedAt)];
        }
    }

    public List<TagConfig> GetTagsByPlc(string plcName)
    {
        lock (_lock)
        {
            var groupIds = _groups
                .Where(g => g.PlcName.Equals(plcName, StringComparison.OrdinalIgnoreCase))
                .Select(g => g.Id)
                .ToHashSet();
            return [.. _tags.Where(t => groupIds.Contains(t.GroupId)).OrderBy(t => t.CreatedAt)];
        }
    }

    public void AddTag(TagConfig tag)
    {
        lock (_lock)
        {
            _tags.Add(tag);
        }
        SaveToFile();
    }

    public TagConfig? RemoveTag(string tagId)
    {
        TagConfig? removed;
        lock (_lock)
        {
            var idx = _tags.FindIndex(t => t.Id == tagId);
            if (idx < 0) return null;
            removed = _tags[idx];
            _tags.RemoveAt(idx);
        }
        SaveToFile();
        return removed;
    }

    public void UpdateTag(TagConfig tag)
    {
        lock (_lock)
        {
            var idx = _tags.FindIndex(t => t.Id == tag.Id);
            if (idx >= 0)
                _tags[idx] = tag;
        }
        SaveToFile();
    }
}

/// <summary>JSON 序列化用的数据容器</summary>
internal sealed class PlcStoreData
{
    public List<PlcConfig> Plcs { get; set; } = [];
    public List<DbGroup> Groups { get; set; } = [];
    public List<TagConfig> Tags { get; set; } = [];
}

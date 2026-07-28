using System.Text.Encodings.Web;
using System.Text.Json;
using VitrinRuntime.Desktop.Services;

namespace VitrinRuntime.Desktop.Stores;

public sealed class JsonLuaGroupStore : ILuaGroupStore
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private List<LuaGroupConfig> _groups;

    public JsonLuaGroupStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "lua-groups.json");
        _groups = [];
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath)) return;
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<List<LuaGroupConfig>>(json);
            if (data is null) return;
            lock (_lock) { _groups = data; }
        }
        catch { }
    }

    private void SaveToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            List<LuaGroupConfig> snapshot;
            lock (_lock) { snapshot = [.. _groups]; }

            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(_filePath, json);
        }
        catch { }
    }

    public List<LuaGroupConfig> GetAll()
    {
        lock (_lock) { return [.. _groups]; }
    }

    public void Add(LuaGroupConfig config)
    {
        lock (_lock) { _groups.Add(config); }
        SaveToFile();
    }

    public bool Remove(string groupName)
    {
        bool removed;
        lock (_lock)
        {
            var idx = _groups.FindIndex(g =>
                g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            _groups.RemoveAt(idx);
            removed = true;
        }
        SaveToFile();
        return removed;
    }

    public bool Contains(string groupName)
    {
        lock (_lock)
        {
            return _groups.Any(g =>
                g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }
    }
}

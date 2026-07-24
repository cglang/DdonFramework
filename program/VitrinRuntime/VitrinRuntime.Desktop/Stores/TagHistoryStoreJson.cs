using System.Text.Encodings.Web;
using System.Text.Json;
using VitrinRuntime.Services;

namespace VitrinRuntime.Desktop.Stores;

/// <summary>点位历史记录的 JSON 文件持久化存储，每个点位最多保留100条记录</summary>
public sealed class JsonTagHistoryStore : ITagHistoryStore
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private readonly Dictionary<string, List<TagHistoryRecord>> _records = new();

    private const int MaxRecordsPerTag = 100;

    public JsonTagHistoryStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "tag-history.json");
        LoadFromFile();
    }

    private void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_filePath)) return;

            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<TagHistoryStoreData>(json);
            if (data?.Records is null) return;

            lock (_lock)
            {
                _records.Clear();
                foreach (var kv in data.Records)
                {
                    _records[kv.Key] = kv.Value;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载点位历史记录失败: {ex.Message}");
        }
    }

    private void SaveToFile()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            TagHistoryStoreData data;
            lock (_lock)
            {
                data = new TagHistoryStoreData
                {
                    Records = new Dictionary<string, List<TagHistoryRecord>>(_records)
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
            System.Diagnostics.Debug.WriteLine($"保存点位历史记录失败: {ex.Message}");
        }
    }

    public void AddRecord(TagHistoryRecord record)
    {
        lock (_lock)
        {
            if (!_records.TryGetValue(record.TagName, out var list))
            {
                list = new List<TagHistoryRecord>();
                _records[record.TagName] = list;
            }

            list.Add(record);

            // 超过最大数量则删除最早的记录
            if (list.Count > MaxRecordsPerTag)
            {
                list.RemoveRange(0, list.Count - MaxRecordsPerTag);
            }
        }
        SaveToFile();
    }

    public List<TagHistoryRecord> GetRecords(string tagName, int limit = 100)
    {
        lock (_lock)
        {
            if (_records.TryGetValue(tagName, out var list))
            {
                // 按时间倒序，取最新的 limit 条
                return [.. list.OrderByDescending(r => r.Timestamp).Take(limit)];
            }
            return [];
        }
    }
}

/// <summary>JSON 序列化用的数据容器</summary>
internal sealed class TagHistoryStoreData
{
    public Dictionary<string, List<TagHistoryRecord>> Records { get; set; } = new();
}

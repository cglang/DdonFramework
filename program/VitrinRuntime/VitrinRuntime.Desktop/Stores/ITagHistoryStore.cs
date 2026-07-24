using VitrinRuntime.Desktop.Services;

namespace VitrinRuntime.Desktop.Stores;

/// <summary>点位历史记录存储接口</summary>
public interface ITagHistoryStore
{
    /// <summary>添加一条历史记录</summary>
    void AddRecord(TagHistoryRecord record);

    /// <summary>获取指定点位的历史记录（按时间倒序）</summary>
    List<TagHistoryRecord> GetRecords(string tagName, int limit = 100);
}

using System.Text.Json;
using Ddon.Desktop.Core.Annotations;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;
using VitrinRuntime.Desktop.Exceptions;
using VitrinRuntime.Desktop.Stores;

namespace VitrinRuntime.Desktop.Services;

[BridgeService(Name = "PlcData")]
public sealed class PlcDataService
{
    private readonly IPlcHub _hub;
    private readonly IPlcConfigStore _store;
    private readonly TagSubscriptionManager _subscriptionManager;
    private readonly ITagHistoryStore _historyStore;
    private readonly ILogger<PlcDataService> _logger;

    public PlcDataService(IPlcHub hub, IPlcConfigStore store, TagSubscriptionManager subscriptionManager,
        ITagHistoryStore historyStore, ILogger<PlcDataService> logger)
    {
        _hub = hub;
        _store = store;
        _subscriptionManager = subscriptionManager;
        _historyStore = historyStore;
        _logger = logger;
    }

    // ── DB Groups ────────────────────────────────────

    [BridgeMethod(Name = "ListDbGroups")]
    public List<object> ListDbGroups(ListDbGroupsRequest req)
    {
        var groups = _store.GetGroupsByPlc(req.PlcName);
        return groups.Select(g => new
        {
            id = g.Id,
            plcName = g.PlcName,
            name = g.Name,
            tagCount = _store.GetTagsByGroup(g.Id).Count,
            createdAt = g.CreatedAt
        } as object).ToList();
    }

    [BridgeMethod(Name = "CreateDbGroup")]
    public object CreateDbGroup(CreateDbGroupRequest req)
    {
        var existing = _store.GetGroupsByPlc(req.PlcName);
        if (existing.Any(g => g.Name.Equals(req.GroupName, StringComparison.OrdinalIgnoreCase)))
            throw new UserFriendlyException($"该 PLC 下已存在同名分组 '{req.GroupName}'。");

        var group = new DbGroup
        {
            PlcName = req.PlcName,
            Name = req.GroupName
        };
        _store.AddGroup(group);
        return new { id = group.Id, name = group.Name };
    }

    [BridgeMethod(Name = "DeleteDbGroup")]
    public bool DeleteDbGroup(GroupIdRequest req) => _store.RemoveGroup(req.GroupId) is not null;

    [BridgeMethod(Name = "RenameDbGroup")]
    public bool RenameDbGroup(RenameDbGroupRequest req)
    {
        var group = _store.GetGroup(req.GroupId)
                    ?? throw new UserFriendlyException($"分组 '{req.GroupId}' 未找到。");

        var existing = _store.GetGroupsByPlc(group.PlcName);
        if (existing.Any(g => g.Id != req.GroupId && g.Name.Equals(req.NewName, StringComparison.OrdinalIgnoreCase)))
            throw new UserFriendlyException($"该 PLC 下已存在同名分组 '{req.NewName}'。");

        return _store.RenameGroup(req.GroupId, req.NewName);
    }

    // ── Tags ─────────────────────────────────────────

    [BridgeMethod(Name = "ListTags")]
    public List<object> ListTags(ListTagsRequest req)
    {
        var tags = _store.GetTagsByGroup(req.GroupId);
        var group = _store.GetGroup(req.GroupId);
        var results = new List<object>();

        if (group is null) return results;

        var plc = _store.GetPlc(group.PlcName);
        var session = plc is not null && plc.IsConnected ? _hub.For(group.PlcName) : null;

        foreach (var tag in tags)
        {
            object? value = null;
            if (session is not null)
            {
                try
                {
                    value = ReadTagValue(session, group, tag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "读取Tag：{TagName} 发生错误！", tag.Name);
                }
            }

            results.Add(new
            {
                id = tag.Id,
                groupId = tag.GroupId,
                name = tag.Name,
                address = tag.Address,
                dataType = tag.DataType.ToString(),
                value,
                createdAt = tag.CreatedAt
            });
        }

        return results;
    }

    [BridgeMethod(Name = "AddTag")]
    public async Task<object> AddTag(AddTagRequest req)
    {
        await Task.CompletedTask;

        if (!Enum.TryParse<PlcDataType>(req.DataType, true, out var plcType))
            throw new UserFriendlyException($"不支持的数据类型: {req.DataType}");

        var group = _store.GetGroup(req.GroupId)
                    ?? throw new UserFriendlyException($"分组 '{req.GroupId}' 未找到。");

        var existingTags = _store.GetTagsByGroup(req.GroupId);
        if (existingTags.Any(t => t.Name.Equals(req.TagName, StringComparison.OrdinalIgnoreCase)))
            throw new UserFriendlyException($"分组中已存在同名点位 '{req.TagName}'。");

        var tag = new TagConfig
        {
            GroupId = req.GroupId,
            Name = req.TagName,
            Address = req.Address,
            DataType = plcType,
            StringLength = req.StringLength
        };
        _store.AddTag(tag);

        try
        {
            var session = _hub.For(group.PlcName);
            var fullName = GetFullTagName(group, tag.Name);
            var tagDefinition = new TagDefinition(fullName, tag.Address, tag.DataType, tag.StringLength);
            session.AddTag(tagDefinition);
            _subscriptionManager.SubscribeTag(group.PlcName, tagDefinition);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "点位 '{TagName}' 已存储但未能注册到 PLC 会话。", req.TagName);
        }

        return new
        {
            id = tag.Id,
            name = tag.Name,
            address = tag.Address,
            dataType = tag.DataType.ToString()
        };
    }

    [BridgeMethod(Name = "RemoveTag")]
    public bool RemoveTag(TagIdRequest req)
    {
        var tag = _store.GetTag(req.TagId);
        if (tag is null) return false;

        var group = _store.GetGroup(tag.GroupId);
        if (group is not null)
        {
            try
            {
                var session = _hub.For(group.PlcName);
                var fullName = GetFullTagName(group, tag.Name);
                _subscriptionManager.UnsubscribeTag(group.PlcName, fullName);
                session.RemoveTag(fullName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除Tag：{TagName}发生错误！", tag.Name);
            }
        }

        _store.RemoveTag(req.TagId);
        return true;
    }

    [BridgeMethod(Name = "UpdateTag")]
    public async Task<object> UpdateTag(UpdateTagRequest req)
    {
        await Task.CompletedTask;

        var oldTag = _store.GetTag(req.TagId)
                     ?? throw new UserFriendlyException($"点位 '{req.TagId}' 未找到。");

        if (!Enum.TryParse<PlcDataType>(req.DataType, true, out var plcType))
            throw new UserFriendlyException($"不支持的数据类型: {req.DataType}");

        // 如果名称变更，检查同一分组内是否存在同名点位（排除自身）
        if (!string.Equals(oldTag.Name, req.TagName, StringComparison.OrdinalIgnoreCase))
        {
            var existingTags = _store.GetTagsByGroup(oldTag.GroupId);
            if (existingTags.Any(t => t.Name.Equals(req.TagName, StringComparison.OrdinalIgnoreCase)))
                throw new UserFriendlyException($"分组中已存在同名点位 '{req.TagName}'。");
        }

        // 更新存储
        var updatedTag = new TagConfig
        {
            Id = oldTag.Id,
            GroupId = oldTag.GroupId,
            Name = req.TagName,
            Address = req.Address,
            DataType = plcType,
            StringLength = req.StringLength,
            CreatedAt = oldTag.CreatedAt
        };
        _store.UpdateTag(updatedTag);

        // 同步更新 PLC 会话中的点位定义
        var group = _store.GetGroup(oldTag.GroupId);
        if (group is not null)
        {
            try
            {
                var session = _hub.For(group.PlcName);

                var oldFullName = GetFullTagName(group, oldTag.Name);
                _subscriptionManager.UnsubscribeTag(group.PlcName, oldFullName);
                session.RemoveTag(oldFullName);

                var newFullName = GetFullTagName(group, req.TagName);
                var tagDefinition = new TagDefinition(newFullName, req.Address, plcType, req.StringLength);
                session.AddTag(tagDefinition);

                _subscriptionManager.SubscribeTag(group.PlcName, tagDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "点位 '{TagName}' 已更新但未能同步到 PLC 会话。", req.TagName);
            }
        }

        return new
        {
            id = updatedTag.Id,
            name = updatedTag.Name,
            address = updatedTag.Address,
            dataType = updatedTag.DataType.ToString()
        };
    }

    [BridgeMethod(Name = "ReadTag")]
    public object? ReadTag(TagIdRequest req)
    {
        var tag = _store.GetTag(req.TagId)
                  ?? throw new UserFriendlyException($"点位 '{req.TagId}' 未找到。");

        var group = _store.GetGroup(tag.GroupId);
        if (group is null) return null;

        var session = _hub.For(group.PlcName);
        return ReadTagValue(session, group, tag);
    }

    [BridgeMethod(Name = "WriteTag")]
    public async Task<object> WriteTag(WriteTagRequest req)
    {
        var tag = _store.GetTag(req.TagId)
                  ?? throw new UserFriendlyException($"点位 '{req.TagId}' 未找到。");

        var group = _store.GetGroup(tag.GroupId)
                    ?? throw new UserFriendlyException($"点位所属分组未找到。");

        var session = _hub.For(group.PlcName);
        var fullName = GetFullTagName(group, tag.Name);

        var typedValue = ConvertFromPayload(req.Value!, tag.DataType);

        try
        {
            var result = await session.SetAsync(fullName, typedValue);
            return new
            {
                success = result.Success,
                error = result.ErrorMessage,
                needConfirmByScan = result.NeedConfirmByScan
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "写入点位 '{TagName}' 失败。", tag.Name);
            return new { success = false, error = ex.Message };
        }
    }

    [BridgeMethod(Name = "GetTagHistory")]
    public List<object> GetTagHistory(TagHistoryRequest req)
    {
        var group = _store.GetGroup(req.GroupId);
        var fullName = group is not null
            ? GetFullTagName(group, req.TagName)
            : req.TagName;
        var records = _historyStore.GetRecords(fullName);
        return records.Select(r => new
        {
            id = r.Id,
            tagName = r.TagName,
            address = r.Address,
            dataType = r.DataType,
            oldValue = r.OldValue,
            newValue = r.NewValue,
            timestamp = r.Timestamp
        } as object).ToList();
    }

    // ── Helper Methods ───────────────────────────────

    private static string GetFullTagName(DbGroup group, string tagName) => $"{group.PlcName}.{group.Name}.{tagName}";

    private static object ReadTagValue(IPlcSession session, DbGroup group, TagConfig tag)
    {
        var fullName = GetFullTagName(group, tag.Name);
        return tag.DataType switch
        {
            PlcDataType.Bool => session.Get<bool>(fullName),
            PlcDataType.Byte => session.Get<byte>(fullName),
            PlcDataType.Int16 => session.Get<short>(fullName),
            PlcDataType.UInt16 => session.Get<ushort>(fullName),
            PlcDataType.Int32 => session.Get<int>(fullName),
            PlcDataType.UInt32 => session.Get<uint>(fullName),
            PlcDataType.Float => session.Get<float>(fullName),
            PlcDataType.Double => session.Get<double>(fullName),
            PlcDataType.String => session.Get<string>(fullName),
            _ => throw new NotSupportedException($"不支持的数据类型: {tag.DataType}")
        };
    }

    private static object ConvertFromPayload(object value, PlcDataType targetType)
    {
        if (value is JsonElement je)
        {
            return targetType switch
            {
                PlcDataType.Bool => je.GetBoolean(),
                PlcDataType.Byte => je.GetByte(),
                PlcDataType.Int16 => je.GetInt16(),
                PlcDataType.UInt16 => je.GetUInt16(),
                PlcDataType.Int32 => je.GetInt32(),
                PlcDataType.UInt32 => je.GetUInt32(),
                PlcDataType.Float => je.GetSingle(),
                PlcDataType.Double => je.GetDouble(),
                PlcDataType.String => je.GetString() ?? string.Empty,
                _ => throw new NotSupportedException($"不支持的数据类型: {targetType}")
            };
        }

        // 如果 value 已经是 CLR 类型，直接转换
        return targetType switch
        {
            PlcDataType.Bool => Convert.ToBoolean(value),
            PlcDataType.Byte => Convert.ToByte(value),
            PlcDataType.Int16 => Convert.ToInt16(value),
            PlcDataType.UInt16 => Convert.ToUInt16(value),
            PlcDataType.Int32 => Convert.ToInt32(value),
            PlcDataType.UInt32 => Convert.ToUInt32(value),
            PlcDataType.Float => Convert.ToSingle(value),
            PlcDataType.Double => Convert.ToDouble(value),
            PlcDataType.String => Convert.ToString(value) ?? string.Empty,
            _ => throw new NotSupportedException($"不支持的数据类型: {targetType}")
        };
    }
}
using System.Text.Json;
using Ddon.Desktop.Core.Annotations;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Services;

[BridgeService(Name = "PlcData")]
public sealed class PlcDataService
{
    private readonly IPlcHub _hub;
    private readonly IPlcConfigStore _store;
    private readonly TagSubscriptionManager _subscriptionManager;
    private readonly ILogger<PlcDataService> _logger;

    public PlcDataService(IPlcHub hub, IPlcConfigStore store, TagSubscriptionManager subscriptionManager, ILogger<PlcDataService> logger)
    {
        _hub = hub;
        _store = store;
        _subscriptionManager = subscriptionManager;
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
            dbNumber = g.DbNumber,
            tagCount = _store.GetTagsByGroup(g.Id).Count,
            createdAt = g.CreatedAt
        } as object).ToList();
    }

    [BridgeMethod(Name = "CreateDbGroup")]
    public object CreateDbGroup(CreateDbGroupRequest req)
    {
        var group = new DbGroup
        {
            PlcName = req.PlcName,
            Name = req.GroupName,
            DbNumber = req.DbNumber,
            DbSize = req.DbSize
        };
        _store.AddGroup(group);
        return new { id = group.Id, name = group.Name, dbNumber = group.DbNumber, dbSize = group.DbSize };
    }

    [BridgeMethod(Name = "DeleteDbGroup")]
    public bool DeleteDbGroup(GroupIdRequest req) => _store.RemoveGroup(req.GroupId) is not null;

    [BridgeMethod(Name = "RenameDbGroup")]
    public bool RenameDbGroup(RenameDbGroupRequest req) => _store.RenameGroup(req.GroupId, req.NewName);

    // ── Tags ─────────────────────────────────────────

    [BridgeMethod(Name = "ListTags")]
    public List<object> ListTags(ListTagsRequest req)
    {
        var tags = _store.GetTagsByGroup(req.GroupId);
        var group = _store.GetGroup(req.GroupId);
        var results = new List<object>();

        if (group is null) return results;

        foreach (var tag in tags)
        {
            object? value = null;
            try
            {
                var session = _hub.For(group.PlcName);
                value = ReadTagValue(session, tag);
            }
            catch
            {
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
        if (!Enum.TryParse<PlcDataType>(req.DataType, true, out var plcType))
            throw new ArgumentException($"不支持的数据类型: {req.DataType}");

        var group = _store.GetGroup(req.GroupId)
            ?? throw new KeyNotFoundException($"分组 '{req.GroupId}' 未找到。");

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
            session.AddTag(new TagDefinition(tag.Name, tag.Address, tag.DataType, tag.StringLength));
            _subscriptionManager.SubscribeTag(group.PlcName, tag.Name, tag.Address, plcType.ToString());
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
                _subscriptionManager.UnsubscribeTag(group.PlcName, tag.Name);
                session.RemoveTag(tag.Name);
            }
            catch { }
        }

        _store.RemoveTag(req.TagId);
        return true;
    }

    [BridgeMethod(Name = "UpdateTag")]
    public async Task<object> UpdateTag(UpdateTagRequest req)
    {
        var oldTag = _store.GetTag(req.TagId)
            ?? throw new KeyNotFoundException($"点位 '{req.TagId}' 未找到。");

        if (!Enum.TryParse<PlcDataType>(req.DataType, true, out var plcType))
            throw new ArgumentException($"不支持的数据类型: {req.DataType}");

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

                // 取消旧订阅
                _subscriptionManager.UnsubscribeTag(group.PlcName, oldTag.Name);

                // 如果名称变了，需要移除旧标签再添加新标签
                if (!string.Equals(oldTag.Name, req.TagName, StringComparison.Ordinal))
                {
                    session.RemoveTag(oldTag.Name);
                }
                session.AddTag(new TagDefinition(req.TagName, req.Address, plcType, req.StringLength));

                // 建立新订阅
                _subscriptionManager.SubscribeTag(group.PlcName, req.TagName, req.Address, plcType.ToString());
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
            ?? throw new KeyNotFoundException($"点位 '{req.TagId}' 未找到。");

        var group = _store.GetGroup(tag.GroupId);
        if (group is null) return null;

        var session = _hub.For(group.PlcName);
        return ReadTagValue(session, tag);
    }

    [BridgeMethod(Name = "WriteTag")]
    public async Task<object> WriteTag(WriteTagRequest req)
    {
        var tag = _store.GetTag(req.TagId!)
            ?? throw new KeyNotFoundException($"点位 '{req.TagId}' 未找到。");

        var group = _store.GetGroup(tag.GroupId)
            ?? throw new KeyNotFoundException($"点位所属分组未找到。");

        var session = _hub.For(group.PlcName);

        var typedValue = ConvertFromPayload(req.Value!, tag.DataType);

        try
        {
            var result = await session.SetAsync(tag.Name, typedValue);
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

    // ── Helper Methods ───────────────────────────────

    private static object ReadTagValue(IPlcSession session, TagConfig tag)
    {
        return tag.DataType switch
        {
            PlcDataType.Bool => session.Get<bool>(tag.Name),
            PlcDataType.Byte => session.Get<byte>(tag.Name),
            PlcDataType.Int16 => session.Get<short>(tag.Name),
            PlcDataType.UInt16 => session.Get<ushort>(tag.Name),
            PlcDataType.Int32 => session.Get<int>(tag.Name),
            PlcDataType.UInt32 => session.Get<uint>(tag.Name),
            PlcDataType.Float => session.Get<float>(tag.Name),
            PlcDataType.Double => session.Get<double>(tag.Name),
            PlcDataType.String => session.Get<string>(tag.Name),
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



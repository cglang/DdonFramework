using Ddon.EventBus.Contracts;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VitrinRuntime.Desktop.Handlers;

namespace VitrinRuntime.Services;

/// <summary>
/// 集中管理所有 PLC 点位的变化订阅。
/// 在 AddTag/ConnectPlc 后立即注册 Subscribe，值变化时发布
/// <see cref="TagValueChangedEvent"/> 到 <see cref="IEventBus"/>。
/// 其他组件通过实现 <see cref="IEventHandler{T}"/> 来处理该事件。
/// Subscribe 返回的 IDisposable 统一在此管理，按 PLC + tagName 索引。
/// </summary>
public sealed class TagSubscriptionManager : IDisposable
{
    private readonly IPlcHub _hub;
    private readonly IEventBus _eventBus;
    private readonly ILogger<TagSubscriptionManager> _logger;

    // plcName → (tagName → IDisposable)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IDisposable>> _subscriptions
        = new(StringComparer.OrdinalIgnoreCase);

    public TagSubscriptionManager(
        IPlcHub hub,
        IEventBus eventBus,
        ILogger<TagSubscriptionManager> logger)
    {
        _hub = hub;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>订阅一个点位的变化，值变化时通过 <see cref="IEventBus"/> 发布 <see cref="TagValueChangedEvent"/></summary>
    public void SubscribeTag(string plcName, TagDefinition tag)
    {
        try
        {
            var session = _hub.For(plcName);

            // 先解除旧的同名订阅（如果存在），避免重复订阅
            UnsubscribeTag(plcName, tag.Name);

            var sub = session.Subscribe<object>(tag.Name, (oldVal, newVal) =>
            {
                // 值变化 → 通过 EventBus 发布事件，Handler 负责推送到前端
                Task.Run(async () =>
                {
                    try
                    {
                        await _eventBus.PublishAsync(new TagValueChangedEvent
                        {
                            TagName = tag.Name,
                            Address = tag.Address,
                            DataType = tag.Type.ToString(),
                            OldValue = oldVal,
                            NewValue = newVal
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogTrace(ex, "发布点位 '{Tag}' 变化事件失败", tag.Name);
                    }
                });
            });

            var plcSubs = _subscriptions.GetOrAdd(plcName,
                _ => new ConcurrentDictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase));
            plcSubs[tag.Name] = sub;

            _logger.LogDebug("已订阅点位 '{Plc}.{Tag}' 的变化", plcName, tag.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "订阅点位 '{Plc}.{Tag}' 失败", plcName, tag.Name);
        }
    }

    /// <summary>取消订阅一个点位的变化</summary>
    public void UnsubscribeTag(string plcName, string tagName)
    {
        if (_subscriptions.TryGetValue(plcName, out var plcSubs))
        {
            if (plcSubs.TryRemove(tagName, out var sub))
            {
                sub.Dispose();
                _logger.LogDebug("已取消订阅点位 '{Plc}.{Tag}'", plcName, tagName);
            }
        }
    }

    /// <summary>取消订阅一个 PLC 的所有点位变化（断开连接时调用）</summary>
    public void UnsubscribePlc(string plcName)
    {
        if (_subscriptions.TryRemove(plcName, out var plcSubs))
        {
            foreach (var sub in plcSubs.Values)
                sub.Dispose();
            _logger.LogDebug("已取消订阅 PLC '{Plc}' 的所有点位", plcName);
        }
    }

    /// <summary>为指定 PLC 会话中的所有已注册点位建立订阅（ConnectPlc 后调用）</summary>
    public void SubscribeAllTags(string plcName)
    {
        try
        {
            var session = _hub.For(plcName);
            foreach (var tag in session.Tags)
            {
                TagConfig tagConfig = new TagConfig
                {
                    Name = tag.Name,
                    Address = tag.Address,
                    DataType = tag.Type
                };

                SubscribeTag(plcName, tagConfig);
            }
            _logger.LogDebug("PLC '{Plc}' 的所有点位订阅完成，共 {Count} 个", plcName, session.Tags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "为 PLC '{Plc}' 批量订阅点位失败", plcName);
        }
    }

    public void Dispose()
    {
        foreach (var plcSubs in _subscriptions.Values)
        {
            foreach (var sub in plcSubs.Values)
                sub.Dispose();
        }
        _subscriptions.Clear();
    }
}

/// <summary>点位值变化事件，通过 IUiBridge 推送到前端</summary>
public sealed class TagValueChanged
{
    public string TagName { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
}

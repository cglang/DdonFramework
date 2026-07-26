using Ddon.EventBus.Contracts;
using Ddon.VitrinPLC.Abstractions;
using Ddon.VitrinPLC.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using VitrinRuntime.Desktop.Handlers;

namespace VitrinRuntime.Desktop.Services;

public sealed class TagSubscriptionManager : IDisposable
{
    private readonly IPlcHub _hub;
    private readonly IEventBus _eventBus;
    private readonly ILogger<TagSubscriptionManager> _logger;

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

    public void SubscribeTag(string plcName, TagDefinition tag)
    {
        try
        {
            var session = _hub.For(plcName);

            UnsubscribeTag(plcName, tag.Name);

            var sub = session.Subscribe<object>(tag.Name, (oldVal, newVal) =>
            {
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

            _logger.LogDebug("已订阅点位 '{Plc}' 下 '{Tag}' 的变化", plcName, tag.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "订阅点位 '{Plc}' 下 '{Tag}' 失败", plcName, tag.Name);
        }
    }

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

    public void UnsubscribePlc(string plcName)
    {
        if (_subscriptions.TryRemove(plcName, out var plcSubs))
        {
            foreach (var sub in plcSubs.Values)
                sub.Dispose();
            _logger.LogDebug("已取消订阅 PLC '{Plc}' 的所有点位", plcName);
        }
    }

    public void SubscribeAllTags(string plcName)
    {
        try
        {
            var session = _hub.For(plcName);
            foreach (var tag in session.Tags)
            {
                SubscribeTag(plcName, new TagDefinition(tag.Name, tag.Address, tag.Type));
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

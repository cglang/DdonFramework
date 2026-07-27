using System.Collections.Concurrent;
using System.Reflection;
using Ddon.Common.EventBus;
using Ddon.EventBus.Contracts;
using Ddon.LuaEngine;
using Microsoft.Extensions.Logging;

namespace VitrinRuntime.Desktop.Services;

/// <summary>
/// 约定式 Lua 事件桥接服务。
/// 扫描 Lua VM 中定义的 OnXxx 函数，自动订阅 <see cref="GeneralEventBus"/>
/// 中对应的 .NET 事件类型，实现 Lua 脚本响应系统事件。
/// </summary>
public sealed class LuaEventBridgeService
{
    private readonly ILuaVmManager _vmManager;
    private readonly ILogger<LuaEventBridgeService> _logger;

    private readonly ConcurrentDictionary<string, List<EventSubscription>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, object> _groupLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Type> _eventTypeCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly MethodInfo InternalSubscribeMethod = typeof(LuaEventBridgeService)
        .GetMethod(nameof(InternalSubscribe), BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("无法获取 InternalSubscribe 方法");

    public LuaEventBridgeService(ILuaVmManager vmManager, ILogger<LuaEventBridgeService> logger)
    {
        _vmManager = vmManager;
        _logger = logger;
    }

    /// <summary>订阅指定组中 Lua 脚本约定的所有事件。</summary>
    public void SubscribeGroup(string groupName)
    {
        var funcNames = _vmManager.GetFunctionNames(groupName);
        if (funcNames.Count == 0) return;

        var vm = _vmManager.GetVm(groupName);
        if (vm is null) return;

        foreach (var funcName in funcNames)
        {
            if (!funcName.StartsWith("On") || funcName.Length <= 2) continue;

            // OnTagValueChanged → TagValueChangedEvent
            var eventTypeName = funcName.Substring(2) + "Event";
            var eventType = ResolveEventType(eventTypeName);
            if (eventType is null)
            {
                _logger.LogDebug("未找到与 Lua 函数 '{Func}' 匹配的事件类型 '{EventType}'", funcName, eventTypeName);
                continue;
            }

            var luaFunc = vm[funcName] as NLua.LuaFunction;
            if (luaFunc is null) continue;

            try
            {
                var genericMethod = InternalSubscribeMethod.MakeGenericMethod(eventType);
                genericMethod.Invoke(this, [groupName, luaFunc]);

                _logger.LogInformation("已订阅事件 '{EventType}' → Lua 函数 '{Func}'（组: {Group}）",
                    eventType.Name, funcName, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订阅事件 '{EventType}' 失败（组: {Group}）", eventType.Name, groupName);
            }
        }
    }

    /// <summary>取消指定组的所有事件订阅。</summary>
    public void UnsubscribeGroup(string groupName)
    {
        if (_subscriptions.TryRemove(groupName, out var subs))
        {
            foreach (var sub in subs)
            {
                sub.Dispose();
            }
            _logger.LogInformation("已取消组 '{Group}' 的所有事件订阅 ({Count} 个)", groupName, subs.Count);
        }
    }

    /// <summary>重新订阅指定组：先取消旧订阅，再重新扫描订阅。</summary>
    public void ResubscribeGroup(string groupName)
    {
        UnsubscribeGroup(groupName);
        SubscribeGroup(groupName);
    }

    /// <summary>取消所有组的订阅。</summary>
    public void UnsubscribeAll()
    {
        foreach (var groupName in _subscriptions.Keys)
        {
            UnsubscribeGroup(groupName);
        }
    }

    private Type? ResolveEventType(string eventTypeName)
    {
        if (_eventTypeCache.TryGetValue(eventTypeName, out var cached))
            return cached;

        var type = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .SelectMany(a =>
            {
                try { return a.GetExportedTypes(); }
                catch { return []; }
            })
            .FirstOrDefault(t =>
                t.Name.Equals(eventTypeName, StringComparison.Ordinal) &&
                typeof(IEventData).IsAssignableFrom(t) &&
                t.IsClass && !t.IsAbstract);

        if (type is not null)
            _eventTypeCache[eventTypeName] = type;

        return type;
    }

    private void InternalSubscribe<TEvent>(string groupName, NLua.LuaFunction luaFunc)
        where TEvent : IEventData
    {
        var lockObj = _groupLocks.GetOrAdd(groupName, _ => new object());

        var subscription = GeneralEventBus.Default.Subscribe((TEvent evt) =>
        {
            try
            {
                lock (lockObj)
                {
                    luaFunc.Call(evt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lua 事件处理函数 '{Func}' 执行失败（组: {Group}）",
                    luaFunc, groupName);
            }
        });

        var subs = _subscriptions.GetOrAdd(groupName, _ => []);
        lock (subs)
        {
            subs.Add(subscription);
        }
    }
}

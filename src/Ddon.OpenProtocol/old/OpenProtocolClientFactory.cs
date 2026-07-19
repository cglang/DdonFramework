using Ddon.OpenProtocol;
using Microsoft.Extensions.Logging;
using OpenProtocol.Events;
using OpenProtocolInterpreter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AppWinUI.DdonOPClient;

public interface IOpenProtocolClientFactory
{
    /// <summary>按名称获取已注册的客户端实例。</summary>
    IOpenProtocolClient GetClient(string name);

    /// <summary>获取所有已注册的客户端名称。</summary>
    IReadOnlyCollection<string> GetRegisteredNames();
}


internal sealed class OpenProtocolClientFactory : IOpenProtocolClientFactory
{
    // 每个 name → 对应的 options + 已懒建的 client
    private readonly IReadOnlyDictionary<string, OpenProtocolClientOptions> _optionsMap;
    private readonly IServiceProvider _sp;
    private readonly ILogger<OpenProtocolClient> _logger;

    // 懒创建 + 线程安全缓存
    private readonly ConcurrentDictionary<string, IOpenProtocolClient> _cache = new();

    public OpenProtocolClientFactory(
        IEnumerable<OpenProtocolClientOptions> allOptions,
        IServiceProvider sp,
        ILogger<OpenProtocolClient> logger)
    {
        _optionsMap = allOptions.ToDictionary(o => o.Name, o => o);
        _sp = sp;
        _logger = logger;
    }

    public IOpenProtocolClient GetClient(string name)
    {
        return _cache.GetOrAdd(name, n =>
        {
            if (!_optionsMap.TryGetValue(n, out var opts))
                throw new InvalidOperationException(
                    $"No OpenProtocol client registered with name '{n}'. " +
                    $"Registered: [{string.Join(", ", _optionsMap.Keys)}]");

            // 每个客户端有自己的 MidInterpreter 和 EventBus
            var interpreter = BuildInterpreter();
            var eventBus = new OpenProtocolEventBus();
            return new OpenProtocolClient(opts, interpreter, eventBus, _logger);
        });
    }

    public IReadOnlyCollection<string> GetRegisteredNames()
        => _optionsMap.Keys.ToList();

    private static MidInterpreter BuildInterpreter() =>
        new MidInterpreter()
            .UseCustomMessage(new Dictionary<int, Type>
            {
                //{ Mid0900.MID, typeof(Mid0900) },
                //{ Mid0901.MID, typeof(Mid0901) },
            })
            .UseAllMessages();
}

using Ddon.UniPLC.Abstractions;
using Ddon.UniPLC.Models;
using System.Collections.Concurrent;

namespace Ddon.UniPLC.Core;

/// <summary>
/// PLC 客户端工厂注册表
/// </summary>
public class PlcClientFactoryRegistry
{
    private readonly ConcurrentDictionary<string, IPlcClientFactory> _factories;

    public PlcClientFactoryRegistry()
    {
        _factories = new ConcurrentDictionary<string, IPlcClientFactory>();
    }

    /// <summary>
    /// 注册工厂
    /// </summary>
    public void Register(string type, IPlcClientFactory factory)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentNullException(nameof(type));

        _factories.AddOrUpdate(type, factory, (_, _) => factory);
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    public IPlcClient Create(PlcOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (!_factories.TryGetValue(options.Type, out var factory))
            throw new KeyNotFoundException($"PLC client factory for type '{options.Type}' not found");

        return factory.Create(options);
    }

    /// <summary>
    /// 是否已注册指定类型
    /// </summary>
    public bool IsRegistered(string type)
    {
        return _factories.ContainsKey(type);
    }
}

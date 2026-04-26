using Ddon.UniPLC.Abstractions;
using System.Collections.Concurrent;

namespace Ddon.UniPLC.Core;

/// <summary>
/// PLC 提供者实现
/// </summary>
public class PlcProvider : IPlcProvider
{
    private readonly ConcurrentDictionary<string, IPlcClient> _clients;

    public PlcProvider()
    {
        _clients = new ConcurrentDictionary<string, IPlcClient>();
    }

    /// <summary>
    /// 注册 PLC 客户端
    /// </summary>
    public void RegisterClient(string name, IPlcClient client)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        _clients.AddOrUpdate(name, client, (_, _) => client);
    }

    public IPlcClient GetClient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (_clients.TryGetValue(name, out var client))
            return client;

        throw new KeyNotFoundException($"PLC client '{name}' not found");
    }

    public T GetClient<T>() where T : IPlcClient
    {
        var client = _clients.Values.FirstOrDefault(c => c is T);
        if (client is T typedClient)
            return typedClient;

        throw new KeyNotFoundException($"PLC client of type '{typeof(T).Name}' not found");
    }

    /// <summary>
    /// 获取所有已注册的客户端
    /// </summary>
    public IEnumerable<IPlcClient> GetAllClients()
    {
        return _clients.Values;
    }

    /// <summary>
    /// 清除所有客户端
    /// </summary>
    public async Task ClearAsync()
    {
        foreach (var client in _clients.Values)
        {
            await client.DisposeAsync();
        }
        _clients.Clear();
    }
}

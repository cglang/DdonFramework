using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Socket.Core.Storage;

public class SocketCoreSessionStorage : ISocketCoreSessionStorage
{
    private static readonly ConcurrentDictionary<Guid, SocketCoreSession> Pairs = new();

    public SocketCoreSession? Get(Guid socketId)
    {
        return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
    }

    public IEnumerable<SocketCoreSession> Get(IEnumerable<Guid> socketIds)
    {
        return Pairs.Values.Where(x => socketIds.Contains(x.SessionId));
    }

    public IEnumerable<SocketCoreSession> GetAll()
    {
        return Pairs.Values;
    }

    public bool Add(SocketCoreSession session)
    {
        return !Pairs.ContainsKey(session.SessionId) && Pairs.TryAdd(session.SessionId, session);
    }

    public void Remove(Guid clientId)
    {
        if (Pairs.ContainsKey(clientId))
            Pairs.Remove(clientId, out _);
    }
}

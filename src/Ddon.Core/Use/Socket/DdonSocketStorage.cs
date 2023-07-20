using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Use.Socket;

public static class DdonSocketStorage
{
    private static readonly ConcurrentDictionary<Guid, DdonSocketSession> Pairs = new();

    public static IEnumerable<DdonSocketSession> Clients => Pairs.Values;

    public static DdonSocketSession? GetClient(Guid socketId)
    {
        return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
    }

    public static IEnumerable<DdonSocketSession> GetClients(IEnumerable<Guid> socketIds)
    {
        return Pairs.Values.Where(x => socketIds.Contains(x.SocketId));
    }

    public static bool Add(DdonSocketSession session)
    {
        return !Pairs.ContainsKey(session.SocketId) && Pairs.TryAdd(session.SocketId, session);
    }

    public static void Remove(Guid clientId)
    {
        if (Pairs.ContainsKey(clientId))
            Pairs.Remove(clientId, out _);
    }
}

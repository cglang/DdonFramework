using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Ddon.Core.Use.Socket;

public static class SocketStorage
{
    private static readonly ConcurrentDictionary<Guid, SocketCoreSession> Pairs = new();

    public static IEnumerable<SocketCoreSession> Clients => Pairs.Values;

    public static SocketCoreSession? GetClient(Guid socketId)
    {
        return Pairs.ContainsKey(socketId) ? Pairs[socketId] : null;
    }

    public static IEnumerable<SocketCoreSession> GetClients(IEnumerable<Guid> socketIds)
    {
        return Pairs.Values.Where(x => socketIds.Contains(x.SessionId));
    }

    public static bool Add(SocketCoreSession session)
    {
        return !Pairs.ContainsKey(session.SessionId) && Pairs.TryAdd(session.SessionId, session);
    }

    public static void Remove(Guid clientId)
    {
        if (Pairs.ContainsKey(clientId))
            Pairs.Remove(clientId, out _);
    }
}

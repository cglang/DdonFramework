using System;
using System.Collections.Generic;
using Ddon.Socket.Core;

namespace Ddon.SimpeSocket.Core.Storage
{
    public interface ISocketSessionStorage
    {
        SocketSession Get(Guid socketId);

        IEnumerable<SocketSession> Get(IEnumerable<Guid> socketIds);

        IEnumerable<SocketSession> GetAll();

        bool Add(SocketSession session);

        void Remove(Guid clientId);
    }
}

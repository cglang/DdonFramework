using System;
using System.Collections.Generic;

namespace Ddon.Socket.Core.Storage
{
    public interface ISocketSessionStorage
    {
        SocketSession? Get(Guid socketId);

        IEnumerable<SocketSession> Get(IEnumerable<Guid> socketIds);

        IEnumerable<SocketSession> GetAll();

        internal bool Add(SocketSession session);

        internal void Remove(Guid clientId);
    }
}

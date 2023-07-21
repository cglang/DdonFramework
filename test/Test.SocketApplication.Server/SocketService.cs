using Ddon.Socket.Handler;
using Ddon.Socket.Hosting;
using Ddon.Socket.Options;

namespace Test.SocketApplication.Server
{
    public class SocketService : SocketBackgroundService
    {
        public SocketService(SocketServerHandler handle) : base(handle)
        {
        }

        protected override SocketServerOption Configure()
        {
            return new()
            {
                Port = 6012,
            };
        }
    }
}

using Ddon.TuuTools.Socket.Exceptions;

namespace Ddon.TuuTools.Socket.Handler
{
    public class DdonSocketCoreHandler
    {
        protected Func<DdonSocketCore, Memory<byte>, Task>? ByteHandler;
        protected Func<DdonSocketCore, string, Task>? StringHandler;
        protected Func<DdonSocketCore, DdonSocketException, Task>? ExceptionHandler;
    }

    public class DdonSocketServerHandler: DdonSocketCoreHandler
    {
        protected Func<DdonSocketCore, Task>? ConnectHandler;
    }
}

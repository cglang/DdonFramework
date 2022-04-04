using Ddon.ConvenientSocket.Extra;
using Ddon.Socket.Connection;

namespace Ddon.Socket
{
    public static class DdonSocketRequest
    {
        public static DdonSocketPackageInfo<string> RequestWait(this DdonSocketConnectionCore connection, string route, string data)
        {
            var timeOut = false;
            DdonSocketPackageInfo<string>? info = null;
            connection.Request(route, data).Then(inf => { info = inf; }).Exception(inf => { info = inf; });
            while (info == null)
            {
                if (timeOut) throw new Exception("请求超时");
            }
            return info!;
        }
    }
}

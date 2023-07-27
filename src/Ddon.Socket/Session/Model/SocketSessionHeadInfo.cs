using System;

namespace Ddon.Socket.Session.Model
{
    public class SocketSessionHeadInfo
    {
        public Guid Id { get; set; }

        public SocketMode Mode { get; set; }

        public SocketResponseCode Code { get; set; }

        public string Route { get; set; }

        public int BlockIndex { get; set; } = 0;

        public string? FileName { get; set; }

        public SocketSessionHeadInfo(Guid id, SocketMode mode, string route)
        {
            Id = id;
            Mode = mode;
            Route = route;
        }

        public SocketSessionHeadInfo Response(SocketResponseCode code = SocketResponseCode.OK)
        {
            Mode = SocketMode.Response;
            Route = string.Empty;
            Code = code;

            return this;
        }
    }
}

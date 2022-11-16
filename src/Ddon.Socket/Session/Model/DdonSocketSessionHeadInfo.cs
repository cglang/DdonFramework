using Ddon.Core.Use.Socket;
using System;
using System.Text;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketSessionHeadInfo
    {
        public Guid Id { get; set; }

        public DdonSocketMode Mode { get; set; }

        public DdonSocketResponseCode Code { get; set; }

        public string Route { get; set; }

        public int BlockIndex { get; set; } = 0;

        public string? FileName { get; set; }

        public DdonSocketSessionHeadInfo(Guid id, DdonSocketMode mode, string route)
        {
            Id = id;
            Mode = mode;
            Route = route;
        }

        public DdonSocketSessionHeadInfo Response(DdonSocketResponseCode code = DdonSocketResponseCode.OK)
        {
            Mode = DdonSocketMode.Response;
            Route = string.Empty;
            Code = code;

            return this;
        }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(SocketSession.JsonSerialize(this));
        }
    }
}

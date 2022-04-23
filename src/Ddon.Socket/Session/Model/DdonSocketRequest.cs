using System;
using System.Text;
using System.Text.Json;

namespace Ddon.Socket.Session.Model
{
    public class DdonSocketRequest
    {
        public Guid Id { get; set; }

        public DdonSocketMode Mode { get; set; }

        public string Api { get; set; }

        public DdonSocketResponseCode Code { get; set; }

        public DdonSocketRequest(Guid id, DdonSocketMode mode, string api)
        {
            Id = id;
            Mode = mode;
            Api = api;
        }

        public static byte[] GetHeadBytes(DdonSocketMode mode, string api, Guid id = default)
        {
            DdonSocketRequest head = new(id, mode, api);
            return head.GetBytes();
        }

        public DdonSocketRequest Response(DdonSocketResponseCode code = DdonSocketResponseCode.OK)
        {
            Mode = DdonSocketMode.Response;
            Api = string.Empty;
            Code = code;

            return this;
        }

        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(ToString());
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}

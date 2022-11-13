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

        public int BlockIndex { get; set; } = 0;

        public string? FileName { get; set; }

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
            //var bytes = new byte[DdonSocketConst.HeadLength];
            //var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
            //Array.Copy(jsonBytes, 0, bytes, 0, jsonBytes.Length);
            //return bytes;
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
        }

        public bool IsEnd => BlockIndex == -1;

        public DdonSocketRequest SetEnd()
        {
            BlockIndex = -1;
            return this;
        }

        public DdonSocketRequest CountAddOne()
        {
            BlockIndex += 1;
            return this;
        }
    }
}

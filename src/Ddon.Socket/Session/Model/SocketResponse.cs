namespace Ddon.Socket.Session.Model
{
    public class SocketResponse<T>
    {
        public SocketResponse(SocketResponseCode code, T data)
        {
            Code = code;
            Data = data;
        }

        public SocketResponseCode Code { get; set; }

        public T Data { get; set; }
    }
}

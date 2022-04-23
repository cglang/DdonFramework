namespace Ddon.Socket.Session.Model
{
    public class DdonSocketResponse<T>
    {
        public DdonSocketResponse(DdonSocketResponseCode code, T data)
        {
            Code = code;
            Data = data;
        }

        public DdonSocketResponseCode Code { get; set; }

        public T Data { get; set; }
    }
}

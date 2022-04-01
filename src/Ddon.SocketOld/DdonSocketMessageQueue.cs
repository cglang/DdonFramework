using Ddon.Socket.Extra;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddon.Socket
{
    /// <summary>
    /// 收到的消息队列
    /// </summary>
    public class DdonSocketMessageQueue
    {
        private int a = 0;
        private static readonly object _lock = new();
        private static DdonSocketMessageQueue? obj;
        private Action<DdonSocketPackageInfo<byte[]>> DataHandle;

        private DdonSocketMessageQueue(Action<DdonSocketPackageInfo<byte[]>> action)
        {
            DataHandle = action;
        }

        public static DdonSocketMessageQueue GetInstance(Action<DdonSocketPackageInfo<byte[]>> action)
        {
            if (obj != null) return obj;
            lock (_lock) obj ??= new DdonSocketMessageQueue(action);
            return obj;
        }

        public void Enqueue(DdonSocketPackageInfo<byte[]> message)
        {
            DataHandle.Invoke(message);
        }
    }

    public class QueueItem<TData>
    {
        public byte[] Bytes { get; set; }
    }
}

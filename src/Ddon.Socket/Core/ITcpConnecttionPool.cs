using System;
using System.Threading.Tasks;

namespace Ddon.Socket.Core
{
    public interface ITcpConnecttionPool
    {
        /// <summary>
        /// 添加一个可用的连接
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        void Add(IDdonTcpClient tcpClient);

        /// <summary>
        /// 移除一个Tcp连接
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        bool Remove(Guid Id);

        /// <summary>
        /// 取出一个需读取数据连接对象
        /// </summary>
        /// <returns></returns>
        Task<IDdonTcpClient> TakeAsync();
    }
}

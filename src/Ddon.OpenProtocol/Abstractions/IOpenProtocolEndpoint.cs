using System;
using System.Threading;
using System.Threading.Tasks;
using Ddon.OpenProtocol.Models;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Abstractions
{
    public interface IOpenProtocolEndpoint : IDisposable
    {
        string Name { get; }

        ConnectionState State { get; }

        bool IsConnected { get; }

        Task ConnectAsync(CancellationToken cancellationToken = default);

        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 发送一个请求，并等待返回一个响应。
        /// 返回的 <see cref="Mid"/> 即为下一个接收到的 MID，不关心其具体类型。
        /// </summary>
        Task<Mid> SendAsync(Mid request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 注册订阅：当收到指定类型的 MID 时执行 <paramref name="handler"/>。
        /// 订阅的 MID 只投递给 handler，不会作为 <see cref="SendAsync"/> 的普通响应。
        /// 返回的 <see cref="IDisposable"/> 用于取消订阅。
        /// </summary>
        IDisposable Subscribe<TMid>(Func<TMid, Task> handler) where TMid : Mid;

        /// <summary>
        /// 发送订阅请求（如 MID0060）并等待其确认响应（请求-响应，如 MID0005），返回该确认响应。
        /// 之后服务端在任意时刻推送的 <typeparamref name="TMid"/>（如 MID0061）都会执行 <paramref name="handler"/>，
        /// 同时调用 <paramref name="ackHandler"/> 生成确认回复（<typeparamref name="TAckMid"/>，如 MID0062）发送给服务端。
        /// </summary>
        Task<Mid> SubscribeAsync<TMid, TAckMid>(Mid subscribeRequest,
            Func<TMid, Task> handler,
            Func<TAckMid> ackHandler,
            CancellationToken cancellationToken = default
            ) where TMid : Mid where TAckMid : Mid;
    }
}

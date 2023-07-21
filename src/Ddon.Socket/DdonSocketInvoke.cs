using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.Core.Use.Socket;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket
{
    public class DdonSocketInvoke
    {
        private readonly IServiceProvider services;

        public DdonSocketInvoke(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task<dynamic?> IvnvokeAsync(
            string className,
            string methodName,
            string parameter,
            Core.Use.Socket.SocketCoreSession connection,
            DdonSocketSessionHeadInfo head)
        {
            using var scope = services.CreateScope();
            var classType = DdonType.GetTypeByName(className);
            var instance = scope.ServiceProvider.GetService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (SocketApiBase)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }

        public async Task<dynamic?> IvnvokeAsync<T>(
            string className,
            string methodName,
            T parameter,
            Core.Use.Socket.SocketCoreSession connection,
            DdonSocketSessionHeadInfo head) where T : notnull
        {
            using var scope = services.CreateScope();
            var classType = DdonType.GetTypeByName(className);
            var instance = scope.ServiceProvider.GetService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (SocketApiBase)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }
    }
}

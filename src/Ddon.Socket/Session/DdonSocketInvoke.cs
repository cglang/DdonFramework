using Ddon.Core.Use.Di;
using Ddon.Core.Use.Reflection;
using Ddon.Socket.Session.Model;
using System;
using System.Threading.Tasks;

namespace Ddon.Socket.Session
{
    public class DdonSocketInvoke : ITransientDependency
    {
        private readonly IServiceProvider services;

        public DdonSocketInvoke(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task<dynamic?> IvnvokeAsync(
            (string, string) methodinfo,
            string parameter,
            SocketSession connection,
            DdonSocketRequest head)
        {
            return await IvnvokeAsync(methodinfo.Item1, methodinfo.Item2, parameter, connection, head);
        }

        public async Task<dynamic?> IvnvokeAsync(
            string className,
            string methodName,
            string parameter,
            SocketSession connection,
            DdonSocketRequest head)
        {
            var classType = DdonType.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (SocketApiCore)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }

        public async Task<dynamic?> IvnvokeAsync<T>(
            string className,
            string methodName,
            T parameter,
            SocketSession connection,
            DdonSocketRequest head) where T : notnull
        {
            var classType = DdonType.GetTypeByName(className);
            var instance = services.GetService(classType) ?? throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            var ddonSocketService = (SocketApiCore)instance;
            ddonSocketService.Session = connection;
            ddonSocketService.Head = head;

            var method = DdonType.GetMothodByName(classType, methodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }
    }
}

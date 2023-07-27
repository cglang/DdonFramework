using System;
using System.Reflection;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.Socket.Core;
using Ddon.Socket.Session;
using Ddon.Socket.Session.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Ddon.Socket.Utility
{
    public class SocketInvoke
    {
        private readonly IServiceProvider _services;

        public SocketInvoke(IServiceProvider services)
        {
            _services = services;
        }

        public Task<dynamic?> IvnvokeAsync(
            string className,
            string methodName,
            string parameter,
            SocketSession connection,
            SocketSessionHeadInfo head)
        {
            using var scope = _services.CreateScope();
            var classType = DdonType.GetTypeByName(className);
            var instance = scope.ServiceProvider.GetService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            if (instance is SocketApiBase socketApi)
            {
                socketApi.Session = connection;
                socketApi.Head = head;
            }

            var method = DdonType.GetMothodByName(classType, methodName);
            return DdonInvoke.InvokeAsync(instance, method, parameter);
        }
    }

    public class SocketEndpoint
    {
        private readonly object _instance;

        private readonly MethodInfo _method;

        private readonly object?[]? _parameters;

        public SocketEndpoint(object instance, MethodInfo method, params object?[]? parameters)
        {
            _instance = instance;
            _method = method;
            _parameters = parameters;
        }

        public Task<object?> IvnvokeAsync()
        {
            return DdonInvoke.InvokeAsync(_instance, _method, _parameters);
        }
    }
}

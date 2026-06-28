using System;
using System.Threading.Tasks;
using Ddon.Core.Use.Reflection;
using Ddon.Socket.Session;
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

        public async Task<dynamic?> InvokeAsync(SocketContext context, string parameter)
        {
            using var scope = _services.CreateScope();
            if (context.EndPoint is null) return null;

            var classType = DdonType.GetTypeByName(context.EndPoint.ClassName);
            var instance = scope.ServiceProvider.GetService(classType) ??
                throw new Exception($"从[ServiceProvider]中找不到[{nameof(classType)}]类型的对象");

            if (instance is SocketApiBase socketApi)
            {
                socketApi.SocketContext = context;
            }

            var method = DdonType.GetMothodByName(classType, context.EndPoint.MethodName);
            return await DdonInvoke.InvokeAsync(instance, method, parameter);
        }
    }
}

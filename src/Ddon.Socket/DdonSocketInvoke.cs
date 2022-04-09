using Ddon.ConvenientSocket.Extra;
using Ddon.Core;
using Ddon.Socket.Connection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ddon.Socket
{
    public class DdonSocketInvoke
    {
        private static DdonSocketConnectionCore? Connection;
        private static DdonSocketHead? Head;

        public static async Task<string> IvnvokeReturnJsonAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string methodParameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head)
        {
            var returnData = await IvnvokeAsync(services, className, methodName, methodParameter, connection, head);
            return JsonSerializer.Serialize(returnData);
        }

        public static async Task<dynamic?> IvnvokeAsync(
            IServiceProvider services,
            string className,
            string methodName,
            string parameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head)
        {
            Connection = connection;
            Head = head;
            return await DdonInvokeForServiceHelper.InvokeAsync(services, className, methodName, parameter);
        }

        public static async Task<dynamic?> IvnvokeAsync(
            IServiceProvider services,
            string className,
            string methodName,
            byte[] parameter,
            DdonSocketConnectionCore connection,
            DdonSocketHead head)
        {
            Connection = connection;
            Head = head;
            return await DdonInvokeForServiceHelper.InvokeAsync(services, className, methodName, new object[] { parameter });
        }
    }
}

using System;
using System.Threading.Tasks;
using OpenProtocolInterpreter;

namespace Ddon.OpenProtocol.Models
{
    internal class MidSendContext
    {
        public Mid Mid { get; }

        public TaskCompletionSource<bool> Tcs { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public MidSendContext(Mid mid)
        {
            Mid = mid;
        }
    }
}

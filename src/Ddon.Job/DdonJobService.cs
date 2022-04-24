using Ddon.Core;

namespace Ddon.Job
{
    public class DdonJobService
    {
        public static void Start(IServiceProvider serviceProvider)
        {
            DdonServiceProvider.InitServiceProvider(serviceProvider);
        }
    }
}

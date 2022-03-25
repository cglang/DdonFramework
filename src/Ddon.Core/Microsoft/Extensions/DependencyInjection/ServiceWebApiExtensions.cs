using Ddon.Core.Filters;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// WebApi拓展
    /// </summary>
    public static class ServiceWebApiExtensions
    {
        /// <summary>
        /// 向容器当中添加一些功能组成WebApi，包含AddControllers、AddHttpContextAccessor、AppSrtings、自动批量注入
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(GlobalExceptionFilter));
            });

            services.AddHttpContextAccessor();
        }
    }
}

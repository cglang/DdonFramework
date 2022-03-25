using Ddon.Core.Services.Permission;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 权限定义拓展
    /// </summary>
    public static class ServicePermissionsExtensions
    {
        public static void AddPermissions<TPermissions>(this IServiceCollection services) where TPermissions : class, IPermissionDefinitionProvider
        {
            services.AddTransient<IPermissionDefinitionProvider, TPermissions>();
        }
    }
}

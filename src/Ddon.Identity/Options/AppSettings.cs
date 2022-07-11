using Ddon.Core.Use.Di;

namespace Ddon.Identity.Options
{
    public class AppSettings : ISingletonDependency
    {
        /// <summary>
        /// 系统管理员名称
        /// </summary>
        public string SystemAdminName { get; set; } = "root";
    }
}

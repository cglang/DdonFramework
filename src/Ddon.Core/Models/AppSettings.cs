using Ddon.Core.DependencyInjection;
using System;

namespace Ddon.Core.Models
{
    public class AppSettings : ISingletonDependency
    {
        /// <summary>
        /// 系统管理员名称
        /// </summary>
        public string SystemAdminName { get; set; } = "root";
    }
}

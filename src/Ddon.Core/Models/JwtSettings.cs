using System;

namespace Ddon.Core.JwtBearer
{
    public class JwtSettings
    {
        public string SecurityKey { get; set; } = string.Empty;

        public TimeSpan ExpiresIn { get; set; }

        /// <summary>
        /// Token 缓存过期时间(RefreshToken)
        /// </summary>
        public TimeSpan TokenCacheExpiration { get; set; }
    }
}

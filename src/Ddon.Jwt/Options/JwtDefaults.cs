namespace Ddon.Jwt.Options
{
    public class JwtDefaults
    {
        /// <summary>
        /// 默认颁发者
        /// </summary>
        public const string Issuer = "default";

        /// <summary>
        /// 默认受众
        /// </summary>
        public const string Audience = "default";

        /// <summary>
        /// 有效期(小时)
        /// </summary>
        public const double ExpiresHour = 24;

        /// <summary>
        /// 有效期(天)
        /// </summary>
        public const double ExpiresDay = 10;

        public const string RefreshTokenCacheKey = "_RefreshTokenKey_";

        public const string CookieKey = "x-access-token";
    }
}
